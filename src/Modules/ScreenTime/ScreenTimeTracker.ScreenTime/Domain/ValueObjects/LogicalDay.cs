using ScreenTimeTracker.BuildingBlocks.Domain;

namespace ScreenTimeTracker.ScreenTime.Domain.ValueObjects;

public readonly record struct LogicalDay : IValueObject
{
    public DateOnly Date { get; }
    public TimeRange UtcWindow { get; }

    private LogicalDay(DateOnly date, TimeRange utcWindow)
    {
        Date = date;
        UtcWindow = utcWindow;
    }

    public static LogicalDay From(DateOnly date, int cutoffHour, TimeZoneInfo timeZoneInfo)
    {
        var startUtc = CalculateLogicalDayStartInUtc(date, cutoffHour, timeZoneInfo);
        var endUtc = CalculateLogicalDayStartInUtc(date.AddDays(1), cutoffHour, timeZoneInfo);

        return new LogicalDay(date, new TimeRange(startUtc, endUtc));
    }

    public static TimeRange CalculateUtcWindow(
        DateOnly startDate,
        DateOnly endDate,
        int dayCutoffHour,
        TimeZoneInfo timeZoneInfo
    )
    {
        var startDay = From(startDate, dayCutoffHour, timeZoneInfo);
        var endDay = From(endDate, dayCutoffHour, timeZoneInfo);

        return new TimeRange(startDay.UtcWindow.Start, endDay.UtcWindow.End);
    }

    private static DateTimeOffset CalculateLogicalDayStartInUtc(
        DateOnly date,
        int cutoffHour,
        TimeZoneInfo timeZoneInfo
    )
    {
        // 构建本地未指定 Kind 的 DateTime
        var localTime = date.ToDateTime(new TimeOnly(cutoffHour, 0), DateTimeKind.Unspecified);

        // 处理夏令时跳过的时间（Invalid Time，例如 02:00 跳到 03:00）
        if (timeZoneInfo.IsInvalidTime(localTime))
            localTime = localTime.AddHours(1);

        // 处理夏令时重叠的时间（Ambiguous Time，例如 02:00 出现两次）
        TimeSpan offset = timeZoneInfo.IsAmbiguousTime(localTime)
            ? timeZoneInfo.GetAmbiguousTimeOffsets(localTime)[0]
            : timeZoneInfo.GetUtcOffset(localTime);

        return new DateTimeOffset(localTime, offset).ToUniversalTime();
    }
}
