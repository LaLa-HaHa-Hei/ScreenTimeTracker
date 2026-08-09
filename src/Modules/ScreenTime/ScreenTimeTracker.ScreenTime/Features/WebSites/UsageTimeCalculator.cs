namespace ScreenTimeTracker.ScreenTime.Features.Websites;

public static class UsageTimeCalculator
{
    public static DateTimeOffset GetLogicalDayStartInUtc(
        DateOnly date,
        int cutoffHour,
        TimeZoneInfo timeZoneInfo
    )
    {
        // 构建本地未指定 Kind 的 DateTime
        var localTime = date.ToDateTime(new TimeOnly(cutoffHour, 0), DateTimeKind.Unspecified);

        // 处理夏令时跳过的时间（Invalid Time，例如 02:00 跳到 03:00）
        if (timeZoneInfo.IsInvalidTime(localTime))
        {
            // 顺延 1 小时避开无效区间
            localTime = localTime.AddHours(1);
        }

        // 处理夏令时重叠的时间（Ambiguous Time，例如 02:00 出现两次）
        TimeSpan offset;
        if (timeZoneInfo.IsAmbiguousTime(localTime))
        {
            // 优先取第一个偏移量（标准时间/夏令时根据需求，通常取 offsets[0]）
            var offsets = timeZoneInfo.GetAmbiguousTimeOffsets(localTime);
            offset = offsets[0];
        }
        else
        {
            offset = timeZoneInfo.GetUtcOffset(localTime);
        }

        var localOffset = new DateTimeOffset(localTime, offset);
        return localOffset.ToUniversalTime();
    }
}
