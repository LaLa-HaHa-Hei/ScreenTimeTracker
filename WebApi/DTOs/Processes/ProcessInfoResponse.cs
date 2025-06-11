namespace WebApi.DTOs.Processes
{
    public class ProcessInfoResponse
    {
        public required string ProcessName { get; set; }
        public string? Alias { get; set; } // 用户自定义别名
        public string? ExecutablePath { get; set; }
        public string? IconPath { get; set; }
        public string? Description { get; set; }
    }
}
