import { ref, type Ref } from 'vue'
import { defineStore } from 'pinia'
import type { DateUsage, ProcessUsage, DateOnly } from "@/types";
import api from '@/api'
import { dateToDateOnly } from '@/utils';

export const useWeeklyUsageStore = defineStore('weeklyUsage', () => {
    const weekUsageSummaries: Ref<Record<DateOnly, DateUsage[]>> = ref<Record<DateOnly, DateUsage[]>>({});
    const topProcessessUsages: Ref<Record<DateOnly, ProcessUsage[]>> = ref<Record<DateOnly, ProcessUsage[]>>({});

    const getWeekUsageSummary = async (endDate: DateOnly) => {
        if (!weekUsageSummaries.value[endDate]) {
            const startDate = new Date(endDate)
            startDate.setDate(startDate.getDate() - 6);
            const dateSummary = await api.getUsageSummaryRange(dateToDateOnly(startDate), endDate);
            weekUsageSummaries.value[endDate] = dateSummary;
        }
        return weekUsageSummaries.value[endDate];
    }

    const getTopProcessessUsage = async (endDate: DateOnly) => {
        if (!topProcessessUsages.value[endDate]) {
            const startDate = new Date(endDate)
            startDate.setDate(startDate.getDate() - 6);
            const topProcesses = await api.getTopProcessesUsageRange(dateToDateOnly(startDate), endDate, 10);
            topProcessessUsages.value[endDate] = topProcesses;
        }
        return topProcessessUsages.value[endDate];
    }

    return {
        weekUsageSummaries,
        topProcessessUsages,
        getWeekUsageSummary,
        getTopProcessessUsage
    }
})