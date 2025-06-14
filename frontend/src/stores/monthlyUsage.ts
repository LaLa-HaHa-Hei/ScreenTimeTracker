import { ref, type Ref } from 'vue'
import { defineStore } from 'pinia'
import type { DateUsage, ProcessUsage, DateOnly } from "@/types";
import api from '@/api'
import { dateToDateOnly, dateOnlyToDate } from '@/utils';

export const useMonthlyUsageStore = defineStore('monthlyUsage', () => {
    const monthUsageSummaries: Ref<Record<DateOnly, DateUsage[]>> = ref<Record<DateOnly, DateUsage[]>>({});
    const topProcessessUsages: Ref<Record<DateOnly, ProcessUsage[]>> = ref<Record<DateOnly, ProcessUsage[]>>({});

    const getMonthUsageSummary = async (endDate: DateOnly) => {
        if (!monthUsageSummaries.value[endDate]) {
            let startDate = dateOnlyToDate(endDate)
            if (dateToDateOnly(new Date()) === endDate) {
                startDate.setDate(startDate.getDate() - 29);
            }
            else {
                startDate.setDate(1);
            }
            const dateSummary = await api.getUsageSummaryRange(dateToDateOnly(startDate), endDate);
            monthUsageSummaries.value[endDate] = dateSummary;
        }
        return monthUsageSummaries.value[endDate];
    }

    const getTopProcessessUsage = async (endDate: DateOnly) => {
        if (!topProcessessUsages.value[endDate]) {
            let startDate = dateOnlyToDate(endDate)
            if (dateToDateOnly(new Date()) === endDate) {
                startDate.setDate(startDate.getDate() - 29);
            }
            else {
                startDate.setDate(1);
            }
            const topProcesses = await api.getTopProcessesUsageRange(dateToDateOnly(startDate), endDate, 10);
            topProcessessUsages.value[endDate] = topProcesses;
        }
        return topProcessessUsages.value[endDate];
    }

    return {
        monthUsageSummaries,
        topProcessessUsages,
        getMonthUsageSummary,
        getTopProcessessUsage
    }
})