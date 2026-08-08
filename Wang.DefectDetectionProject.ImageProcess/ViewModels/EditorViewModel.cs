using Nodify;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.ImageProcess.ViewModels
{
    // 编辑器整体视图模型，包含所有节点和连线
    public class EditorViewModel : BindableBase
    {
        public EditorViewModel()
        {
            CreateConnectionCommand = new DelegateCommand<object>(parameter =>
            {
                var (source, target) = ((object, object))parameter;

                if (target == null) return;
                if (((ConnectorViewModel)source).IsConnected == true || ((ConnectorViewModel)target).IsConnected == true) return;

                if (((ConnectorViewModel)source).IsInput == false && ((ConnectorViewModel)target).IsInput == true)
                {
                    var connection = new ConnectionViewModel
                    {
                        Source = (ConnectorViewModel)source,
                        Target = (ConnectorViewModel)target
                    };
                    Connections.Add(connection);
                }
            });

            DisconnectConnectorCommand = new DelegateCommand<ConnectorViewModel>(connector =>
            {
                var connection = Connections.First(x => x.Source == connector || x.Target == connector);
                connection.Source.IsConnected = false;
                connection.Target.IsConnected = false;
                Connections.Remove(connection);
            });

            DeleteSelectedNodeCommand = new DelegateCommand<NodeViewModel>(node =>
            {
                if (node == null) return;

                // 找出所有相关连接：该节点的输入或输出连接器是任一连接的 Source 或 Target
                var relatedConnections = Connections
                    .Where(c => node.InputConnectors.Contains(c.Source) ||
                                node.InputConnectors.Contains(c.Target) ||
                                node.OutputConnectors.Contains(c.Source) ||
                                node.OutputConnectors.Contains(c.Target))
                    .ToList();

                foreach (var connection in relatedConnections)
                {
                    // 恢复连接器的未连接状态
                    connection.Source.IsConnected = false;
                    connection.Target.IsConnected = false;
                    // 移除连接
                    Connections.Remove(connection);
                }

                // 移除节点
                Nodes.Remove(node);
            });
        }

        /// <summary>
        /// 遍历的开始结点
        /// </summary>
        private NodeViewModel? startNode;
        /// <summary>
        /// 遍历的开始结点
        /// </summary>
        public NodeViewModel? StartNode
        {
            get { return startNode; }
            set { startNode = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<NodeViewModel> nodes = new ObservableCollection<NodeViewModel>();
        public ObservableCollection<NodeViewModel> Nodes
        {
            get { return nodes; }
            set { nodes = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ConnectionViewModel> connections = new ObservableCollection<ConnectionViewModel>();
        public ObservableCollection<ConnectionViewModel> Connections 
        {
            get { return connections; }
            set
            {
                connections = value; 
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 创建连接命令
        /// </summary>
        public DelegateCommand<object> CreateConnectionCommand { get; }

        /// <summary>
        /// 连接器断开连接命令(按住ALT键+鼠标左键点击连接器以触发断开连接)
        /// </summary>
        public DelegateCommand<ConnectorViewModel> DisconnectConnectorCommand { get; }

        /// <summary>
        /// 删除结点命令
        /// </summary>
        public DelegateCommand<NodeViewModel> DeleteSelectedNodeCommand { get; }
    }
}
