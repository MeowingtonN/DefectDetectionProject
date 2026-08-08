using System.Windows.Media;
using Wang.DefectDetectionProject.Core;
using Wang.DefectDetectionProject.Core.Tools;
using Wang.DefectDetectionProject.Extensions;
using Wang.DefectDetectionProject.Models;
using Wang.DefectDetectionProject.Services;
using Wang.DefectDetectionProject.Shared.Event;
using Wang.DefectDetectionProject.Shared.Services;

namespace Wang.DefectDetectionProject.ViewModels
{
    /// <summary>
    /// MainView的ViewModel层，继承自导航基类NavigationViewModel
    /// </summary>
    public class MainViewModel : NavigationViewModel
    {
        /// <summary>
        /// MainViewModel的构造方法。
        /// MainViewModel具有INavigationMenuService成员，MainViewModel通过构造函数注入接收INavigationMenuService。
        /// 而由于应用启动时（见App.xaml.cs文件的RegisterTypes方法）把 NavigationMenuService 注册为 INavigationMenuService 接口的单例，
        /// 故依赖注入容器每次构造MainViewModel时，向容器请求的INavigationMenuService实例都是同一个单例。
        /// </summary>
        /// <param name="navigationMenuService">请求的INavigationMenuService实例</param>
        public MainViewModel(IRegionManager regionManager, INavigationMenuService navigationMenuService,
                             IEventAggregator eventAggregator, ISettingService settingService)
        {
            this.regionManager = regionManager;

            NavigationMenuService = navigationMenuService;

            this.settingService = settingService;

            // MainView界面订阅了（Subscribe）LanguageEventBus事件。
            //（这样在其它模块发出信号时，本视图模型就能接收到该事件信号并触发执行LanguageChanged回调方法以更新对应的视图）
            eventAggregator.GetEvent<LanguageEventBus>().Subscribe(LanguageChanged);

            // 在TopBar中，更改选中的ListBox的Item时，触发执行绑定的ViewModel中的NavigateCommand委托，
            // 并将选中的Item所代表的数据源集合的对应元素作为入参传入该委托。
            NavigateCommand = new DelegateCommand<NavigationItem>(NavigateCommandMethod);

            GoOperationCommand = new DelegateCommand(GoOperation);
        }

        /// <summary>
        /// 返回首页
        /// </summary>
        private void GoOperation()
        {
            NavigatePage("OperationView");
        }

        /// <summary>
        /// 语言更改时刷新菜单显示
        /// </summary>
        /// <param name="status"></param>
        private void LanguageChanged(bool status)
        {
            NavigationMenuService.RefreshMenus();
        }

        /// <summary>
        /// 更改选中的ListBox的Item时所触发执行的逻辑
        /// </summary>
        /// <param name="navigationItem">选中的ListBox的Item所代表的数据源集合的对应元素</param>
        private void NavigateCommandMethod(NavigationItem navigationItem)
        {
            if (navigationItem == null) return;

            // 导航到对应页面
            NavigatePage(navigationItem.PageName);
        }

        /// <summary>
        /// 区域管理器字段，用于导航
        /// </summary>
        private readonly IRegionManager regionManager;

        /// <summary>
        /// 系统设置服务，用于数据库操作
        /// </summary>
        private readonly ISettingService settingService;

        /// <summary>
        /// 命令模式。在TopBar中，更改选中的ListBox的Item时，触发执行绑定的ViewModel中的NavigateCommand委托，
        /// 并将选中的Item所代表的数据源集合的对应元素作为入参传入该委托。
        /// </summary>
        public DelegateCommand<NavigationItem> NavigateCommand { get; }

        /// <summary>
        /// 返回首页命令
        /// </summary>
        public DelegateCommand GoOperationCommand { get; }

        /// <summary>
        /// 导航跳转来或跳转走时的服务，用其提供的服务方法可在导航跳转来时初始化服务内部的相关字段和属性或在导航跳转走时析构字段。
        /// （是的，该导航服务类内部还提供了本界面的显示数据源）
        /// </summary>
        public INavigationMenuService NavigationMenuService { get; }

        /// <summary>
        /// 导航跳转到本界面（MainView）来时触发执行，用于在本界面显示前调用相关服务提供的方法以初始化菜单（初始化显示数据源）
        /// 和向本界面的相关区域加载其它视图。
        /// </summary>
        /// <param name="navigationContext"></param>
        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            // 将视图OperationView加载到MainView的MainViewRegion区域中显示
            NavigatePage("OperationView");
            // 初始化菜单（初始化该服务内部的服务显示数据源；该服务在依赖注入容器中是单例的，故只要这里初始化一次菜单内容即可）
            NavigationMenuService.InitMenus();
            // 初始化设置系统配置，包括语言和主题（异步地）
            await ApplySettingAsync();

            base.OnNavigatedTo(navigationContext);
        }

        /// <summary>
        /// 将pageName指定的视图加载到MainView的MainViewRegion区域中显示。
        ///（重要：所谓的导航功能，就是：将指定视图或控件加载到窗体的指定区域（容器）中显示。）
        /// </summary>
        /// <param name="pageName">指定视图的名称（在依赖注入容器中的导航键，在 Prism 中默认为视图类的类名）</param>
        private void NavigatePage(string? pageName)
        {
            if (pageName == null) return;
            // RequestNavigate 是 Prism 区域导航的核心方法，用于将一个目标视图（通常是UserControl类型的）动态加载并显示到指定的区域中。
            // 从全局区域管理器中取出名为 MainViewRegion 的 IRegion 对象（即代表 XAML 中 prism:RegionManager.RegionName="MainViewRegion" 的容器），
            // 根据 pageName 解析对应视图的实例（默认为瞬态，每次都会新建），同时通过 ViewModelLocator 自动创建对应的 ViewModel 并绑定到 DataContext。
            // 而后将视图放入区域（容器）并管理生命周期，如果区域是 ContentControl（Content属性承载单一内容），新视图会替换旧视图（旧视图被移除，触发OnNavigatedFrom）。
            //（调用 RequestNavigate 方法会自动触发导航感知接口操作：区域中旧视图的 ViewModel 会调用 INavigationAware.OnNavigatedFrom，
            // 新视图的 ViewModel 会调用 OnNavigatedTo（若实现了 INavigationAware 或 IConfirmNavigation 等）。
            // 导航是异步的，RequestNavigate方法立即返回，完成时通过回调通知结果。）
            regionManager.Regions["MainViewRegion"].RequestNavigate(pageName, back =>
            {
                if (!back.Success)
                {
                    System.Diagnostics.Debug.WriteLine(back.Exception?.Message);
                }
            });
        }

        /// <summary>
        /// 初始化设置系统配置，包括语言和主题
        /// </summary>
        /// <returns></returns>
        private async Task ApplySettingAsync()
        {
            // 从数据库中读取系统配置
            var setting = await settingService.GetSettingAsync();
            if (setting != null)
            {
                // 初始化设置语言（更新语言资源在资源字典中的顺序；更新多语言功能键值对）
                LanguageHelper.SetLanguage(setting.Language!);
                LanguageChanged(true);    // 刷新菜单显示
                // 初始化设置主题（深色浅色模式）
                if (!string.IsNullOrWhiteSpace(setting.SkinName))
                {
                    SettingViewModel.InitIsDarkMode = setting.SkinName;
                }
                // 初始化设置颜色
                if (!string.IsNullOrWhiteSpace(setting.SkinColor))
                {
                    var color = ColorConverter.ConvertFromString(setting.SkinColor);
                    SettingViewModel.ChangeHue(color);
                }
            }
        }
    }
}
