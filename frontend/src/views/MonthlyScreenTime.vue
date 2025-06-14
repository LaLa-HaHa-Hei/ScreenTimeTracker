<template>
    <div class="rounded-xl mt-4 p-4 bg-white dark:bg-neutral-800">
        <div class="flex flex-col gap-2 items-center ">
            <div class="flex items-center border border-gray-400 rounded-full py-1 px-2">
                <button @click="displayPreviousMonth" class="flex items-center cursor-pointer">
                    <el-icon>
                        <ArrowLeft />
                    </el-icon>
                </button>
                <span>{{ monthsAgo === 0 ? '近30天' : monthsAgo === 1 ? '上月' : (formatMonthDay(displayedStartDate) + '-' +
                    formatMonthDay(displayedEndDate))
                    }}使用时长</span>
                <button @click="displayNextMonth" :disabled="monthsAgo === 0" class="flex items-center cursor-pointer">
                    <el-icon>
                        <ArrowRight />
                    </el-icon>
                </button>
            </div>
            <!-- 日均时长 -->
            <span class="font-bold text-lg">日均
                {{ formatDuration(avgDurationMs) }}
            </span>
        </div>
        <div class="w-full">
            <MonthlyUsageChart :usage="monthUsageSummary" />
        </div>
    </div>
    <div class="rounded-xl mt-4 p-4 bg-white dark:bg-neutral-800">
        <ProcessUsageList :usage="topProcessesUsage" :totalDurationMs="totalDurationMs"
            :onItemClick="onProcessItemClick" />
    </div>
</template>

<script lang="ts" setup>
import { onMounted, ref, watch, computed, type Ref } from 'vue';
import { ArrowLeft, ArrowRight } from '@element-plus/icons-vue'
import type { DateUsage, ProcessUsage, DateOnly } from "@/types";
import { dateToDateOnly, formatMonthDay } from "@/utils/date";
import { formatDuration } from "@/utils/duration";
import { useRouter } from 'vue-router';
import { useMonthlyUsageStore } from '@/stores/monthlyUsage'
import { useRoute } from 'vue-router';

import MonthlyUsageChart from '@/components/MonthlyUsageChart.vue';
import ProcessUsageList from '@/components/ProcessUsageList.vue';

const route = useRoute();
const router = useRouter();
const monthlyUsageStore = useMonthlyUsageStore();

const monthsAgo: Ref<number> = ref(parseInt(route.params.monthsAgo as any) || 0)
const displayedStartDate: Ref<Date> = ref(new Date());
const displayedEndDate: Ref<Date> = ref(new Date());

const monthUsageSummary: Ref<DateUsage[]> = ref([]);
const topProcessesUsage: Ref<ProcessUsage[]> = ref([]);
const totalDurationMs = computed(() => monthUsageSummary.value.reduce((total: number, item: DateUsage) => total + item.durationMs, 0))
const avgDurationMs = computed(() => totalDurationMs.value / monthUsageSummary.value.length)

const displayPreviousMonth = () => {
    router.push({ name: 'MonthlyScreenTime', params: { monthsAgo: (monthsAgo.value + 1).toString() } });
}

const displayNextMonth = () => {
    router.push({ name: 'MonthlyScreenTime', params: { monthsAgo: (monthsAgo.value - 1).toString() } });
}

const onProcessItemClick = (processName: string) => {
    router.push({ name: 'MonthlyProcessUsage', params: { processName, startDate: dateToDateOnly(displayedStartDate.value), endDate: dateToDateOnly(displayedEndDate.value) } });
}

const getPreviousMonthLastDay = (date: Date, n: number = 1) => {
    const result = new Date(date);
    result.setMonth(result.getMonth() + 1 - n, 1);
    result.setDate(0);
    return result;
}

const fetchData = async () => {
    if (monthsAgo.value === 0) {
        displayedEndDate.value = new Date();
        const tempDate = new Date(displayedEndDate.value);
        tempDate.setDate(tempDate.getDate() - 29);
        displayedStartDate.value = tempDate;
    }
    else {
        displayedEndDate.value = getPreviousMonthLastDay(new Date(), monthsAgo.value);
        const previousDate = new Date(displayedEndDate.value);
        previousDate.setDate(1);
        displayedStartDate.value = previousDate;
    }
    const date: DateOnly = dateToDateOnly(displayedEndDate.value);
    monthUsageSummary.value = await monthlyUsageStore.getMonthUsageSummary(date);
    topProcessesUsage.value = await monthlyUsageStore.getTopProcessessUsage(date);
}

onMounted(async () => {
    await fetchData();
})

// 监听路由变化
watch(() => route.params.monthsAgo, async (newMonthsAgo) => {
    monthsAgo.value = parseInt(newMonthsAgo as any) || 0;
    await fetchData();
});
</script>

<style scoped></style>