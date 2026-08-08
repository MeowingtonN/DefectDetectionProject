using HalconDotNet;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Wang.DefectDetectionProject.Core.Extensions;
using Wang.DefectDetectionProject.Core.ImageProcess.Enums;
using Wang.DefectDetectionProject.Core.ImageProcess.RecipeService.RecipeModels;
using Wang.DefectDetectionProject.Core.ROI;
using Wang.DefectDetectionProject.Core.Tools;
using Wang.DefectDetectionProject.ImageProcess.ViewModels;

namespace Wang.DefectDetectionProject.ImageProcess.Services
{
    /// <summary>
    /// 图像处理服务
    /// </summary>
    public class ImageProcessService : BindableBase
    {
        /// <summary>
        /// Nodify Editor
        /// </summary>
        private EditorViewModel editorViewModel = new EditorViewModel();
        /// <summary>
        /// Nodify Editor
        /// </summary>
        public EditorViewModel EditorViewModel
        {
            get { return editorViewModel; }
            set { editorViewModel = value; RaisePropertyChanged(); }
        }

        #region 图像处理调试运行、执行并保存
        /// <summary>
        /// 图像处理调试运行
        /// </summary>
        public void RunDebugging(HWindow? hWindow, ObservableCollection<HObject>? srcImages, ref HObject? currentImage, ROIParams? roi)
        {
            NodeViewModel? currentNode = EditorViewModel.StartNode;

            if (currentNode == null || srcImages == null || srcImages.Count <= 0) return;

            hWindow?.ClearWindow();
            currentImage = srcImages[0].ReduceDomain(roi);
            HOperatorSet.CropDomain(currentImage, out HObject tempImg);  // 将图像有定义部分裁剪出来成像
            currentImage = tempImg;

            while (currentNode != null)
            {
                // 处理当前节点
                currentImage = ImgProcess(currentNode, currentImage!);

                // 寻找下一个节点（单一路径）
                var nextConnector = currentNode.OutputConnectors.FirstOrDefault();
                if (nextConnector == null) break;

                var connection = EditorViewModel.Connections.FirstOrDefault(c => c.Source == nextConnector);
                if (connection == null) break;

                currentNode = connection.Target.OwnerNode!;
            }
        }

        /// <summary>
        /// 执行图像处理并保存
        /// </summary>
        /// <param name="hWindow"></param>
        /// <param name="srcImages"></param>
        /// <param name="srcImageFileNames"></param>
        /// <param name="roi"></param>
        /// <param name="imageSaveDirectory"></param>
        /// <param name="imageFormat"></param>
        /// <returns></returns>
        public async Task RunAndSave(HWindow? hWindow, ObservableCollection<HObject>? srcImages, ObservableCollection<string>? srcImageFileNames,
                               ROIParams? roi, string? imageSaveDirectory, string imageFormat = "png")
        {
            NodeViewModel? currentNode = EditorViewModel.StartNode;

            if (currentNode == null || srcImages == null || srcImages.Count <= 0 || imageSaveDirectory == null ||
                srcImageFileNames == null || srcImageFileNames.Count != srcImages.Count)
                return;

            // 清空窗口
            hWindow?.ClearWindow();

            await Task.Run(() =>
            {
                // 确保保存目录存在
                if (!Directory.Exists(imageSaveDirectory))
                    Directory.CreateDirectory(imageSaveDirectory);

                for (int i = 0; i < srcImages.Count; i++)
                {
                    currentNode = EditorViewModel.StartNode;

                    HObject? currentImage = srcImages[i].ReduceDomain(roi);
                    HOperatorSet.CropDomain(currentImage, out HObject tempImg);  // 将图像有定义部分裁剪出来成像
                    currentImage = tempImg;

                    while (currentNode != null)
                    {
                        // 处理当前节点
                        currentImage = ImgProcess(currentNode, currentImage!);

                        // 寻找下一个节点（单一路径）
                        var nextConnector = currentNode.OutputConnectors.FirstOrDefault();
                        if (nextConnector == null) break;

                        var connection = EditorViewModel.Connections.FirstOrDefault(c => c.Source == nextConnector);
                        if (connection == null) break;

                        currentNode = connection.Target.OwnerNode!;
                    }

                    // 保存处理结果
                    string baseName = srcImageFileNames[i];
                    string fileName = $"{baseName}.{imageFormat}";  // 使用原图名保存
                    string filePath = Path.Combine(imageSaveDirectory, fileName);
                    HOperatorSet.WriteImage(currentImage, imageFormat, 0, filePath);
                }
            });
        }

