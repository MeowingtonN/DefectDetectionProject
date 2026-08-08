using System.Collections.ObjectModel;
using Wang.DefectDetectionProject.Core.Tools;
using Wang.DefectDetectionProject.Models;

namespace Wang.DefectDetectionProject.Services
{
    /// <summary>
    /// 导航跳转来或跳转走时的服务类，可 实现接口定义的初始化方法或析构方法 和 提供跳转到界面时界面的相关显示数据源。
    /// </summary>
    public class NavigationMenuService : BindableBase, INavigationMenuService
    {
        public NavigationMenuService()
        {
            NavigationItems = new ObservableCollection<NavigationItem>();
        }

        private ObservableCollection<NavigationItem>? navigationItems;
        /// <summary>
        /// 导航菜单集合，显示数据源。使用ObservableCollection是因为其的Add等方法会自动通知WPF相关绑定控件的显示更新。
        /// </summary>
        public ObservableCollection<NavigationItem>? NavigationItems
        {
            get { return navigationItems; }
            set
            {
                navigationItems = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 初始化菜单（初始化本服务内部的数据源）
        /// </summary>
        public void InitMenus()
        {
            NavigationItems?.Clear();

            NavigationItems?.Add(new NavigationItem("Setting", "", "系统设置", "SettingView"));
        }

        /// <summary>
        /// 刷新菜单显示（更新数据源NavigationItem的Name——多语言支持）。
        /// 主要用于更新TopBar的菜单栏显示和DashBoardView的卡片菜单显示。
        /// </summary>
        public void RefreshMenus()
        {
            foreach (var item in NavigationItems!)
            {
                item.Name = LanguageHelper.TranslationKeyValues![item.Key!];
            }
        }
    }
}
