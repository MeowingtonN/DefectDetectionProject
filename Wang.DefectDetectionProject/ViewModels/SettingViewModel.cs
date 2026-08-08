using MaterialDesignColors;
using MaterialDesignColors.ColorManipulation;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Wang.DefectDetectionProject.Core;
using Wang.DefectDetectionProject.Core.Tools;
using Wang.DefectDetectionProject.Models;
using Wang.DefectDetectionProject.Shared.Event;
using Wang.DefectDetectionProject.Shared.Services;
using Wang.DefectDetectionProject.Shared.Services.Tables;

namespace Wang.DefectDetectionProject.ViewModels
{
    public class SettingViewModel : NavigationViewModel
    {
        public SettingViewModel(IEventAggregator eventAggregator, ISettingService settingService)
        {
            this.eventAggregator = eventAggregator;
            this.settingService = settingService;
            LanguageInfoList = new ObservableCollection<LanguageInfo>();
            SaveCommand = new DelegateCommand(Save);
            ChangeHueCommand = new DelegateCommand<object>(ChangeHue);
        }

        /// <summary>
        /// 保存设置命令
        /// </summary>
        public DelegateCommand SaveCommand { get; }

        /// <summary>
        /// 保存设置方法
        /// </summary>
        private void Save()
        {
            if (CurrentLanguage == null || themeColor == null) return;
            settingEntity!.SkinName = IsDarkTheme.ToString();
            settingEntity!.SkinColor = themeColor;
            settingEntity!.Language = CurrentLanguage.Key;
            settingService.SaveSetting(settingEntity);
            // 保存系统设置后更新静态字段：initIsDarkMode
            initIsDarkMode = IsDarkTheme.ToString();
        }

        /// <summary>
        /// 事件聚合器
        /// </summary>
        private readonly IEventAggregator eventAggregator;

        /// <summary>
        /// 系统设置服务，用于数据库操作
        /// </summary>
        private readonly ISettingService settingService;


        private ObservableCollection<LanguageInfo>? languageInfoList;
        /// <summary>
        /// 支持的语言集合
        /// </summary>
		public ObservableCollection<LanguageInfo>? LanguageInfoList
        {
            get { return languageInfoList; }
            set
            {
                languageInfoList = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 在重新为当前语言（CurrentLanguage）赋值时，触发执行本方法，
        /// 以更改当前实际应用的资源，更改当前全局语言，更改缓存的语言键值对，
        /// 并且发出信号通知刷新语言。
        /// </summary>
        private void LanguageChanged()
        {
            if (LanguageHelper.AppCurrentLanguage == CurrentLanguage!.Key) return;

            // 更改资源顺序，更改当前全局语言，更改缓存的语言键值对
            LanguageHelper.SetLanguage(CurrentLanguage!.Key!);

            // 通知（Publish）所有界面刷新语言（发出事件信号/事件通知）
            eventAggregator.GetEvent<LanguageEventBus>().Publish(true);
        }

        private LanguageInfo? currentLanguage;
        /// <summary>
        /// 当前选择的语言，与ComboBox的SelectedItem双向绑定，SelectedItem对应的数据源可设置给本CurrentLanguage属性。
        /// </summary>
        public LanguageInfo? CurrentLanguage
        {
            get { return currentLanguage; }
            set
            {
                currentLanguage = value;
                LanguageChanged();
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 系统设置实体对象
        /// </summary>
        private SettingEntity? settingEntity;

        /// <summary>
        /// 在导航到当前视图后调用，此时页面已创建但可能尚未显示。
        /// </summary>
        /// <param name="navigationContext"></param>
        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            InitLanguageInfoList();
            // 每次导航到本页面就触发查询数据库中此时存储的当前系统语言
            settingEntity = await settingService.GetSettingAsync();
            CurrentLanguage = LanguageInfoList!.FirstOrDefault(t => t.Key!.Equals(settingEntity!.Language));
            if (initIsDarkMode != null)
                // 在导航到本页面时更新_isDarkTheme的值并更新绑定的对应控件显示
                SetProperty(ref _isDarkTheme, initIsDarkMode.Equals("True") ? true : false, nameof(IsDarkTheme));

            base.OnNavigatedTo(navigationContext);
        }

        /// <summary>
        /// 在导航离开当前视图时调用，可用于保存状态或取消操作，此时页面尚未被移除。
        /// </summary>
        /// <param name="navigationContext"></param>
        public override async void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // 每次离开本页面就触发查询数据库中此时存储的当前系统语言
            settingEntity = await settingService.GetSettingAsync();
            CurrentLanguage = LanguageInfoList!.FirstOrDefault(t => t.Key!.Equals(settingEntity!.Language));
            // 在离开本页面时设置主题（深浅色模式）
            IsDarkTheme = settingEntity!.SkinName == "True" ? true : false;
            if (!string.IsNullOrWhiteSpace(settingEntity!.SkinColor))
            {
                var color = ColorConverter.ConvertFromString(settingEntity.SkinColor);
                // 在离开本页面时设置主题颜色
                ChangeHue(color);
            }
            base.OnNavigatedFrom(navigationContext);
        }

        private void InitLanguageInfoList()
        {
            // 不使用LanguageInfoList!.Clear()，因为这会触发CurrentLanguage属性的setter，进而导致空引用异常（赋值currentLanguage为null）。
            if (LanguageInfoList!.Count == 0)
            {
                LanguageInfoList!.Add(new LanguageInfo() { Key = "zh-CN", Value = "中文" });
                LanguageInfoList!.Add(new LanguageInfo() { Key = "en-US", Value = "English" });
            }
        }

        #region 主题设置
        private bool _isDarkTheme = true;
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (SetProperty(ref _isDarkTheme, value))
                {
                    ModifyTheme(theme => theme.SetBaseTheme(value ? Theme.Dark : Theme.Light));
                }
            }
        }

        public IEnumerable<ISwatch> Swatches { get; } = SwatchHelper.Swatches;

        public DelegateCommand<object> ChangeHueCommand { get; }

        private readonly static PaletteHelper paletteHelper = new PaletteHelper();

        /// <summary>
        /// 修改主题（深浅色模式）
        /// </summary>
        /// <param name="modificationAction"></param>
        private static void ModifyTheme(Action<ITheme> modificationAction)
        {
            var paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();
            modificationAction?.Invoke(theme);
            paletteHelper.SetTheme(theme);
        }

        private static string? themeColor;

        private static string? initIsDarkMode;
        /// <summary>
        /// 用于初始化时保存数据库读取到的是否是深色模式的数据
        /// </summary>
        public static string? InitIsDarkMode
        {
            get { return initIsDarkMode; }
            set
            {
                initIsDarkMode = value;
                ModifyTheme(theme => theme.SetBaseTheme(value!.Equals("True") ? Theme.Dark : Theme.Light));
            }
        }

        /// <summary>
        /// 修改颜色
        /// </summary>
        /// <param name="obj">颜色对象</param>
        public static void ChangeHue(object obj)
        {
            themeColor = obj.ToString()!;
            var hue = (Color)obj;
            ITheme theme = paletteHelper.GetTheme();
            theme.PrimaryLight = new ColorPair(hue.Lighten());
            theme.PrimaryMid = new ColorPair(hue);
            theme.PrimaryDark = new ColorPair(hue.Darken());
            paletteHelper.SetTheme(theme);
        }
        #endregion
    }
}
