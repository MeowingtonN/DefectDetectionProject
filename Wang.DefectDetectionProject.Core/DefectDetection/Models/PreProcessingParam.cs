using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.Core.DefectDetection.Models
{
    /// <summary>
    /// 模型训练前的图像预处理参数
    /// </summary>
    public class PreProcessingParam : BindableBase
    {
        #region Image dimensions the images are rescaled to during preprocessing, include width, height and number of channels.
        /// <summary>
        /// Image's Width the Images are Rescaled to.
        /// </summary>
        private int imageScaledWidth = 400;
        /// <summary>
        /// Image's Width the Images are Rescaled to.
        /// </summary>
        public int ImageScaledWidth
        {
            get { return imageScaledWidth; }
            set
            {
                imageScaledWidth = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Image's Height the Images are Rescaled to.
        /// </summary>
        private int imageScaledHeight = 400;
        /// <summary>
        /// Image's Height the Images are Rescaled to.
        /// </summary>
        public int ImageScaledHeight
        {
            get { return imageScaledHeight; }
            set
            {
                imageScaledHeight = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Number of Channels the Images are Rescaled to.
        /// </summary>
        private int imageScaledNumChannels = 3;
        /// <summary>
        /// Number of Channels the Images are Rescaled to.
        /// </summary>
        public int ImageScaledNumChannels
        {
            get { return imageScaledNumChannels; }
            set
            {
                imageScaledHeight = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region 归一化
        /// <summary>
        /// 图像归一化后灰度值的最小值
        /// </summary>
        private int imageRangeMin = -127;
        /// <summary>
        /// 图像归一化后灰度值的最小值
        /// </summary>
        public int ImageRangeMin
        {
            get { return imageRangeMin; }
            set
            {
                imageRangeMin = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 图像归一化后灰度值的最大值
        /// </summary>
        private int imageRangeMax = 128;
        /// <summary>
        /// 图像归一化后灰度值的最大值
        /// </summary>
        public int ImageRangeMax
        {
            get { return imageRangeMax; }
            set
            {
                imageRangeMax = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 图像归一化类型
        /// </summary>
        private string? normalizationType = "none";
        /// <summary>
        /// 图像归一化类型
        /// </summary>
        public string? NormalizationType
        {
            get { return normalizationType; }
            set
            {
                normalizationType = value;
                RaisePropertyChanged();
            }
        }
        #endregion
    }
}
