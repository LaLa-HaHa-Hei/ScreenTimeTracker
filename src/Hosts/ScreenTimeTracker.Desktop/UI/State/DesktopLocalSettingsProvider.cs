using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScreenTimeTracker.DesktopSettings.Contracts.Enums;
using ScreenTimeTracker.DesktopSettings.Contracts.IntegrationEvents;
using ScreenTimeTracker.DesktopSettings.Contracts.Queries;

namespace ScreenTimeTracker.Desktop.UI.State;

public class DesktopLocalSettingsProviderInitializer(IServiceScopeFactory scopeFactory)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        var desktopLocalSettingsProvider =
            scope.ServiceProvider.GetRequiredService<DesktopLocalSettingsProvider>();

        var result = await mediator.Send(new GetLocalSettingsQuery(), cancellationToken);

        desktopLocalSettingsProvider.Update(
            result.DefaultUIOpenMode,
            result.IsAutoStartEnabled,
            result.IsSilentStartEnabled,
            result.Language
        );
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public interface IDesktopLocalSettingsProvider
{
    UIOpenMode DefaultUIOpenMode { get; }
    bool IsAutoStartEnabled { get; }
    bool IsSilentStartEnabled { get; }
    string Language { get; }

    event EventHandler<SettingChangedEventArgs>? OnSettingChanged;

    void Update(
        UIOpenMode defaultUIOpenMode,
        bool isAutoStartEnabled,
        bool isSilentStartEnabled,
        string language
    );
}

public class SettingChangedEventArgs(string settingName) : EventArgs
{
    public string SettingName { get; } = settingName;
}

public class DesktopLocalSettingsProvider : IDesktopLocalSettingsProvider
{
    public UIOpenMode DefaultUIOpenMode { get; private set; } = UIOpenMode.Window;
    public bool IsAutoStartEnabled { get; private set; }
    public bool IsSilentStartEnabled { get; private set; }
    public string Language { get; private set; } = "en-US";

    public event EventHandler<SettingChangedEventArgs>? OnSettingChanged;

    public void Update(
        UIOpenMode defaultUIOpenMode,
        bool isAutoStartEnabled,
        bool isSilentStartEnabled,
        string language
    )
    {
        if (defaultUIOpenMode != DefaultUIOpenMode)
        {
            DefaultUIOpenMode = defaultUIOpenMode;
            OnSettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(DefaultUIOpenMode)));
        }
        if (isAutoStartEnabled != IsAutoStartEnabled)
        {
            IsAutoStartEnabled = isAutoStartEnabled;
            OnSettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(IsAutoStartEnabled)));
        }
        if (isSilentStartEnabled != IsSilentStartEnabled)
        {
            IsSilentStartEnabled = isSilentStartEnabled;
            OnSettingChanged?.Invoke(
                this,
                new SettingChangedEventArgs(nameof(IsSilentStartEnabled))
            );
        }
        if (language != Language)
        {
            Language = language;
            OnSettingChanged?.Invoke(this, new SettingChangedEventArgs(nameof(Language)));
        }
    }
}

public class DesktopLocalSettingsChangedHandler(
    IDesktopLocalSettingsProvider desktopLocalSettingsProvider
) : INotificationHandler<LocalSettingsUpdatedIntegrationEvent>
{
    public ValueTask Handle(
        LocalSettingsUpdatedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        desktopLocalSettingsProvider.Update(
            notification.DefaultUIOpenMode,
            notification.IsAutoStartEnabled,
            notification.IsSilentStartEnabled,
            notification.Language
        );
        return ValueTask.CompletedTask;
    }
}
