import request from './request'

export async function getProcessUsageRankEntryForPeriod(
    startDate: string,
    endDate: string,
    topN: number,
) {
    return await request.get(
        `/usage-reports/ranks/processes?startDate=${startDate}&endDate=${endDate}&topN=${topN}`,
    )
}

export async function getTotalHourlyUsage(date: string) {
    return await request.get(`/usage-reports/summaries/hourly?date=${date}`)
}

export async function getTotalDailyUsage(startDate: string, endDate: string) {
    return await request.get(
        `/usage-reports/summaries/daily?startDate=${startDate}&endDate=${endDate}`,
    )
}