        /// <summary>
        /// 依据结点进行对应图像处理，并返回处理后的图像
        /// </summary>
        /// <param name="node"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        private HObject? ImgProcess(NodeViewModel node, HObject image)
        {
            HObject? outputImage = null;
            switch (node.ImageProcessOperator)
            {
                case ImgProcessOperator.ImgSrc:
                    outputImage = image;
                    break;
                case ImgProcessOperator.Grayscale:
                    HOperatorSet.Rgb1ToGray(image, out outputImage);
                    break;
                case ImgProcessOperator.HistogramEqualization:
                    HOperatorSet.EquHistoImage(image, out outputImage);
                    break;
                case ImgProcessOperator.MedianFilter:
                    string? maskType = node.ComboOptions.FirstOrDefault(o => o.Label == "MaskType")!.SelectedItem;
                    string? radius = node.ComboOptions.FirstOrDefault(o => o.Label == "Radius")!.SelectedItem;
                    string? margin = node.ComboOptions.FirstOrDefault(o => o.Label == "Margin")!.SelectedItem;
                    if (maskType == null || radius == null || margin == null) break;
                    HOperatorSet.MedianImage(image, out outputImage, maskType, Convert.ToInt32(radius), margin);
                    break;
                case ImgProcessOperator.GaussianFilter:
                    string? filterSize = node.ComboOptions.FirstOrDefault(o => o.Label == "FilterSize")!.SelectedItem;
                    if (filterSize == null) break;
                    HOperatorSet.GaussFilter(image, out outputImage, Convert.ToInt32(filterSize));
                    break;
            }
            return outputImage;
        }
        #endregion

        #region 添加Nodify结点
        /// <summary>
        /// 添加灰度化结点
        /// </summary>
        public void AddGrayscaleNode()
        {
            if (EditorViewModel == null) return;

            var grayscaleNode = new NodeViewModel
            {
                TitleKey = "GrayscaleNode",
                Location = new Point(200, 100),
                CanDelete = true,
                ImageProcessOperator = ImgProcessOperator.Grayscale
            };
            grayscaleNode.Title = LanguageHelper.TranslationKeyValues![grayscaleNode.TitleKey];

            var grayscaleNodeInput = new ConnectorViewModel
            {
                TitleKey = "Input",
                Anchor = new Point(0, 30),
                IsInput = true,
                OwnerNode = grayscaleNode
            };
            grayscaleNodeInput.Title = LanguageHelper.TranslationKeyValues![grayscaleNodeInput.TitleKey];
            grayscaleNode.InputConnectors.Add(grayscaleNodeInput);

            var grayscaleNodeOutput = new ConnectorViewModel
            {
                TitleKey = "Output",
                Anchor = new Point(120, 30),
                IsInput = false,
                OwnerNode = grayscaleNode
            };
            grayscaleNodeOutput.Title = LanguageHelper.TranslationKeyValues![grayscaleNodeOutput.TitleKey];
            grayscaleNode.OutputConnectors.Add(grayscaleNodeOutput);

            EditorViewModel.Nodes.Add(grayscaleNode);
        }

