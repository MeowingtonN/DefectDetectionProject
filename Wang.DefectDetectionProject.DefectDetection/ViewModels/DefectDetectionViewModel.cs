using HalconDotNet;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using Wang.DefectDetectionProject.Core;
using Wang.DefectDetectionProject.Core.DefectDetection;
using Wang.DefectDetectionProject.Core.Extensions;
using Wang.DefectDetectionProject.Core.Models;
using Wang.DefectDetectionProject.Shared.Controls;
using System.IO;
using System.Text;
using Wang.DefectDetectionProject.Core.ExcelHelper;

namespace Wang.DefectDetectionProject.DefectDetection.ViewModels
{
    /// <summary>
    /// 缺陷检测ViewModel层
    /// </summary>
    public class DefectDetectionViewModel : NavigationViewModel
    {
        public DefectDetectionViewModel(DefectDetectionService defectDetectionService, ModelTrainService modelTrainService)
        {
            Images = new ObservableCollection<HObject>();
            DrawingObjectList = new ObservableCollection<DrawingObjectInfo>();

            DefectDetectionService = defectDetectionService;
            ModelTrainService = modelTrainService;

            LoadImagesCommand = new DelegateCommand(LoadImages);
            LoadDirectoryCommand = new DelegateCommand(LoadDirectory);
            RunCommand = new DelegateCommand(Run);
            LoadPreprocessParamDictCommand = new DelegateCommand(LoadPreprocessParamDict);
            LoadTrainedModelCommand = new DelegateCommand(LoadTrainedModel);
            ChangeImageCommand = new DelegateCommand<ImageListItem>(ChangeImage);
            ExportExcelCommand = new DelegateCommand(ExportExcel);
        }

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
            set 
            { 
                hWindow = value; 
                ModelTrainService.ExternalWindowHandle = hWindow;
                DefectDetectionService.ExternalWindowHandle = hWindow; 
                RaisePropertyChanged(); 
            }
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
        /// 待检测的源图像集合
        /// </summary>
        private ObservableCollection<HObject>? images;
        /// <summary>
        /// 待检测的源图像集合
        /// </summary>
        public ObservableCollection<HObject>? Images
        {
            get { return images; }
            set
            {
                images = value;
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
        /// 图像预处理参数字典文件路径
        /// </summary>
        private string? preprocessParamDictFileName;
        /// <summary>
        /// 图像预处理参数字典文件路径
        /// </summary>
        public string? PreprocessParamDictFileName 
        {
            get { return preprocessParamDictFileName; }
            set { preprocessParamDictFileName = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 训练好的模型文件路径
        /// </summary>
        private string? trainedParamDictFileName;
        /// <summary>
        /// 训练好的模型文件路径
        /// </summary>
        public string? TrainedModelFileName 
        {
            get { return trainedParamDictFileName; }
            set { trainedParamDictFileName = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 更改选中图像Item的回调方法
        /// </summary>
        /// <param name="item"></param>
        private void ChangeImage(ImageListItem item)
        {
            if(item != null && item.DefectDetectionResults != null && item.DefectDetectionResults.Count > 0)
            {
                CurrentImage = item.Image;
                dev_display_ok_ng(item.DefectDetectionResults[0].DetectionResult == "NG", HWindow);

                HTuple width, height;
                HOperatorSet.GetWindowExtents(HWindow, out _, out _, out width, out height);    // 获取窗口尺寸
                int margin_right = 100;            // 距离Halcon窗口右边缘的像素
                int margin_bottom = 50;            // 距离Halcon窗口下边缘的像素
                int lineHeight = 35;        // 行高（根据字体大小调整）

                for (int i = 0; i < item.DefectDetectionResults.Count; i++)
                {
                    if (item.DefectDetectionResults[i].DetectionResult == "NG")
                    {
                        // 计算文本宽度（以便右对齐）
                        HTuple textWidth, textHeight;
                        HOperatorSet.GetStringExtents(HWindow, item.DefectDetectionResults[i].DefectName, 
                                                      out _, out _, out textWidth, out textHeight);

                        double row = height.D - margin_bottom - i * lineHeight;                 // 行坐标（从底部往上）
                        double col = width.D - margin_right - textWidth.D;                     // 列坐标（右对齐）

                        HOperatorSet.DispText(HWindow, item.DefectDetectionResults[i].DefectName + ": " + item.DefectDetectionResults[i].Count?.ToString(), 
                            "window", row,col, item.DefectDetectionResults[i].MarkingColor, new HTuple(), new HTuple());
                    }
                }
            }
            else if(item != null)
            {
                CurrentImage = item.Image;
            }
        }

        private void LoadTrainedModel()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "请选择训练好的模型的文件路径";
            dialog.Filter = "深度学习模型文件|*.hdl";
            var dialogResult = (bool)dialog.ShowDialog()!;
            if (dialogResult)
            {
                TrainedModelFileName = dialog.FileName;
            }
        }

        private void LoadPreprocessParamDict()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "请选择图像预处理参数字典文件路径";
            dialog.Filter = "字典文件|*.hdict";
            var dialogResult = (bool)dialog.ShowDialog()!;
            if (dialogResult)
            {
                PreprocessParamDictFileName = dialog.FileName;
            }
        }

        private async void Run()
        {
            if (Images == null || Images.Count <= 0 || PreprocessParamDictFileName == null || PreprocessParamDictFileName.Length <= 0 
                || TrainedModelFileName == null || TrainedModelFileName.Length <= 0) return;
            try
            {
                await DefectDetectionService.Run(Images, PreprocessParamDictFileName, TrainedModelFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                            $"缺陷检测过程中抛出异常：{ex.Message} 注意，此窗口未关闭时界面无法更新。",
                            "检测中发生错误",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 批量加载图片
        /// </summary>
        private void LoadImages()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "请选择要进行缺陷检测的图像";
            dialog.Multiselect = true;  // 允许多选
            dialog.Filter = "图像文件|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|所有文件|*.*";

            if (dialog.ShowDialog() == true)
            {
                if (Images == null) 
                    Images = new ObservableCollection<HObject>();
                if (DefectDetectionService.ImageListItems == null) 
                    DefectDetectionService.ImageListItems = new ObservableCollection<ImageListItem>();

                // 每次重新加载时清空旧数据
                Images.Clear(); 
                DefectDetectionService.ImageListItems.Clear();
                DrawingObjectList?.Clear();

                StringBuilder stringBuilder = new StringBuilder();

                foreach (string filePath in dialog.FileNames)
                {
                    HImage img = new HImage();
                    try
                    {
                        img.ReadImage(filePath);
                        Images.Add(img);
                        DefectDetectionService.ImageListItems.Add(new ImageListItem()
                        {
                            Title = Path.GetFileName(filePath),
                            ImgSource = img.ToBitmapSource(),
                            Image = img
                        });
                        stringBuilder.Append(filePath);
                        stringBuilder.Append(';');
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

                // 加载完成后自动显示第一张
                if (Images.Count > 0)
                    CurrentImage = Images[0];
                LoadedImgsArray = stringBuilder.ToString().Remove(stringBuilder.ToString().Length - 1);
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
            folderDialog.Title = "请选择包含需要进行缺陷检测图像的文件夹";

            if (folderDialog.ShowDialog() == true)
            {
                if (Images == null)
                    Images = new ObservableCollection<HObject>();
                if (DefectDetectionService.ImageListItems == null)
                    DefectDetectionService.ImageListItems = new ObservableCollection<ImageListItem>();

                // 每次重新加载时清空旧数据
                Images.Clear();
                DefectDetectionService.ImageListItems.Clear();
                DrawingObjectList?.Clear();

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
                        Images.Add(img);
                        DefectDetectionService.ImageListItems.Add(new ImageListItem()
                        {
                            Title = Path.GetFileName(filePath),
                            ImgSource = img.ToBitmapSource(),
                            Image = img
                        });
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

                if (Images.Count > 0)
                    CurrentImage = Images[0];
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
        /// 将检测结果导出成Excel表格
        /// </summary>
        private void ExportExcel()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "导出Excel";
            dialog.Filter = "Excel文件|*.xlsx|所有文件|*.*";
            dialog.DefaultExt = ".xlsx";
            dialog.FileName = "DefectDetectionResults.xlsx";
            var dialogResult = (bool)dialog.ShowDialog()!;
            if (dialogResult && DefectDetectionService.ImageListItems != null)
            {
                DefectDetectionResultExcelHelper.ExportDefectDetectionResults(DefectDetectionService.ImageListItems, dialog.FileName);
            }
        }

        #region 按钮命令属性
        /// <summary>
        /// 执行按钮命令
        /// </summary>
        public DelegateCommand RunCommand { get; }

        /// <summary>
        /// 加载图像按钮命令
        /// </summary>
        public DelegateCommand LoadImagesCommand { get; }

        /// <summary>
        /// 加载目录中的所有图像按钮命令
        /// </summary>
        public DelegateCommand LoadDirectoryCommand { get; }

        /// <summary>
        /// 加载图像预处理参数字典文件按钮命令
        /// </summary>
        public DelegateCommand LoadPreprocessParamDictCommand {  get; }

        /// <summary>
        /// 加载训练好的模型文件按钮命令
        /// </summary>
        public DelegateCommand LoadTrainedModelCommand {  get; }

        /// <summary>
        /// 更改展示的图像按钮命令
        /// </summary>
        public DelegateCommand<ImageListItem> ChangeImageCommand { get; }

        /// <summary>
        /// 导出检测结果Excel表格按钮命令
        /// </summary>
        public DelegateCommand ExportExcelCommand { get; }
        #endregion

        #region 服务
        /// <summary>
        /// 缺陷检测服务
        /// </summary>
        public DefectDetectionService DefectDetectionService { get; }

        /// <summary>
        /// 模型训练服务
        /// </summary>
        public ModelTrainService ModelTrainService { get; }
        #endregion

        #region 辅助方法
        private void dev_display_ok_ng(bool isNG, HTuple hv_WindowHandleImage)
        {
            // Local iconic variables 

            // Local control variables 

            HTuple hv_Text = new HTuple(), hv_BoxColor = new HTuple();
            // Initialize local and output iconic variables 
            try
            {
                //This procedures displays OK if no defects are segmented and NG otherwise.
                //
                //The first entry of Area corresponds to class 'good'.
                if (isNG)
                {
                    hv_Text.Dispose();
                    hv_Text = "NG";
                    hv_BoxColor.Dispose();
                    hv_BoxColor = "red";
                }
                else
                {
                    hv_Text.Dispose();
                    hv_Text = "OK";
                    hv_BoxColor.Dispose();
                    hv_BoxColor = "green";
                }
                set_display_font(hv_WindowHandleImage, 24, "mono", "true", "false");
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    HOperatorSet.DispText(hv_WindowHandleImage, hv_Text, "window", "top",
                        "left", "black", (new HTuple("box_color")).TupleConcat("shadow"), hv_BoxColor.TupleConcat("false"));
                }

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                hv_Text.Dispose();
                hv_BoxColor.Dispose();

                throw new HalconException(HDevExpDefaultException.Message);
            }
        }

        // Chapter: Graphics / Text
        // Short Description: Set font independent of OS 
        private void set_display_font(HTuple hv_WindowHandle, HTuple hv_Size, HTuple hv_Font,
            HTuple hv_Bold, HTuple hv_Slant)
        {
            // Local iconic variables 

            // Local control variables 

            HTuple hv_OS = new HTuple(), hv_Fonts = new HTuple();
            HTuple hv_Style = new HTuple(), hv_Exception = new HTuple();
            HTuple hv_AvailableFonts = new HTuple(), hv_Fdx = new HTuple();
            HTuple hv_Indices = new HTuple();
            HTuple hv_Font_COPY_INP_TMP = new HTuple(hv_Font);
            HTuple hv_Size_COPY_INP_TMP = new HTuple(hv_Size);

            // Initialize local and output iconic variables 
            try
            {
                //This procedure sets the text font of the current window with
                //the specified attributes.
                //
                //Input parameters:
                //WindowHandle: The graphics window for which the font will be set
                //Size: The font size. If Size=-1, the default of 16 is used.
                //Bold: If set to 'true', a bold font is used
                //Slant: If set to 'true', a slanted font is used
                //
                hv_OS.Dispose();
                HOperatorSet.GetSystem("operating_system", out hv_OS);
                if ((int)((new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                    new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(-1)))) != 0)
                {
                    hv_Size_COPY_INP_TMP.Dispose();
                    hv_Size_COPY_INP_TMP = 16;
                }
                if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
                {
                    //Restore previous behavior
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Size = ((1.13677 * hv_Size_COPY_INP_TMP)).TupleInt()
                                ;
                            hv_Size_COPY_INP_TMP.Dispose();
                            hv_Size_COPY_INP_TMP = ExpTmpLocalVar_Size;
                        }
                    }
                }
                else
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Size = hv_Size_COPY_INP_TMP.TupleInt()
                                ;
                            hv_Size_COPY_INP_TMP.Dispose();
                            hv_Size_COPY_INP_TMP = ExpTmpLocalVar_Size;
                        }
                    }
                }
                if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("Courier"))) != 0)
                {
                    hv_Fonts.Dispose();
                    hv_Fonts = new HTuple();
                    hv_Fonts[0] = "Courier";
                    hv_Fonts[1] = "Courier 10 Pitch";
                    hv_Fonts[2] = "Courier New";
                    hv_Fonts[3] = "CourierNew";
                    hv_Fonts[4] = "Liberation Mono";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("mono"))) != 0)
                {
                    hv_Fonts.Dispose();
                    hv_Fonts = new HTuple();
                    hv_Fonts[0] = "Consolas";
                    hv_Fonts[1] = "Menlo";
                    hv_Fonts[2] = "Courier";
                    hv_Fonts[3] = "Courier 10 Pitch";
                    hv_Fonts[4] = "FreeMono";
                    hv_Fonts[5] = "Liberation Mono";
                    hv_Fonts[6] = "DejaVu Sans Mono";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("sans"))) != 0)
                {
                    hv_Fonts.Dispose();
                    hv_Fonts = new HTuple();
                    hv_Fonts[0] = "Luxi Sans";
                    hv_Fonts[1] = "DejaVu Sans";
                    hv_Fonts[2] = "FreeSans";
                    hv_Fonts[3] = "Arial";
                    hv_Fonts[4] = "Liberation Sans";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("serif"))) != 0)
                {
                    hv_Fonts.Dispose();
                    hv_Fonts = new HTuple();
                    hv_Fonts[0] = "Times New Roman";
                    hv_Fonts[1] = "Luxi Serif";
                    hv_Fonts[2] = "DejaVu Serif";
                    hv_Fonts[3] = "FreeSerif";
                    hv_Fonts[4] = "Utopia";
                    hv_Fonts[5] = "Liberation Serif";
                }
                else
                {
                    hv_Fonts.Dispose();
                    hv_Fonts = new HTuple(hv_Font_COPY_INP_TMP);
                }
                hv_Style.Dispose();
                hv_Style = "";
                if ((int)(new HTuple(hv_Bold.TupleEqual("true"))) != 0)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Style = hv_Style + "Bold";
                            hv_Style.Dispose();
                            hv_Style = ExpTmpLocalVar_Style;
                        }
                    }
                }
                else if ((int)(new HTuple(hv_Bold.TupleNotEqual("false"))) != 0)
                {
                    hv_Exception.Dispose();
                    hv_Exception = "Wrong value of control parameter Bold";
                    throw new HalconException(hv_Exception);
                }
                if ((int)(new HTuple(hv_Slant.TupleEqual("true"))) != 0)
                {
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        {
                            HTuple
                              ExpTmpLocalVar_Style = hv_Style + "Italic";
                            hv_Style.Dispose();
                            hv_Style = ExpTmpLocalVar_Style;
                        }
                    }
                }
                else if ((int)(new HTuple(hv_Slant.TupleNotEqual("false"))) != 0)
                {
                    hv_Exception.Dispose();
                    hv_Exception = "Wrong value of control parameter Slant";
                    throw new HalconException(hv_Exception);
                }
                if ((int)(new HTuple(hv_Style.TupleEqual(""))) != 0)
                {
                    hv_Style.Dispose();
                    hv_Style = "Normal";
                }
                hv_AvailableFonts.Dispose();
                HOperatorSet.QueryFont(hv_WindowHandle, out hv_AvailableFonts);
                hv_Font_COPY_INP_TMP.Dispose();
                hv_Font_COPY_INP_TMP = "";
                for (hv_Fdx = 0; (int)hv_Fdx <= (int)((new HTuple(hv_Fonts.TupleLength())) - 1); hv_Fdx = (int)hv_Fdx + 1)
                {
                    hv_Indices.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_Indices = hv_AvailableFonts.TupleFind(
                            hv_Fonts.TupleSelect(hv_Fdx));
                    }
                    if ((int)(new HTuple((new HTuple(hv_Indices.TupleLength())).TupleGreater(
                        0))) != 0)
                    {
                        if ((int)(new HTuple(((hv_Indices.TupleSelect(0))).TupleGreaterEqual(0))) != 0)
                        {
                            hv_Font_COPY_INP_TMP.Dispose();
                            using (HDevDisposeHelper dh = new HDevDisposeHelper())
                            {
                                hv_Font_COPY_INP_TMP = hv_Fonts.TupleSelect(
                                    hv_Fdx);
                            }
                            break;
                        }
                    }
                }
                if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual(""))) != 0)
                {
                    throw new HalconException("Wrong value of control parameter Font");
                }
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_Font = (((hv_Font_COPY_INP_TMP + "-") + hv_Style) + "-") + hv_Size_COPY_INP_TMP;
                        hv_Font_COPY_INP_TMP.Dispose();
                        hv_Font_COPY_INP_TMP = ExpTmpLocalVar_Font;
                    }
                }
                HOperatorSet.SetFont(hv_WindowHandle, hv_Font_COPY_INP_TMP);

                hv_Font_COPY_INP_TMP.Dispose();
                hv_Size_COPY_INP_TMP.Dispose();
                hv_OS.Dispose();
                hv_Fonts.Dispose();
                hv_Style.Dispose();
                hv_Exception.Dispose();
                hv_AvailableFonts.Dispose();
                hv_Fdx.Dispose();
                hv_Indices.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                hv_Font_COPY_INP_TMP.Dispose();
                hv_Size_COPY_INP_TMP.Dispose();
                hv_OS.Dispose();
                hv_Fonts.Dispose();
                hv_Style.Dispose();
                hv_Exception.Dispose();
                hv_AvailableFonts.Dispose();
                hv_Fdx.Dispose();
                hv_Indices.Dispose();

                throw new HalconException(HDevExpDefaultException.Message);
            }
        }
        #endregion
    }
}
