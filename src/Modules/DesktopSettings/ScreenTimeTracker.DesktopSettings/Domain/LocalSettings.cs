using ScreenTimeTracker.BuildingBlocks.Domain;
using ScreenTimeTracker.BuildingBlocks.Types;
using ScreenTimeTracker.DesktopSettings.Contracts.Enums;
using ScreenTimeTracker.DesktopSettings.Domain.Events;

namespace ScreenTimeTracker.DesktopSettings.Domain;

public class LocalSettings : AggregateRoot
{
    public static readonly Guid DefaultId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static readonly IReadOnlySet<string> SupportedLanguages = new HashSet<string>()
    {
        "en-US",
        "zh-CN",
    };

    public UIOpenMode DefaultUIOpenMode { get; private set; }
    public bool IsAutoStartEnabled { get; private set; }
    public bool IsSilentStartEnabled { get; private set; }
    public string Language { get; private set; }

    private LocalSettings(
        Guid id,
        UIOpenMode defaultUIOpenMode,
        bool isAutoStartEnabled,
        bool isSilentStartEnabled,
        string language
    )
        : base(id)
    {
        DefaultUIOpenMode = defaultUIOpenMode;
        IsAutoStartEnabled = isAutoStartEnabled;
        IsSilentStartEnabled = isSilentStartEnabled;
        Language = language;
    }

    public static LocalSettings CreateDefault() =>
        new(DefaultId, UIOpenMode.Window, false, false, "en-US");

    public void Update(
        OptionalValue<UIOpenMode> defaultUIOpenMode = default,
        OptionalValue<bool> isAutoStartEnabled = default,
        OptionalValue<bool> isSilentStartEnabled = default,
        OptionalValue<string> language = default
    )
    {
        if (language.HasValue && !SupportedLanguages.Contains(language.Value))
            throw new ArgumentException(
                $"Unsupported language: {language.Value}",
                nameof(language)
            );

        if (defaultUIOpenMode.HasValue)
            DefaultUIOpenMode = defaultUIOpenMode.Value;
        if (isAutoStartEnabled.HasValue)
            IsAutoStartEnabled = isAutoStartEnabled.Value;
        if (isSilentStartEnabled.HasValue)
            IsSilentStartEnabled = isSilentStartEnabled.Value;
        if (language.HasValue)
            Language = language.Value;

        AddDomainEvent(
            new LocalSettingsUpdatedDomainEvent(
                DefaultUIOpenMode,
                IsAutoStartEnabled,
                IsSilentStartEnabled,
                Language
            )
        );
    }
}
