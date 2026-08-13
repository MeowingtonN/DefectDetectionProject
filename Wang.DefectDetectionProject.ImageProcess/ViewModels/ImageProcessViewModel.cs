using HalconDotNet;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using Wang.DefectDetectionProject.Core;
using Wang.DefectDetectionProject.Core.ImageProcess.Enums;
using Wang.DefectDetectionProject.Core.ImageProcess.RecipeService.RecipeModels;
using Wang.DefectDetectionProject.Core.ROI;
using Wang.DefectDetectionProject.Core.Tools;
using Wang.DefectDetectionProject.ImageProcess.RecipeService;
using Wang.DefectDetectionProject.ImageProcess.Services;
using Wang.DefectDetectionProject.Shared.Controls;
using Wang.DefectDetectionProject.Shared.Event;

namespace Wang.DefectDetectionProject.ImageProcess.ViewModels
{
    public class ImageProcessViewModel : NavigationViewModel
    {
        public ImageProcessViewModel(IEventAggregator eventAggregator, ImageProcessService imageProcessService)
        {
            eventAggregator.GetEvent<LanguageEventBus>().Subscribe(LanguageChanged);

            ImageProcessService = imageProcessService;

            DrawingObjectList = new ObservableCollection<DrawingObjectInfo>();
            SrcImages = new ObservableCollection<HObject>();
            SrcImageFileNames = new ObservableCollection<string>();

            LoadSingleImageCommand = new DelegateCommand(LoadSingleImage);
            LoadImagesCommand = new DelegateCommand(LoadImages);
            LoadDirectoryCommand = new DelegateCommand(LoadDirectory);
            LoadImgSaveDirectoryCommand = new DelegateCommand(LoadImgSaveDirectory);
            RunDebuggingCommand = new DelegateCommand(RunDebugging);
            RunAndSaveCommand = new DelegateCommand(RunAndSave);

            SetROIAreaCommand = new DelegateCommand(SetROIArea);
            ClearROIAreaCommand = new DelegateCommand(() =>
            {
                if(ROI != null)
                {
                    ROI = null;
                    Message = $"{DateTime.Now: 清除矩形ROI区域成功！}";
                }
            });

            RecipeSavingCommand = new DelegateCommand(RecipeSaving);
            RecipeApplyingCommand = new DelegateCommand(RecipeApplying);

            GrayscaleCommand = new DelegateCommand(ImageProcessService.AddGrayscaleNode);
            HistogramEqualizationCommand = new DelegateCommand(ImageProcessService.AddHistogramEqualizationNode);
            MedianFilterCommand = new DelegateCommand(ImageProcessService.AddMedianFilterNode);
            GaussianFilterCommand = new DelegateCommand(ImageProcessService.AddGaussianFilterNode);

            // 在Nodify Editor中添加不可删除的“图像源”结点，并将其设置为遍历的开始结点
            var srcImgNode = new NodeViewModel
            {
                TitleKey = "ImgSrc",
                Location = new Point(70, 70),
                CanDelete = false
            };
            srcImgNode.Title = LanguageHelper.TranslationKeyValues![srcImgNode.TitleKey];
            var srcImgNodeOutput = new ConnectorViewModel
            {
                TitleKey = "Output",
                Anchor = new Point(0, 30),
                IsInput = false,
                OwnerNode = srcImgNode
            };
            srcImgNodeOutput.Title = LanguageHelper.TranslationKeyValues![srcImgNodeOutput.TitleKey];
            srcImgNode.OutputConnectors.Add(srcImgNodeOutput);
            srcImgNode.ImageProcessOperator = ImgProcessOperator.ImgSrc;
            ImageProcessService.EditorViewModel.Nodes.Add(srcImgNode);
            ImageProcessService.EditorViewModel.StartNode = srcImgNode;
        }

        /// <summary>
        /// 语言更改回调处理
        /// </summary>
        /// <param name="status"></param>
        private void LanguageChanged(bool status)
        {
            foreach(var node in ImageProcessService.EditorViewModel.Nodes)
            {
                node.Title = LanguageHelper.TranslationKeyValues![node.TitleKey!];
                foreach(var connector1 in node.InputConnectors)
                {
                    connector1.Title = LanguageHelper.TranslationKeyValues![connector1.TitleKey!];
                }
                foreach (var connector2 in node.OutputConnectors)
                {
                    connector2.Title = LanguageHelper.TranslationKeyValues![connector2.TitleKey!];
                }
            }
        }

