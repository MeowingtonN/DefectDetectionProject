using HalconDotNet;
using Wang.DefectDetectionProject.Core.ROI;

namespace Wang.DefectDetectionProject.Core.Extensions
{
    public static class HObjectExtensions
    {
        /// <summary>
        /// Reduce image's Domain To a Region.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="region"></param>
        /// <returns>Domain After Reduction.</returns>
        public static HObject ReduceDomain(this HObject image, HObject region)
        {
            HOperatorSet.ReduceDomain(image, region, out HObject template);
            return template;
        }

        /// <summary>
        /// Reduce image's Domain To a Rectangle-ROI.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="roi"></param>
        /// <returns>Domain After Reduction.</returns>
        public static HObject ReduceDomain(this HObject image, ROIParams? roi)
        {
            if (roi == null) return image;
            if (roi.Row1 == 0 && roi.Column1 == 0 && roi.Row2 == 0 && roi.Column2 == 0) return image;

            HOperatorSet.GenRectangle1(out HObject rectangle, roi.Row1, roi.Column1, roi.Row2, roi.Column2);
            HOperatorSet.ReduceDomain(image, rectangle, out HObject imageReduced);
            return imageReduced;
        }

        /// <summary>
        /// Cut out of defined gray values.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static HObject CropDomain(this HObject image)
        {
            HOperatorSet.CropDomain(image, out HObject region);
            return region;
        }

        /// <summary>
        /// Save Image.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="format"></param>
        /// <param name="fileName"></param>
        public static void SaveImage(this HObject image, string format, string fileName)
        {
            HOperatorSet.WriteImage(image, format, 0, fileName);
        }

        /// <summary>
        /// Transform an RGB image into a gray scale image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static HObject Rgb1ToGray(this HObject image)
        {
            HOperatorSet.Rgb1ToGray(image, out HObject grayImage);
            return grayImage;
        }
    }
}
