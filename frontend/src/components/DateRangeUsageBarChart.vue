<template>
    <UsageBarChart :xValues="xValues" :yValues="yValues" :yFormater="(value) => formatDuration(value)"
        :tooltipFormatter="(axisValue, data) => `${axisValue}<br />${formatDuration(data)}`" />
</template>

<script setup lang="ts">
import UsageBarChart from './UsageBarChart.vue';
import { formatDuration } from "@/utils";
import { computed } from 'vue';
import type { DateUsage } from "@/types";
import { formatMonthDay, dateOnlyToDate } from "@/utils";


const props = defineProps<{
    usage: DateUsage[];
}>();


const xValues = computed(() =>
    props.usage.map(i => formatMonthDay(dateOnlyToDate(i.date)))
);
const yValues = computed(() =>
    props.usage.map(i => i.durationMs)
);
</script>