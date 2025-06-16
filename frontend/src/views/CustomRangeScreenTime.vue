<template>
    <div class="w-full h-full">
        <div class="card flex flex-row gap-10 justify-center">
            <div>
                <span>起止日期：</span>
                <el-date-picker v-model="dateRange" type="daterange" start-placeholder="开始日期" end-placeholder="结束日期"
                    value-format="YYYY-M-D" />
            </div>
            <div>
                <span>显示程序的个数：</span>
                <el-input-number v-model="limit" :min="1" />
            </div>
            <div>
                <el-button type="primary" @click="query">查询</el-button>
            </div>
        </div>
        <div class="mt-4 card">
            <div class="w-full">
                <DateRangeUsageBarChart :usage="usageSummary" />
            </div>
        </div>
        <div class="mt-4 card">
            <ProcessUsageList :usage="topProcessessUsage" :totalDurationMs="totalDurationMs"
                :onItemClick="onProcessItemClick" />
        </div>
    </div>
</template>

<script lang="ts" setup>
import { onMounted, ref, watch, computed, type Ref } from 'vue';
import type { DateUsage, ProcessUsage, DateOnly } from "@/types";
import { useRouter } from 'vue-router';
import { useRoute } from 'vue-router';
import api from '@/api';

import DateRangeUsageBarChart from '@/components/DateRangeUsageBarChart.vue';
import ProcessUsageList from '@/components/ProcessUsageList.vue';

const route = useRoute();
const router = useRouter();

const displayedStartDate: Ref<DateOnly | null> = ref(route.params.startDate as any || null);
const displayedEndDate: Ref<DateOnly | null> = ref(route.params.endDate as any || null);
const limit: Ref<number> = ref(parseInt(route.params.limit as any) || 10);
const dateRange = ref([displayedStartDate.value, displayedEndDate.value])

const usageSummary: Ref<DateUsage[]> = ref([]);
const topProcessessUsage: Ref<ProcessUsage[]> = ref([]);
const totalDurationMs = computed(() => usageSummary.value.reduce((total: number, item: DateUsage) => total + item.durationMs, 0))

const onProcessItemClick = (processName: string) => {
    router.push({ name: 'DateRangeProcessUsage', params: { processName, startDate: displayedStartDate.value, endDate: displayedEndDate.value } });
}

const query = () => {
    router.push({ name: 'CustomRangeScreenTime', params: { startDate: dateRange.value[0], endDate: dateRange.value[1], limit: limit.value } });
}

const fetchData = async () => {
    if (displayedStartDate.value !== null && displayedEndDate.value !== null) {
        usageSummary.value = await api.getUsageSummaryRange(displayedStartDate.value, displayedEndDate.value);
        topProcessessUsage.value = await api.getTopProcessesUsageRange(displayedStartDate.value, displayedEndDate.value, limit.value);
    }
    else {
        usageSummary.value = [];
        topProcessessUsage.value = [];
    }
}

onMounted(async () => {
    await fetchData();
})

// 监听路由变化
watch([() => route.params.startDate, () => route.params.endDate, () => route.params.limit], async () => {
    displayedStartDate.value = route.params.startDate as any || null;
    displayedEndDate.value = route.params.endDate as any || null;
    limit.value = parseInt(route.params.limit as any) || 10;
    await fetchData();
});
</script>

<style scoped></style>