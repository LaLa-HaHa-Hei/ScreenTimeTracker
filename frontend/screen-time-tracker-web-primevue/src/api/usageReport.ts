import request from './request'

function buildListParams(paramName: string, list: string[]): string {
    if (!Array.isArray(list) || list.length === 0) {
        return ''
    }
    return list
        .map((item) => `${encodeURIComponent(paramName)}=${encodeURIComponent(item)}`)
        .join('&')
}

export async function getProcessUsageRankEntry(
    startDate: string,
    endDate: string,
    topN: number = 10,
    excludedProcessIds: string[] = [],
) {
    let url = `api/usage-reports/ranks/processes?startDate=${startDate}&endDate=${endDate}&topN=${topN}`
    if (excludedProcessIds.length > 0) {
        url += `&${buildListParams('excludedProcessIds', excludedProcessIds)}`
    }
    return await request.get(url)
}

export async function getTotalHourlyUsage(date: string, excludedProcessIds: string[] = []) {
    let url = `api/usage-reports/summaries/hourly?date=${date}`
    if (excludedProcessIds.length > 0) {
        url += `&${buildListParams('excludedProcessIds', excludedProcessIds)}`
    }
    return await request.get(url)
}

export async function getTotalDailyUsage(
    startDate: string,
    endDate: string,
    excludedProcessIds: string[] = [],
) {
    let url = `api/usage-reports/summaries/daily?startDate=${startDate}&endDate=${endDate}`
    if (excludedProcessIds.length > 0) {
        url += `&${buildListParams('excludedProcessIds', excludedProcessIds)}`
    }
    return await request.get(url)
}
