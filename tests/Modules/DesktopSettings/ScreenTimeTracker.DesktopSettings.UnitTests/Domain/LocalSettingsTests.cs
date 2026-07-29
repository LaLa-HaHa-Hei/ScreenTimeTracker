using ScreenTimeTracker.DesktopSettings.Contracts.Enums;
using ScreenTimeTracker.DesktopSettings.Domain;
using ScreenTimeTracker.DesktopSettings.Domain.Events;
using Shouldly;

namespace ScreenTimeTracker.DesktopSettings.UnitTests.Domain;

public class LocalSettingsTests
{
    [Fact]
    public void CreateDefault_ShouldInitializeWithDefaultValues()
    {
        // Act
        var settings = LocalSettings.CreateDefault();

        // Assert
        settings.Id.ShouldBe(LocalSettings.DefaultId);
        settings.DefaultUIOpenMode.ShouldBe(UIOpenMode.Window);
        settings.IsAutoStartEnabled.ShouldBeFalse();
        settings.IsSilentStartEnabled.ShouldBeFalse();
        settings.Language.ShouldBe("en-US");
        settings.DomainEvents.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("zh-CN")]
    public void Update_WithValidSupportedLanguage_ShouldUpdateLanguageAndAddDomainEvent(
        string validLanguage
    )
    {
        // Arrange
        var settings = LocalSettings.CreateDefault();

        // Act
        settings.Update(language: validLanguage);

        // Assert
        settings.Language.ShouldBe(validLanguage);

        // 验证领域事件
        var domainEvent = settings
            .DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<LocalSettingsUpdatedDomainEvent>();

        domainEvent.Language.ShouldBe(validLanguage);
        domainEvent.DefaultUIOpenMode.ShouldBe(settings.DefaultUIOpenMode);
        domainEvent.IsAutoStartEnabled.ShouldBe(settings.IsAutoStartEnabled);
        domainEvent.IsSilentStartEnabled.ShouldBe(settings.IsSilentStartEnabled);
    }

    [Theory]
    [InlineData("fr-FR")]
    [InlineData("ja-JP")]
    [InlineData("invalid-lang")]
    public void Update_WithUnsupportedLanguage_ShouldThrowArgumentException(
        string unsupportedLanguage
    )
    {
        // Arrange
        var settings = LocalSettings.CreateDefault();

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            settings.Update(language: unsupportedLanguage)
        );

        exception.ParamName.ShouldBe("language");
        exception.Message.ShouldContain($"Unsupported language: {unsupportedLanguage}");
    }

    [Fact]
    public void Update_WithPartialParameters_ShouldOnlyUpdateProvidedFields()
    {
        // Arrange
        var settings = LocalSettings.CreateDefault();
        var newAutoStart = true;

        // Act - 仅更新 IsAutoStartEnabled
        settings.Update(isAutoStartEnabled: newAutoStart);

        // Assert
        settings.IsAutoStartEnabled.ShouldBeTrue();
        // 未提供的参数应保持默认值
        settings.DefaultUIOpenMode.ShouldBe(UIOpenMode.Window);
        settings.IsSilentStartEnabled.ShouldBeFalse();
        settings.Language.ShouldBe("en-US");

        // 验证发布的领域事件
        var domainEvent = settings
            .DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<LocalSettingsUpdatedDomainEvent>();

        domainEvent.IsAutoStartEnabled.ShouldBeTrue();
        domainEvent.DefaultUIOpenMode.ShouldBe(UIOpenMode.Window);
    }

    [Fact]
    public void Update_WithAllParameters_ShouldUpdateAllPropertiesAndAddDomainEvent()
    {
        // Arrange
        var settings = LocalSettings.CreateDefault();
        const UIOpenMode newMode = UIOpenMode.Browser;
        const bool newAutoStart = true;
        const bool newSilentStart = true;
        const string newLanguage = "zh-CN";

        // Act
        settings.Update(
            defaultUIOpenMode: newMode,
            isAutoStartEnabled: newAutoStart,
            isSilentStartEnabled: newSilentStart,
            language: newLanguage
        );

        // Assert
        settings.DefaultUIOpenMode.ShouldBe(newMode);
        settings.IsAutoStartEnabled.ShouldBe(newAutoStart);
        settings.IsSilentStartEnabled.ShouldBe(newSilentStart);
        settings.Language.ShouldBe(newLanguage);

        var domainEvent = settings
            .DomainEvents.ShouldHaveSingleItem()
            .ShouldBeOfType<LocalSettingsUpdatedDomainEvent>();

        domainEvent.DefaultUIOpenMode.ShouldBe(newMode);
        domainEvent.IsAutoStartEnabled.ShouldBe(newAutoStart);
        domainEvent.IsSilentStartEnabled.ShouldBe(newSilentStart);
        domainEvent.Language.ShouldBe(newLanguage);
    }

    [Fact]
    public void Update_WithoutAnyChanges_ShouldNotRaiseDomainEvent()
    {
        // Arrange
        var settings = LocalSettings.CreateDefault();

        // Act - 不传任何参数，或者传入与当前值完全相同的值
        settings.Update(defaultUIOpenMode: UIOpenMode.Window, language: "en-US");

        // Assert - 状态未发生改变，不应产生领域事件
        settings.DomainEvents.ShouldBeEmpty();
    }
}
