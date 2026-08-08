using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wang.DefectDetectionProject.Core;

namespace Wang.DefectDetectionProject.ViewModels
{
    public class OperationViewModel : NavigationViewModel
    {
        private readonly IRegionManager _regionManager;

        public OperationViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            TabSelectionChangedCommand = new DelegateCommand<TabItem>(OnTabSelectionChanged);
        }

        /// <summary>
        /// 更改“图像处理”、“缺陷检测”等页面
        /// </summary>
        /// <param name="item"></param>
        private void OnTabSelectionChanged(TabItem item)
        {
            if (item == null) return;
            _regionManager.Regions["TabRegion"].RequestNavigate(item.Tag.ToString(), back =>
            {
                if (!back.Success)
                {
                    System.Diagnostics.Debug.WriteLine(back.Exception?.Message);
                }
            });
        }

        public DelegateCommand<TabItem> TabSelectionChangedCommand { get; }
    }
}
