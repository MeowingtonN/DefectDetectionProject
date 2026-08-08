using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wang.DefectDetectionProject.Core.ROI;

namespace Wang.DefectDetectionProject.Core.ImageProcess.RecipeService.RecipeModels
{
    /// <summary>
    /// 完整的图像处理配方
    /// </summary>
    public class ImageProcessRecipe
    {
        /// <summary>
        /// ROI 区域信息
        /// </summary>
        public ROIParams? ROI { get; set; }

        /// <summary>
        /// 有序的处理步骤
        /// </summary>
        public List<RecipeStep>? Steps { get; set; }
    }
}
