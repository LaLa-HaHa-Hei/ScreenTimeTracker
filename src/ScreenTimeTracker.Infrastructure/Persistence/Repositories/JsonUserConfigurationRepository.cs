using Microsoft.Extensions.Options;
using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.DomainLayer.Interfaces;
using ScreenTimeTracker.Infrastructure.Persistence.Mappers;
using ScreenTimeTracker.Infrastructure.Persistence.Models;
using ScreenTimeTracker.Infrastructure.Persistence.Options;
using System.Text.Json;

namespace ScreenTimeTracker.Infrastructure.Persistence.Repositories
{
    public class JsonUserConfigurationRepository(IOptions<UserConfigStorageOptions> options) : IUserConfigurationRepository
    {
        private readonly IOptions<UserConfigStorageOptions> _options = options;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
        };

        public async Task<UserConfiguration> GetConfig()
        {
            if (!File.Exists(_options.Value.FilePath))
            {
                return UserConfiguration.Default;
            }

            var json = await File.ReadAllTextAsync(_options.Value.FilePath);
            var entity = JsonSerializer.Deserialize<UserConfigEntity>(json, _jsonSerializerOptions);
            return entity is null ? UserConfiguration.Default : UserConfigMapper.Map(entity);
        }

        public async Task SaveConfig(UserConfiguration config)
        {
            var json = JsonSerializer.Serialize(UserConfigMapper.Map(config), _jsonSerializerOptions);
            await File.WriteAllTextAsync(_options.Value.FilePath, json);
        }
    }
}
