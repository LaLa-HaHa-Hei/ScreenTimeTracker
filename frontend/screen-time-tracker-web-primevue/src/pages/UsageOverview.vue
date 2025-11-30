<template>
    <div class="w-full">
        <div class="flex flex-row justify-between">
            <SelectButton
                v-model="selectedTimeRange"
                optionLabel="label"
                :options="timeRangeOptions"
                :allowEmpty="false"
            />
            <span>
                <label class="mr-2">排除的进程</label>
                <MultiSelect v-model="excludedProcesses" :options="processes" filter>
                    <template #option="slotProps">
                        <div class="ml-2 flex items-center gap-2">
                            <Image
                                :src="
                                    slotProps.option.iconPath
                                        ? getProcessIconUrl(slotProps.option.id)
                                        : defaultFileIcon
                                "
                                alt="Icon"
                                imageClass=" w-5 h-5"
                            />
                            <div>{{ slotProps.option.alias || slotProps.option.name }}</div>
                        </div>
                    </template>
                    <template #value="slotProps">
                        <div v-if="slotProps.value?.length" class="flex items-center">
                            {{
                                slotProps.value.length > 3
                                    ? `${slotProps.value.length} 项被选中`
                                    : slotProps.value
                                          .map((item: ProcessInfo) => item.alias || item.name)
                                          .join(', ')
                            }}
                        </div>
                    </template>
                </MultiSelect>
            </span>
        </div>
        <div class="mt-2 flex justify-center">
            <Stepper
                v-show="selectedTimeRange?.value !== 'Custom'"
                class="w-70"
                :text="
                    selectedTimeRange?.value === 'Daily'
                        ? dailyText
                        : selectedTimeRange?.value === 'Weekly'
                          ? weeklyText
                          : monthlyText
                "
                :onLeftClick="handlePreviousClick"
                :onRightClick="handleNextClick"
            />
            <DatePicker
                v-show="selectedTimeRange?.value === 'Custom'"
                class="w-70!"
                fluid
                dateFormat="yy/mm/dd"
                v-model="customDateRange"
                selectionMode="range"
                :manualInput="true"
                placeholder="选择日期范围"
            />
        </div>
        <UsageBarChart
            class="mt-5"
            :startDate="startDate"
            :endDate="endDate"
            :xAxisType="
                selectedTimeRange?.value === 'Daily'
                    ? 'Hour'
                    : selectedTimeRange?.value === 'Weekly'
                      ? 'Week'
                      : 'Day'
            "
            mode="Total"
            :excludedProcesses="excludedProcessIds"
        />

        <div class="mt-5 flex justify-end">
            <Select v-model="topN" :options="[5, 10, 15, 20]" />
        </div>
        <ProcessUsageRank
            class="mt-2"
            :startDate="startDate"
            :endDate="endDate"
            :topN="topN"
            :excludedProcesses="excludedProcessIds"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, watch, computed, onBeforeMount } from 'vue'
import type { ProcessInfo } from '@/types'
import { getAllProcesses, getProcessIconUrl } from '@/api'
import defaultFileIcon from '@/assets/defaultFileIcon.svg'
import { StorageKey } from '@/constants/storageKeys'
import ProcessUsageRank from '@/components/ProcessUsageRank.vue'
import UsageBarChart from '@/components/UsageBarChart.vue'
import Stepper from '@/components/Stepper.vue'
import { SpecialProcessId } from '@/constants/specialProcess'

const timeRangeOptions = [
    { label: '日', value: 'Daily' },
    { label: '周', value: 'Weekly' },
    { label: '月', value: 'Monthly' },
    { label: '自定义', value: 'Custom' },
]
const selectedTimeRange = ref(timeRangeOptions[0])
const topN = ref(10)
const processes = ref<ProcessInfo[]>([])
const excludedProcesses = ref<ProcessInfo[]>([])
const excludedProcessIds = computed(() => excludedProcesses.value.map((p) => p.id))
const today = ref(new Date())

