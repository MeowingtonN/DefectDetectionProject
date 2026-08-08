using FreeSql.DataAnnotations;

namespace Wang.DefectDetectionProject.Shared.Services.Tables
{
    /// <summary>
    /// 数据表实体基类
    /// </summary>
    public class EntityBase
    {
        /// <summary>
        /// ID，自增，主键
        /// </summary>
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
    }
}
