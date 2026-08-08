using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Wang.DefectDetectionProject.Core.ImageProcess.Enums;

namespace Wang.DefectDetectionProject.Core.ImageProcess.RecipeService.RecipeModels
{
    /// <summary>
    /// 单个图像处理步骤
    /// </summary>
    public class RecipeStep
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ImgProcessOperator Operator { get; set; }

        /// <summary>
        /// 该步骤的参数（键为参数名，值为参数值）
        /// 对于无参数的操作可为空
        /// </summary>
        public Dictionary<string, string>? Parameters { get; set; }
    }
}
