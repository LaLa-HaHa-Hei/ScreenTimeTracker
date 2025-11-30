<template>
    <div class="w-full">
        <Fieldset legend="记录器">
            <div class="flex flex-col gap-3">
                <div class="flex items-center justify-between">
                    <label for="pollingInterval" class="font-bold">获取顶层窗口间隔</label>
                    <InputGroup class="w-40!">
                        <InputNumber
                            :min="1"
                            id="pollingInterval"
                            v-model="trackerSettings.pollingIntervalMilliseconds"
                        />
                        <InputGroupAddon>毫秒</InputGroupAddon>
                    </InputGroup>
                </div>
                <div class="flex items-center justify-between">
                    <label for="processInfoStaleThreshold" class="font-bold"
                        >进程信息过期时间</label
                    >
                    <InputGroup class="w-40!">
                        <InputNumber
                            :min="1"
                            id="processInfoStaleThreshold"
                            v-model="trackerSettings.processInfoStaleThresholdHours"
                        />
                        <InputGroupAddon>小时</InputGroupAddon>
                    </InputGroup>
                </div>
                <div class="flex items-center justify-between gap-10">
                    <label for="processIconDirPath" class="font-bold">进程图标保存路径</label>
                    <InputText
                        class="flex-auto"
                        id="processIconDirPath"
                        v-model="trackerSettings.processIconDirPath"
                    />
                </div>
                <div class="flex items-center justify-between gap-10">
                    <label for="enableIdleDetection" class="font-bold">启用空闲检测</label>
                    <ToggleSwitch
                        id="enableIdleDetection"
                        v-model="trackerSettings.enableIdleDetection"
                    />
                </div>
                <div class="flex items-center justify-between">
                    <label for="idleTimeout" class="font-bold">空闲超时时间</label>
                    <InputGroup class="w-40!">
                        <InputNumber
                            :min="1"
                            id="idleTimeout"
                            v-model="trackerSettings.idleTimeoutSeconds"
                        />
                        <InputGroupAddon>秒</InputGroupAddon>
                    </InputGroup>
                </div>
                <div class="flex justify-end gap-2">
                    <Button label="重置" @click="handlReresetTrackerSettings" severity="warn" />
                    <Button label="保存" @click="handleSaveTrackerSettings" />
                </div>
            </div>
        </Fieldset>
        <Fieldset legend="聚合">
            <div class="flex flex-col gap-3">
                <div class="flex items-center justify-between">
                    <label for="pollingIntervalHours" class="font-bold">聚合间隔</label>
                    <InputGroup class="w-40!">
                        <InputNumber
                            :min="1"
                            id="pollingInterval"
                            v-model="aggregationSettings.pollingIntervalMinutes"
                        />
                        <InputGroupAddon>分钟</InputGroupAddon>
                    </InputGroup>
                </div>
                <div class="flex justify-end gap-2">
                    <Button label="重置" @click="handleResetAggregationSettings" severity="warn" />
                    <Button label="保存" @click="handleSaveAggregationSettings" />
                </div>
            </div>
        </Fieldset>
    </div>
</template>

<script setup lang="ts">
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import type { AggregationSettings, TrackerSettings } from '@/types'
import { onBeforeMount, ref } from 'vue'
import {
    getTrackerSettings,
    getAggregationSettings,
    updateTrackerSettings,
    resetTrackerSettings,
    updateAggregationSettings,
    resetAggregationSettings,
} from '@/api'

const toast = useToast()
const confirm = useConfirm()

const trackerSettings = ref<TrackerSettings>({
    pollingIntervalMilliseconds: -1,
    processInfoStaleThresholdHours: -1,
    processIconDirPath: 'Error',
    enableIdleDetection: true,
    idleTimeoutSeconds: -1,
})
const aggregationSettings = ref<AggregationSettings>({
    pollingIntervalMinutes: -1,
})

function handlReresetTrackerSettings() {
    confirm.require({
        message: '你确定要重置所有记录器配置吗？',
        header: '确认重置',
        icon: 'pi pi-info-circle',
        rejectProps: {
            label: '取消',
            severity: 'secondary',
        },
        acceptProps: {
            label: '确认',
            severity: 'warn',
        },
        accept: () => {
            resetTrackerSettings().then((result) => {
                if (result.status == 204) {
                    toast.add({
                        severity: 'success',
                        summary: '成功',
                        detail: '记录器配置重置',
                        life: 3000,
                    })
                }
                loadData()
            })
        },
        reject: () => {},
    })
}

function handleSaveTrackerSettings() {
    updateTrackerSettings(trackerSettings.value).then((result) => {
        if (result.status == 204) {
            toast.add({
                severity: 'success',
                summary: '成功',
                detail: '记录器配置更新',
                life: 3000,
            })
        }
        loadData()
    })
}

function handleResetAggregationSettings() {
    confirm.require({
        message: '你确定要重置所有聚合配置吗？',
        header: '确认重置',
        icon: 'pi pi-info-circle',
        rejectProps: {
            label: '取消',
            severity: 'secondary',
        },
        acceptProps: {
            label: '确认',
            severity: 'warn',
        },
        accept: () => {
            resetAggregationSettings().then((result) => {
                if (result.status == 204) {
                    toast.add({
                        severity: 'success',
                        summary: '成功',
                        detail: '聚合配置重置',
                        life: 3000,
                    })
                }
                loadData()
            })
        },
        reject: () => {},
    })
}
function handleSaveAggregationSettings() {
    updateAggregationSettings(aggregationSettings.value).then((result) => {
        if (result.status == 204) {
            toast.add({
                severity: 'success',
                summary: '成功',
                detail: '聚合配置更新',
                life: 3000,
            })
        }
        loadData()
    })
}

async function loadData() {
    let res = await getTrackerSettings()
    trackerSettings.value = res.data
    res = await getAggregationSettings()
    aggregationSettings.value = res.data
}

onBeforeMount(async () => {
    await loadData()
})
</script>

<style></style>
