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

namespace Wang.DefectDetectionProject.Views
{
    /// <summary>
    /// OperationView.xaml 的交互逻辑
    /// </summary>
    public partial class OperationView : UserControl
    {
        public OperationView()
        {
            InitializeComponent();
        }

        private void MainTabControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 取消选中所有项
            MainTabControl.SelectedIndex = -1;
        }
    }
}
