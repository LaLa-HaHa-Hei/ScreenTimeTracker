namespace ScreenTimeTracker.WebApi.DTOs.Processes
{
    public class UpdateProcessRequest
    {
        public string? Alias { get; set; }
        public bool AutoUpdate { get; set; }
        public string? IconPath { get; set; }
    }
}