        /// <summary>
        /// 添加直方图均衡化结点
        /// </summary>
        public void AddHistogramEqualizationNode()
        {
            if (EditorViewModel == null) return;

            var histogramEqualizationNode = new NodeViewModel
            {
                TitleKey = "HistogramEqualizationNode",
                Location = new Point(200, 100),
                CanDelete = true,
                ImageProcessOperator = ImgProcessOperator.HistogramEqualization
            };
            histogramEqualizationNode.Title = LanguageHelper.TranslationKeyValues![histogramEqualizationNode.TitleKey];

            var histogramEqualizationNodeInput = new ConnectorViewModel
            {
                TitleKey = "Input",
                Anchor = new Point(0, 30),
                IsInput = true,
                OwnerNode = histogramEqualizationNode
            };
            histogramEqualizationNodeInput.Title = LanguageHelper.TranslationKeyValues![histogramEqualizationNodeInput.TitleKey];
            histogramEqualizationNode.InputConnectors.Add(histogramEqualizationNodeInput);

            var histogramEqualizationNodeOutput = new ConnectorViewModel
            {
                TitleKey = "Output",
                Anchor = new Point(120, 30),
                IsInput = false,
                OwnerNode = histogramEqualizationNode
            };
            histogramEqualizationNodeOutput.Title = LanguageHelper.TranslationKeyValues![histogramEqualizationNodeOutput.TitleKey];
            histogramEqualizationNode.OutputConnectors.Add(histogramEqualizationNodeOutput);

            EditorViewModel.Nodes.Add(histogramEqualizationNode);
        }

        /// <summary>
        /// 添加中值滤波结点
        /// </summary>
        public void AddMedianFilterNode()
        {
            if (EditorViewModel == null) return;

            var medianFilterNode = new NodeViewModel
            {
                TitleKey = "MedianFilterNode",
                Location = new Point(200, 100),
                CanDelete = true,
                ImageProcessOperator = ImgProcessOperator.MedianFilter
            };
            medianFilterNode.Title = LanguageHelper.TranslationKeyValues![medianFilterNode.TitleKey];

            medianFilterNode.ComboOptions.Add(new ComboOptionViewModel()
            {
                Label = "MaskType",
                Items = new ObservableCollection<string>() { "circle", "square" },
            });
            medianFilterNode.ComboOptions[0].SelectedItem = medianFilterNode.ComboOptions[0].Items?.FirstOrDefault();
            medianFilterNode.ComboOptions.Add(new ComboOptionViewModel()
            {
                Label = "Radius",
                Items = new ObservableCollection<string>()
                {
                    "1", "2", "3", "4", "5", "6", "7", "8", "9", "11", "15", "19", "25", "31", "39", "47", "59"
                }
            });
            medianFilterNode.ComboOptions[1].SelectedItem = medianFilterNode.ComboOptions[1].Items?.FirstOrDefault();
            medianFilterNode.ComboOptions.Add(new ComboOptionViewModel()
            {
                Label = "Margin",
                Items = new ObservableCollection<string>()
                {
                    "mirrored", "cyclic", "continued",
                    "0", "30", "60", "90", "120", "150", "180", "210", "240", "255"
                }
            });
            medianFilterNode.ComboOptions[2].SelectedItem = medianFilterNode.ComboOptions[2].Items?.FirstOrDefault();

            var medianFilterNodeInput = new ConnectorViewModel
            {
                TitleKey = "Input",
                Anchor = new Point(0, 30),
                IsInput = true,
                OwnerNode = medianFilterNode
            };
            medianFilterNodeInput.Title = LanguageHelper.TranslationKeyValues![medianFilterNodeInput.TitleKey];
            medianFilterNode.InputConnectors.Add(medianFilterNodeInput);

            var medianFilterNodeOutput = new ConnectorViewModel
            {
                TitleKey = "Output",
                Anchor = new Point(120, 30),
                IsInput = false,
                OwnerNode = medianFilterNode
            };
            medianFilterNodeOutput.Title = LanguageHelper.TranslationKeyValues![medianFilterNodeOutput.TitleKey];
            medianFilterNode.OutputConnectors.Add(medianFilterNodeOutput);

            EditorViewModel.Nodes.Add(medianFilterNode);
        }

