import axios from 'axios';
import config from '@/config';
import type { ProcessInfo, DateOnly, DateUsage, ProcessUsage } from '@/types';

const server = axios.create({
    baseURL: config.baseApiUrl
})

export default {
    // 用进程名获取进程信息
    async GetProcessByName(name: string): Promise<ProcessInfo | null> {
        var response = await server({
            method: 'get',
            url: `/processes/name/${name}`
        })
        if (response.data === '' || response.data === null || response.data === undefined)
            return null;
        else
            return response.data as ProcessInfo;
    },
    // 修改进程的别名
    async UpdateProcessAlias(name: string, alias: string) {
        var response = await server({
            method: 'put',
            url: `/processes/name/${name}/alias`,
            data: { alias }
        })
        return response.data;
    },

    /// 删除指定日期以前的数据
    async DeleteUsagesBeforeDate(date: DateOnly) {
        var response = await server({
            method: 'delete',
            url: `/screen-time/cleanup/before`,
            params: { date }
        })
        return response.data;
    },
    // 获取指定进程在某天的24小时使用情况
    async getProcessDailyUsage(name: string, date: DateOnly): Promise<number[]> {
        var response = await server({
            method: 'get',
            url: `/screen-time/processes/${name}/daily`,
            params: { date }
        })
        return response.data;
    },
    // 获取指定进程在日期范围内的每日使用情况
    async getProcessUsageRange(name: string, startDate: DateOnly, endDate: DateOnly): Promise<DateUsage[]> {
        var response = await server({
            method: 'get',
            url: `/screen-time/processes/${name}/range`,
            params: {
                startDate,
                endDate
            }
        })
        return response.data;
    },
    async getTopProcessesUsageRange(startDate: DateOnly, endDate: DateOnly, limit: number = 10): Promise<ProcessUsage[]> {
        var response = await server({
            method: 'get',
            url: `/screen-time/processes/range`,
            params: {
                startDate,
                endDate,
                limit
            }
        })
        return response.data;
    },
    // 获取指定日期内每小时内所有进程使用情况
    async getDailyUsageSummary(date: DateOnly): Promise<number[]> {
        var response = await server({
            method: 'get',
            url: `/screen-time/summary/daily`,
            params: {
                date,
            }
        })
        return response.data;
    },
    // 获取指定日期范围内的所有进程使用情况
    async getUsageSummaryRange(startDate: DateOnly, endDate: DateOnly): Promise<DateUsage[]> {
        var response = await server({
            method: 'get',
            url: `/screen-time/summary/range`,
            params: {
                startDate,
                endDate
            }
        })
        return response.data;
    }
}