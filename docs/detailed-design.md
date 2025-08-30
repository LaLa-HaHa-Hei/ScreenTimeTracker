## Domain
```mermaid 
classDiagram
    direction TB
    namespace ScreenTimeTracker.Domain.Entities {
        class Usage {
            + string ProcessName
            + DateOnly Date
            + int Hour
            + long DurationMs
        }
        class ProcessInfo {
            + string Name
            + string? Alias
            + string? CategoryName
            + string? ExecutablePath
            + string? IconPath
            + string? Description
            + DateTime LastUpdated
        }
        class ProcessRule {
            + int Id
            + string ProcessName
            + ProcessRuleType RuleType
            + string? TimeLimitJson
            + int? AllowedMinutes
        }
        class Category {
            + string Name
        }
        class Setting {
             + string Key
             + string Value
        }
    }
    ProcessInfo ..> Category : FK_CategoryName
    ProcessRule ..> ProcessInfo : FK_ProcessName

    namespace ScreenTimeTracker.Domain.Enums {
         class ProcessRuleType {
            <<enumeration>>
            Whitelist
            DailyTimeLimit
            ScheduleLimit
        }
    }

    namespace ScreenTimeTracker.Domain.Interfaces {
        class IUsageRepository {
            <<interface>>
            + GetUsagesAsync(DateOnly start, DateOnly end) Task~IEnumerable~Usage~~
            + AddOrUpdateUsagesAsync(IEnumerable~Usage~ usages) Task
        }
        class IProcessInfoRepository {
            <<interface>>
            + GetByNameAsync(string name) Task~ProcessInfo?~
            + AddOrUpdateAsync(ProcessInfo processInfo) Task
        }
        class IProcessRuleRepository {
            <<interface>>
            + GetRuleForProcessAsync(string processName) Task~ProcessRule?~
            + GetAllRulesAsync() Task~IEnumerable~ProcessRule~~
            %% ... 其他CRUD方法 ...
        }
    }
```

## Application  
```mermaid 
classDiagram
    direction TB
    namespace ScreenTimeTracker.Application.Services {
        class IUsageTrackingService {
            <<interface>>
            + TrackAsync(string processName, string? exePath, TimeSpan duration) Task
        }
        class UsageTrackingService {
            - IUsageRepository _usageRepo
            - IProcessRuleRepository _ruleRepo
            - IUserNotificationService _notificationSvc
            + TrackAsync(string processName, string? exePath, TimeSpan duration) Task
        }
    }
        note for UsageTrackingService "实现了核心跟踪逻辑:<br/>1. 检查白名单规则<br/>2. 记录使用时长<br/>3. 检查时间限制并触发通知"

    namespace ScreenTimeTracker.Application.DTOs {
        class UsageDto {
            + string ProcessName
            + long TotalDurationMs
        }
        class ProcessInfoDto {
             + string Name
             + string? Alias
             + string? CategoryName
        }
    }

    namespace ScreenTimeTracker.Application.Interfaces {
        class IUserNotificationService {
            <<interface>>
            + ShowTimeLimitWarningAsync(string processName, TimeSpan timeRemaining) Task
        }
    }
        note for IUserNotificationService "依赖倒置的范例：<br/>由外层(Tracker)实现此接口,<br/>供本层(Application)调用。"

    %% Dependencies
    UsageTrackingService ..|> IUsageTrackingService
    UsageTrackingService ..> ScreenTimeTracker.Domain.Interfaces.IUsageRepository : 注入
    UsageTrackingService ..> ScreenTimeTracker.Domain.Interfaces.IProcessRuleRepository : 注入
    UsageTrackingService ..> IUserNotificationService : 注入
```

