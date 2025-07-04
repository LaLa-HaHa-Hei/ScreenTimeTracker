﻿using System.Drawing;
using Microsoft.Extensions.Logging;
using Tracker.Options;
using Tracker.Services.Interfaces;

namespace Tracker.Services
{
    internal class IconService : IIconService
    {
        private readonly string _iconDirPath;
        private readonly ILogger<IconService> _logger;

        public IconService(AppOptions recorderOptions,
            ILogger<IconService> logger)
        {
            _iconDirPath = recorderOptions.IconDirPath;
            string _absluteIconDirPath = Path.Combine(AppContext.BaseDirectory, _iconDirPath);
            _logger = logger;
            if (!Directory.Exists(_absluteIconDirPath))
                Directory.CreateDirectory(_absluteIconDirPath);
        }

        public string? SaveIcon(string exePath, string processName)
        {
            string safeProcessName = string.Concat(processName.Split(Path.GetInvalidFileNameChars()));
            string filePath = Path.Combine(_iconDirPath, safeProcessName + ".png");
            try
            {
                using Icon icon = Icon.ExtractAssociatedIcon(exePath) ?? throw new Exception("No icon");
                using Bitmap bmp = icon.ToBitmap();
                bmp.Save(Path.Combine(AppContext.BaseDirectory, filePath), System.Drawing.Imaging.ImageFormat.Png);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get icon from exe **{ExePath}**", exePath);
                return null;
            }
        }
    }
}
