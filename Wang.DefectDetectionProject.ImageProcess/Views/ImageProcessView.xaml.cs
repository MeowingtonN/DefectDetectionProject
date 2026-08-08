using Nodify;
using System.Windows.Controls;
using System.Windows.Input;
using Wang.DefectDetectionProject.ImageProcess.ViewModels;

namespace Wang.DefectDetectionProject.ImageProcess.Views
{
    /// <summary>
    /// ImageProcessView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageProcessView : UserControl
    {
        public ImageProcessView()
        {
            InitializeComponent();
        }

        private void Editor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Delete || e.Key == Key.Back) && sender is NodifyEditor editor)
            {
                // 通过 TemplatedParent 获取 EditorViewModel
                var editorVM = (editor.TemplatedParent as ContentPresenter)?.Content as EditorViewModel;
                if (editorVM == null)
                {
                    // 如果上面取不到，再尝试从 DataContext 中获取（万一它确实是 EditorViewModel）
                    editorVM = editor.DataContext as EditorViewModel;
                }

                if (editorVM == null) return;

                var selectedNodes = editorVM.Nodes.Where(n => n.IsSelected && n.CanDelete).ToList();

                // 获取选中的节点
                if (selectedNodes != null && selectedNodes.Count > 0)
                {
                    foreach (var node in selectedNodes)
                    {
                        editorVM?.DeleteSelectedNodeCommand.Execute(node);
                    }
                    e.Handled = true;
                }
                else if (editor.SelectedItem is NodeViewModel selectedNode)
                {
                    if (selectedNode.CanDelete)
                    {
                        editorVM?.DeleteSelectedNodeCommand.Execute(selectedNode);
                    }
                    e.Handled = true; // 阻止 Delete 键继续传递，避免副作用
                }
            }
        }
    }
}
