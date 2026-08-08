using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.ImageProcess.ViewModels
{
    public class ComboOptionViewModel : BindableBase
    {
        /// <summary>
        /// 下拉框前标签
        /// </summary>
        private string? label;
        /// <summary>
        /// 下拉框前标签
        /// </summary>
        public string? Label
        {
            get { return label; }
            set { label = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 可选列表
        /// </summary>
        private ObservableCollection<string>? items = new ObservableCollection<string>();
        /// <summary>
        /// 可选列表
        /// </summary>
        public ObservableCollection<string>? Items
        {
            get { return items; }
            set { items = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 下拉列表选中项内容
        /// </summary>
        private string? _selectedItem;
        /// <summary>
        /// 下拉列表选中项内容
        /// </summary>
        public string? SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; RaisePropertyChanged(); }
        }
    }
}