        /// <summary>
        /// 添加高斯滤波结点
        /// </summary>
        public void AddGaussianFilterNode()
        {
            if (EditorViewModel == null) return;

            var gaussianFilterNode = new NodeViewModel
            {
                TitleKey = "GaussianFilterNode",
                Location = new Point(200, 100),
                CanDelete = true,
                ImageProcessOperator = ImgProcessOperator.GaussianFilter
            };
            gaussianFilterNode.Title = LanguageHelper.TranslationKeyValues![gaussianFilterNode.TitleKey];

            gaussianFilterNode.ComboOptions.Add(new ComboOptionViewModel()
            {
                Label = "FilterSize",
                Items = new ObservableCollection<string>() { "3", "5", "7", "9", "11" }
            });
            gaussianFilterNode.ComboOptions[0].SelectedItem = gaussianFilterNode.ComboOptions[0].Items?.FirstOrDefault();

            var gaussianFilterNodeInput = new ConnectorViewModel
            {
                TitleKey = "Input",
                Anchor = new Point(0, 30),
                IsInput = true,
                OwnerNode = gaussianFilterNode
            };
            gaussianFilterNodeInput.Title = LanguageHelper.TranslationKeyValues![gaussianFilterNodeInput.TitleKey];
            gaussianFilterNode.InputConnectors.Add(gaussianFilterNodeInput);

            var gaussianFilterNodeOutput = new ConnectorViewModel
            {
                TitleKey = "Output",
                Anchor = new Point(120, 30),
                IsInput = false,
                OwnerNode = gaussianFilterNode
            };
            gaussianFilterNodeOutput.Title = LanguageHelper.TranslationKeyValues![gaussianFilterNodeOutput.TitleKey];
            gaussianFilterNode.OutputConnectors.Add(gaussianFilterNodeOutput);

            EditorViewModel.Nodes.Add(gaussianFilterNode);
        }
        #endregion

