<template>
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
                        <div>{{ formatSeconds(slotProps.option.totalDuration) }}</div>
                    </div>
                    <ProgressBar :value="slotProps.option.percentage" />
                </div>
            </div>
        </template>
    </Listbox>
</template>

<script setup lang="ts">
import type { ProcessUsageRankEntry } from '@/types'
import { ref, watch } from 'vue'
import { getProcessUsageRankEntry, getProcessIconUrl } from '@/api'
import defaultFileIcon from '@/assets/defaultFileIcon.svg'
import { formatSeconds } from '@/utils'

interface Props {
    startDate: Date
    endDate: Date
    topN: number
    excludedProcesses: string[]
}

const props = defineProps<Props>()

const processUsageRanks = ref<ProcessUsageRankEntry[]>([])

async function loadProcessUsageRanks() {
    const res = await getProcessUsageRankEntry(
        dateToString(props.startDate),
        dateToString(props.endDate),
        props.topN,
        props.excludedProcesses,
    )
    processUsageRanks.value = res.data
}

watch(
    () => [props.startDate, props.endDate, props.topN, props.excludedProcesses.join(',')],
    loadProcessUsageRanks,
    { immediate: true },
)

function dateToString(date: Date) {
    const y = date.getFullYear()
    const m = String(date.getMonth() + 1).padStart(2, '0')
    const d = String(date.getDate()).padStart(2, '0')
    return `${y}-${m}-${d}`
}
</script>
