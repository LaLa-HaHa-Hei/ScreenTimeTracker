using ScreenTimeTracker.ScreenTime.Domain;
using Shouldly;

namespace ScreenTimeTracker.ScreenTime.UnitTests.Domain;

public class UserSettingsTests
{
    #region CreateDefault 默认创建测试

    [Fact]
    public void CreateDefault_ShouldInitializeWithExpectedDefaults()
    {
        // Act
        var settings = UserSettings.CreateDefault();

        // Assert (使用 Shouldly 进行流式断言)
        settings.Id.ShouldBe(UserSettings.DefaultId);
        settings.AppIconDirectory.ShouldBe("./Data/AppIcons");
        settings.AppMetadataStaleThreshold.ShouldBe(TimeSpan.FromHours(24));
        settings.ActiveAppUsageSessionAutoSaveInterval.ShouldBe(TimeSpan.FromSeconds(15));
        settings.IsIdleDetectionEnabled.ShouldBeFalse();
        settings.IdleThreshold.ShouldBe(TimeSpan.FromMinutes(10));
        settings.IdleDetectionPollingInterval.ShouldBe(TimeSpan.FromSeconds(10));
        settings.MinValidAppUsageSessionDuration.ShouldBe(TimeSpan.FromSeconds(5));
        settings.AppUsageSessionMergeTolerance.ShouldBe(TimeSpan.FromSeconds(6));
        settings.AppUsageSessionOptimizationInterval.ShouldBe(TimeSpan.FromMinutes(10));
        settings.DayCutoffHour.ShouldBe(5);
    }

    #endregion

    #region Update 成功更新测试

    [Fact]
    public void Update_WithValidValues_ShouldUpdateAllProperties()
    {
        // Arrange
        var settings = UserSettings.CreateDefault();

        const string newIconDir = "./Custom/AppIcons";
        var newStaleThreshold = TimeSpan.FromHours(12);
        var newAutoSaveInterval = TimeSpan.FromSeconds(30);
        const bool newIsIdleEnabled = true;
        var newIdleThreshold = TimeSpan.FromMinutes(5);
        var newIdlePollingInterval = TimeSpan.FromSeconds(5);
        var newMinValidDuration = TimeSpan.FromSeconds(2);
        var newMergeTolerance = TimeSpan.FromSeconds(3);
        var newOptInterval = TimeSpan.FromMinutes(15);
        const int newDayCutoffHour = 6;

        // Act
        settings.Update(
            appIconDirectory: newIconDir,
            appMetadataStaleThreshold: newStaleThreshold,
            activeAppUsageSessionAutoSaveInterval: newAutoSaveInterval,
            isIdleDetectionEnabled: newIsIdleEnabled,
            idleThreshold: newIdleThreshold,
            idleDetectionPollingInterval: newIdlePollingInterval,
            minValidAppUsageSessionDuration: newMinValidDuration,
            appUsageSessionMergeTolerance: newMergeTolerance,
            appUsageSessionOptimizationInterval: newOptInterval,
            dayCutoffHour: newDayCutoffHour
        );

        // Assert
        settings.AppIconDirectory.ShouldBe(newIconDir);
        settings.AppMetadataStaleThreshold.ShouldBe(newStaleThreshold);
        settings.ActiveAppUsageSessionAutoSaveInterval.ShouldBe(newAutoSaveInterval);
        settings.IsIdleDetectionEnabled.ShouldBe(newIsIdleEnabled);
        settings.IdleThreshold.ShouldBe(newIdleThreshold);
        settings.IdleDetectionPollingInterval.ShouldBe(newIdlePollingInterval);
        settings.MinValidAppUsageSessionDuration.ShouldBe(newMinValidDuration);
        settings.AppUsageSessionMergeTolerance.ShouldBe(newMergeTolerance);
        settings.AppUsageSessionOptimizationInterval.ShouldBe(newOptInterval);
        settings.DayCutoffHour.ShouldBe(newDayCutoffHour);
    }

