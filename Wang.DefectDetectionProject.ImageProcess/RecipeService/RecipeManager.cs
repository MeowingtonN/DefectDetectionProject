using HalconDotNet;
using Newtonsoft.Json;
using Wang.DefectDetectionProject.Core.ImageProcess.Enums;
using Wang.DefectDetectionProject.Core.ImageProcess.RecipeService.RecipeModels;
using Wang.DefectDetectionProject.Core.ROI;
using Wang.DefectDetectionProject.ImageProcess.ViewModels;

namespace Wang.DefectDetectionProject.ImageProcess.RecipeService
{
    /// <summary>
    /// 配方保存与读取工具类
    /// </summary>
    public static class RecipeManager
    {
        /// <summary>
        /// 从当前编辑器的节点链和 ROI 生成配方 JSON 字符串
        /// </summary>
        /// <param name="editorViewModel">编辑器视图模型，需包含 StartNode 和 Connections 属性</param>
        /// <param name="roi">当前的 ROI 参数</param>
        /// <returns>JSON 字符串</returns>
        public static string SaveRecipeToJson(EditorViewModel editorViewModel, ROIParams? roi)
        {
            var recipe = new ImageProcessRecipe
            {
                ROI = roi,
                Steps = new List<RecipeStep>()
            };

            NodeViewModel? currentNode = editorViewModel.StartNode;
            while (currentNode != null)
            {
                var step = new RecipeStep
                {
                    Operator = currentNode.ImageProcessOperator,
                    Parameters = new Dictionary<string, string>()
                };

                // 根据操作类型提取参数
                if (currentNode.ImageProcessOperator == ImgProcessOperator.MedianFilter)
                {
                    step.Parameters["MaskType"] = currentNode.ComboOptions.FirstOrDefault(o => o.Label == "MaskType")?.SelectedItem ?? "";
                    step.Parameters["Radius"] = currentNode.ComboOptions.FirstOrDefault(o => o.Label == "Radius")?.SelectedItem ?? "";
                    step.Parameters["Margin"] = currentNode.ComboOptions.FirstOrDefault(o => o.Label == "Margin")?.SelectedItem ?? "";
                }
                else if (currentNode.ImageProcessOperator == ImgProcessOperator.GaussianFilter)
                {
                    step.Parameters["FilterSize"] = currentNode.ComboOptions.FirstOrDefault(o => o.Label == "FilterSize")?.SelectedItem ?? "";
                }
                // 无参数操作不添加任何键

                if (step.Parameters.Count == 0)
                    step.Parameters = null; // 无参数时输出 null，让 JSON 更简洁

                recipe.Steps.Add(step);

                // 沿着单一路径寻找下一个节点
                var nextConnector = currentNode.OutputConnectors.FirstOrDefault();
                if (nextConnector == null) break;
                var connection = editorViewModel.Connections.FirstOrDefault(c => c.Source == nextConnector);
                if (connection == null) break;
                currentNode = connection.Target.OwnerNode;
            }

            return JsonConvert.SerializeObject(recipe, Formatting.Indented);
        }

        /// <summary>
        /// 从 JSON 字符串加载配方
        /// </summary>
        /// <param name="json">配方 JSON 字符串</param>
        /// <returns>配方对象</returns>
        public static ImageProcessRecipe? LoadRecipeFromJson(string json)
        {
            return JsonConvert.DeserializeObject<ImageProcessRecipe>(json);
        }

        /// <summary>
        /// 根据配方对图像进行处理，返回处理后的图像
        /// </summary>
        /// <param name="recipe">图像处理配方</param>
        /// <param name="originalImage">原始输入图像</param>
        /// <returns>处理后的图像，若失败则返回 null</returns>
        public static HObject? ApplyRecipeToImage(ImageProcessRecipe recipe, HObject originalImage)
        {
            if (recipe == null || originalImage == null)
                return null;

            HObject currentImage = originalImage;

            // 1. 应用 ROI 区域（若存在）
            if (recipe.ROI != null)
            {
                HObject? roiRegion = null;
                try
                {
                    // 根据矩形坐标生成区域
                    HOperatorSet.GenRectangle1(out roiRegion, recipe.ROI.Row1, recipe.ROI.Column1,
                                               recipe.ROI.Row2, recipe.ROI.Column2);
                    // 缩小图像定义域至 ROI
                    HObject reducedImage;
                    HOperatorSet.ReduceDomain(currentImage, roiRegion, out reducedImage);
                    // 裁剪出有效区域
                    HOperatorSet.CropDomain(reducedImage, out currentImage);
                }
                catch (Exception ex)
                {
                    // 记录日志或抛出
                    throw new Exception("应用 ROI 失败！", ex);
                }
                finally
                {
                    roiRegion?.Dispose();
                }
            }

            // 2. 按顺序执行处理步骤
            if (recipe.Steps != null)
            {
                foreach (var step in recipe.Steps)
                {
                    if (step == null) continue;

                    try
                    {
                        currentImage = ProcessStep(currentImage, step);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"执行步骤 {step.Operator} 失败！", ex);
                    }
                }
            }

            return currentImage;
        }

        /// <summary>
        /// 执行单个处理步骤
        /// </summary>
        private static HObject ProcessStep(HObject image, RecipeStep step)
        {
            HObject? outputImage = null;

            switch (step.Operator)
            {
                case ImgProcessOperator.ImgSrc:
                    // 源图像直接返回
                    outputImage = image;
                    break;

                case ImgProcessOperator.Grayscale:
                    HOperatorSet.Rgb1ToGray(image, out outputImage);
                    break;

                case ImgProcessOperator.HistogramEqualization:
                    HOperatorSet.EquHistoImage(image, out outputImage);
                    break;

                case ImgProcessOperator.MedianFilter:
                    string maskType = step.Parameters?["MaskType"] ?? "circle";
                    string radiusStr = step.Parameters?["Radius"] ?? "3";
                    string margin = step.Parameters?["Margin"] ?? "mirrored";

                    if (!int.TryParse(radiusStr, out int radius))
                        radius = 3; // 默认值

                    HOperatorSet.MedianImage(image, out outputImage, maskType, radius, margin);
                    break;

                case ImgProcessOperator.GaussianFilter:
                    string filterSizeStr = step.Parameters?["FilterSize"] ?? "3";

                    if (!int.TryParse(filterSizeStr, out int filterSize))
                        filterSize = 3;

                    HOperatorSet.GaussFilter(image, out outputImage, filterSize);
                    break;

                default:
                    throw new NotSupportedException($"不支持的算子类型: {step.Operator}。");
            }

            return outputImage;
        }
    }
}