        #region 保存配方与应用配方
        /// <summary>
        /// 保存图像处理配方
        /// </summary>
        private void RecipeSaving()
        {
            // 1. 生成 JSON 字符串
            string json = RecipeManager.SaveRecipeToJson(ImageProcessService.EditorViewModel, ROI);

            // 2. 配置保存对话框
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                DefaultExt = ".json",
                FileName = "ImageProcessRecipe.json",
                Title = "保存图像处理配方"
            };

            // 3. 显示对话框，用户点击保存则写入文件
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, json, System.Text.Encoding.UTF8);
                    Message = $"{DateTime.Now}: 保存配方成功。";
                    return;
                }
                catch (Exception ex)
                {
                    // 根据项目需要处理异常（如记录日志）
                    System.Diagnostics.Debug.WriteLine($"保存配方失败: {ex.Message}！");
                    Message = $"{DateTime.Now}: 保存配方失败: {ex.Message}！";
                    return;
                }
            }
            return; // 用户取消
        }

        /// <summary>
        /// 应用图像处理配方
        /// </summary>
        private void RecipeApplying()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*";
            dialog.Title = "选择图像处理配方";
            var dialogResult = (bool)dialog.ShowDialog()!;
            if (dialogResult)
            {
                string json;
                try
                {
                    json = File.ReadAllText(dialog.FileName, System.Text.Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"读取失败: {ex.Message}。");
                    return;
                }

                // 从 JSON 加载配方
                ImageProcessRecipe? recipe = RecipeManager.LoadRecipeFromJson(json);

                if (recipe != null)
                {
                    ROIParams? roi = ROI;
                    // 应用配方到编辑器
                    ImageProcessService.ApplyRecipeToEditor(recipe, ref roi, ImageProcessService.EditorViewModel);
                    ROI = roi;

                    RecipeFileName = dialog.FileName;
                }
            }
        }

        /// <summary>
        /// 应用的配方文件名称
        /// </summary>
        private string? recipeFileName;
        /// <summary>
        /// 应用的配方文件名称
        /// </summary>
        public string? RecipeFileName
        {
            get { return recipeFileName; }
            set { recipeFileName = value; RaisePropertyChanged(); }
        }
        #endregion

        #region 运行
        /// <summary>
        /// 图像处理调试运行
        /// </summary>
        private void RunDebugging()
        {
            HObject? currentImg = CurrentImage;
            ImageProcessService.RunDebugging(HWindow, SrcImages, ref currentImg, ROI);
            CurrentImage = currentImg;
        }

        /// <summary>
        /// 执行图像处理并保存
        /// </summary>
        private async void RunAndSave()
        {
            try
            {
                await ImageProcessService.RunAndSave(HWindow, SrcImages, SrcImageFileNames, ROI, ImageSaveDirectory);
            }
            catch (Exception ex)
            {
                Message = $"{DateTime.Now}: 执行图像处理并保存过程发生异常：{ex.Message}。";
                return;
            }
            Message = $"{DateTime.Now}: 执行图像处理并保存过程执行完毕。";
        }
        #endregion

        #region 设置矩形ROI范围
        /// <summary>
        /// 设置矩形ROI范围
        /// </summary>
        private void SetROIArea()
        {
            DrawingObjectInfo? hObjectInfo = DrawingObjectList!.LastOrDefault();
            if (hObjectInfo != null && hObjectInfo.HTuples != null && hObjectInfo.ShapeType == ShapeType.Rectangle)
            {
                ROI = new ROIParams()
                {
                    Row1 = hObjectInfo.HTuples[0]!.D,
                    Column1 = hObjectInfo.HTuples[1]!.D,
                    Row2 = hObjectInfo.HTuples[2]!.D,
                    Column2 = hObjectInfo.HTuples[3]!.D
                };
                Message = $"{DateTime.Now: 设置矩形ROI区域成功！}";
            }
            else
            {
                Message = $"{DateTime.Now: 请先绘制矩形ROI识别范围后再点击对应按钮设置ROI区域。}";
            }
        }
        #endregion

        #region 加载图像与加载图像保存路径
        /// <summary>
        /// 加载单张图像
        /// </summary>
        private void LoadSingleImage()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "请选择要进行图像处理的图像";
            dialog.Filter = "图像文件|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|所有文件|*.*";
            var dialogResult = (bool)dialog.ShowDialog()!;
            if (dialogResult)
            {
                HImage img = new HImage();
                try
                {
                    img.ReadImage(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载图像出错，原因：{ex.Message}.", "确认", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SrcImages!.Clear();
                DrawingObjectList?.Clear();
                SrcImages.Add(img);
                CurrentImage = img;
            }
        }

        /// <summary>
        /// 批量加载图像
        /// </summary>
        private void LoadImages()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "请选择要进行图像处理的图像";
            dialog.Multiselect = true;  // 允许多选
            dialog.Filter = "图像文件|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|所有文件|*.*";

            if (dialog.ShowDialog() == true)
            {
                if (SrcImages == null)
                    SrcImages = new ObservableCollection<HObject>();
                if (SrcImageFileNames == null)
                    SrcImageFileNames = new ObservableCollection<string>();

                // 每次重新加载时清空旧数据
                SrcImages.Clear();
                SrcImageFileNames.Clear();
                DrawingObjectList?.Clear();
                HWindow?.ClearWindow();

                StringBuilder sb = new StringBuilder();

                foreach (string filePath in dialog.FileNames)
                {
                    HImage img = new HImage();
                    try
                    {
                        img.ReadImage(filePath);
                        SrcImages.Add(img);
                        SrcImageFileNames.Add(System.IO.Path.GetFileNameWithoutExtension(filePath));
                        sb.Append(filePath);
                        sb.Append(';');
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"加载图像 \"{System.IO.Path.GetFileName(filePath)}\" 失败：{ex.Message}",
                            "加载错误",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        img.Dispose(); // 释放已创建但加载失败的图像对象
                    }
                }

                CurrentImage = null;
                LoadedImgsArray = sb.ToString().Remove(sb.ToString().Length - 1);
            }
        }

        /// <summary>
        /// 批量加载的图像名称
        /// </summary>
        private string? loadedImgsArray;
        /// <summary>
        /// 批量加载的图像名称
        /// </summary>
        public string? LoadedImgsArray
        {
            get { return loadedImgsArray; }
            set { loadedImgsArray = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 加载目录中的所有图像
        /// </summary>
        private void LoadDirectory()
        {
            var folderDialog = new OpenFolderDialog();
            folderDialog.Title = "请选择包含需要进行图像处理图像的文件夹";

            if (folderDialog.ShowDialog() == true)
            {
                if (SrcImages == null)
                    SrcImages = new ObservableCollection<HObject>();
                if (SrcImageFileNames == null)
                    SrcImageFileNames = new ObservableCollection<string>();

                // 每次重新加载时清空旧数据
                SrcImages.Clear();
                SrcImageFileNames.Clear();
                DrawingObjectList?.Clear();
                HWindow?.ClearWindow();

                string folderPath = folderDialog.FolderName;
                string[] files = Directory.GetFiles(folderPath, "*.*");
                Array.Sort(files);

                foreach (string filePath in files)
                {
                    string ext = Path.GetExtension(filePath).ToLower();
                    if (ext != ".bmp" && ext != ".jpg" && ext != ".jpeg" &&
                        ext != ".png" && ext != ".tif" && ext != ".tiff")
                        continue;

                    HImage img = new HImage();
                    try
                    {
                        img.ReadImage(filePath);
                        SrcImages.Add(img);
                        SrcImageFileNames.Add(System.IO.Path.GetFileNameWithoutExtension(filePath));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"加载图像 \"{Path.GetFileName(filePath)}\" 失败：{ex.Message}",
                            "加载错误",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        img.Dispose();
                    }
                }

                CurrentImage = null;
                ImgSrcDirectoryName = folderDialog.FolderName;
            }
        }

        /// <summary>
        /// 图像源所在的目录
        /// </summary>
        private string? imgSrcDirectoryName;
        /// <summary>
        /// 图像源所在的目录
        /// </summary>
        public string? ImgSrcDirectoryName
        {
            get { return imgSrcDirectoryName; }
            set { imgSrcDirectoryName = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 加载图像保存目录路径
        /// </summary>
        private void LoadImgSaveDirectory()
        {
            var folderDialog = new OpenFolderDialog();
            folderDialog.Title = "请选择图像保存目录";

            if (folderDialog.ShowDialog() == true)
            {
                ImageSaveDirectory = folderDialog.FolderName;
            }
        }
        #endregion

        /// <summary>
        /// HalconWindow
        /// </summary>
        private HWindow? hWindow;
        /// <summary>
        /// HalconWindow
        /// </summary>
        public HWindow? HWindow
        {
            get { return hWindow; }
            set { hWindow = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 加载的原图像集合
        /// </summary>
        private ObservableCollection<HObject>? srcImages;
        /// <summary>
        /// 加载的原图像集合
        /// </summary>
        public ObservableCollection<HObject>? SrcImages
        {
            get { return srcImages; }
            set { srcImages = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 加载的原图像文件名（不含后缀名）
        /// </summary>
        private ObservableCollection<string>? srcImageFileNames;
        /// <summary>
        /// 加载的原图像文件名（不含后缀名）
        /// </summary>
        public ObservableCollection<string>? SrcImageFileNames
        {
            get { return srcImageFileNames; }
            set { srcImageFileNames = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 当前要在HalconWindow上展示的图像
        /// </summary>
        private HObject? currentImage;
        /// <summary>
        /// 当前要在HalconWindow上展示的图像
        /// </summary>
        public HObject? CurrentImage
        {
            get { return currentImage; }
            set
            {
                currentImage = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 绘制图形集合，存放HalconWindow中绘制的形状
        /// </summary>
        private ObservableCollection<DrawingObjectInfo>? drawingObjectList;
        /// <summary>
        /// 绘制图形集合，存放HalconWindow中绘制的形状
        /// </summary>
        public ObservableCollection<DrawingObjectInfo>? DrawingObjectList
        {
            get { return drawingObjectList; }
            set
            {
                drawingObjectList = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 图像保存目录路径
        /// </summary>
        private string? imgSaveDirectory;
        /// <summary>
        /// 图像保存目录路径
        /// </summary>
        public string? ImageSaveDirectory
        {
            get { return imgSaveDirectory; }
            set { imgSaveDirectory = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 在TextBlock中展示的消息
        /// </summary>
        private string? message;
        /// <summary>
        /// 在TextBlock中展示的消息
        /// </summary>
        public string? Message
        {
            get { return message; }
            set { message = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// ROI区域
        /// </summary>
        private ROIParams? roi = null;
        /// <summary>
        /// ROI区域
        /// </summary>
        public ROIParams? ROI
        {
            get { return roi; }
            set { roi = value; RaisePropertyChanged(); }
        }

        #region 按钮命令
        /// <summary>
        /// 加载单张图像按钮命令
        /// </summary>
        public DelegateCommand LoadSingleImageCommand { get; }

        /// <summary>
        /// 批量加载图像按钮命令
        /// </summary>
        public DelegateCommand LoadImagesCommand { get; }

        /// <summary>
        /// 加载目录中所有图像按钮命令
        /// </summary>
        public DelegateCommand LoadDirectoryCommand { get; }

        /// <summary>
        /// 加载图像保存目录路径按钮命令
        /// </summary>
        public DelegateCommand LoadImgSaveDirectoryCommand { get; }

        /// <summary>
        /// 运行图像处理调试按钮命令
        /// </summary>
        public DelegateCommand RunDebuggingCommand { get; }

        /// <summary>
        /// 运行并保存按钮命令
        /// </summary>
        public DelegateCommand RunAndSaveCommand { get; }

        /// <summary>
        /// 设置ROI范围按钮命令
        /// </summary>
        public DelegateCommand SetROIAreaCommand { get; }

        /// <summary>
        /// 清除ROI区域按钮命令
        /// </summary>
        public DelegateCommand ClearROIAreaCommand { get; }

        /// <summary>
        /// 添加灰度化结点按钮命令
        /// </summary>
        public DelegateCommand GrayscaleCommand { get; }

        /// <summary>
        /// 添加直方图均衡化结点按钮命令
        /// </summary>
        public DelegateCommand HistogramEqualizationCommand { get; }

        /// <summary>
        /// 添加中值滤波结点按钮命令
        /// </summary>
        public DelegateCommand MedianFilterCommand { get; }

        /// <summary>
        /// 添加高斯滤波结点按钮命令
        /// </summary>
        public DelegateCommand GaussianFilterCommand { get; }

        /// <summary>
        /// 保存配方按钮命令
        /// </summary>
        public DelegateCommand RecipeSavingCommand { get; }

        /// <summary>
        /// 应用配方按钮命令
        /// </summary>
        public DelegateCommand RecipeApplyingCommand { get; }
        #endregion

        #region 图像处理服务
        /// <summary>
        /// 图像处理服务
        /// </summary>
        public ImageProcessService ImageProcessService { get; }
        #endregion
    }
}
