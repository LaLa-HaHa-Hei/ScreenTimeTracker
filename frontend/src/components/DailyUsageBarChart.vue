<template>
    <UsageBarChart :xValues="xValues" :yValues="usage" :yFormater="(value) => formatDuration(value)"
        :tooltipFormatter="(axisValue, data) => `${axisValue}~${parseInt(axisValue) + 1} 点<br/>${formatDuration(data)}`"
        :maxY="60 * 60 * 1000" />
</template>

<script setup lang="ts">
import UsageBarChart from './UsageBarChart.vue';
import { formatDuration } from "@/utils";


const props = defineProps<{
    usage: number[];
}>();

const xValues = Array.from({ length: 24 }, (_, i) => i.toString());
</script>