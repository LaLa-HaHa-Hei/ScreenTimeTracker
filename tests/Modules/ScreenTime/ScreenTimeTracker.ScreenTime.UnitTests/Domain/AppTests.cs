using ScreenTimeTracker.ScreenTime.Domain;
using Shouldly;

namespace ScreenTimeTracker.ScreenTime.UnitTests.Domain;

public class AppTests
{
    #region Factory Method Tests

    [Fact]
    public void RegisterDiscovered_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var discoveredAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var name = "Visual Studio Code";
        var processName = "Code";
        var executablePath = @"C:\Program Files\Microsoft VS Code\Code.exe";
        var iconPath = @"C:\Program Files\Microsoft VS Code\Code.ico";

        // Act
        var app = App.RegisterDiscovered(discoveredAt, name, processName, executablePath, iconPath);

        // Assert
        app.Id.ShouldNotBe(Guid.Empty);
        app.Name.ShouldBe(name);
        app.ProcessName.ShouldBe(processName);
        app.ExecutablePath.ShouldBe(executablePath);
        app.IconPath.ShouldBe(iconPath);
        app.AllowMetadataAutoUpdate.ShouldBeTrue();
        app.MetadataLastUpdatedAt.ShouldBe(discoveredAt);
        app.IconPathLastUpdatedAt.ShouldBe(discoveredAt);
        app.IsSystem.ShouldBeFalse();
        app.AppCategoryId.ShouldBe(AppCategory.UncategorizedId);

