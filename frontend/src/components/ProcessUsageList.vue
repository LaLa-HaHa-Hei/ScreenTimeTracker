<template>
    <div class="list">
        <ProcessUsageItem v-for="processUsage in props.usage" @click="props.onItemClick(processUsage.processName)"
            :key="processUsage.processName" :processUsage="processUsage" :totaledDurationMs="props.totalDurationMs" />
    </div>
</template>

<script setup lang="ts">
import type { ProcessUsage } from "@/types";
import ProcessUsageItem from "./ProcessUsageItem.vue";

const props = defineProps<{
    usage: ProcessUsage[] | null | undefined;
    totalDurationMs: number;
    onItemClick: (processName: string) => void;
}>();
</script>

<style scoped></style>