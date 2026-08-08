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
using System.Windows.Shapes;

namespace Wang.DefectDetectionProject.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// TopBar鼠标点击事件处理方法——取消listMenuBox中的所有Items的选中状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 检查鼠标点击的原始源是否属于 listMenuBox 的可视树
            if (!IsMouseOverListBox(e.OriginalSource as DependencyObject))
            {
                listMenuBox.SelectedItem = null;
            }
        }

        private bool IsMouseOverListBox(DependencyObject? element)
        {
            while (element != null)
            {
                if (element == listMenuBox)
                    return true;
                element = VisualTreeHelper.GetParent(element);
            }
            return false;
        }
    }
}
