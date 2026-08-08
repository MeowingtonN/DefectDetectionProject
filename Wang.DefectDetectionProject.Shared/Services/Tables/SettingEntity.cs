using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.Shared.Services.Tables
{
    /// <summary>
    /// 系统设置实体类
    /// </summary>
    public class SettingEntity : EntityBase
    {
        /// <summary>
        /// 当前语言
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// 当前主题
        /// </summary>
        public string? SkinName { get; set; }

        /// <summary>
        /// 主题颜色
        /// </summary>
        public string? SkinColor { get; set; }
    }
}
