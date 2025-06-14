import { ref } from 'vue'
import { defineStore } from 'pinia'
import type { ProcessInfo } from '@/types';
import api from '@/api'

export const useProcessStore = defineStore('process', () => {
    const processes = ref<Record<string, ProcessInfo>>({})

    const getProcessInfo = async (name: string): Promise<ProcessInfo | null> => {
        if (processes.value[name]) {
            return processes.value[name]
        }

        try {
            const data = await api.GetProcessByName(name)
            if (data !== null)
                processes.value[name] = data
            return data
        }
        catch (error) {
            console.error(`Error fetching process ${name}:`, error)
            return null
        }
    }

    const updateAlias = async (name: string, alias: string) => {
        api.UpdateProcessAlias(name, alias)
        processes.value[name].alias = alias
    }

    return {
        processes,
        getProcessInfo,
        updateAlias
    }
})