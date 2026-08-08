using Nodify;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wang.DefectDetectionProject.Core.ImageProcess.Enums;

namespace Wang.DefectDetectionProject.ImageProcess.ViewModels
{
    // 节点视图模型，实现 INodifyCanvasItem 以提供位置和大小
    public class NodeViewModel : BindableBase, INodifyCanvasItem
    {
        /// <summary>
        /// 节点标题
        /// </summary>
        private string? title;
        /// <summary>
        /// 节点标题
        /// </summary>
        public string? Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 节点标题的资源键
        /// </summary>
        private string? titleKey;
        /// <summary>
        /// 节点标题的资源键
        /// </summary>
        public string? TitleKey
        {
            get { return titleKey; }
            set {  titleKey = value; RaisePropertyChanged(); }
        }

        private Point location = new Point(0, 0);
        public Point Location
        {
            get { return location; }
            set { location = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ConnectorViewModel> inputConnectors = new ObservableCollection<ConnectorViewModel>();
        public ObservableCollection<ConnectorViewModel> InputConnectors
        {
            get { return inputConnectors; }
            set
            {
                inputConnectors = value; 
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ConnectorViewModel> outputConnectors = new ObservableCollection<ConnectorViewModel>();
        public ObservableCollection<ConnectorViewModel> OutputConnectors
        {
            get { return outputConnectors; }
            set
            {
                outputConnectors = value;
                RaisePropertyChanged();
            }
        }

        private Size desiredSize = new Size(120, 60);
        public Size DesiredSize
        {
            get { return desiredSize; }
            set
            {
                desiredSize = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 该节点所代表的图像处理算子
        /// </summary>
        private ImgProcessOperator imgProcessOperator;
        /// <summary>
        /// 该节点所代表的图像处理算子
        /// </summary>
        public ImgProcessOperator ImageProcessOperator
        {
            get { return imgProcessOperator; }
            set { imgProcessOperator = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 该节点是否能被删除
        /// </summary>
        private bool canDelete = true;
        /// <summary>
        /// 该节点是否能被删除
        /// </summary>
        public bool CanDelete
        {
            get { return canDelete; }
            set { canDelete = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 该节点是否被选中
        /// </summary>
        private bool _isSelected = false;
        /// <summary>
        /// 该节点是否被选中
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 节点内的下拉选项集合
        /// </summary>
        private ObservableCollection<ComboOptionViewModel> _comboOptions = new ObservableCollection<ComboOptionViewModel>();
        /// <summary>
        /// 节点内的下拉选项集合
        /// </summary>
        public ObservableCollection<ComboOptionViewModel> ComboOptions
        {
            get { return _comboOptions; }
            set { _comboOptions = value; RaisePropertyChanged(); }
        }

        public void Arrange(Rect rect)
        {
            
        }
    }
}
