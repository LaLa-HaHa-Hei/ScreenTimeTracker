using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class ProcessInfo
    {
        [Key]
        public required string ProcessName { get; set; }
        public string? Alias { get; set; } // 用户自定义别名
        public string? ExecutablePath { get; set; }
        public string? IconPath { get; set; }
        public string? Description { get; set; }
        public DateOnly LastUpdated { get; set; } // 最后一次更新进程信息的日期
    }
}
