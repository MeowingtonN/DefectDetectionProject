using System.Windows;
using Wang.DefectDetectionProject.Core.DefectDetection;
using Wang.DefectDetectionProject.Core.DefectDetection.ViewModels;
using Wang.DefectDetectionProject.Core.DefectDetection.Views;
using Wang.DefectDetectionProject.DefectDetection;
using Wang.DefectDetectionProject.DefectDetection.ViewModels;
using Wang.DefectDetectionProject.ImageProcess;
using Wang.DefectDetectionProject.Services;
using Wang.DefectDetectionProject.Shared.Services;
using Wang.DefectDetectionProject.ViewModels;
using Wang.DefectDetectionProject.Views;

namespace Wang.DefectDetectionProject
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window? CreateShell() => null;

        /// <summary>
        /// 自定义应用启动时的初始化流程，用于解析要实际展示的主窗口实例、设置应用当前主窗口和展示主窗口。
        /// </summary>
        protected override void OnInitialized()
        {
            // 从依赖注入容器中获取MainView的实例对象

            var container = ContainerLocator.Container;

            // 通过名称解析MainView视图实例（手动解析一个命名视图 "MainView" 来作为主窗口）
            //（在 Prism 中，通过 RegisterForNavigation 注册的视图默认是瞬态的，意味着每次调用 Resolve 都会创建一个全新的实例。
            // 在这里，container.Resolve<object>("MainView") 会触发创建新的 MainView 实例和新的 MainViewModel 实例，并将
            // 该 MainViewModel 实例赋值给新的 MainView 实例的数据上下文。）
            var shell = container.Resolve<object>("MainView");
            if (shell is Window view)
            {
                // 更新 Prism 注册区域信息

                // 在 Prism 应用中，IRegionManager 由框架内部自动注册，默认注册为单例（Singleton）。
                // 这意味着整个应用生命周期中，容器只会创建一个 RegionManager 实例，每次通过 Resolve 请求 IRegionManager 都会返回这同一个对象。
                var regionManager = container.Resolve<IRegionManager>();    // 解析出全局 IRegionManager 对象

                // 将全局的 IRegionManager 附加到该窗口，这样 XAML 中的 RegionName 才能被识别。
                //（RegionName 是 Prism 框架中 RegionManager 定义的一个附加属性。
                // 它用来给 XAML 中的 UI 容器（如 ContentControl、ItemsControl、TabControl 等）标记一个唯一的区域名称，
                // 这样 Prism 的区域管理器就能识别这个容器，完成视图的加载、切换和生命周期管理。
                // 在 XAML 中引入 Prism 命名空间后，在容器控件上设置：
                // <ContentControl prism:RegionManager.RegionName="MainContentRegion" />
                // 其中，"MainContentRegion" 就是这个区域的名称。
                // 区域管理器会扫描可视树，找到所有声明了 RegionName 的控件，为它们创建对应的 IRegion 对象。
                // RegionName的作用是：
                // 标识目标容器：告诉 Prism“这个位置可以承载动态视图”；
                // 导航的地址：当调用 regionManager.RequestNavigate("区域名", "视图名") 时，第一个参数就是这个 RegionName。）
                /* 正常流程中，Prism 会为 CreateShell() 返回的窗口自动附加 RegionManager 并扫描其中的 Region。
                 * 但这里窗口是完全手工解析的（MainView 被注册为命名视图，直接从容器解析），没有经过 Prism 的内部处理，所以必须
                 * 手动调用RegionManager.SetRegionManager(view, regionManager) 和 RegionManager.UpdateRegions()。 */
                RegionManager.SetRegionManager(view, regionManager);
                // 强制立即更新所有区域，确保视图加载完成。通常在窗口显示前调用，可以避免延迟或空区域。
                RegionManager.UpdateRegions();

                // 调用首页（MainView）数据上下文的INavigationAware接口的相关方法做一个初始化操作
                if (view.DataContext is INavigationAware navigationAware)
                {
                    // 要跳转到主页（MainView）了。故实际调用了MainViewModel重写的OnNavigatedTo方法，初始化了MainView的菜单。
                    /* 在 Prism 中，当通过 IRegionManager.RequestNavigate 导航到某个视图时，
                     * 框架会自动调用 ViewModel 的 OnNavigatedTo / OnNavigatedFrom 等方法。
                     * 但现在主窗口不是通过导航进入的，而是直接设置为根窗口，所以 ViewModel 永远不会收到导航通知。
                     * 为了让 MainViewModel 能执行导航后的初始化逻辑（如加载菜单、权限等），这里手动触发了 OnNavigatedTo，
                     * 传递 null 作为导航上下文，模拟一次“进入主页”的导航。 */
                    navigationAware.OnNavigatedTo(null);

                    // 初始化MainView的相关字段或属性后再设置应用的当前MainWindow为主页（MainView）。
                    App.Current.MainWindow = view;
                }
            }

            // base.OnInitialized()包含应该最后发生的操作（MainWindow?.Show();），故必须放在最后！
            base.OnInitialized();
        }

        /// <summary>
        /// 向 依赖注入容器 注册 需要导航的视图 与 服务。
        /// </summary>
        /// <param name="services"></param>
        protected override void RegisterTypes(IContainerRegistry services)
        {
            // Registers an object for navigation with the ViewModel type to be used as the DataContext.
            // 将 MainView 注册为可导航的页面，同时指定它的 DataContext（即ViewModel）为 MainViewModel。
            // Prism 的 RegisterForNavigation 有一个默认命名规则：如果没有显式指定名称，会用视图类的类型名称（不包含命名空间）作为导航键。
            services.RegisterForNavigation<MainView, MainViewModel>();
            services.RegisterForNavigation<SettingView, SettingViewModel>();
            services.RegisterForNavigation<OperationView, OperationViewModel>();
            services.RegisterForNavigation<DefectInfoEditView, DefectInfoEditViewModel>();

            // 把 NavigationMenuService 注册为 INavigationMenuService 接口的【单例】，此处不会触发NavigationMenuService实例构造。
            // 效果：整个应用生命周期中，容器只会创建一个 NavigationMenuService 实例，每次请求 INavigationMenuService 都会返回同一个对象。
            // 这通常用于全局状态管理、导航菜单服务等场景。
            //（注册后可以通过 var menuService = container.Resolve<INavigationMenuService>(); 的方式拿到 NavigationMenuService 的单例实例。）
            /* 重要：MainViewModel具有INavigationMenuService成员。MainViewModel通过构造函数注入接收INavigationMenuService：
             * public MainViewModel(INavigationMenuService navigationMenuService){...} 
             * 那么当容器创建每个 MainViewModel 时（比如程序员调用container.Resolve<object>("MainView")让容器构造主视窗时触发构造MainViewModel实例），
             * 都会向容器请求 INavigationMenuService 实例以作为构造方法的入参，此时容器返回的是同一个单例。 */
            services.RegisterSingleton<INavigationMenuService, NavigationMenuService>();

            // 注册服务后，便可以使用【依赖注入】，典型的应用就是容器在构造入参需要该服务的对象时可以自动解析实例并传参构造。
            // （应用依赖注入时，构造方法入参类型需为接口。）
            //services.Register<ISettingService, SettingService>();
            services.RegisterSingleton<ISettingService, SettingService>();

            // 在依赖注入容器中注册缺陷检测服务、模型训练服务
            services.Register<DefectDetectionService>();
            services.Register<ModelTrainService>();
        }

        /// <summary>
        /// 重写ConfigureModuleCatalog方法以添加Prism模块
        /// </summary>
        /// <param name="moduleCatalog"></param>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            // 添加DefectDetectionModule模块
            moduleCatalog.AddModule<DefectDetectionModule>();
            // 添加ImageProcessModule模块
            moduleCatalog.AddModule<ImageProcessModule>();
            
            base.ConfigureModuleCatalog(moduleCatalog);
        }
    }
}
