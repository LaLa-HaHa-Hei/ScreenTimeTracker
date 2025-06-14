<template>
    <div ref="chartRef" class="w-full" style="height:300px;"></div>
</template>

<script setup lang="ts">
import * as echarts from 'echarts';
import { defineProps, ref, onMounted, onUnmounted, watch, computed } from 'vue';
import { formatDuration, dateOnlyToDate, getWeek } from '@/utils';
import { useDark } from '@vueuse/core'
import type { DateUsage } from '@/types'

const props = defineProps<{
    usage: DateUsage[] | null | undefined;
}>();

const isDark = useDark()

const chartRef = ref<HTMLDivElement | null>(null)
let myChart: echarts.ECharts | null = null
const maxValue = computed(() => props.usage ? Math.max(...props.usage.map(item => item.durationMs)) : 0)
const midValue = computed(() => maxValue.value / 2)
const avgDuration = computed(() => {
    if (!props.usage) return 0
    const arr = props.usage
    if (!props.usage.length) return 0
    return arr.reduce((total: number, item: DateUsage) => total + item.durationMs, 0) / arr.length
})
const option = computed(() => ({
    grid: {
        left: '3%',
        right: '100px',
        bottom: '3%',
        containLabel: true
    },
    tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'shadow' },
        formatter: (params: any) => {
            if (params && params.length > 0) {
                const item = params[0];
                return `${item.name}<br/>${formatDuration(item.data)}`;
            }
            return '';
        }
    },
    xAxis: {
        type: 'category',
        data: props.usage ? [...props.usage.map(item => getWeek(dateOnlyToDate(item.date)))] : [],
        axisTick: {
            alignWithLabel: false
        }
    },
    yAxis: {
        type: 'value',
        position: 'left',
        min: 0,
        max: maxValue.value,
        interval: midValue.value,
        splitNumber: 1,
        axisLabel: {
            formatter: (value: number) => formatDuration(value)
        }
    },
    series: [
        {
            type: 'bar',
            data: props.usage ? props.usage.map(item => item.durationMs) : [],
            itemStyle: {
                borderRadius: [8, 8, 0, 0],
                color: new echarts.graphic.LinearGradient(0, 1, 0, 0, [
                    { offset: 0, color: '#287AF7' },
                    { offset: 1, color: '#6766EE' }
                ])
            },
            markLine: {
                animationDuration: 500,
                data: [{
                    yAxis: avgDuration.value,
                    label: {
                        formatter: '平均\n' + formatDuration(avgDuration.value),
                        color: isDark.value ? '#fff' : '#000',
                        backgroundColor: 'transparent',
                    }
                }]
            }
        }
    ]
}))

const handleResize = () => {
    if (myChart) {
        myChart.resize()
    }
}

const initChart = () => {
    if (chartRef.value) {
        myChart = echarts.init(chartRef.value)
        myChart.setOption(option.value)
    }
}

onMounted(() => {
    initChart()
    window.addEventListener('resize', handleResize)
})
onUnmounted(() => {
    window.removeEventListener('resize', handleResize)
})

watch([() => props.usage, isDark], () => {
    if (myChart) {
        myChart.setOption(option.value, true)  // true 表示不合并，完全替换
    }
})
</script>

<style scoped></style>