        #region 配方应用辅助方法
        /// <summary>
        /// 应用配方中的ROI，同时将配方应用到当前 Nodify 编辑器
        /// </summary>
        /// <param name="recipe">要应用的配方</param>
        /// <param name="roi">ROI区域</param>
        /// <param name="editorViewModel">编辑器视图模型实例</param>
        public static void ApplyRecipeToEditor(ImageProcessRecipe recipe, ref ROIParams? roi, EditorViewModel editorViewModel)
        {
            if (recipe == null || editorViewModel == null) return;

            // 应用ROI范围
            roi = recipe.ROI;

            // 1. 清空现有内容
            editorViewModel.Nodes?.Clear();
            editorViewModel.Connections?.Clear();
            editorViewModel.StartNode = null;

            // 2. 重建节点链
            List<NodeViewModel> newNodes = new List<NodeViewModel>();

            // 在Nodify Editor中添加不可删除的“图像源”结点，并将其设置为遍历的开始结点
            var srcImgNode = new NodeViewModel
            {
                TitleKey = "ImgSrc",
                Location = new Point(70, 150),
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
            editorViewModel.Nodes?.Add(srcImgNode);
            editorViewModel.StartNode = srcImgNode;

            NodeViewModel? previousNode = srcImgNode;

            double startX = 70;
            double startY = 150;
            double xOffset = 200; // 节点之间的水平间隔

            // 跳过配方步骤中的图像源部分
            for (int i = 1; i < recipe.Steps!.Count; i++)
            {
                var step = recipe.Steps[i];
                var node = CreateNodeFromStep(step, startX + i * xOffset, startY);
                newNodes.Add(node);

                // 连接前一个节点
                if (previousNode != null)
                {
                    var srcConnector = previousNode.OutputConnectors.FirstOrDefault();
                    var tgtConnector = node.InputConnectors.FirstOrDefault();
                    if (srcConnector != null && tgtConnector != null)
                    {
                        var connection = new ConnectionViewModel
                        {
                            Source = srcConnector,
                            Target = tgtConnector
                        };
                        editorViewModel.Connections?.Add(connection);
                    }
                }

                previousNode = node;
            }

            // 3. 将所有节点加入编辑器
            foreach (var node in newNodes)
            {
                editorViewModel.Nodes?.Add(node);
            }
        }

        /// <summary>
        /// 根据配方步骤创建对应的节点，并设置参数
        /// </summary>
        private static NodeViewModel CreateNodeFromStep(RecipeStep step, double x, double y)
        {
            var node = new NodeViewModel
            {
                TitleKey = GetNodeTitleKey(step.Operator),
                Location = new Point(x, y),
                CanDelete = true,
                ImageProcessOperator = step.Operator
            };
            node.Title = LanguageHelper.TranslationKeyValues![node.TitleKey];

            // 添加输入连接器
            var inputConnector = new ConnectorViewModel
            {
                TitleKey = "Input",
                Anchor = new Point(0, 30),
                IsInput = true,
                OwnerNode = node
            };
            inputConnector.Title = LanguageHelper.TranslationKeyValues![inputConnector.TitleKey];
            node.InputConnectors.Add(inputConnector);

            // 添加输出连接器
            var outputConnector = new ConnectorViewModel
            {
                TitleKey = "Output",
                Anchor = new Point(120, 30),
                IsInput = false,
                OwnerNode = node
            };
            outputConnector.Title = LanguageHelper.TranslationKeyValues![outputConnector.TitleKey];
            node.OutputConnectors.Add(outputConnector);

            // 根据操作类型创建参数选项并设置选中值
            switch (step.Operator)
            {
                case ImgProcessOperator.MedianFilter:
                    AddMedianFilterOptions(node, step.Parameters!);
                    break;
                case ImgProcessOperator.GaussianFilter:
                    AddGaussianFilterOptions(node, step.Parameters!);
                    break;
                    // 其他操作无额外参数
            }

            return node;
        }

        /// <summary>
        /// 为中值滤波节点添加选项并根据配方设置选中项
        /// </summary>
        private static void AddMedianFilterOptions(NodeViewModel node, Dictionary<string, string> parameters)
        {
            // MaskType
            var maskTypeOption = new ComboOptionViewModel
            {
                Label = "MaskType",
                Items = new ObservableCollection<string> { "circle", "square" }
            };
            maskTypeOption.SelectedItem = parameters?.ContainsKey("MaskType") == true &&
                                          maskTypeOption.Items.Contains(parameters["MaskType"])
                                          ? parameters["MaskType"]
                                          : maskTypeOption.Items.FirstOrDefault();
            node.ComboOptions.Add(maskTypeOption);

            // Radius
            var radiusOption = new ComboOptionViewModel
            {
                Label = "Radius",
                Items = new ObservableCollection<string>
                {
                    "1", "2", "3", "4", "5", "6", "7", "8", "9", "11", "15", "19", "25", "31", "39", "47", "59"
                }
            };
            radiusOption.SelectedItem = parameters?.ContainsKey("Radius") == true &&
                                        radiusOption.Items.Contains(parameters["Radius"])
                                        ? parameters["Radius"]
                                        : radiusOption.Items.FirstOrDefault();
            node.ComboOptions.Add(radiusOption);

            // Margin
            var marginOption = new ComboOptionViewModel
            {
                Label = "Margin",
                Items = new ObservableCollection<string>
                {
                    "mirrored", "cyclic", "continued",
                    "0", "30", "60", "90", "120", "150", "180", "210", "240", "255"
                }
            };
            marginOption.SelectedItem = parameters?.ContainsKey("Margin") == true &&
                                        marginOption.Items.Contains(parameters["Margin"])
                                        ? parameters["Margin"]
                                        : marginOption.Items.FirstOrDefault();
            node.ComboOptions.Add(marginOption);
        }

        /// <summary>
        /// 为高斯滤波节点添加选项并根据配方设置选中项
        /// </summary>
        private static void AddGaussianFilterOptions(NodeViewModel node, Dictionary<string, string> parameters)
        {
            var filterSizeOption = new ComboOptionViewModel
            {
                Label = "FilterSize",
                Items = new ObservableCollection<string> { "3", "5", "7", "9", "11" }
            };
            filterSizeOption.SelectedItem = parameters?.ContainsKey("FilterSize") == true &&
                                            filterSizeOption.Items.Contains(parameters["FilterSize"])
                                            ? parameters["FilterSize"]
                                            : filterSizeOption.Items.FirstOrDefault();
            node.ComboOptions.Add(filterSizeOption);
        }

        /// <summary>
        /// 根据操作类型返回节点显示标题的资源键
        /// </summary>
        private static string GetNodeTitleKey(ImgProcessOperator op)
        {
            return op switch
            {
                ImgProcessOperator.Grayscale => "GrayscaleNode",
                ImgProcessOperator.HistogramEqualization => "HistogramEqualizationNode",
                ImgProcessOperator.MedianFilter => "MedianFilterNode",
                ImgProcessOperator.GaussianFilter => "GaussianFilterNode",
                _ => "Unknown"
            };
        }
        #endregion
    }
}
