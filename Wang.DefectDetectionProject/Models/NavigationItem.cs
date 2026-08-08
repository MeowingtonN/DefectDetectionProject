using System.Collections.ObjectModel;

namespace Wang.DefectDetectionProject.Models
{
    /// <summary>
    /// 菜单实体类，用于描述导航项
    /// </summary>
    public class NavigationItem : BindableBase
    {
        public NavigationItem(string key, string icon, string name, string pageName,
                              ObservableCollection<NavigationItem>? items = null)
        {
            Key = key;
            Icon = icon;
            Name = name;
            PageName = pageName;
            Items = items;
        }

        private string? key;
        /// <summary>
        /// 导航项的键，用于在多语言资源中寻找对应的值，实现多语言功能
        /// </summary>
        public string? Key
        {
            get { return key; }
            set { key = value; RaisePropertyChanged(); }
        }

        private string? name;
        /// <summary>
        /// 导航菜单名称
        /// </summary>
        public string? Name
        {
            get { return name; }
            set
            {
                name = value;
                // 设置该属性后更新绑定的相关目标控件的显示（提供通知）
                RaisePropertyChanged();
            }
        }

        private string? icon;
        /// <summary>
        /// 导航菜单图标
        /// </summary>
        public string? Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<NavigationItem>? items;
        /// <summary>
        /// 子项集合
        /// </summary>
        public ObservableCollection<NavigationItem>? Items
        {
            get { return items; }
            set
            {
                items = value;
                RaisePropertyChanged();
            }
        }

        private string? pageName;
        /// <summary>
        /// 菜单导航指向的页面的名称（即触发导航跳转的实际页面所对应的类名，即视图类的类型名称）
        /// </summary>
        public string? PageName
        {
            get { return pageName; }
            set
            {
                pageName = value;
                RaisePropertyChanged();
            }
        }
    }
}
