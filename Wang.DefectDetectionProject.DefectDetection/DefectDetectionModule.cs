using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wang.DefectDetectionProject.DefectDetection.ViewModels;
using Wang.DefectDetectionProject.DefectDetection.Views;

namespace Wang.DefectDetectionProject.DefectDetection
{
    /// <summary>
    /// 缺陷检测相关的Prism模块
    /// </summary>
    public class DefectDetectionModule : IModule
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
            services.RegisterForNavigation<DefectDetectionView, DefectDetectionViewModel>();
        }
    }
}
