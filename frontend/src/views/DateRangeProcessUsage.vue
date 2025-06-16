<template>
    <div class="rounded-xl p-4 w-full flex flex-col justify-center items-center bg-white dark:bg-neutral-800">
        <ProcessDetailInfo class="mt-3" :processName="processName" />
        <DateRangeUsageBarChart :usage="processDateRangeUsage" />
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, type Ref } from 'vue';
import { useRoute } from 'vue-router';
import type { DateUsage, DateOnly } from "@/types";
import DateRangeUsageBarChart from '@/components/DateRangeUsageBarChart.vue';
import ProcessDetailInfo from "@/components/ProcessDetailInfo.vue";
import api from "@/api";

interface RouteParams {
    processName: string;
    startDate: DateOnly;
    endDate: DateOnly;
}
const route = useRoute();
const { processName, startDate, endDate } = route.params as any as RouteParams;
const processDateRangeUsage: Ref<DateUsage[]> = ref([]);

onMounted(async () => {
    processDateRangeUsage.value = await api.getProcessUsageRange(processName, startDate, endDate);
})
</script>

<style scoped></style>