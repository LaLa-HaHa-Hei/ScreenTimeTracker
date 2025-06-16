<template>
    <div class="flex flex-col gap-1">
        <div class="flex">
            <img v-if="processInfo?.iconPath" :src='config.baseUrl + processInfo.iconPath' :width="imgWidth"
                :height="imgHeight" alt="丢失" />
            <IconDefaultFileIcon v-if="!processInfo?.iconPath" :width="imgWidth" :height="imgHeight" />
            <span class="ml-2 text-lg font-bold">{{ processInfo?.processName }}</span>
        </div>
        <div class="flex justify-center items-center">
            <span>别名：</span>
            <el-input class="flex-1" v-model="newAlias" placeholder="按回车保存" clearable @keyup.enter="saveAlias" />
        </div>
        <div>
            <span>描述：</span>
            <span>{{ processInfo?.description }}</span>
        </div>
        <div>
            <span>路径：</span>
            <span>{{ processInfo?.executablePath }}</span>
        </div>
    </div>
</template>

<script setup lang="ts">
import { defineProps, ref, onMounted, onUnmounted, watch, computed } from 'vue';
import type { Ref } from 'vue';
import { useProcessStore } from '@/stores/process';
import type { ProcessInfo } from '@/types';
import config from '@/config';
import { ElMessage } from 'element-plus'
import IconDefaultFileIcon from '@/components/icons/IconDefaultFileIcon.vue'

const props = defineProps<{
    processName: string;
}>();

const imgWidth = '45px';
const imgHeight = '45px';
const processStore = useProcessStore();
const processInfo: Ref<ProcessInfo | null | undefined> = ref()
const newAlias: Ref<string> = ref('')

const saveAlias = async () => {
    processStore.updateAlias(props.processName || '', newAlias.value)
    ElMessage({
        message: '保存成功',
        type: 'success',
    })
}

onMounted(async () => {
    processInfo.value = await processStore.getProcessInfo(props.processName)
    newAlias.value = processInfo.value?.alias || ''
})
</script>

<style scoped></style>