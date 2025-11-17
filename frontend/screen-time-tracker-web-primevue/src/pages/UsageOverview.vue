<template>
    <div class="w-full">
        <SelectButton
            v-model="selectedTimeRange"
            optionLabel="label"
            :options="timeRangeOptions"
            :allowEmpty="false"
        />
        <div class="mt-5 flex justify-end">
            <Select v-model="topN" :options="topNOptions" />
        </div>
        <Listbox
            :options="processUsageRanks"
            optionLabel="processName"
            class="mt-2 w-full"
            listStyle="max-height: 100%"
        >
            <template #option="slotProps">
                <div class="flex w-full flex-row items-center gap-5">
                    <Image
                        :src="
                            slotProps.option.processIconPath
                                ? getProcessIconUrl(slotProps.option.processId)
                                : defaultFileIcon
                        "
                        alt="Icon"
                        imageClass="min-w-8 w-8 h-8 min-h-8"
                    />
                    <div class="flex-auto">
                        <div class="flex flex-row justify-between">
                            <div>
                                {{ slotProps.option.processAlias || slotProps.option.processName }}
                            </div>
                            <div>{{ slotProps.option.totalDuration }}</div>
                        </div>
                        <ProgressBar :value="slotProps.option.percentage" />
                    </div>
                </div>
            </template>
        </Listbox>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import type { ProcessUsageRankEntry } from '@/types/process'
import { getProcessUsageRankEntryForPeriod, getProcessIconUrl } from '@/api'
import defaultFileIcon from '@/assets/defaultFileIcon.svg'

const processUsageRanks = ref<ProcessUsageRankEntry[]>([])
const timeRangeOptions = [
    { label: '日', value: 'Daily' },
    { label: '周', value: 'Weekly' },
    { label: '月', value: 'Monthly' },
    { label: '自定义', value: 'Custom' },
]
const selectedTimeRange = ref(timeRangeOptions[0])
const startDate = ref('2025-11-16')
const endDate = ref('2025-11-17')
const topNOptions = [5, 10, 15, 20]
const topN = ref(10)

function loadProcessUsageRanks() {
    getProcessUsageRankEntryForPeriod(startDate.value, endDate.value, topN.value).then((res) => {
        processUsageRanks.value = res.data
    })
}

onMounted(async () => {
    loadProcessUsageRanks()
})
</script>