    [Fact]
    public void Update_WithUnspecifiedOptionalValues_ShouldNotModifyProperties()
    {
        // Arrange
        var settings = UserSettings.CreateDefault();

        // Act - 传入 default (HasValue 为 false)
        settings.Update(
            appIconDirectory: default,
            appMetadataStaleThreshold: default,
            activeAppUsageSessionAutoSaveInterval: default,
            isIdleDetectionEnabled: default,
            idleThreshold: default,
            idleDetectionPollingInterval: default,
            minValidAppUsageSessionDuration: default,
            appUsageSessionMergeTolerance: default,
            appUsageSessionOptimizationInterval: default,
            dayCutoffHour: default
        );

        // Assert - 所有属性应保持默认值不变
        settings.AppIconDirectory.ShouldBe("./Data/AppIcons");
        settings.AppMetadataStaleThreshold.ShouldBe(TimeSpan.FromHours(24));
        settings.ActiveAppUsageSessionAutoSaveInterval.ShouldBe(TimeSpan.FromSeconds(15));
        settings.IsIdleDetectionEnabled.ShouldBeFalse();
        settings.IdleThreshold.ShouldBe(TimeSpan.FromMinutes(10));
        settings.IdleDetectionPollingInterval.ShouldBe(TimeSpan.FromSeconds(10));
        settings.MinValidAppUsageSessionDuration.ShouldBe(TimeSpan.FromSeconds(5));
        settings.AppUsageSessionMergeTolerance.ShouldBe(TimeSpan.FromSeconds(6));
        settings.AppUsageSessionOptimizationInterval.ShouldBe(TimeSpan.FromMinutes(10));
        settings.DayCutoffHour.ShouldBe(5);
    }

    #endregion

    #region Update 异常校验测试 (Boundary Validation)

    [Fact]
    public void Update_WithNegativeAppMetadataStaleThreshold_ShouldThrowArgumentException()
    {
        // Arrange
        var settings = UserSettings.CreateDefault();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() =>
            settings.Update(
                appIconDirectory: default,
                appMetadataStaleThreshold: TimeSpan.FromSeconds(-1),
                activeAppUsageSessionAutoSaveInterval: default,
                isIdleDetectionEnabled: default,
                idleThreshold: default,
                idleDetectionPollingInterval: default,
                minValidAppUsageSessionDuration: default,
                appUsageSessionMergeTolerance: default,
                appUsageSessionOptimizationInterval: default,
                dayCutoffHour: default
            )
        );

        ex.ParamName.ShouldBe("appMetadataStaleThreshold");
        ex.Message.ShouldContain(
            "App metadata stale threshold must be greater than or equal to zero."
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_WithNonPositiveActiveAppUsageSessionAutoSaveInterval_ShouldThrowArgumentException(
        int seconds
    )
    {
        // Arrange
        var settings = UserSettings.CreateDefault();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() =>
            settings.Update(
                appIconDirectory: default,
                appMetadataStaleThreshold: default,
                activeAppUsageSessionAutoSaveInterval: TimeSpan.FromSeconds(seconds),
                isIdleDetectionEnabled: default,
                idleThreshold: default,
                idleDetectionPollingInterval: default,
                minValidAppUsageSessionDuration: default,
                appUsageSessionMergeTolerance: default,
                appUsageSessionOptimizationInterval: default,
                dayCutoffHour: default
            )
        );

        ex.ParamName.ShouldBe("activeAppUsageSessionAutoSaveInterval");
        ex.Message.ShouldContain(
            "Active app usage session auto save interval must be greater than zero."
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Update_WithNonPositiveIdleThreshold_ShouldThrowArgumentException(int seconds)
    {
        // Arrange
        var settings = UserSettings.CreateDefault();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() =>
            settings.Update(
                appIconDirectory: default,
                appMetadataStaleThreshold: default,
                activeAppUsageSessionAutoSaveInterval: default,
                isIdleDetectionEnabled: default,
                idleThreshold: TimeSpan.FromSeconds(seconds),
                idleDetectionPollingInterval: default,
                minValidAppUsageSessionDuration: default,
                appUsageSessionMergeTolerance: default,
                appUsageSessionOptimizationInterval: default,
                dayCutoffHour: default
            )
        );

        ex.ParamName.ShouldBe("idleThreshold");
        ex.Message.ShouldContain("Idle threshold must be greater than zero.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Update_WithNonPositiveIdleDetectionPollingInterval_ShouldThrowArgumentException(
        int seconds
    )
    {
        // Arrange
        var settings = UserSettings.CreateDefault();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() =>
            settings.Update(
                appIconDirectory: default,
                appMetadataStaleThreshold: default,
                activeAppUsageSessionAutoSaveInterval: default,
                isIdleDetectionEnabled: default,
                idleThreshold: default,
                idleDetectionPollingInterval: TimeSpan.FromSeconds(seconds),
                minValidAppUsageSessionDuration: default,
                appUsageSessionMergeTolerance: default,
                appUsageSessionOptimizationInterval: default,
                dayCutoffHour: default
            )
        );

        ex.ParamName.ShouldBe("idleDetectionPollingInterval");
        ex.Message.ShouldContain("Idle detection polling interval must be greater than zero.");
    }

    [Fact]
    public void Update_WithNegativeMinValidAppUsageSessionDuration_ShouldThrowArgumentException()
    {
        // Arrange
        var settings = UserSettings.CreateDefault();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() =>
            settings.Update(
                appIconDirectory: default,
                appMetadataStaleThreshold: default,
                activeAppUsageSessionAutoSaveInterval: default,
                isIdleDetectionEnabled: default,
                idleThreshold: default,
                idleDetectionPollingInterval: default,
                minValidAppUsageSessionDuration: TimeSpan.FromSeconds(-1),
                appUsageSessionMergeTolerance: default,
                appUsageSessionOptimizationInterval: default,
                dayCutoffHour: default
            )
        );

        ex.ParamName.ShouldBe("minValidAppUsageSessionDuration");
        ex.Message.ShouldContain(
            "Min valid app usage session duration must be greater than or equal to zero."
        );
    }

