# 类图设计文档

## 项目概述
ScreenTimeTracker，记录电脑屏幕使用时间

## 整体架构
```mermaid
graph TD
    subgraph "Presentation Layer"
        direction LR
        ST_WebApi["ScreenTimeTracker.WebApi"]
        ST_Tracker["ScreenTimeTracker.Tracker"]
    end

    subgraph "Application Layer"
        ST_Application["ScreenTimeTracker.Application"]
    end

    subgraph "Domain Layer"
        ST_Domain["ScreenTimeTracker.Domain"]
    end



    subgraph "Infrastructure Layer"
        ST_Infrastructure["ScreenTimeTracker.Infrastructure"]
    end

    subgraph "External"
        direction TB
        ST_Tray["ScreenTimeTracker.Tray (Launcher)"]
        Vue_Frontend["sreentime-tracker-frontend (Vue)"]
    end

    %% Dependencies
    ST_WebApi --> ST_Application
    ST_Tracker --> ST_Application

    %% Host projects need Infrastructure for DI setup
    ST_WebApi --> ST_Infrastructure
    ST_Tracker --> ST_Infrastructure

    ST_Application --> ST_Domain

    %% Infrastructure implements Application's repository interfaces
    ST_Infrastructure --> ST_Application
    ST_Infrastructure --> ST_Domain

    %% External Interactions
    Vue_Frontend -- "HTTP/API Calls" --> ST_WebApi
    ST_Tray -- "Starts Process" --> ST_WebApi
    ST_Tray -- "Starts Process" --> ST_Tracker
```

## 文件结构
```
ScreenTimeTracker/
├── .gitignore
├── README.md
├── ScreenTimeTracker.sln
├── src/
│   ├── ScreenTimeTracker.Application/ 
│   ├── ScreenTimeTracker.Domain/ 
│   ├── ScreenTimeTracker.Infrastructure/     
│   ├── ScreenTimeTracker.Tracker/
│   ├── ScreenTimeTracker.Tray/ 
│   └── ScreenTimeTracker.WebApi/ 
├── frontend/
│   ├── screentime-tracker-frontend/    
│   │   ├── public/
│   │   ├── src/
│   │   ├── vite.config.js
│   └   └── ...
├── docs/
└── tests/
```

## Domain 类图
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
            + string? Category
            + string? ExecutablePath
            + string? IconPath
            + string? Description
            + DateTime LastUpdated
        }
        %% ... 其他实体 ...
    }
    namespace ScreenTimeTracker.Domain.Enums {
         class ProcessRuleType {
            <<enumeration>>
        }
    }
    namespace ScreenTimeTracker.Domain.Interfaces {
        class IUsageRepository {
            <<interface>>
            + GetUsagesAsync(DateOnly start, DateOnly end) Task~IEnumerable~Usage~~
            + AddUsagesAsync(IEnumerable~Usage~ usages) Task
        }
        class IProcessInfoRepository {
            <<interface>>
            + GetByNameAsync(string name) Task~ProcessInfo?~
            + AddOrUpdateAsync(ProcessInfo processInfo) Task
        }
        %% ... 其他仓储接口 ...
    }
```

## Application 类图  
```mermaid 
classDiagram
    direction TB
    namespace ScreenTimeTracker.Application.Services {
        class IUsageService {
            <<interface>>
            + GetUsageReportAsync(DateOnly start, DateOnly end) Task~IEnumerable~UsageDto~~
        }
        class UsageService {
            - IUsageRepository _usageRepo
            + GetUsageReportAsync(DateOnly start, DateOnly end) Task~IEnumerable~UsageDto~~
        }
        %% ... 其他服务接口和实现 ...
    }
    namespace ScreenTimeTracker.Application.DTOs {
        class UsageDto {
            + string ProcessName
            + DateOnly Date
            + int Hour
            + long DurationMs
        }
        class ProcessInfoDto {
             + string Name
             + string? Alias
        }
        %% ... 其他DTOs ...
    }
    %% Dependencies
    UsageService ..|> IUsageService
    UsageService ..> ScreenTimeTracker.Domain.Interfaces.IUsageRepository : 注入
    IUsageService ..> UsageDto
```

## Infrastructure 类图
```mermaid 
classDiagram
    direction TB
    class ScreenTimeDbContext {
        + DbSet~Usage~ Usages
        + DbSet~ProcessInfo~ ProcessInfos
    }
    namespace ScreenTimeTracker.Infrastructure.Repositories {
        class UsageRepository {
            - ScreenTimeDbContext _context
            + GetUsagesAsync(...) Task~IEnumerable~Usage~~
            + AddUsagesAsync(...) Task
        }
        class ProcessInfoRepository {
            - ScreenTimeDbContext _context
            + GetByNameAsync(...) Task~ProcessInfo?~
            + AddOrUpdateAsync(...) Task
        }
    }

    %% Dependencies
    UsageRepository ..|> ScreenTimeTracker.Domain.Interfaces.IUsageRepository : 实现
    ProcessInfoRepository ..|> ScreenTimeTracker.Domain.Interfaces.IProcessInfoRepository : 实现
    UsageRepository ..> ScreenTimeDbContext : 注入
    ProcessInfoRepository ..> ScreenTimeDbContext : 注入
```

## WebApi 类图
```mermaid 
classDiagram
    direction TB
    namespace ScreenTimeTracker.WebApi.Controllers {
        class UsageController {
            - IUsageService _usageService  // 来自Application层
            + GetUsages(DateOnly startDate, DateOnly endDate) Task~IActionResult~
        }
        class ProcessController {
            - IProcessService _processService // 来自Application层
            + GetProcessInfo(string name) Task~IActionResult~
        }
    }

    %% Dependencies
    UsageController ..> ScreenTimeTracker.Application.Services.IUsageService : 注入
    ProcessController ..> ScreenTimeTracker.Application.Services.IProcessService : 注入
```
