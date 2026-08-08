using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.ImageProcess.ViewModels
{
    // 连接视图模型，指定起点和终点连接器
    public class ConnectionViewModel : BindableBase
    {
        private ConnectorViewModel source = null!;
        public ConnectorViewModel Source
        {
            get { return source; }
            set { source = value; source.IsConnected = true; RaisePropertyChanged(); }
        }

        private ConnectorViewModel target = null!;
        public ConnectorViewModel Target
        {
            get { return target; }
            set { target = value; target.IsConnected = true; RaisePropertyChanged(); }
        }
    }
}
