<template>
    <div class="rounded-xl mt-4 p-4 bg-white dark:bg-neutral-800">
        <div class="flex flex-col gap-2 items-center ">
            <div class="flex items-center border border-gray-400 rounded-full py-1 px-2">
                <button @click="displayPreviousDay" class="flex items-center cursor-pointer">
                    <el-icon>
                        <ArrowLeft />
                    </el-icon>
                </button>
                <span>{{ daysAgo === 0 ? '今日' : daysAgo === 1 ? '昨日' : formatMonthDay(displayedDate)
                }}使用时长</span>
                <button @click="displayNextDay" :disabled="daysAgo === 0" class="flex items-center cursor-pointer">
                    <el-icon>
                        <ArrowRight />
                    </el-icon>
                </button>
            </div>
            <!-- 总时长 -->
            <span class="font-bold text-lg">
                {{ formatDuration(totalDurationMs) }}
            </span>
        </div>
        <div class="w-full">
            <DailyUsageChart :usage="hourlyUsageSummary" />
        </div>
    </div>
    <div class="rounded-xl mt-4 p-4 bg-white dark:bg-neutral-800">
        <ProcessUsageList :usage="topProcessessUsage" :totalDurationMs="totalDurationMs"
            :onItemClick="onProcessItemClick" />
    </div>
</template>

<script lang="ts" setup>
import { onMounted, ref, watch, computed } from 'vue';
import type { ComputedRef, Ref } from 'vue';
import { ArrowLeft, ArrowRight } from '@element-plus/icons-vue'
import type { ProcessUsage, DateOnly } from "@/types";
import { dateToDateOnly, formatMonthDay } from "@/utils/date";
import { formatDuration } from "@/utils/duration";
import { useRouter } from 'vue-router';
import { useDailyUsageStore } from '@/stores/dailyUsage'
import { useRoute } from 'vue-router';

import DailyUsageChart from '@/components/DailyUsageChart.vue';
import ProcessUsageList from '@/components/ProcessUsageList.vue';

const route = useRoute();
const router = useRouter();
const dailyUsageStore = useDailyUsageStore();

const daysAgo: Ref<number> = ref(parseInt(route.params.daysAgo as any) || 0)
const displayedDate: Ref<Date> = ref(new Date());

const hourlyUsageSummary: Ref<number[]> = ref([]);
const topProcessessUsage: Ref<ProcessUsage[]> = ref([]);
const totalDurationMs: ComputedRef<number> = computed(() => hourlyUsageSummary.value.reduce((total: number, item: number) => total + item, 0))

const displayPreviousDay = () => {
    router.push({ name: 'DailyScreenTime', params: { daysAgo: (daysAgo.value + 1).toString() } });
}

const displayNextDay = () => {
    router.push({ name: 'DailyScreenTime', params: { daysAgo: (daysAgo.value - 1).toString() } });
}

const onProcessItemClick = (processName: string) => {
    router.push({ name: 'DailyProcessUsage', params: { processName, date: dateToDateOnly(displayedDate.value) } });
}

const fetchData = async () => {
    const previousDate = new Date();
    previousDate.setDate(previousDate.getDate() - daysAgo.value);
    displayedDate.value = previousDate;
    const date: DateOnly = dateToDateOnly(displayedDate.value);
    hourlyUsageSummary.value = await dailyUsageStore.getHourlyUsageSummary(date);
    topProcessessUsage.value = await dailyUsageStore.getTopProcessessUsage(date);
}

onMounted(async () => {
    await fetchData();
})

// 监听路由变化
watch(() => route.params.daysAgo, async (newDaysAgo) => {
    daysAgo.value = parseInt(newDaysAgo as any) || 0;
    await fetchData();
});
</script>

<style scoped></style>