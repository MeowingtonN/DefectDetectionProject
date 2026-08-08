using HalconDotNet;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using Wang.DefectDetectionProject.Core.DefectDetection.Models;

namespace Wang.DefectDetectionProject.Core.Models
{
    /// <summary>
    /// Item展示的图像信息
    /// </summary>
    public class ImageListItem : BindableBase
    {
        public ImageListItem()
        {
            DefectDetectionResults = new ObservableCollection<DefectDetectionResult>();
        }

        private string? title;
        private BitmapSource? imgSource;
        private HObject? image;
        private ObservableCollection<DefectDetectionResult>? defectDetectionResults;

        /// <summary>
        /// Item的名称
        /// </summary>
        public string? Title { get { return title; }  set { title = value; RaisePropertyChanged(); } }

        /// <summary>
        /// Item展示的图像
        /// </summary>
        public BitmapSource? ImgSource { get { return imgSource; }  set { imgSource = value; RaisePropertyChanged(); } }

        /// <summary>
        /// Item展示的图像所对应的HObject格式
        /// </summary>
        public HObject? Image { get { return image; }  set { image = value; RaisePropertyChanged(); } }

        /// <summary>
        /// 缺陷检测结果集合
        /// </summary>
        public ObservableCollection<DefectDetectionResult>? DefectDetectionResults 
        { 
            get { return defectDetectionResults; } 
            set { defectDetectionResults = value; RaisePropertyChanged(); } 
        }
    }
}
