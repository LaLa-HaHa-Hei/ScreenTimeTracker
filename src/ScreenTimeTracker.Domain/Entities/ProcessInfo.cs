namespace ScreenTimeTracker.Domain.Entities
{
    public class ProcessInfo(Guid id, string name)
    {
        public Guid Id { get; init; } = id;
        public string Name { get; init; } = name;
        public string? Alias { get; private set; }
        public bool AutoUpdate { get; private set; } = true;
        // 最后一次自动更新进程信息的日期
        public DateTime LastAutoUpdated { get; private set; } = DateTime.Now;
        // 自动更新开启后，将更新下面信息，否则不会
        public string? ExecutablePath { get; private set; }
        public string? IconPath { get; private set; }
        public string? Description { get; private set; }

        public static ProcessInfo Reconstitute(Guid id, string name, string? alias, bool autoUpdate,
            DateTime lastAutoUpdated, string? executablePath, string? iconPath, string? description)
        {
            return new ProcessInfo(id, name)
            {
                Alias = alias,
                AutoUpdate = autoUpdate,
                LastAutoUpdated = lastAutoUpdated,
                ExecutablePath = executablePath,
                IconPath = iconPath,
                Description = description,
            };
        }

        public void UpdateUserDetails(string? alias, bool autoUpdate, string? iconPath)
        {
            Alias = alias;
            AutoUpdate = autoUpdate;
            IconPath = iconPath;
        }

        public void UpdateSystemDetails(string? executablePath, string? iconPath, string? description)
        {
            if (!AutoUpdate) return;

            ExecutablePath = executablePath;
            IconPath = iconPath;
            Description = description;

            LastAutoUpdated = DateTime.Now;
        }

        public static readonly Guid UnknownProcessId = new("00000000-0000-0000-0000-000000000001");
        public static readonly string UnknownProcessName = "Unknown";

        public static readonly Guid IdleProcessId = new("00000000-0000-0000-0000-000000000002");
        public static readonly string IdleProcessName = "Idle";
    }
}
