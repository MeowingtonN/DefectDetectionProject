using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.Core.DefectDetection.Models
{
    public class DefectInfo : BindableBase
    {
        /// <summary>
        /// 缺陷名称
        /// </summary>
        private string? className;
        /// <summary>
        /// 缺陷名称
        /// </summary>
        public string? ClassName 
        {
            get { return className; }
            set { className = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 缺陷ID（ID即为标注图中缺陷对应的像素值）
        /// </summary>
        private int classID;
        /// <summary>
        /// 缺陷ID（ID即为标注图中缺陷对应的像素值）
        /// </summary>
        public int ClassID
        {
            get { return classID; }
            set { classID = value; RaisePropertyChanged(); }
        }
    }
}
