using HalconDotNet;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Wang.DefectDetectionProject.Core.Extensions;

namespace Wang.DefectDetectionProject.Shared.Controls
{
    /// <summary>
    /// 图像编辑控件，可像一般的控件一样复用和绑定属性。（控件开发，关注 控件模板 和 依赖属性系统 ！）
    /// </summary>
    public class ImageEditView : Control
    {
        private HSmartWindowControlWPF? hSmart;
        private HWindow? hWindow;

        private TextBlock? txtMsg;

        // 用于是否可改变图像的标志
        private bool canChangeDispImg = true;

        /// <summary>
        /// HalconWindow（将控件的HalconWindow暴露给服务，这样服务就能直接通过HalconWindow引用在Halcon窗口上进行绘制）
        /// </summary>
        public HWindow? HWindow
        {
            get { return (HWindow?)GetValue(HWindowProperty); }
            set { SetValue(HWindowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HWindow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HWindowProperty =
            DependencyProperty.Register("HWindow", typeof(HWindow), typeof(ImageEditView), new PropertyMetadata(null));


        /// <summary>
        /// 图像，需要开放给外部绑定，需要在get和set方法中访问依赖属性
        /// </summary>
        public HObject? Image
        {
            get
            {
                // 从依赖属性系统中获取对应值
                return (HObject)GetValue(ImageProperty);
            }
            set
            {
                // SetValue方法让用户调用的RaisePropertyChanged等方法向本控件更新的值能够传递到依赖属性系统中。
                SetValue(ImageProperty, value);
            }
        }

        /// <summary>
        /// 作为一个控件类，要对外开放能够通过{Binding ...}让用户动态绑定的属性，该属性就必须注册一个对应的依赖属性，否则在加载页面中的相关控件(XamlParse)时会抛出XamlParseException异常。
        /// 在需要的情况下，还应设置该属性被设置时触发的回调函数以实时渲染或进行其它逻辑操作（具体地，该设置的回调函数由SetValue方法触发执行，执行该回调函数的线程和执行触发该回调函数执行的SetValue方法的线程是同一个线程）。
        /// DependencyProperty.Register(
        /// string name,                  // 属性名称（与CLR包装属性名对应）
        /// Type propertyType,            // 属性值的类型
        /// Type ownerType,               // 拥有该属性的类
        /// PropertyMetadata typeMetadata // 元数据（默认值、回调等）
        /// );
        /// 这里让本控件的Image属性注册一个对应的依赖属性，使得在使用本控件时可以通过Image="{Binding ...}"让用户动态绑定Image属性。
        /// </summary>
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(HObject), typeof(ImageEditView), new PropertyMetadata(null, ImageChangedCallBack, CoerceImageCallback));

        /// <summary>
        /// 当本类中的 Image 属性值发生变化时，触发执行本静态回调方法进行相关渲染（清空绘制的图形，更改展示图像）。（属性变化通知）
        /// 这个方法本身不会影响属性的赋值，除非该方法体内对同一属性进行二次赋值。
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        public static void ImageChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageEditView view && e.NewValue != null && view.canChangeDispImg)
            {
                view.DrawingObjectList.Clear();
                view.Display(null);  // 先清空HWindow的显示
                view.Display((HObject)e.NewValue);
            }
        }

        /// <summary>
        /// 对Image的依赖属性赋予值时会触发执行本回调方法
        /// </summary>
        /// <param name="d"></param>
        /// <param name="baseValue"></param>
        /// <returns></returns>
        private static object CoerceImageCallback(DependencyObject d, object baseValue)
        {
            if (d is ImageEditView view && !view.canChangeDispImg)
            {
                // 直接返回当前值（旧值），而后属性系统会比较 Coerce 后的值与当前有效值，发现值相同，故不会触发 ImageChangedCallBack 回调方法
                return d.GetValue(ImageProperty);
            }
            return baseValue;  // 否则返回基础值baseValue（用户通过RaisePropertyChanged向绑定的依赖属性更新的值），表示不修改
        }


