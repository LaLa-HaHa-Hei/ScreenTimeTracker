import axios from 'axios';
import config from '@/config';
import type { ProcessInfo, DateOnly, DateUsage, ProcessUsage } from '@/types';
import { dateToDateOnly } from '@/utils';

const server = axios.create({
    baseURL: config.baseApiUrl
})

// 检查日期是否为今天，决定是否添加缓存控制头
const getCacheControlHeaderIfToday = (date: DateOnly) => {
    return date === dateToDateOnly(new Date())
        ? { 'Cache-Control': 'no-cache, no-store' }
        : {};
};

export default {
    // 用进程名获取进程信息
    async getProcessByName(name: string): Promise<ProcessInfo | null> {
        const response = await server.get(`/processes/name/${name}`)
        return response.data || null;
    },
    // 修改进程的别名
    async updateProcessAlias(name: string, alias: string) {
        const response = await server.put(
            `/processes/name/${name}/alias`,
            { alias })
        return response.data;
    },

    /// 删除指定日期以前的数据
    async DeleteUsagesBeforeDate(date: DateOnly) {
        const response = await server.delete(
            `/screen-time/cleanup/before`,
            { params: { date } })
        return response.data;
    },
    // 获取指定进程在某天的24小时使用情况
    async getProcessDailyUsage(name: string, date: DateOnly): Promise<number[]> {
        const response = await server.get(
            `/screen-time/processes/${name}/daily`,
            {
                params: { date },
                headers: getCacheControlHeaderIfToday(date)
            }
        )
        return response.data;
    },
    // 获取指定进程在日期范围内的每日使用情况
    async getProcessUsageRange(name: string, startDate: DateOnly, endDate: DateOnly): Promise<DateUsage[]> {
        const response = await server.get(
            `/screen-time/processes/${name}/range`,
            {
                params: { startDate, endDate },
                headers: getCacheControlHeaderIfToday(endDate)
            })
        return response.data;
    },
    async getTopProcessesUsageRange(startDate: DateOnly, endDate: DateOnly, limit: number = 10): Promise<ProcessUsage[]> {
        const response = await server.get(
            `/screen-time/processes/range`,
            {
                params: { startDate, endDate, limit },
                headers: getCacheControlHeaderIfToday(endDate)
            })
        return response.data;
    },
    // 获取指定日期内每小时内所有进程使用情况
    async getDailyUsageSummary(date: DateOnly): Promise<number[]> {
        const response = await server.get(
            `/screen-time/summary/daily`,
            {
                params: { date },
                headers: getCacheControlHeaderIfToday(date)
            })
        return response.data;
    },
    // 获取指定日期范围内的所有进程使用情况
    async getUsageSummaryRange(startDate: DateOnly, endDate: DateOnly): Promise<DateUsage[]> {
        const response = await server.get(
            `/screen-time/summary/range`,
            {
                params: { startDate, endDate },
                headers: getCacheControlHeaderIfToday(endDate)
            })
        return response.data;
    }
}