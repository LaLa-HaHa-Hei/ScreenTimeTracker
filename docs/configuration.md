## appsettings.json
// 和 /* ... */ 注释不是标准的 JSON 行为，但 .NET 支持这种注释。
```json
{
    "Tray": {
        "WebApiExecutablePath": "./WebApi.exe",
        "TrackerExecutablePath": "./Tracker.exe",
        // 是否在启动时打开UI
        "OpenUIOnStartup": true,
    },
    "WebApi": {
        // 监听地址，若设为 http://0.0.0.0:xxxx ，则可通过局域网访问
        "BaseUrl": "http://localhost:5123",
        "Logger": {
            "LogDirPath": "./Logs/WebApi",
            "LogFileName": "log-{Date:yyyyMMdd}.txt",
            // 日志文件保留数量，超过此数量将自动删除旧文件
            "RetainedFileCountLimit": 7,
        },
    },
    "Tracker": {
        // 获取顶层窗口对应进程的间隔，作为当前正在使用的进程
        "GetTopProcessIntervalMilliseconds": 1000,
        // 获取到进程后若上次更新进程信息时间距当前时间超过此值，则重新获取
        "ProcessInfoStaleThresholdMinutes": 600,
        // 进程图标存储目录
        "ProcessIconDirPath": "./Data/ProcessIcons",
        "Logger": {
            "LogDirPath": "./Logs/Tracker",
            "LogFileName": "log-{Date:yyyyMMdd}.txt",
            "RetainedFileCountLimit": 7,
        },
    },
}
```