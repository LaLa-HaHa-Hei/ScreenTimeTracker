<template>
    <div class="card flex flex-col items-center">
        <ProcessDetailInfo class="mt-3" :processName="processName" />
        <WeeklyUsageBarChart :usage="processDateRangeUsage" />
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, type Ref } from 'vue';
import { useRoute } from 'vue-router';
import type { DateUsage, ProcessUsage, DateOnly } from "@/types";
import WeeklyUsageBarChart from "@/components/WeeklyUsageBarChart.vue";
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