using System.Diagnostics;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;
using WebApi.Options;

namespace WebApi
{
    public class WebApiHost
    {
        private WebApplication? _app;
        private readonly string _absluteDataDirPath = Path.Combine(AppContext.BaseDirectory, Shared.Constants.FilePaths.DataDirPath);
        private readonly string _logDirPath = Path.Combine(Shared.Constants.FilePaths.DataDirPath, "WebApiLogs");
        private readonly string _absluteLogDirPath;

        public WebApiHost()
        {
            _absluteLogDirPath = Path.Combine(AppContext.BaseDirectory, _logDirPath);
        }

        public async Task StartAsync(string[]? args = null)
        {
            PrepareFolders();
            SetupLogger();

            var builder = WebApplication.CreateBuilder(args ?? []);
            builder.WebHost.UseUrls(Shared.Constants.Web.BaseUrl);

            // 允许所有跨域
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Host.UseSerilog();
            //builder.Services.AddControllers(); // 由于会从主程序集寻找，所以作为类库时会找不到任何控制器
            builder.Services
                .AddControllers()
                .AddApplicationPart(typeof(WebApiHost).Assembly); // 显式添加所在程序集

            var dbPath = Path.Combine(AppContext.BaseDirectory, Shared.Constants.FilePaths.DbFilePath);
            builder.Services.AddDbContext<ScreenTimeContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            builder.Services.AddSingleton(new AppOptions
            {
                DataDirPath = Shared.Constants.FilePaths.DataDirPath,
                DataRequestPath = Shared.Constants.Web.DataRequestPath
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            _app = builder.Build();

            _app.UseDefaultFiles();

            _app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(_absluteDataDirPath),
                RequestPath = Shared.Constants.Web.DataRequestPath
            });

            _app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "wwwroot")),
                RequestPath = ""
            });

            _app.UseRouting();
            _app.UseCors();
            _app.MapControllers();
            _app.MapFallbackToFile("index.html");

            Log.Warning("Listening on: {BaseUrl}", Shared.Constants.Web.BaseUrl);

            await _app.StartAsync();
        }

        public async Task StopAsync()
        {
            if (_app != null)
            {
                await _app.StopAsync();
                await _app.DisposeAsync();
            }
        }

        private void PrepareFolders()
        {
            if (!Directory.Exists(_absluteDataDirPath))
                Directory.CreateDirectory(_absluteDataDirPath);

            if (!Directory.Exists(_absluteLogDirPath))
                Directory.CreateDirectory(_absluteLogDirPath);
        }

        private void SetupLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(_absluteLogDirPath, "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .Enrich.FromLogContext()
                .MinimumLevel.Warning()
                .MinimumLevel.Override("WebApi", LogEventLevel.Information)
                .CreateLogger();
        }
    }
}
