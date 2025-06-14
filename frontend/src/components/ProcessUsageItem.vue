<template>
    <div class="rounded-lg flex flex-row p-3 gap-3 hover:bg-gray-100 dark:hover:bg-neutral-900">
        <img :src='config.baseUrl + (processInfo?.iconPath ? processInfo.iconPath : "/defaultIcon.png")' alt="未知" />
        <div class="flex-1 flex flex-col">
            <div class="flex items-center justify-between">
                <span>{{ processInfo?.alias || processInfo?.processName }}</span>
                <span>{{ formatDuration(processUsage.durationMs) }}</span>
            </div>
            <el-progress class="mt-2" :percentage="percentage <= 100 ? percentage : 100" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { onMounted, ref, type Ref } from 'vue';
import type { ProcessUsage, ProcessInfo } from '@/types';
import { useProcessStore } from '@/stores/process';
import { formatDuration } from '@/utils/duration';
import config from '@/config';

const processStore = useProcessStore();
const props = defineProps<{
    processUsage: ProcessUsage;
    totaledDurationMs: number;
}>();

const processInfo: Ref<ProcessInfo | null | undefined> = ref()
const percentage = Math.floor((props.processUsage.durationMs / props.totaledDurationMs) * 100);

onMounted(async () => {
    processInfo.value = await processStore.getProcessInfo(props.processUsage.processName)
})
</script>

<style scoped>
img {
    width: 45px;
    height: 45px;
}
</style>