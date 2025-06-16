<template>
    <div ref="chartRef" class="w-full h-[300px]"></div>
</template>

<script setup lang="ts">
import * as echarts from 'echarts';
import { defineProps, ref, onMounted, onUnmounted, watch, computed } from 'vue';
import { useDark } from '@vueuse/core'

const props = withDefaults(
    defineProps<{
        yValues?: number[];
        yFormater?: (value: number) => string;
        xValues?: string[];
        tooltipFormatter?: (axisValue: string, data: number) => string;
        maxY?: number;
    }>(),
    {
        yValues: () => [],
        xValues: () => [],
        yFormater: (value: number) => value.toString(),
        xFormater: (value: string) => value,
        tooltipFormatter: (axisValue: string, data: number) => `${axisValue}<br />${data}`,
    }
)

const isDark = useDark()

const chartRef = ref<HTMLDivElement | null>(null)
let usageChart: echarts.ECharts | null = null
const maxValue = computed(() => props.maxY || Math.max(...props.yValues))
const interval = computed(() => maxValue.value / 2)
const avgDuration = computed(() =>
    props.yValues.reduce((total: number, item: number) => total + item, 0) / props.yValues.length)

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
            if (params && params.length > 0)
                return props.tooltipFormatter(params[0].axisValue, params[0].data)
            return '';
        }
    },
    xAxis: {
        type: 'category',
        data: props.xValues,
        axisTick: {
            alignWithLabel: false
        }
    },
    yAxis: {
        type: 'value',
        position: 'left',
        min: 0,
        max: maxValue.value,
        interval: interval.value,
        splitNumber: 1,
        axisLabel: {
            formatter: (value: number) => props.yFormater(value)
        }
    },
    series: [
        {
            type: 'bar',
            data: props.yValues || [],
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
                        formatter: '平均\n' + props.yFormater(avgDuration.value),
                        color: isDark.value ? '#fff' : '#000',
                        backgroundColor: 'transparent',
                    }
                }]
            }
        }
    ]
}))

const handleResize = () => {
    if (usageChart) {
        usageChart.resize()
    }
}

const initChart = () => {
    if (chartRef.value) {
        usageChart = echarts.init(chartRef.value)
        usageChart.setOption(option.value)
    }
}

onMounted(() => {
    initChart()
    window.addEventListener('resize', handleResize)
})
onUnmounted(() => {
    window.removeEventListener('resize', handleResize)
})

watch([() => props.yValues, () => props.xValues, () => props.maxY, () => isDark.value], () => {
    if (usageChart) {
        usageChart.setOption(option.value, true)  // true 表示不合并，完全替换
    }
})
</script>

<style scoped></style>