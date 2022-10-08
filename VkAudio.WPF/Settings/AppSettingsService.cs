using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using VkAudio.WPF.Helpers;

namespace VkAudio.WPF.Settings
{
    internal class AppSettingsService : IAppSettings
    {
        private readonly ILogger<AppSettingsService> _logger;

        public AppSettings Settings { get; } = new();

        public string Token
        {
            get => Settings.Token;
            set => Settings.Token = value;
        }

        public AppSettingsService(ILogger<AppSettingsService> logger)
        {
            _logger = logger;

            if (File.Exists(Constants.CommonSettingsFilePath))
            {
                // Read existing
                try
                {
                    using var fileStream = new FileStream(Constants.CommonSettingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    Settings = JsonSerializer.Deserialize<AppSettings>(fileStream);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, null);
                }
            }
        }

        public async Task Save()
        {
            // Use local lock instead of interprocess lock - only one instance of app will work with this file
            using var fileStream = new FileStream(Constants.CommonSettingsFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            await JsonSerializer.SerializeAsync(fileStream, Settings).ConfigureAwait(false);
            _logger.LogInformation("Settings saved to file");
        }
    }
}
