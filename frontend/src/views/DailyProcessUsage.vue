<template>
    <div class="card flex flex-col items-center">
        <ProcessDetailInfo class="mt-3" :processName="processName" />
        <DailyUsageBarChart :usage="processDailyUsage" />
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, type Ref } from 'vue';
import { useRoute } from 'vue-router';
import type { DateUsage, ProcessUsage, DateOnly } from "@/types";
import DailyUsageBarChart from "@/components/DailyUsageBarChart.vue";
import ProcessDetailInfo from "@/components/ProcessDetailInfo.vue";
import api from "@/api";

interface RouteParams {
    processName: string;
    date: DateOnly;
}
const route = useRoute();
const { processName, date } = route.params as any as RouteParams;
const processDailyUsage: Ref<number[]> = ref([]);

onMounted(async () => {
    processDailyUsage.value = await api.getProcessDailyUsage(processName, date);
})
</script>

<style scoped></style>