<template>
    <div class="w-full">
        <Fieldset legend="记录器">
            <div class="flex flex-col gap-3">
                <div class="flex items-center justify-between">
                    <div class="flex items-center gap-2">
                        <label class="font-bold">获取顶层窗口间隔</label>
                        <i
                            class="pi pi-question-circle"
                            v-tooltip="
                                '每隔这里设定的时间后获取一次顶层窗口对应的进程，并认为这段时间内一直使用的就是这个进程。间隔越短统计结果将越精确但对电脑资源占用增大，反正越不精确但占用小'
                            "
                        ></i>
                    </div>
                    <InputGroup class="w-40!">
                        <InputNumber
                            :min="1"
                            v-model="trackerSettings.pollingIntervalMilliseconds"
                        />
                        <InputGroupAddon>毫秒</InputGroupAddon>
                    </InputGroup>
                </div>
                <div class="flex items-center justify-between">
                    <div class="flex items-center gap-2">
                        <label class="font-bold">进程信息过期时间</label>
                        <i
                            class="pi pi-question-circle"
                            v-tooltip.top="
                                '程序会在第一次遇到某进程时获取对应的可执行文件路径、图标、描述，下次遇到这个进程时，判断上次获取信息的时间距离现在是否超过了这里设定的过期时间，如果超过了则重新获取，否则不获取，保留旧信息'
                            "
                        ></i>
                    </div>
                    <InputGroup class="w-40!">
                        <InputNumber
                            :min="1"
                            v-model="trackerSettings.processInfoStaleThresholdHours"
                        />
                        <InputGroupAddon>小时</InputGroupAddon>
                    </InputGroup>
                </div>
                <div class="flex items-center justify-between gap-10">
                    <div class="flex items-center gap-2">
                        <label class="font-bold">进程图标保存路径</label>
                        <i
                            class="pi pi-question-circle"
                            v-tooltip.top="
                                '程序自动获取的进程的图标将会保存在这里设定的文件夹路径下，修改后并不会改变已有图标的路径，只会影响新获取到的进程图标包括更新信息时获取的图标'
                            "
                        ></i>
                    </div>
                    <InputText class="flex-auto" v-model="trackerSettings.processIconDirPath" />
                </div>
                <div class="flex items-center justify-between gap-10">
                    <div class="flex items-center gap-2">
                        <label class="font-bold">启用空闲检测</label>
                        <i
                            class="pi pi-question-circle"
                            v-tooltip.top="
                                '如果启动了空闲检测，程序会在用户没有操作鼠标或键盘超过下面设定的时间后认为用户已经空闲，然后将新的记录标记为 Idle 进程的使用时长，并把从没有操作键鼠到现在这段时间也改为 Idle 进程的使用时长，直到用户再次操作键鼠'
                            "
                        ></i>
                    </div>
                    <ToggleSwitch v-model="trackerSettings.enableIdleDetection" />
                </div>
                <div class="flex items-center justify-between">
                    <label class="font-bold">空闲超时时间</label>
                    <InputGroup class="w-40!">
                        <InputNumber :min="1" v-model="trackerSettings.idleTimeoutSeconds" />
                        <InputGroupAddon>秒</InputGroupAddon>
                    </InputGroup>
                </div>
                <div class="flex justify-end gap-2">
                    <Button label="重置" @click="handleResetTrackerSettings" severity="warn" />
                    <Button label="保存" @click="handleSaveTrackerSettings" />
                </div>
            </div>
        </Fieldset>
        <Fieldset legend="聚合">
            <div class="flex flex-col gap-3">
                <div class="flex items-center justify-between">
                    <div class="flex items-center gap-2">
                        <label class="font-bold">聚合间隔</label>
                        <i class="pi pi-question-circle" v-tooltip.top="'不建议修改该值'"></i>
                    </div>
                    <InputGroup class="w-40!">
                        <InputNumber
                            :min="1"
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

function handleResetTrackerSettings() {
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
