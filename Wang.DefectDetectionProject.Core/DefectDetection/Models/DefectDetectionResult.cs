using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.Core.DefectDetection.Models
{
    /// <summary>
    /// 缺陷检测结果实体类
    /// </summary>
    public class DefectDetectionResult : BindableBase
    {
        /// <summary>
        /// 图像名称
        /// </summary>
        private string? fileName;
        /// <summary>
        /// 图像名称
        /// </summary>
        public string? FileName
        {
            get { return fileName; }
            set { fileName = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 缺陷名称
        /// </summary>
        private string? defectName;
        /// <summary>
        /// 缺陷名称
        /// </summary>
        public string? DefectName
        {
            get {  return defectName; } 
            set { defectName = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 标识颜色
        /// </summary>
        private HTuple? markingColor;
        /// <summary>
        /// 标识颜色
        /// </summary>
        public HTuple? MarkingColor
        {
            get { return markingColor; }
            set { markingColor = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 缺陷总面积
        /// </summary>
        private HTuple? area;
        /// <summary>
        /// 缺陷总面积
        /// </summary>
        public HTuple? Area
        {
            get { return area; }
            set { area = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 该缺陷的区域个数
        /// </summary>
        private HTuple? count;
        /// <summary>
        /// 该缺陷的区域个数
        /// </summary>
        public HTuple? Count
        {
            get { return count; }
            set { count = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 检测结果，OK或NG
        /// </summary>
        private string? detectionResult;
        /// <summary>
        /// 检测结果，OK或NG
        /// </summary>
        public string? DetectionResult
        {
            get { return detectionResult; }
            set { detectionResult = value; RaisePropertyChanged(); }
        }
    }
}