const totalDayMs = 24 * 60 * 60 * 1000
const startDate = computed(() => {
    if (selectedTimeRange.value?.value === 'Daily') return new Date(dailyDay.value)
    else if (selectedTimeRange.value?.value === 'Weekly') return new Date(weeklyStartDay.value)
    else if (selectedTimeRange.value?.value === 'Monthly') return new Date(monthlyStartDay.value)
    else return customDateRange.value?.[0] || today.value
})
const endDate = computed(() => {
    if (selectedTimeRange.value?.value === 'Daily') return new Date(dailyDay.value)
    else if (selectedTimeRange.value?.value === 'Weekly') return new Date(weeklyEndDay.value)
    else if (selectedTimeRange.value?.value === 'Monthly') return new Date(monthlyEndDay.value)
    else return customDateRange.value?.[1] || today.value
})
// 日
const dailyDayDiff = ref(0)
const dailyDay = computed(() => new Date(today.value.getTime() - totalDayMs * dailyDayDiff.value))
const dailyText = computed(() => {
    if (dailyDayDiff.value === 0) return '今天'
    else if (dailyDayDiff.value === 1) return '昨天'
    else return `${dailyDay.value.getMonth() + 1}/${dailyDay.value.getDate()}`
})
// 周
const weeklyWeekDiff = ref(0)
const weeklyEndDay = computed(() =>
    weeklyWeekDiff.value === 0
        ? today.value
        : getPreviousWeekLastDay(today.value, weeklyWeekDiff.value),
)
const weeklyStartDay = computed(() => new Date(weeklyEndDay.value.getTime() - 6 * totalDayMs))
const weeklyText = computed(() => {
    if (weeklyWeekDiff.value === 0) return '近7天'
    else if (weeklyWeekDiff.value === 1) return '上周'
    else
        return `${weeklyStartDay.value.getMonth() + 1}/${weeklyStartDay.value.getDate()} - ${weeklyEndDay.value.getMonth() + 1}/${weeklyEndDay.value.getDate()}`
})
// 月
const monthlyMonthDiff = ref(0)
const monthlyEndDay = computed(() =>
    monthlyMonthDiff.value === 0
        ? today.value
        : getPreviousMonthLastDay(today.value, monthlyMonthDiff.value),
)
const monthlyStartDay = computed(() =>
    monthlyMonthDiff.value === 0
        ? new Date(today.value.getTime() - 31 * totalDayMs)
        : getFirstDayOfMonth(monthlyEndDay.value),
)
const monthlyText = computed(() => {
    if (monthlyMonthDiff.value === 0) return '近31天'
    else if (monthlyMonthDiff.value === 1) return '上月'
    else
        return `${monthlyStartDay.value.getMonth() + 1}/${monthlyStartDay.value.getDate()} - ${monthlyEndDay.value.getMonth() + 1}/${monthlyEndDay.value.getDate()}`
})
// 自定义
const customDateRange = ref()

function handlePreviousClick() {
    if (selectedTimeRange.value?.value === 'Daily') {
        dailyDayDiff.value++
    } else if (selectedTimeRange.value?.value === 'Weekly') {
        weeklyWeekDiff.value++
    } else if (selectedTimeRange.value?.value === 'Monthly') {
        monthlyMonthDiff.value++
    }
}

function handleNextClick() {
    if (selectedTimeRange.value?.value === 'Daily') {
        dailyDayDiff.value = dailyDayDiff.value > 1 ? dailyDayDiff.value - 1 : 0
    } else if (selectedTimeRange.value?.value === 'Weekly') {
        weeklyWeekDiff.value = weeklyWeekDiff.value > 1 ? weeklyWeekDiff.value - 1 : 0
    } else if (selectedTimeRange.value?.value === 'Monthly') {
        monthlyMonthDiff.value = monthlyMonthDiff.value > 1 ? monthlyMonthDiff.value - 1 : 0
    }
}

function getPreviousMonthLastDay(date: Date, n: number = 1) {
    return new Date(date.getFullYear(), date.getMonth() - n + 1, 0)
}

function getFirstDayOfMonth(date: Date) {
    return new Date(date.getFullYear(), date.getMonth(), 1)
}

function getPreviousWeekLastDay(date: Date, n: number = 1) {
    const result = new Date(date)
    const dayOfWeek = result.getDay()
    const daysToSubtract = (dayOfWeek === 0 ? 7 : dayOfWeek) + 7 * (n - 1)
    result.setDate(result.getDate() - daysToSubtract)
    return result
}

async function loadData() {
    today.value = new Date()
    const res = await getAllProcesses()
    processes.value = res.data
    const exclude = localStorage.getItem(StorageKey.USAGE_OVERVIEW_EXCLUDED_PROCESSES)
    const ids = exclude === null ? SpecialProcessId.Idle : exclude.split(',') || []
    excludedProcesses.value = processes.value.filter((p) => ids.includes(p.id))
}

watch(excludedProcessIds, () => {
    localStorage.setItem(
        StorageKey.USAGE_OVERVIEW_EXCLUDED_PROCESSES,
        excludedProcessIds.value.join(','),
    )
})

onBeforeMount(async () => {
    await loadData()
})
</script>

<style scoped>
.time-stepper {
    display: flex;
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
    width: 250px;
    max-width: 100%;
    border-radius: 9999px;
    border: 1px solid var(--p-content-border-color);
}
</style>
