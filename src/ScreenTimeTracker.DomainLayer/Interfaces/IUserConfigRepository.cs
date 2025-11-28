using ScreenTimeTracker.DomainLayer.Entities;

namespace ScreenTimeTracker.DomainLayer.Interfaces;

public interface IUserConfigurationRepository
{
    Task<UserConfiguration> GetConfig();
    Task SaveConfig(UserConfiguration config);
}
