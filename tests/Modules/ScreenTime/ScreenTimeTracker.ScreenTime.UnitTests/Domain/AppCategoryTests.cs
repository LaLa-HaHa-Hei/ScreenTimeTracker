using ScreenTimeTracker.ScreenTime.Domain;
using Shouldly;

namespace ScreenTimeTracker.ScreenTime.UnitTests.Domain;

public class AppCategoryTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var name = "Work";
        var color = "#FF0000";
        var iconPath = "/icons/work.png";

        // Act
        var category = AppCategory.Create(createdAt, name, color, iconPath);

        // Assert
        category.ShouldNotBeNull();
        category.Id.ShouldNotBe(Guid.Empty);
        category.Name.ShouldBe(name);
        category.Color.ShouldBe(color);
        category.IconPath.ShouldBe(iconPath);
        category.IconPathLastUpdatedAt.ShouldBe(createdAt);
        category.IsSystem.ShouldBeFalse();
    }

    [Fact]
    public void Import_WithValidParameters_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var name = "Social";
        var color = "#00FF00";
        var iconPath = "/icons/social.png";
        var lastUpdated = DateTime.UtcNow.AddDays(-1);

        // Act
        var category = AppCategory.Import(name, color, iconPath, lastUpdated);

        // Assert
        category.ShouldNotBeNull();
        category.Id.ShouldNotBe(Guid.Empty);
        category.Name.ShouldBe(name);
        category.Color.ShouldBe(color);
        category.IconPath.ShouldBe(iconPath);
        category.IconPathLastUpdatedAt.ShouldBe(lastUpdated);
        category.IsSystem.ShouldBeFalse();
    }

    [Fact]
    public void CreateUncategorized_ShouldCreateSystemCategoryWithPredefinedValues()
    {
        // Act
        var category = AppCategory.CreateUncategorized();

        // Assert
        category.ShouldNotBeNull();
        category.Id.ShouldBe(AppCategory.UncategorizedId);
        category.Name.ShouldBe("Uncategorized");
        category.Color.ShouldBe("#8E8E93");
        category.IconPath.ShouldBeNull();
        category.IconPathLastUpdatedAt.ShouldBe(DateTime.MinValue);
        category.IsSystem.ShouldBeTrue();
    }

    [Fact]
    public void Update_WhenUpdatingNameAndColor_ShouldUpdateProperties()
    {
        // Arrange
        var category = AppCategory.Create(DateTime.UtcNow, "OldName", "#000000");

        // Act
        category.Update(name: "NewName", color: "#FFFFFF");

        // Assert
        category.Name.ShouldBe("NewName");
        category.Color.ShouldBe("#FFFFFF");
    }

    [Fact]
    public void Update_WhenUpdatingIconPathWithValidTimestamp_ShouldUpdateIconAndTimestamp()
    {
        // Arrange
        var initialTime = DateTime.UtcNow.AddHours(-1);
        var category = AppCategory.Create(initialTime, "Games", "#0000FF");
        var newIconPath = "/icons/games.png";
        var updateTime = DateTime.UtcNow;

        // Act
        category.Update(iconPath: newIconPath, iconPathLastUpdatedAt: updateTime);

        // Assert
        category.IconPath.ShouldBe(newIconPath);
        category.IconPathLastUpdatedAt.ShouldBe(updateTime);
    }

    [Fact]
    public void Update_WhenUpdatingIconPathWithoutTimestamp_ShouldThrowArgumentException()
    {
        // Arrange
        var category = AppCategory.Create(DateTime.UtcNow, "Games", "#0000FF");

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
        {
            category.Update(iconPath: "/icons/games.png", iconPathLastUpdatedAt: null);
        });

        exception.Message.ShouldContain(
            "Icon path must be updated with a valid icon path last updated time."
        );
    }

    [Theory]
    [InlineData("New Category", "#123456")]
    [InlineData("Study", "#654321")]
    public void Update_WithTheoryData_ShouldUpdatePropertiesCorrectly(
        string newName,
        string newColor
    )
    {
        // Arrange
        var category = AppCategory.Create(DateTime.UtcNow, "Initial", "#000000");

        // Act
        category.Update(name: newName, color: newColor);

        // Assert
        category.Name.ShouldBe(newName);
        category.Color.ShouldBe(newColor);
    }

    [Fact]
    public void Update_WhenNoParametersProvided_ShouldNotChangeAnyProperty()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var category = AppCategory.Create(createdAt, "Entertainment", "#FF00FF", "/icons/ent.png");

        // Act
        category.Update();

        // Assert
        category.Name.ShouldBe("Entertainment");
        category.Color.ShouldBe("#FF00FF");
        category.IconPath.ShouldBe("/icons/ent.png");
        category.IconPathLastUpdatedAt.ShouldBe(createdAt);
    }
}
