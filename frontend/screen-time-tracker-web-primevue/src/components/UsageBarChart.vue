<template>
    <div class="card">
        <div class="flex flex-row justify-center gap-10 text-lg font-bold">
            <span> 总时长：{{ formatSeconds(totalUsage) }} </span>
            <span> 平均：{{ formatSeconds(averageUsage) }} </span>
        </div>
        <Chart
            type="bar"
            class="mt-3 aspect-3/1 w-full"
            :data="chartData"
            :options="chartOptions"
        />
    </div>
</template>

<script setup lang="ts">
import { watch, ref, nextTick } from 'vue'
import {
    getTotalHourlyUsage,
    getTotalDailyUsage,
    getProcessHourlyDistribution,
    getProcessDailyDistribution,
} from '@/api'
import { formatSeconds, dateToDateOnly } from '@/utils'
import type { ChartData, ChartOptions } from 'chart.js'
import { isDark } from '@/composables/darkMode'

interface Props {
    startDate: Date
    endDate: Date
    xAxisType: 'Hour' | 'Day' | 'Week'
    mode: 'Total' | 'Process'
    excludedProcesses?: string[]
    process?: string
}

const props = defineProps<Props>()

const chartData = ref<ChartData<'bar'>>({
    labels: [],
    datasets: [],
})
const totalUsage = ref<number>(0)
const averageUsage = ref<number>(0)
const chartOptions = ref<ChartOptions<'bar'>>({
    responsive: true,
    maintainAspectRatio: true,
    aspectRatio: 3 / 1,
    animation: false,
    plugins: {
        legend: {
            display: false,
        },
        tooltip: {
            callbacks: {
                label: (context) => {
                    const value = context.raw as number
                    return `${formatSeconds(value)}`
                },
            },
        },
    },
    scales: {
        x: {
            ticks: {
                color: '#64748b',
            },
            grid: {
                color: '#e2e8f0',
            },
        },
        y: {
            beginAtZero: true,
            ticks: {
                color: '#64748b',
                callback: (value: number | string) => formatSeconds(Number(value)),
                maxTicksLimit: 3,
            },
            grid: {
                color: '#e2e8f0',
            },
            max: 1,
        },
    },
})

watch(
    isDark,
    async () => {
        // 等待 Vue 更新 DOM 和 CSS 变量
        await nextTick()
        updateChartColorOptions()
    },
    { immediate: true },
)

function updateChartColorOptions() {
    const documentStyle = getComputedStyle(document.documentElement)
    const textColorSecondary = documentStyle.getPropertyValue('--p-text-muted-color')
    const surfaceBorder = documentStyle.getPropertyValue('--p-content-border-color')

    if (chartOptions.value?.scales?.x?.ticks)
        chartOptions.value.scales.x.ticks.color = textColorSecondary
    if (chartOptions.value?.scales?.x?.grid) chartOptions.value.scales.x.grid.color = surfaceBorder
    if (chartOptions.value?.scales?.y?.ticks)
        chartOptions.value.scales.y.ticks.color = textColorSecondary
    if (chartOptions.value?.scales?.y?.grid) chartOptions.value.scales.y.grid.color = surfaceBorder

    chartOptions.value = { ...chartOptions.value }
}

async function loadUsage() {
    if (props.mode === 'Process' && !props.process) return
    if (props.mode === 'Total' && !props.excludedProcesses) return

    let labels: string[] = []
    let usageList: number[] = []
    if (props.xAxisType === 'Hour') {
        const res =
            props.mode === 'Total'
                ? await getTotalHourlyUsage(
                      dateToDateOnly(props.startDate),
                      props.excludedProcesses,
                  )
                : await getProcessHourlyDistribution(
                      props.process!,
                      dateToDateOnly(props.startDate),
                  )
        labels = Array.from({ length: 24 }, (_, i) => String(i))
        usageList = Array.from({ length: 24 }, (_, hour) => res.data[hour] ?? 0)
    } else if (props.xAxisType === 'Week') {
        const res =
            props.mode === 'Total'
                ? await getTotalDailyUsage(
                      dateToDateOnly(props.startDate),
                      dateToDateOnly(props.endDate),
                      props.excludedProcesses,
                  )
                : await getProcessDailyDistribution(
                      props.process!,
                      dateToDateOnly(props.startDate),
                      dateToDateOnly(props.endDate),
                  )
        labels = getWeekdayRangeList(props.startDate, props.endDate)
        usageList = mapDictToListByDateRange(res.data, props.startDate, props.endDate)
    } else {
        const res =
            props.mode === 'Total'
                ? await getTotalDailyUsage(
                      dateToDateOnly(props.startDate),
                      dateToDateOnly(props.endDate),
                      props.excludedProcesses,
                  )
                : await getProcessDailyDistribution(
                      props.process!,
                      dateToDateOnly(props.startDate),
                      dateToDateOnly(props.endDate),
                  )
        labels = getDateRangeList(props.startDate, props.endDate)
        usageList = mapDictToListByDateRange(res.data, props.startDate, props.endDate)
    }
    chartData.value = {
        labels: labels,
        datasets: [
            {
                data: usageList,
                borderWidth: 1,
            },
        ],
    }
    if (chartOptions.value?.scales?.y) {
        const maxYValue = Math.max(...usageList)
        chartOptions.value.scales.y.max = maxYValue <= 0 ? 1 : maxYValue
    }
    totalUsage.value = usageList.reduce((sum, curr) => sum + curr, 0)
    averageUsage.value = Math.round(totalUsage.value / usageList.length)
}

function mapDictToListByDateRange(dict: Record<string, number>, startDate: Date, endDate: Date) {
    const result = []
    const cur = new Date(startDate)

    while (cur <= endDate) {
        const key = dateToDateOnly(cur)
        result.push(dict[key] ?? 0)
        cur.setDate(cur.getDate() + 1)
    }

    return result
}

function getWeekdayRangeList(startDate: Date, endDate: Date): string[] {
    const weekdays = ['周日', '周一', '周二', '周三', '周四', '周五', '周六']
    const list = []
    const current = new Date(startDate)

    while (current <= endDate) {
        list.push(weekdays[current.getDay()])
        current.setDate(current.getDate() + 1)
    }
    return list as string[]
}

function getDateRangeList(startDate: Date, endDate: Date) {
    const list = []
    const current = new Date(startDate)

    while (current <= endDate) {
        const month = current.getMonth() + 1
        const day = current.getDate()
        list.push(`${month}/${day}`)

        current.setDate(current.getDate() + 1)
    }
    return list
}

watch(
    () => [
        props.startDate,
        props.endDate,
        props.xAxisType,
        props.excludedProcesses?.join(','),
        props.process,
    ],
    loadUsage,
    { immediate: true },
)
</script>

<style scoped>
.card {
    padding: 0.5rem;
    background: var(--p-form-field-background);
    border-radius: var(--p-form-field-border-radius);
    border: 1px solid var(--p-form-field-border-color);
    outline-color: transparent;
    box-shadow: var(--p-form-field-shadow);
    transition:
        background var(--p-form-field-transition-duration),
        color var(--p-form-field-transition-duration),
        border-color var(--p-form-field-transition-duration),
        box-shadow var(--p-form-field-transition-duration),
        outline-color var(--p-form-field-transition-duration);
}
</style>
