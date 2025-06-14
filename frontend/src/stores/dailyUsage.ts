import { ref, type Ref } from 'vue'
import { defineStore } from 'pinia'
import type { ProcessUsage, DateOnly } from "@/types";
import api from '@/api'

export const useDailyUsageStore = defineStore('dailyUsage', () => {
    const hourlyUsageSummaries: Ref<Record<DateOnly, number[]>> = ref<Record<DateOnly, number[]>>({});
    const topProcessessUsages: Ref<Record<DateOnly, ProcessUsage[]>> = ref<Record<DateOnly, ProcessUsage[]>>({});

    const getHourlyUsageSummary = async (date: DateOnly) => {
        if (!hourlyUsageSummaries.value[date]) {
            const dailySummary = await api.getDailyUsageSummary(date);
            hourlyUsageSummaries.value[date] = dailySummary;
        }
        return hourlyUsageSummaries.value[date];
    }
    const getTopProcessessUsage = async (date: DateOnly) => {
        if (!topProcessessUsages.value[date]) {
            const topProcesses = await api.getTopProcessesUsageRange(date, date, 10);
            topProcessessUsages.value[date] = topProcesses;
        }
        return topProcessessUsages.value[date];
    }

    return {
        hourlyUsageSummaries,
        topProcessessUsages,
        getHourlyUsageSummary,
        getTopProcessessUsage
    }
})