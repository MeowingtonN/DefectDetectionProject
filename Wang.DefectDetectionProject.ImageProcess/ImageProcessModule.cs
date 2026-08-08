using Wang.DefectDetectionProject.Core.DefectDetection;
using Wang.DefectDetectionProject.ImageProcess.Services;
using Wang.DefectDetectionProject.ImageProcess.ViewModels;
using Wang.DefectDetectionProject.ImageProcess.Views;

namespace Wang.DefectDetectionProject.ImageProcess
{
    /// <summary>
    /// 图像处理相关的Prism模块
    /// </summary>
    public class ImageProcessModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        /// <summary>
        /// 在Prism模块中也能通过RegisterTypes方法注册 需要导航的视图 与 服务。
        /// 但是要在App类中添加本模块。
        /// </summary>
        /// <param name="services"></param>
        public void RegisterTypes(IContainerRegistry services)
        {
            // 注册需要导航的视图及其对应视图模型
            services.RegisterForNavigation<ImageProcessView, ImageProcessViewModel>();

            // 注册图像处理服务
            services.Register<ImageProcessService>();
        }
    }
}
