import { ref } from 'vue'
import { defineStore, acceptHMRUpdate } from 'pinia'

export interface TabInfoItem {
    name: string
    title: string
}

export const useTabStore = defineStore('tab', () => {
    const tabArray: TabInfoItem[] = [
        {
            name: 'UsageOverview',
            title: '使用概览',
        },
        {
            name: 'ProcessDetail',
            title: '进程详情',
        },
        {
            name: 'ProcessManagement',
            title: '进程管理',
        },
        {
            name: 'ConfigurationEditor',
            title: '配置',
        },
        {
            name: 'AboutProject',
            title: '关于',
        },
    ] as const

    const tabDict = tabArray.reduce(
        (dict, tab) => {
            dict[tab.name] = tab
            return dict
        },
        {} as Record<string, TabInfoItem>,
    )

    const currentTab = ref<TabInfoItem>(tabDict['UsageOverview']!)

    return { tabArray, tabDict, currentTab }
})

if (import.meta.hot) {
    import.meta.hot.accept(acceptHMRUpdate(useTabStore, import.meta.hot))
}
