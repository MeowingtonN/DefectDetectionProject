using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wang.DefectDetectionProject.ImageProcess.ViewModels
{
    // 连接器视图模型（输入端/输出端）
    public class ConnectorViewModel : BindableBase
    {
        /// <summary>
        /// 连接器标题
        /// </summary>
        private string? title;
        /// <summary>
        /// 连接器标题
        /// </summary>
        public string? Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 连接器标题的资源键
        /// </summary>
        private string? titleKey;
        /// <summary>
        /// 连接器标题的资源键
        /// </summary>
        public string? TitleKey
        {
            get { return titleKey; }
            set { titleKey = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// true=输入，false=输出
        /// </summary>
        private bool isInput;
        /// <summary>
        /// true=输入，false=输出
        /// </summary>
        public bool IsInput
        {
            get { return isInput; }
            set { isInput = value; RaisePropertyChanged(); }
        }

        private Point _anchor;
        public Point Anchor
        {
            get => _anchor;
            set
            {
                _anchor = value;
                RaisePropertyChanged();
            }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set 
            {
                _isConnected = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 所属结点
        /// </summary>
        private NodeViewModel? ownerNode;
        /// <summary>
        /// 所属结点
        /// </summary>
        public NodeViewModel? OwnerNode
        {
            get { return ownerNode; }
            set { ownerNode = value; RaisePropertyChanged(); }
        }
    }
}
