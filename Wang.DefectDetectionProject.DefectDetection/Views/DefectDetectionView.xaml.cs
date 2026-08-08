using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wang.DefectDetectionProject.DefectDetection.Views
{
    /// <summary>
    /// DefectDetectionView.xaml 的交互逻辑
    /// </summary>
    public partial class DefectDetectionView : UserControl
    {
        public DefectDetectionView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ListBox鼠标点击事件处理方法——点击ListBox的空白区域时取消imgListBox中的所有Items的选中状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 获取鼠标点击的原始元素
            DependencyObject? source = e.OriginalSource as DependencyObject;
            if (source == null) return;

            // 如果点击的元素不是某个 ListBoxItem 的后代，则视为点击空白区域
            if (FindVisualParent<ListBoxItem>(source) == null)
            {
                imgListBox.SelectedItem = null;
            }
        }

        // 辅助方法：向上查找指定类型的父级元素
        private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T) return (T)child;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }
    }
}
