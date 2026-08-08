using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.Services
{
    /// <summary>
    /// 系统设置服务基类，内有数据库操作静态对象
    /// </summary>
    public class SettingServiceBase
    {
        static Lazy<IFreeSql> sqliteLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
            .UseMonitorCommand(cmd => Trace.WriteLine($"Sql: {cmd.CommandText}"))   // 监听SQL语句，Trace在输出选项卡中查看
            .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=SystemSetting.db")
            .UseAutoSyncStructure(true) // 自动同步实体结构在数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
            .Build());

        public static IFreeSql Sqlite => sqliteLazy.Value;
    }
}
