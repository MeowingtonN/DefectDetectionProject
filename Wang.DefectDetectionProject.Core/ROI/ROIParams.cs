using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.Core.ROI
{
    /// <summary>
    /// ROI区域参数信息
    /// </summary>
    public class ROIParams
    {
        /// <summary>
        /// 矩形ROI区域左上角Row坐标
        /// </summary>
        public double Row1 { get; set; }

        /// <summary>
        /// 矩形ROI区域左上角Column坐标
        /// </summary>
        public double Column1 { get; set; }

        /// <summary>
        /// 矩形ROI区域右下角Row坐标
        /// </summary>
        public double Row2 { get; set; }

        /// <summary>
        /// 矩形ROI区域右下角Column坐标
        /// </summary>
        public double Column2 { get; set; }
    }
}