    [Fact]
    public void Update_WithNegativeAppUsageSessionMergeTolerance_ShouldThrowArgumentException()
    {
        // Arrange
        var settings = UserSettings.CreateDefault();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() =>
            settings.Update(
                appIconDirectory: default,
                appMetadataStaleThreshold: default,
                activeAppUsageSessionAutoSaveInterval: default,
                isIdleDetectionEnabled: default,
                idleThreshold: default,
                idleDetectionPollingInterval: default,
                minValidAppUsageSessionDuration: default,
                appUsageSessionMergeTolerance: TimeSpan.FromSeconds(-1),
                appUsageSessionOptimizationInterval: default,
                dayCutoffHour: default
            )
        );

        ex.ParamName.ShouldBe("appUsageSessionMergeTolerance");
        ex.Message.ShouldContain(
            "App usage session merge tolerance must be greater than or equal to zero."
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Update_WithNonPositiveAppUsageSessionOptimizationInterval_ShouldThrowArgumentException(
        int seconds
    )
    {
        // Arrange
        var settings = UserSettings.CreateDefault();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() =>
            settings.Update(
                appIconDirectory: default,
                appMetadataStaleThreshold: default,
                activeAppUsageSessionAutoSaveInterval: default,
                isIdleDetectionEnabled: default,
                idleThreshold: default,
                idleDetectionPollingInterval: default,
                minValidAppUsageSessionDuration: default,
                appUsageSessionMergeTolerance: default,
                appUsageSessionOptimizationInterval: TimeSpan.FromSeconds(seconds),
                dayCutoffHour: default
            )
        );

        ex.ParamName.ShouldBe("appUsageSessionOptimizationInterval");
        ex.Message.ShouldContain(
            "App usage session optimization interval must be greater than zero."
        );
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(24)]
    [InlineData(100)]
    public void Update_WithInvalidDayCutoffHour_ShouldThrowArgumentException(int invalidHour)
    {
        // Arrange
        var settings = UserSettings.CreateDefault();

        // Act & Assert
        var ex = Should.Throw<ArgumentException>(() =>
            settings.Update(
                appIconDirectory: default,
                appMetadataStaleThreshold: default,
                activeAppUsageSessionAutoSaveInterval: default,
                isIdleDetectionEnabled: default,
                idleThreshold: default,
                idleDetectionPollingInterval: default,
                minValidAppUsageSessionDuration: default,
                appUsageSessionMergeTolerance: default,
                appUsageSessionOptimizationInterval: default,
                dayCutoffHour: invalidHour
            )
        );

        ex.ParamName.ShouldBe("dayCutoffHour");
        ex.Message.ShouldContain("Day cutoff hour must be between 0 and 23.");
    }

    #endregion
}
