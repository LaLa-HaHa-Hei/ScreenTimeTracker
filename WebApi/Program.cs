using System.Diagnostics;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;
using WebApi.Options;

namespace WebApi
{
    public class Program
    {
        private static Mutex? _mutex;
        private const string LogDirName = "WebApiLogs";
        private static readonly string DataDirPath = Path.Combine(AppContext.BaseDirectory, Shared.Constants.FilePaths.DataDirPath);
        private static readonly string LogDirPath = Path.Combine(DataDirPath, LogDirName);

        public static void Main(string[] args)
        {
            if (!EnsureSingleInstance())
                return;

            {// 创建文件夹
                if (!Directory.Exists(DataDirPath))
                    Directory.CreateDirectory(DataDirPath);
                if (!Directory.Exists(LogDirPath))
                    Directory.CreateDirectory(LogDirPath);
            }

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(LogDirPath, "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .Enrich.FromLogContext()
                .MinimumLevel.Warning()
                .MinimumLevel.Override("WebApi", LogEventLevel.Information)
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls(Shared.Constants.Web.BaseUrl); // 设置监听地址和端口

            // 允许跨域请求
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Add services to the container.

            builder.Host.UseSerilog();
            builder.Services.AddControllers();

            var dbPath = Path.Combine(DataDirPath, Shared.Constants.FilePaths.DbFileName);
            builder.Services.AddDbContext<ScreenTimeContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            builder.Services.AddSingleton(new AppOptions()
            {
                DataDirPath = DataDirPath,
                DataRequestPath = Shared.Constants.Web.DataRequestPath
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                Process.Start(new ProcessStartInfo("cmd", $"/c start {Shared.Constants.Web.BaseUrl}/swagger/index.html") { CreateNoWindow = true });
            }

            // wwwroot为html目录
            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "wwwroot")),
                RequestPath = ""
            });
            // 公开 DataDirPath 目录下的文件，访问路径为 /{DataDirPath}
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(DataDirPath),
                RequestPath = Shared.Constants.Web.DataRequestPath
            });

            app.UseRouting();

            app.UseCors();

            app.MapControllers();

            Log.Warning("Data directory: {DataDirPath}", DataDirPath);
            Log.Warning("Listening on: {BaseUrl}", Shared.Constants.Web.BaseUrl);
            app.Run();

            _mutex?.ReleaseMutex();
        }

        private static bool EnsureSingleInstance()
        {
            _mutex = new Mutex(true, "ScreenTimeWebApiUniqueMutexName", out bool createdNew);
            if (!createdNew)
            {
                return false;
            }
            return true;
        }
    }
}
