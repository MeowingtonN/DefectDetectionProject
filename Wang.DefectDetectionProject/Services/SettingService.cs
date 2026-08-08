using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wang.DefectDetectionProject.Shared.Services;
using Wang.DefectDetectionProject.Shared.Services.Tables;

namespace Wang.DefectDetectionProject.Services
{
    /// <summary>
    /// 系统设置服务类（数据库交互）
    /// </summary>
    public class SettingService : SettingServiceBase, ISettingService
    {
        public async Task<SettingEntity?> GetSettingAsync()
        {
            var setting = await Sqlite.Select<SettingEntity>().FirstAsync();

            if (setting == null)
            {
                await InsertDefaultSettingAsync();

                return await GetSettingAsync();
            }

            return setting;
        }

        public async Task SaveSetting(SettingEntity input)
        {
            var setting = await Sqlite.Select<SettingEntity>().FirstAsync(t => t.Id.Equals(input.Id));
            if (!setting)
            {
                await Sqlite.Insert(input).ExecuteAffrowsAsync();
            }
            else
            {
                await Sqlite.Update<SettingEntity>()
                            .SetDto(input)
                            .Where(a => a.Id == input.Id)
                            .ExecuteAffrowsAsync();
            }
        }

        /// <summary>
        /// 若系统设置数据表中没有数据，就插入一条默认数据
        /// </summary>
        /// <returns></returns>
        private async Task InsertDefaultSettingAsync()
        {
            await Sqlite.Insert(new SettingEntity()
            {
                Language = "zh-CN",
                // 主题和颜色。。。
                //SkinName = "Light",
            }).ExecuteAffrowsAsync();
        }
    }
}
