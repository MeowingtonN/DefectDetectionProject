using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.Core
{
    /// <summary>
    /// 导航基类。需要被ViewModel类继承，故要继承自BindableBase类以便于ViewModel层使用RaisePropertyChanged方法更新绑定控件显示（MVVM框架）。
    /// </summary>
    public class NavigationViewModel : BindableBase, INavigationAware
    {
        /// <summary>
        /// 是否重用导航对象
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <returns></returns>
        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// 导航切换时触发（导航从本界面跳转走时触发执行）
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        /// <summary>
        /// 导航被执行触发（导航跳转到本界面来时触发执行，可用于在界面显示前初始化一些字段或属性）
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
    }
}
