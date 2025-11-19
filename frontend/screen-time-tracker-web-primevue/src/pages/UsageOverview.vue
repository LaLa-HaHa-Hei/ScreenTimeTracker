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
                <MultiSelect
                    v-model="excludedProcesses"
                    :options="processes"
                    optionLabel="name"
                    filter
                    :maxSelectedLabels="3"
                />
            </span>
        </div>
        <div class="mt-2 flex justify-center">
            <Stepper
                v-show="selectedTimeRange?.value === 'Daily'"
                class="w-70"
                :text="dailyText"
                :onLeftClick="displayPreviousDay"
                :onRightClick="displayNextDay"
            />
            <Stepper
                v-show="selectedTimeRange?.value === 'Weekly'"
                class="w-70"
                :text="weeklyText"
                :onLeftClick="displayPreviousWeek"
                :onRightClick="displayNextWeek"
            />
            <Stepper
                v-show="selectedTimeRange?.value === 'Monthly'"
                class="w-70"
                :text="monthlyText"
                :onLeftClick="displayPreviousMonth"
                :onRightClick="displayNextMonth"
            />
            <div v-show="selectedTimeRange?.value === 'Custom'" class="w-70">
                <DatePicker
                    fluid
                    dateFormat="yy/mm/dd"
                    v-model="customDateRange"
                    selectionMode="range"
                    :manualInput="true"
                    placeholder="选择日期范围"
                />
            </div>
        </div>
        <!-- 数据展示区 -->
        <TotalUsageChart
            class="mt-5"
            :startDate="startDate"
            :endDate="endDate"
            :mode="
                selectedTimeRange?.value === 'Daily'
                    ? 'Hour'
                    : selectedTimeRange?.value === 'Weekly'
                      ? 'Week'
                      : 'Day'
            "
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
import { ref, onMounted, watch, computed } from 'vue'
import type { ProcessInfo } from '@/types'
import { getAllProcesses } from '@/api'
import { StorageKey } from '@/constants/storageKeys'
import ProcessUsageRank from '@/components/ProcessUsageRank.vue'
import TotalUsageChart from '@/components/TotalUsageChart.vue'
import Stepper from '@/components/Stepper.vue'

const timeRangeOptions = [
    { label: '日', value: 'Daily' },
    { label: '周', value: 'Weekly' },
    { label: '月', value: 'Monthly' },
    { label: '自定义', value: 'Custom' },
]
const selectedTimeRange = ref(timeRangeOptions[0])
const topN = ref(Number(localStorage.getItem(StorageKey.USAGE_OVERVIEW_TOP_N) || 10))
const processes = ref<{ name: string; id: string }[]>([])
const excludedProcesses = ref<{ name: string; id: string }[]>([])
const excludedProcessIds = ref<string[]>([])
const today = new Date()
const startDate = ref(new Date())
const endDate = ref(new Date())

const dailyDayDiff = ref(0)
const dailyDay = ref(new Date())
const dailyText = computed(() => {
    if (dailyDayDiff.value === 0) return '今天'
    else if (dailyDayDiff.value === 1) return '昨天'
    else return `${dailyDay.value.getMonth() + 1}/${dailyDay.value.getDate()}`
})

const weeklyWeekDiff = ref(0)
const weeklyEndDay = ref(new Date())
const weeklyStartDay = ref(new Date(weeklyEndDay.value.getTime() - 6 * 24 * 60 * 60 * 1000))
const weeklyText = computed(() => {
    if (weeklyWeekDiff.value === 0) return '本周'
    else if (weeklyWeekDiff.value === 1) return '上周'
    else
        return `${weeklyStartDay.value.getMonth() + 1}/${weeklyStartDay.value.getDate()} - ${weeklyEndDay.value.getMonth() + 1}/${weeklyEndDay.value.getDate()}`
})

const monthlyMonthDiff = ref(0)
const monthlyEndDay = ref(new Date())
const monthlyStartDay = ref(
    isLastDayOfMonth(monthlyEndDay.value)
        ? getFirstDayOfMonth(monthlyEndDay.value)
        : new Date(monthlyEndDay.value.getTime() - 30 * 24 * 60 * 60 * 1000),
)
const monthlyText = computed(() => {
    if (monthlyMonthDiff.value === 0) return '本月'
    else if (monthlyMonthDiff.value === 1) return '上月'
    else
        return `${monthlyStartDay.value.getMonth() + 1}/${monthlyStartDay.value.getDate()} - ${monthlyEndDay.value.getMonth() + 1}/${monthlyEndDay.value.getDate()}`
})

const customDateRange = ref([
    new Date(localStorage.getItem(StorageKey.USAGE_OVERVIEW_CUSTOM_START_DATE) || Date.now()),
    new Date(localStorage.getItem(StorageKey.USAGE_OVERVIEW_CUSTOM_END_DATE) || Date.now()),
])

watch(customDateRange, () => {
    if (!customDateRange.value[0] || !customDateRange.value[1]) return
    startDate.value = customDateRange.value[0]
    endDate.value = customDateRange.value[1]
    localStorage.setItem(
        StorageKey.USAGE_OVERVIEW_CUSTOM_START_DATE,
        customDateRange.value[0].toISOString(),
    )
    localStorage.setItem(
        StorageKey.USAGE_OVERVIEW_CUSTOM_END_DATE,
        customDateRange.value[1].toISOString(),
    )
})

