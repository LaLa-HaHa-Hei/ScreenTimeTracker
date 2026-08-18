namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteUsageSessions;

public record WebsiteUsageSessionOptimizationResult(
    IReadOnlySet<WebsiteUsageSession> SessionsToRemove
);

public class WebsiteUsageSessionOptimizer
{
    public static WebsiteUsageSessionOptimizationResult Optimize(
        IReadOnlyList<WebsiteUsageSession> sessions,
        TimeSpan mergeTolerance,
        TimeSpan minValidDuration
    )
    {
        var sessionsToRemove = new HashSet<WebsiteUsageSession>();

        // 执行容差合并算法
        for (int i = 0; i < sessions.Count; i++)
        {
            var current = sessions[i];
            for (int j = i + 1; j < sessions.Count; j++)
            {
                var next = sessions[j];
                // 超出合并容差，断开合并
                if (next.UsagePeriod.Start > current.UsagePeriod.End + mergeTolerance)
                    break;

                if (current.WebsiteId == next.WebsiteId)
                {
                    current.UpdateEndTime(next.UsagePeriod.End); // 扩展当前记录的结束时间
                    // 标记中间被合并吸收掉的杂项记录
                    for (int k = i + 1; k <= j; k++)
                        sessionsToRemove.Add(sessions[k]);

                    i = j; // 跳跃指针
                }
            }
        }

        // 执行时长有效性过滤算法
        foreach (var session in sessions)
        {
            if (sessionsToRemove.Contains(session))
                continue; // 已经被合并废弃的不再处理

            var duration = session.UsagePeriod.End - session.UsagePeriod.Start;

            // 如果有效时长小于设定的最小阈值，剔除
            if (duration < minValidDuration)
                sessionsToRemove.Add(session);
            // 存活下来的记录标记为已优化
            else
                session.MarkAsOptimized();
        }

        return new WebsiteUsageSessionOptimizationResult(sessionsToRemove);
    }
}