## Infrastructure
```mermaid 
classDiagram
    direction TB
    class ScreenTimeDbContext {
        + DbSet~Usage~ Usages
        + DbSet~ProcessInfo~ ProcessInfos
        + DbSet~ProcessRule~ ProcessRules
        # OnConfiguring(DbContextOptionsBuilder options) void
    }
    note for ScreenTimeDbContext "使用 Entity Framework Core。<br/>连接字符串在表现层配置并注入。"

    namespace ScreenTimeTracker.Infrastructure.Repositories {
        class UsageRepository {
            - ScreenTimeDbContext _context
            + GetUsagesAsync(DateOnly start, DateOnly end) Task~IEnumerable~Usage~~
            + AddOrUpdateUsagesAsync(IEnumerable~Usage~ usages) Task
        }
        class ProcessInfoRepository {
            - ScreenTimeDbContext _context
            + GetByNameAsync(string name) Task~ProcessInfo?~
            + AddOrUpdateAsync(ProcessInfo processInfo) Task
        }
    }

    %% Dependencies
    UsageRepository ..|> ScreenTimeTracker.Domain.Interfaces.IUsageRepository : 实现
    ProcessInfoRepository ..|> ScreenTimeTracker.Domain.Interfaces.IProcessInfoRepository : 实现
    UsageRepository ..> ScreenTimeDbContext : 注入
    ProcessInfoRepository ..> ScreenTimeDbContext : 注入
```

## WebApi
保持轻量。  
运行后调整工作目录为程序所在目录。  
```mermaid 
classDiagram
    direction TB
    class Program {
        + static Main(string[] args) void
    }
    note for Program "配置依赖注入(DI),<br/>中间件管道(Middleware),<br/>和API路由。"

    namespace ScreenTimeTracker.WebApi.Controllers {
        class UsageController {
            - IUsageQueryService _usageQueryService  // 来自Application层
            + GetUsages(DateOnly startDate, DateOnly endDate) Task~IActionResult~
        }
        class ProcessRuleController {
            - IProcessRuleService _ruleService // 来自Application层
            + CreateRule(CreateRuleDto dto) Task~IActionResult~
        }
    }
    note for UsageController "控制器不包含业务逻辑，<br/>仅负责模型验证、调用服务和返回HTTP响应。"

    %% Dependencies
    UsageController ..> ScreenTimeTracker.Application.Services.IUsageQueryService : 注入
    ProcessRuleController ..> ScreenTimeTracker.Application.Services.IProcessRuleService : 注入
```

## Tracker
保持轻量。  
运行后调整工作目录为程序所在目录。  
每隔一小段时间获取一次顶层窗口的进程名作为正在使用的程序。  
每隔几次获取顶层窗口的进程名，写入到数据库，防止频繁调用数据库导致性能问题。  
暂时将限制用户指定的程序功能放入，后期相关逻辑较复杂时再考虑拆分为独立的ScreenTimeTracker.Enforcer.exe。  
```mermaid 
classDiagram
    direction TB
    namespace ScreenTimeTracker.Tracker {
        class Program {
            + static Main(string[] args) void
        }

        class TrackingWorker {
            <<BackgroundService>>
            - readonly IServiceScopeFactory _scopeFactory
            - readonly IForegroundWindowService _foregroundWindowService
            # override ExecuteAsync(CancellationToken stoppingToken) Task
            - TrackCurrentProcess(object? state) void
        }
    }
        note for Program "配置依赖注入(DI),<br/>并将TrackingWorker注册为托管服务。"
        note for TrackingWorker "使用IServiceScopeFactory<br/>为每次跟踪操作创建新的依赖注入作用域。"

    namespace ScreenTimeTracker.Tracker.Services {
        class IForegroundWindowService { <<interface>> + GetForegroundProcess() Process? }
        class WindowsForegroundWindowService { + GetForegroundProcess() Process? }
        class WindowsToastNotificationService {
            + ShowTimeLimitWarningAsync(string processName, TimeSpan timeRemaining) Task
        }
    }

    %% --- 外部依赖 (来自其他项目) ---
    namespace ScreenTimeTracker.Application.Services {
        class IUsageTrackingService { <<interface>> }
    }
    namespace ScreenTimeTracker.Application.Interfaces {
        class IUserNotificationService { <<interface>> }
    }

    %% --- 依赖关系 ---
    Program ..> TrackingWorker : 注册并启动

    TrackingWorker ..> IForegroundWindowService : 注入并使用
    TrackingWorker ..> IServiceScopeFactory : 注入
    TrackingWorker ..> ScreenTimeTracker.Application.Services.IUsageTrackingService : (在Scope内)调用

    WindowsForegroundWindowService ..|> IForegroundWindowService : 实现
    WindowsToastNotificationService ..|> ScreenTimeTracker.Application.Interfaces.IUserNotificationService : 实现
```

## Tray
运行后调整工作目录为程序所在目录。  
```mermaid 
classDiagram
    direction TB
    namespace ScreenTimeTracker.Tray.ViewModels {
        class TaskbarIconViewModel {
        }
    }
```