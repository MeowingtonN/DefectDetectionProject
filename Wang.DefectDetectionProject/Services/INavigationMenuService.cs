using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.Services
{
    /// <summary>
    /// 导航跳转来或跳转走时的服务接口，可定义初始化方法或析构方法
    /// </summary>
    public interface INavigationMenuService
    {
        /// <summary>
        /// 初始化菜单
        /// </summary>
        void InitMenus();

        /// <summary>
        /// 刷新菜单内容（多语言支持）
        /// </summary>
        void RefreshMenus();
    }
}
