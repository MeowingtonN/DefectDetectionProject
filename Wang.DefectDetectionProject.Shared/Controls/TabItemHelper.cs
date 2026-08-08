using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Wang.DefectDetectionProject.Shared.Controls
{
    /// <summary>
    /// 用于声明TabItem的自定义附加属性的静态类
    /// </summary>
    public static class TabItemHelper
    {
        // ============ 声明自定义附加属性：选中前景色 ============
        public static readonly DependencyProperty SelectedForegroundProperty =
            DependencyProperty.RegisterAttached(
                "SelectedForeground",
                typeof(Brush),
                typeof(TabItemHelper),
                new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x62, 0x00, 0xEE)))); // 默认 MaterialPrimaryColor

        public static Brush GetSelectedForeground(DependencyObject obj) =>
            (Brush)obj.GetValue(SelectedForegroundProperty);
        public static void SetSelectedForeground(DependencyObject obj, Brush value) =>
            obj.SetValue(SelectedForegroundProperty, value);

        // ============ 声明自定义附加属性：选中指示器颜色 ============
        public static readonly DependencyProperty SelectedIndicatorBrushProperty =
            DependencyProperty.RegisterAttached(
                "SelectedIndicatorBrush",
                typeof(Brush),
                typeof(TabItemHelper),
                new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x62, 0x00, 0xEE)))); // 默认 PrimaryBrush

        // 当识别到 <TabItem local:TabItemHelper.SelectedIndicatorBrush="Red" /> （访问自定义附加属性）时，
        // XAML 编译器会寻找 TabItemHelper 类中名为 GetSelectedIndicatorBrush 和 SetSelectedIndicatorBrush 的静态方法，通过调用它们来读取或写入值。
        // 如果缺少这对方法，编译时会报错：“无法找到属性 'SelectedIndicatorBrush'”。
        //（对于名为 Xxx 的附加属性，方法名必须是 GetXxx 和 SetXxx。）

        public static Brush GetSelectedIndicatorBrush(DependencyObject obj) =>
            (Brush)obj.GetValue(SelectedIndicatorBrushProperty);
        public static void SetSelectedIndicatorBrush(DependencyObject obj, Brush value) =>
            obj.SetValue(SelectedIndicatorBrushProperty, value);
    }
}
