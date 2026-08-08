using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.Core.ImageProcess.Enums
{
    /// <summary>
    /// 图像处理算子枚举
    /// </summary>
    public enum ImgProcessOperator
    {
        ImgSrc,    // 图像源
        Grayscale,
        HistogramEqualization,
        MedianFilter,
        GaussianFilter
    }
}