        // 校验随机生成的 Hex 颜色格式 (#RRGGBB)
        app.Color.ShouldMatch("^#[0-9A-FA-f]{6}$");
    }

    [Fact]
    public void Import_ShouldInitializeWithProvidedValues()
    {
        // Arrange
        var name = "Spotify";
        var color = "#1DB954";
        var processName = "spotify";
        var isAutoUpdateEnabled = false;
        var lastAutoUpdated = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var categoryId = Guid.NewGuid();
        var execPath = @"C:\Users\Test\AppData\Roaming\Spotify\Spotify.exe";
        var iconPath = @"C:\Users\Test\AppData\Roaming\Spotify\Spotify.ico";
        var iconUpdatedAt = new DateTime(2026, 1, 1, 11, 0, 0, DateTimeKind.Utc);

        // Act
        var app = App.Import(
            name,
            color,
            processName,
            isAutoUpdateEnabled,
            lastAutoUpdated,
            categoryId,
            execPath,
            iconPath,
            iconUpdatedAt
        );

        // Assert
        app.Id.ShouldNotBe(Guid.Empty);
        app.Name.ShouldBe(name);
        app.Color.ShouldBe(color);
        app.ProcessName.ShouldBe(processName);
        app.AllowMetadataAutoUpdate.ShouldBeFalse();
        app.MetadataLastUpdatedAt.ShouldBe(lastAutoUpdated);
        app.AppCategoryId.ShouldBe(categoryId);
        app.ExecutablePath.ShouldBe(execPath);
        app.IconPath.ShouldBe(iconPath);
        app.IconPathLastUpdatedAt.ShouldBe(iconUpdatedAt);
        app.IsSystem.ShouldBeFalse();
    }

    [Fact]
    public void CreateIdleApp_ShouldInitializeSystemIdleApp()
    {
        // Act
        var app = App.CreateIdleApp();

        // Assert
        app.Id.ShouldBe(App.IdleAppId);
        app.Name.ShouldBe("Idle");
        app.ProcessName.ShouldBe(App.IdleAppProcessName);
        app.Color.ShouldBe("#C7C7CC");
        app.AllowMetadataAutoUpdate.ShouldBeFalse();
        app.MetadataLastUpdatedAt.ShouldBe(DateTime.MinValue);
        app.IconPathLastUpdatedAt.ShouldBe(DateTime.MinValue);
        app.AppCategoryId.ShouldBe(AppCategory.UncategorizedId);
        app.ExecutablePath.ShouldBeNull();
        app.IconPath.ShouldBeNull();
        app.IsSystem.ShouldBeTrue();
    }

    [Fact]
    public void CreateUnknownApp_ShouldInitializeSystemUnknownApp()
    {
        // Act
        var app = App.CreateUnknownApp();

        // Assert
        app.Id.ShouldBe(App.UnknownAppId);
        app.Name.ShouldBe("Unknown");
        app.ProcessName.ShouldBe(App.UnknownAppProcessName);
        app.Color.ShouldBe("#636366");
        app.AllowMetadataAutoUpdate.ShouldBeFalse();
        app.MetadataLastUpdatedAt.ShouldBe(DateTime.MinValue);
        app.IconPathLastUpdatedAt.ShouldBe(DateTime.MinValue);
        app.AppCategoryId.ShouldBe(AppCategory.UncategorizedId);
        app.ExecutablePath.ShouldBeNull();
        app.IconPath.ShouldBeNull();
        app.IsSystem.ShouldBeTrue();
    }

    #endregion

    #region Update Methods Tests

    [Fact]
    public void Update_WhenIconPathProvidedWithoutUpdatedAt_ShouldThrowArgumentException()
    {
        // Arrange
        var app = App.RegisterDiscovered(DateTime.UtcNow, "TestApp", "testapp");

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
        {
            app.Update(iconPath: "new/icon/path.png", iconPathUpdatedAt: null);
        });

        exception.Message.ShouldContain(
            "Icon path must be updated with a valid icon path last updated time."
        );
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateSpecifiedProperties()
    {
        // Arrange
        var app = App.RegisterDiscovered(DateTime.UtcNow, "Old Name", "testapp");
        var newCategoryId = Guid.NewGuid();
        var iconUpdatedAt = DateTime.UtcNow.AddHours(1);

        // Act
        app.Update(
            name: "New Name",
            color: "#FFFFFF",
            allowMetadataAutoUpdate: false,
            appCategoryId: newCategoryId,
            iconPath: "new/icon.png",
            iconPathUpdatedAt: iconUpdatedAt
        );

        // Assert
        app.Name.ShouldBe("New Name");
        app.Color.ShouldBe("#FFFFFF");
        app.AllowMetadataAutoUpdate.ShouldBeFalse();
        app.AppCategoryId.ShouldBe(newCategoryId);
        app.IconPath.ShouldBe("new/icon.png");
        app.IconPathLastUpdatedAt.ShouldBe(iconUpdatedAt);
    }

    [Fact]
    public void Update_WithDefaultOptionalValues_ShouldNotModifyProperties()
    {
        // Arrange
        var app = App.RegisterDiscovered(DateTime.UtcNow, "Name", "app");
        var originalName = app.Name;
        var originalColor = app.Color;
        var originalAutoUpdate = app.AllowMetadataAutoUpdate;
        var originalCategoryId = app.AppCategoryId;
        var originalIconPath = app.IconPath;

        // Act
        app.Update(); // 均使用 OptionalValue 默认值

        // Assert
        app.Name.ShouldBe(originalName);
        app.Color.ShouldBe(originalColor);
        app.AllowMetadataAutoUpdate.ShouldBe(originalAutoUpdate);
        app.AppCategoryId.ShouldBe(originalCategoryId);
        app.IconPath.ShouldBe(originalIconPath);
    }

    [Fact]
    public void UpdateMetadata_WhenAutoUpdateIsDisabled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var app = App.CreateIdleApp(); // AllowMetadataAutoUpdate 默认为 false

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            app.UpdateMetadata(DateTime.UtcNow, @"C:\app.exe", @"C:\app.ico");
        });

        exception.Message.ShouldContain("Cannot update system details for App");
    }

    [Fact]
    public void UpdateMetadata_WhenAutoUpdateIsEnabled_ShouldUpdateMetadataAndTimestamps()
    {
        // Arrange
        var app = App.RegisterDiscovered(DateTime.UtcNow, "TestApp", "testapp");
        var updateTime = DateTime.UtcNow.AddMinutes(10);
        var execPath = @"C:\Path\To\App.exe";
        var iconPath = @"C:\Path\To\App.ico";

        // Act
        app.UpdateMetadata(updateTime, execPath, iconPath);

        // Assert
        app.ExecutablePath.ShouldBe(execPath);
        app.IconPath.ShouldBe(iconPath);
        app.IconPathLastUpdatedAt.ShouldBe(updateTime);
        app.MetadataLastUpdatedAt.ShouldBe(updateTime);
    }

    #endregion

    #region NeedsMetadataUpdate Tests

    [Theory]
    [InlineData(10, 5, true)] // 相隔 10 分钟 >= 阈值 5 分钟 -> true
    [InlineData(5, 5, true)] // 相隔 5 分钟 >= 阈值 5 分钟 -> true
    [InlineData(3, 5, false)] // 相隔 3 分钟 < 阈值 5 分钟 -> false
    public void NeedsMetadataUpdate_WhenAutoUpdateIsEnabled_ShouldCompareWithThreshold(
        int elapsedMinutes,
        int thresholdMinutes,
        bool expectedResult
    )
    {
        // Arrange
        var lastUpdated = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var app = App.RegisterDiscovered(lastUpdated, "TestApp", "testapp"); // AllowMetadataAutoUpdate 为 true
        var now = lastUpdated.AddMinutes(elapsedMinutes);
        var threshold = TimeSpan.FromMinutes(thresholdMinutes);

        // Act
        var result = app.NeedsMetadataUpdate(now, threshold);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public void NeedsMetadataUpdate_WhenAutoUpdateIsDisabled_ShouldReturnFalse()
    {
        // Arrange
        var app = App.CreateIdleApp(); // AllowMetadataAutoUpdate 为 false
        var now = DateTime.UtcNow;
        var threshold = TimeSpan.Zero;

        // Act
        var result = app.NeedsMetadataUpdate(now, threshold);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion
}