function displayPreviousMonth() {
    monthlyMonthDiff.value++
    monthlyEndDay.value = getPreviousMonthLastDay(today, monthlyMonthDiff.value)
    monthlyStartDay.value = getFirstDayOfMonth(monthlyEndDay.value)
    startDate.value = new Date(monthlyStartDay.value)
    endDate.value = new Date(monthlyEndDay.value)
}

function displayNextMonth() {
    if (monthlyMonthDiff.value === 0) return
    monthlyMonthDiff.value--
    if (monthlyMonthDiff.value === 0) {
        monthlyEndDay.value = new Date()
        monthlyStartDay.value = isLastDayOfMonth(monthlyEndDay.value)
            ? getFirstDayOfMonth(monthlyEndDay.value)
            : new Date(monthlyEndDay.value.getTime() - 30 * 24 * 60 * 60 * 1000)
    } else {
        monthlyEndDay.value = getPreviousMonthLastDay(today, monthlyMonthDiff.value)
        monthlyStartDay.value = getFirstDayOfMonth(monthlyEndDay.value)
    }
    startDate.value = new Date(monthlyStartDay.value)
    endDate.value = new Date(monthlyEndDay.value)
}

function isLastDayOfMonth(date: Date) {
    const nextDay = new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1)
    return nextDay.getDate() === 1
}

function getPreviousMonthLastDay(date: Date, n: number = 1) {
    return new Date(date.getFullYear(), date.getMonth() - n + 1, 0)
}

function getFirstDayOfMonth(date: Date) {
    return new Date(date.getFullYear(), date.getMonth(), 1)
}

function getPreviousSunday(date: Date, n: number = 1) {
    const result = new Date(date)
    const dayOfWeek = result.getDay()
    const daysToSubtract = (dayOfWeek === 0 ? 7 : dayOfWeek) + 7 * (n - 1)
    result.setDate(result.getDate() - daysToSubtract)
    return result
}

function displayPreviousWeek() {
    weeklyWeekDiff.value++
    weeklyEndDay.value = getPreviousSunday(today, weeklyWeekDiff.value)
    weeklyStartDay.value = new Date(
        weeklyEndDay.value.getFullYear(),
        weeklyEndDay.value.getMonth(),
        weeklyEndDay.value.getDate() - 6,
    )
    startDate.value = new Date(weeklyStartDay.value)
    endDate.value = new Date(weeklyEndDay.value)
}

function displayNextWeek() {
    if (weeklyWeekDiff.value === 0) return
    weeklyWeekDiff.value--
    if (weeklyWeekDiff.value === 0) {
        weeklyEndDay.value = new Date()
        weeklyStartDay.value = new Date(weeklyEndDay.value.getTime() - 6 * 24 * 60 * 60 * 1000)
    } else {
        weeklyEndDay.value = getPreviousSunday(today, weeklyWeekDiff.value)
        weeklyStartDay.value = new Date(
            weeklyEndDay.value.getFullYear(),
            weeklyEndDay.value.getMonth(),
            weeklyEndDay.value.getDate() - 6,
        )
    }
    startDate.value = new Date(weeklyStartDay.value)
    endDate.value = new Date(weeklyEndDay.value)
}

function displayPreviousDay() {
    dailyDayDiff.value++
    dailyDay.value = new Date(
        today.getFullYear(),
        today.getMonth(),
        today.getDate() - dailyDayDiff.value,
    )
    startDate.value = new Date(dailyDay.value)
    endDate.value = new Date(dailyDay.value)
}

function displayNextDay() {
    if (dailyDayDiff.value === 0) return
    dailyDayDiff.value--
    dailyDay.value = new Date(
        today.getFullYear(),
        today.getMonth(),
        today.getDate() - dailyDayDiff.value,
    )
    startDate.value = new Date(dailyDay.value)
    endDate.value = new Date(dailyDay.value)
}

watch(selectedTimeRange, () => {
    if (selectedTimeRange.value?.value === 'Daily') {
        startDate.value = new Date(dailyDay.value)
        endDate.value = new Date(dailyDay.value)
    } else if (selectedTimeRange.value?.value === 'Weekly') {
        startDate.value = new Date(weeklyStartDay.value)
        endDate.value = new Date(weeklyEndDay.value)
    } else if (selectedTimeRange.value?.value === 'Monthly') {
        startDate.value = new Date(monthlyStartDay.value)
        endDate.value = new Date(monthlyEndDay.value)
    } else {
        if (!customDateRange.value[0] || !customDateRange.value[1]) return
        startDate.value = new Date(customDateRange.value[0])
        endDate.value = new Date(customDateRange.value[1])
    }
})

async function loadProcessesWithExclusions() {
    const res = await getAllProcesses()
    processes.value = res.data.map((p: ProcessInfo) => ({ name: p.alias || p.name, id: p.id }))
    excludedProcessIds.value =
        localStorage.getItem(StorageKey.USAGE_OVERVIEW_EXCLUDED_PROCESSES)?.split(',') || []
    excludedProcesses.value = processes.value.filter((p) => excludedProcessIds.value.includes(p.id))
}

watch(excludedProcesses, () => {
    excludedProcessIds.value = excludedProcesses.value.map((p) => p.id)
    localStorage.setItem(
        StorageKey.USAGE_OVERVIEW_EXCLUDED_PROCESSES,
        excludedProcessIds.value.join(','),
    )
})

watch(topN, () => {
    localStorage.setItem(StorageKey.USAGE_OVERVIEW_TOP_N, topN.value.toString())
})

onMounted(async () => {
    await loadProcessesWithExclusions()
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
