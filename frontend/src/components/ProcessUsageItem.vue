<template>
    <div class="rounded-lg flex flex-row p-3 gap-3 hover:bg-gray-100 dark:hover:bg-neutral-900">
        <img v-if="processInfo?.iconPath" :src='config.baseUrl + processInfo.iconPath' :width="imgWidth"
            :height="imgHeight" alt="丢失" />
        <IconDefaultFileIcon v-if="!processInfo?.iconPath" :width="imgWidth" :height="imgHeight" />
        <div class="flex-1 flex flex-col">
            <div class="flex items-center justify-between">
                <span>{{ processInfo?.alias || processUsage.processName }}</span>
                <span>{{ formatDuration(processUsage.durationMs) }}</span>
            </div>
            <el-progress class="mt-2" :percentage="percentage <= 100 ? percentage : 100" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed, type Ref } from 'vue';
import type { ProcessUsage, ProcessInfo } from '@/types';
import { useProcessStore } from '@/stores/process';
import { formatDuration } from '@/utils/duration';
import config from '@/config';
import IconDefaultFileIcon from './icons/IconDefaultFileIcon.vue';

const processStore = useProcessStore();
const props = defineProps<{
    processUsage: ProcessUsage;
    totaledDurationMs: number;
}>();

const imgWidth = '45px';
const imgHeight = '45px';
const processInfo: Ref<ProcessInfo | null | undefined> = ref()
const percentage = computed(() => Math.floor((props.processUsage.durationMs / props.totaledDurationMs) * 100))

onMounted(async () => {
    processInfo.value = await processStore.getProcessInfo(props.processUsage.processName)
})
</script>

<style scoped></style>