        /// <summary>
        /// 绘制的图形的集合（需供外部绑定）
        /// </summary>
        public ObservableCollection<DrawingObjectInfo> DrawingObjectList
        {
            get
            {
                return (ObservableCollection<DrawingObjectInfo>)GetValue(DrawingObjectListProperty);
            }
            set
            {
                SetValue(DrawingObjectListProperty, value);
            }
        }

        public static readonly DependencyProperty DrawingObjectListProperty =
            DependencyProperty.Register("DrawingObjectList", typeof(ObservableCollection<DrawingObjectInfo>), typeof(ImageEditView), new PropertyMetadata(new ObservableCollection<DrawingObjectInfo>()));


        /// <summary>
        /// 显示图片或图形到HWindow上，传入null表示清空HWindow的显示。保证跨线程访问控件的安全。
        /// </summary>
        /// <param name="hObject"></param>
        private void Display(HObject? hObject, HTuple? color = null)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (hObject == null)
                {
                    this.hWindow?.ClearWindow();
                    return;
                }

                if (color != null)
                {
                    HOperatorSet.SetColor(hWindow, color);
                }
                this.hWindow?.DispObj(hObject);
                // 让Halcon窗口以最适大小及比例呈现。
                this.hWindow?.SetPart(0, 0, -2, -2);
            });
        }

        /// <summary>
        /// 重写 OnApplyTemplate 方法的主要作用是：当控件的控件模板（ControlTemplate）被加载并应用到该控件上时，
        /// 获取模板中定义的命名部件，并完成相关初始化工作。
        /// </summary>
        public override void OnApplyTemplate()
        {
            // 依据名称在控件模板中找对应命名的控件
            txtMsg = (TextBlock)GetTemplateChild("PART_MSG");

            if (GetTemplateChild("PART_SMART") is HSmartWindowControlWPF hSmartWinControl)
            {
                this.hSmart = hSmartWinControl;
                this.hSmart.Loaded += HSmart_Loaded;
            }

            if (GetTemplateChild("PART_RECT") is MenuItem btnRect)
            {
                btnRect.Click += BtnRect_Click;
            }

            if (GetTemplateChild("PART_ELLIPSE") is MenuItem btnEllipse)
            {
                btnEllipse.Click += BtnEllipse_Click;
            }

            if (GetTemplateChild("PART_CIRCLE") is MenuItem btnCircle)
            {
                btnCircle.Click += BtnCircle_Click;
            }

            if (GetTemplateChild("PART_REGION") is MenuItem btnRegion)
            {
                btnRegion.Click += BtnRegion_Click;
            }

            if (GetTemplateChild("PART_CLEAR") is MenuItem btnClear)
            {
                btnClear.Click += BtnClear_Click;
            }

            base.OnApplyTemplate();
        }

        /// <summary>
        /// 清空绘制的图形和掩膜
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            DrawingObjectList.Clear();
            Display(null);
            Display(Image);
        }

        private async void BtnRegion_Click(object sender, RoutedEventArgs e)
        {
            canChangeDispImg = false;

            await DrawShape(ShapeType.Region);

            canChangeDispImg = true;
        }

        private async void BtnCircle_Click(object sender, RoutedEventArgs e)
        {
            canChangeDispImg = false;

            HTuple? row = null, column = null, radius = null;
            await DrawShape(ShapeType.Circle, row, column, radius);

            canChangeDispImg = true;
        }

        private async void BtnEllipse_Click(object sender, RoutedEventArgs e)
        {
            canChangeDispImg = false;

            HTuple? row = null, column = null, phi = null, radius1 = null, radius2 = null;
            await DrawShape(ShapeType.Ellipse, row, column, phi, radius1, radius2);

            canChangeDispImg = true;
        }

        private async void BtnRect_Click(object sender, RoutedEventArgs e)
        {
            canChangeDispImg = false;

            HTuple? row1 = null, column1 = null, row2 = null, column2 = null;
            await DrawShape(ShapeType.Rectangle, row1, column1, row2, column2);

            canChangeDispImg = true;
        }

        /// <summary>
        /// 私有绘制矩形、椭圆、圆形和任意形状辅助方法（async可等待方法）
        /// </summary>
        /// <param name="shapeType"></param>
        /// <param name="hTuples"></param>
        /// <returns></returns>
        private async Task DrawShape(ShapeType shapeType, params HTuple?[] hTuples)
        {
            HObject? drawObj = null;

            switch (shapeType)
            {
                case ShapeType.Rectangle:
                    txtMsg!.Text = "按住鼠标左键开始绘制矩形，点击鼠标右键结束绘制。";
                    break;
                case ShapeType.Ellipse:
                    txtMsg!.Text = "按住鼠标左键开始绘制椭圆，点击鼠标右键结束绘制。";
                    break;
                case ShapeType.Circle:
                    txtMsg!.Text = "按住鼠标左键开始绘制圆形，点击鼠标右键结束绘制。";
                    break;
                case ShapeType.Region:
                    txtMsg!.Text = "点击鼠标左键开始绘制任意形状，点击鼠标右键结束绘制。";
                    break;
                default:
                    break;
            }

            // 绘制期间禁止缩放图像
            hSmart!.HZoomContent = HSmartWindowControlWPF.ZoomContent.Off;

            await Task.Run(() =>
            {
                switch (shapeType)
                {
                    case ShapeType.Rectangle:
                        HOperatorSet.SetColor(hWindow, "blue");
                        HOperatorSet.DrawRectangle1(hWindow, out hTuples[0], out hTuples[1], out hTuples[2], out hTuples[3]);
                        drawObj = hTuples!.GenRectangle();
                        break;
                    case ShapeType.Ellipse:
                        HOperatorSet.SetColor(hWindow, "cyan");
                        HOperatorSet.DrawEllipse(hWindow, out hTuples[0], out hTuples[1], out hTuples[2], out hTuples[3], out hTuples[4]);
                        drawObj = hTuples!.GenEllipse();
                        break;
                    case ShapeType.Circle:
                        HOperatorSet.SetColor(hWindow, "cyan");
                        HOperatorSet.DrawCircle(hWindow, out hTuples[0], out hTuples[1], out hTuples[2]);
                        drawObj = hTuples!.GenCircle();
                        break;
                    case ShapeType.Region:
                        HOperatorSet.SetColor(hWindow, "orange");
                        HOperatorSet.DrawRegion(out drawObj, hWindow);
                        HTuple area1;
                        // 计算drawObj的面积
                        HOperatorSet.AreaCenter(drawObj, out area1, out _, out _);
                        if (area1.D == 0 || area1.D == 1)
                        {
                            // 若用户在绘制时只点击右键（area.D == 1），或绘制了一个点（area.D == 1），则过滤该生成的区域
                            drawObj = null;
                        }
                        break;
                }
            });

            if (drawObj != null)
            {
                DrawingObjectList.Add(new DrawingObjectInfo()
                {
                    ShapeType = shapeType,
                    HTuples = hTuples,
                    Hobject = drawObj
                });

                // 绘制图形轮廓
                HOperatorSet.GenContourRegionXld(drawObj, out HObject contour, "border");  // 获取drawObj的轮廓
                HOperatorSet.DispObj(contour, hWindow);
            }

            txtMsg!.Text = string.Empty;    // 这里回到UI线程执行。
            hSmart!.HZoomContent = HSmartWindowControlWPF.ZoomContent.WheelForwardZoomsIn;
        }

        /// <summary>
        /// Occurs when the element is laid out, rendered, and ready for interaction.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HSmart_Loaded(object sender, RoutedEventArgs e)
        {
            this.hWindow = this.hSmart?.HalconWindow;
            this.hWindow?.SetPart(0, 0, -2, -2);
            HWindow = hWindow;
        }
    }
}
