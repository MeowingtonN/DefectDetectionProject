using HalconDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Wang.DefectDetectionProject.Core.Extensions
{
    public static class HImageExtensions
    {
        /// <summary>
        /// 将 HImage 转为 BitmapSource（默认 DPI=32）。
        /// 支持灰度（1通道）和彩色（3通道）图像。
        /// </summary>
        public static BitmapSource ToBitmapSource(this HImage image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            HImage workImage;
            // 1. 保证数据类型为 byte
            HOperatorSet.GetImageType(image, out HTuple imgType);
            if (imgType != "byte")
                workImage = image.Clone().ConvertImageType("byte");
            else
                workImage = image;

            // 2. 获取图像尺寸与通道数
            workImage.GetImageSize(out int width, out int height);
            int channels = workImage.CountChannels();

            PixelFormat pixelFormat;
            HImage sourceImage;

            if (channels == 1)
            {
                pixelFormat = PixelFormats.Gray8;
                sourceImage = workImage;
            }
            else if (channels == 3)
            {
                byte[] interleaved = ExtractInterleavedRgb(workImage);
                pixelFormat = PixelFormats.Rgb24;
                BitmapSource bitmap_3 = BitmapSource.Create(width, height, 32, 32, pixelFormat, null, interleaved, width * 3);
                bitmap_3.Freeze();
                return bitmap_3;
            }
            else
            {
                throw new NotSupportedException($"不支持的通道数: {channels}");
            }

            // 3. 获取图像数据指针与步长
            IntPtr ptr = sourceImage.GetImagePointer1(out string type, out int imgWidth, out int imgHeight);
            int stride = imgWidth * (pixelFormat.BitsPerPixel / 8);
            int bufferSize = stride * imgHeight;

            // 4. 拷贝到托管数组（避免直接持有非托管指针）
            byte[] pixels = new byte[bufferSize];
            System.Runtime.InteropServices.Marshal.Copy(ptr, pixels, 0, bufferSize);

            // 5. 创建 BitmapSource（冻结以跨线程使用）
            BitmapSource bitmap = BitmapSource.Create(imgWidth, imgHeight, 32, 32, pixelFormat, null, pixels, stride);
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// 手动提取三个通道并构建像素数组（手动交错RGB通道）
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static byte[] ExtractInterleavedRgb(HImage image)
        {
            image.GetImageSize(out int width, out int height);
            int stride = width * 3;
            byte[] rgb = new byte[stride * height];

            // 分别提取三个通道的数据
            HImage r = image.AccessChannel(1);
            HImage g = image.AccessChannel(2);
            HImage b = image.AccessChannel(3);

            HTuple m, n, p;

            IntPtr rPtr = r.GetImagePointer1(out m, out n, out p);
            IntPtr gPtr = g.GetImagePointer1(out m, out n, out p);
            IntPtr bPtr = b.GetImagePointer1(out m, out n, out p);

            byte[] rData = new byte[width * height];
            byte[] gData = new byte[width * height];
            byte[] bData = new byte[width * height];

            System.Runtime.InteropServices.Marshal.Copy(rPtr, rData, 0, rData.Length);
            System.Runtime.InteropServices.Marshal.Copy(gPtr, gData, 0, gData.Length);
            System.Runtime.InteropServices.Marshal.Copy(bPtr, bData, 0, bData.Length);

            for (int i = 0; i < width * height; i++)
            {
                rgb[i * 3 + 0] = rData[i];
                rgb[i * 3 + 1] = gData[i];
                rgb[i * 3 + 2] = bData[i];
            }
            return rgb;
        }
    }
}
