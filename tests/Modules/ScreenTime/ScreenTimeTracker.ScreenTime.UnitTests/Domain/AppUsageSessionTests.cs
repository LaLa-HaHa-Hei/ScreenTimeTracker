using ScreenTimeTracker.ScreenTime.Domain;
using Shouldly;

namespace ScreenTimeTracker.ScreenTime.UnitTests.Domain;

public class AppUsageSessionTests
{
    private readonly Guid _appId = Guid.NewGuid();
    private readonly DateTime _now = DateTime.UtcNow;

    #region Create 方法测试

    [Fact]
    public void Create_WithValidParameters_ShouldReturnAppUsageSession()
    {
        // Arrange
        var startTime = _now;
        var endTime = _now.AddHours(1);

        // Act
        var session = AppUsageSession.Create(_appId, startTime, endTime);

        // Assert (使用 Shouldly 断言)
        session.ShouldNotBeNull();
        session.Id.ShouldNotBe(Guid.Empty); // 验证使用 Guid.CreateVersion7() 生成了有效 ID
        session.AppId.ShouldBe(_appId);
        session.StartTime.ShouldBe(startTime);
        session.EndTime.ShouldBe(endTime);
        session.IsOptimized.ShouldBeFalse();
        session.App.ShouldBeNull();
    }

    [Theory]
    [MemberData(nameof(InvalidTimeOffsets))]
    public void Create_WhenEndTimeIsLessThanOrEqualToStartTime_ShouldThrowArgumentException(
        int offsetInSeconds
    )
    {
        // Arrange
        var startTime = _now;
        var invalidEndTime = _now.AddSeconds(offsetInSeconds);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            AppUsageSession.Create(_appId, startTime, invalidEndTime)
        );

        exception.ParamName.ShouldBe("endTime");
        exception.Message.ShouldContain("End time must be greater than start time.");
    }

    #endregion

    #region Import 方法测试

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Import_WithValidParameters_ShouldCreateSessionWithSpecifiedIsOptimized(
        bool isOptimized
    )
    {
        // Arrange
        var startTime = _now;
        var endTime = _now.AddMinutes(30);

        // Act
        var session = AppUsageSession.Import(_appId, startTime, endTime, isOptimized);

        // Assert
        session.ShouldNotBeNull();
        session.Id.ShouldNotBe(Guid.Empty);
        session.AppId.ShouldBe(_appId);
        session.StartTime.ShouldBe(startTime);
        session.EndTime.ShouldBe(endTime);
        session.IsOptimized.ShouldBe(isOptimized);
    }

    [Theory]
    [MemberData(nameof(InvalidTimeOffsets))]
    public void Import_WhenEndTimeIsLessThanOrEqualToStartTime_ShouldThrowArgumentException(
        int offsetInSeconds
    )
    {
        // Arrange
        var startTime = _now;
        var invalidEndTime = _now.AddSeconds(offsetInSeconds);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            AppUsageSession.Import(_appId, startTime, invalidEndTime, isOptimized: true)
        );

        exception.ParamName.ShouldBe("endTime");
    }

    #endregion

    #region MarkAsIdle 方法测试

    [Fact]
    public void MarkAsIdle_ShouldUpdateAppIdToNewIdleAppId()
    {
        // Arrange
        var session = AppUsageSession.Create(_appId, _now, _now.AddHours(1));
        var idleAppId = Guid.NewGuid();

        // Act
        session.MarkAsIdle(idleAppId);

        // Assert
        session.AppId.ShouldBe(idleAppId);
    }

    #endregion

    #region UpdateEndTime 方法测试

    [Fact]
    public void UpdateEndTime_WithValidEndTime_ShouldUpdateEndTime()
    {
        // Arrange
        var startTime = _now;
        var initialEndTime = _now.AddMinutes(30);
        var session = AppUsageSession.Create(_appId, startTime, initialEndTime);
        var newEndTime = _now.AddHours(2);

        // Act
        session.UpdateEndTime(newEndTime);

        // Assert
        session.EndTime.ShouldBe(newEndTime);
    }

    [Theory]
    [MemberData(nameof(InvalidTimeOffsets))]
    public void UpdateEndTime_WhenEndTimeIsLessThanOrEqualToStartTime_ShouldThrowArgumentException(
        int offsetInSeconds
    )
    {
        // Arrange
        var startTime = _now;
        var session = AppUsageSession.Create(_appId, startTime, startTime.AddHours(1));
        var invalidEndTime = startTime.AddSeconds(offsetInSeconds);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            session.UpdateEndTime(invalidEndTime)
        );

        exception.ParamName.ShouldBe("endTime");
    }

    #endregion

    #region MarkAsOptimized 方法测试

    [Fact]
    public void MarkAsOptimized_ShouldSetIsOptimizedToTrue()
    {
        // Arrange
        var session = AppUsageSession.Create(_appId, _now, _now.AddHours(1));

        // Act
        session.MarkAsOptimized();

        // Assert
        session.IsOptimized.ShouldBeTrue();
    }

    #endregion

    #region xUnit v3 强类型测试数据集 (TheoryData)

    /// <summary>
    /// 使用 xUnit v3 强类型的 TheoryData 充当测试数据源
    /// </summary>
    public static TheoryData<int> InvalidTimeOffsets =>
        new()
        {
            0, // 结束时间等于开始时间
            -1, // 结束时间早于开始时间 1 秒
            -3600, // 结束时间早于开始时间 1 小时
        };

    #endregion
}
