<template>
    <div class="rounded-xl mt-4 p-4 bg-white dark:bg-neutral-800">
        <div class="flex flex-col gap-2 items-center ">
            <div class="flex items-center border border-gray-400 rounded-full py-1 px-2">
                <button @click="displayPreviousWeek" class="flex items-center cursor-pointer">
                    <el-icon>
                        <ArrowLeft />
                    </el-icon>
                </button>
                <span>{{ weeksAgo === 0 ? '近7天' : weeksAgo === 1 ? '上周' : (formatMonthDay(displayedStartDate) + '-' +
                    formatMonthDay(displayedEndDate))
                }}使用时长</span>
                <button @click="displayNextWeek" :disabled="weeksAgo === 0" class="flex items-center cursor-pointer">
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
            <WeeklyUsageChart :usage="weekUsageSummary" />
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
import { useWeeklyUsageStore } from '@/stores/weeklyUsage'
import { useRoute } from 'vue-router';

import WeeklyUsageChart from '@/components/WeeklyUsageChart.vue';
import ProcessUsageList from '@/components/ProcessUsageList.vue';

const route = useRoute();
const router = useRouter();
const weeklyUsageStore = useWeeklyUsageStore();

const weeksAgo: Ref<number> = ref(parseInt(route.params.weeksAgo as any) || 0)
const displayedStartDate: Ref<Date> = ref(new Date());
const displayedEndDate: Ref<Date> = ref(new Date());

const weekUsageSummary: Ref<DateUsage[]> = ref([]);
const topProcessesUsage: Ref<ProcessUsage[]> = ref([]);
const totalDurationMs = computed(() => weekUsageSummary.value.reduce((total: number, item: DateUsage) => total + item.durationMs, 0))
const avgDurationMs = computed(() => totalDurationMs.value / weekUsageSummary.value.length)

const displayPreviousWeek = () => {
    router.push({ name: 'WeeklyScreenTime', params: { weeksAgo: (weeksAgo.value + 1).toString() } });
}

const displayNextWeek = () => {
    router.push({ name: 'WeeklyScreenTime', params: { weeksAgo: (weeksAgo.value - 1).toString() } });
}

const onProcessItemClick = (processName: string) => {
    router.push({ name: 'WeeklyProcessUsage', params: { processName, startDate: dateToDateOnly(displayedStartDate.value), endDate: dateToDateOnly(displayedEndDate.value) } });
}

const getPreviousSunday = (date: Date, n = 1) => {
    const result = new Date(date);
    const dayOfWeek = result.getDay();
    const daysToSubtract = (dayOfWeek === 0 ? 7 : dayOfWeek) + 7 * (n - 1);
    result.setDate(result.getDate() - daysToSubtract);
    return result;
}

const fetchData = async () => {
    if (weeksAgo.value === 0)
        displayedEndDate.value = new Date();
    else
        displayedEndDate.value = getPreviousSunday(new Date(), weeksAgo.value);
    const previousDate = new Date();
    previousDate.setDate(displayedEndDate.value.getDate() - 6);
    displayedStartDate.value = previousDate;
    const date: DateOnly = dateToDateOnly(displayedEndDate.value);
    weekUsageSummary.value = await weeklyUsageStore.getWeekUsageSummary(date);
    topProcessesUsage.value = await weeklyUsageStore.getTopProcessessUsage(date);
}

onMounted(async () => {
    await fetchData();
})

// 监听路由变化
watch(() => route.params.weeksAgo, async (newWeeksAgo) => {
    weeksAgo.value = parseInt(newWeeksAgo as any) || 0;
    await fetchData();
});
</script>

<style scoped></style>