using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wang.DefectDetectionProject.Shared.Services.Tables;

namespace Wang.DefectDetectionProject.Shared.Services
{
    /// <summary>
    /// 系统设置服务接口
    /// </summary>
    public interface ISettingService
    {
        /// <summary>
        /// 读取表项
        /// </summary>
        /// <returns></returns>
        Task<SettingEntity?> GetSettingAsync();

        /// <summary>
        /// 更新表项
        /// </summary>
        /// <returns></returns>
        Task SaveSetting(SettingEntity setting);
    }
}
