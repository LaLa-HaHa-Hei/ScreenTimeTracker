<template>
    <div class="card">
        <Chart type="bar" class="aspect-3/1 w-full" :data="chartData" :options="chartOptions" />
    </div>
</template>

<script setup lang="ts">
import { watch, ref, nextTick } from 'vue'
import { getProcessHourlyDistribution, getProcessDailyDistribution } from '@/api'
import { formatSeconds } from '@/utils'
import type { ChartData, ChartOptions } from 'chart.js'
import { isDark } from '@/composables/darkMode'

interface Props {
    startDate: Date
    endDate: Date
    mode: 'Hour' | 'Day' | 'Week'
    process: string
}

const props = defineProps<Props>()

const chartData = ref<ChartData<'bar'>>()
let maxYValue = 1
const chartOptions = ref<ChartOptions<'bar'>>({
    responsive: true,
    maintainAspectRatio: true,
    aspectRatio: 3 / 1,
    animation: {
        duration: 1000,
        easing: 'easeOutQuart',
    },
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
    chartOptions.value = {
        responsive: true,
        maintainAspectRatio: true,
        aspectRatio: 3 / 1,
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
                    color: textColorSecondary,
                },
                grid: {
                    color: surfaceBorder,
                },
            },
            y: {
                beginAtZero: true,
                ticks: {
                    color: textColorSecondary,
                    callback: (value: number | string) => formatSeconds(Number(value)),
                    maxTicksLimit: 3,
                },
                grid: {
                    color: surfaceBorder,
                },
                max: maxYValue,
            },
        },
    }
}

async function loadTotalUsage() {
    if (props.mode === 'Hour') {
        const res = await getProcessHourlyDistribution(props.process, dateToString(props.startDate))
        chartData.value = {
            labels: Array.from({ length: 24 }, (_, i) => String(i)),
            datasets: [
                {
                    data: Array.from({ length: 24 }, (_, hour) => res.data[hour] ?? 0),
                    borderWidth: 1,
                },
            ],
        }
        if (chartOptions.value?.scales?.y) {
            maxYValue = Math.max(...(Object.values(res.data) as number[]))
            maxYValue = maxYValue <= 0 ? 1 : maxYValue
            chartOptions.value.scales.y.max = maxYValue
        }
    } else if (props.mode === 'Week') {
        const res = await getProcessDailyDistribution(
            props.process,
            dateToString(props.startDate),
            dateToString(props.endDate),
        )
        chartData.value = {
            labels: getWeekdayRangeList(props.startDate, props.endDate),
            datasets: [
                {
                    data: mapDictToListByDateRange(res.data, props.startDate, props.endDate),
                    borderWidth: 1,
                },
            ],
        }
        if (chartOptions.value?.scales?.y) {
            maxYValue = Math.max(...(Object.values(res.data) as number[]))
            maxYValue = maxYValue <= 0 ? 1 : maxYValue
            chartOptions.value.scales.y.max = maxYValue
        }
    } else {
        const res = await getProcessDailyDistribution(
            props.process,
            dateToString(props.startDate),
            dateToString(props.endDate),
        )
        chartData.value = {
            labels: getDateRangeList(props.startDate, props.endDate),
            datasets: [
                {
                    data: mapDictToListByDateRange(res.data, props.startDate, props.endDate),
                    borderWidth: 1,
                },
            ],
        }
        if (chartOptions.value?.scales?.y) {
            maxYValue = Math.max(...(Object.values(res.data) as number[]))
            maxYValue = maxYValue <= 0 ? 1 : maxYValue
            chartOptions.value.scales.y.max = maxYValue
        }
    }
}

function mapDictToListByDateRange(dict: Record<string, number>, startDate: Date, endDate: Date) {
    const result = []
    const cur = new Date(startDate)

    while (cur <= endDate) {
        const key = `${cur.getFullYear()}-${cur.getMonth() + 1}-${cur.getDate()}`
        result.push(dict[key] ?? 0)
        cur.setDate(cur.getDate() + 1)
    }

    return result
}

function getWeekdayRangeList(startDate: Date, endDate: Date) {
    const weekdays = ['周日', '周一', '周二', '周三', '周四', '周五', '周六']
    const list = []
    const current = new Date(startDate)

    while (current <= endDate) {
        list.push(weekdays[current.getDay()])
        current.setDate(current.getDate() + 1)
    }
    return list
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

watch(() => [props.startDate, props.endDate, props.mode, props.process], loadTotalUsage, {
    immediate: true,
})

function dateToString(date: Date) {
    const y = date.getFullYear()
    const m = String(date.getMonth() + 1).padStart(2, '0')
    const d = String(date.getDate()).padStart(2, '0')
    return `${y}-${m}-${d}`
}
</script>

<style>
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
