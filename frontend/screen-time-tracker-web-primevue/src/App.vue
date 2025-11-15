<template>
    <div class="min-h-screen w-full">
        <header class="fixed top-0 right-0 left-0 z-100 h-15">
            <AppHeader />
        </header>
        <aside class="fixed h-screen w-60 pt-20 pb-5 pl-5">
            <nav class="h-full overflow-auto">
                <AppMenu />
            </nav>
        </aside>
        <main class="w-full pt-20 pr-5 pb-5 pl-65">
            <Panel class="w-full">
                <Tabs v-model:value="tabStore.currentTab.name">
                    <TabPanel value="UsageOverview">
                        <UsageOverview />
                    </TabPanel>
                    <TabPanel value="ProcessDetail">
                        <ProcessDetail />
                    </TabPanel>
                    <TabPanel value="ProcessManagement">
                        <ProcessManagement />
                    </TabPanel>
                    <TabPanel value="AboutProject">
                        <AboutProject />
                    </TabPanel>
                </Tabs>
            </Panel>
        </main>
    </div>
</template>

<script setup lang="ts">
import { useDark } from '@vueuse/core'
import AppHeader from '@/components/AppHeader.vue'
import AppMenu from '@/components/AppMenu.vue'
import Tabs from 'primevue/tabs'
import TabPanel from 'primevue/tabpanel'
import { useTabStore } from '@/stores/tab'
import Panel from 'primevue/panel'
import UsageOverview from '@/pages/UsageOverview.vue'
import ProcessDetail from '@/pages/ProcessDetail.vue'
import ProcessManagement from '@/pages/ProcessManagement.vue'
import AboutProject from '@/pages/AboutProject.vue'

useDark({
    disableTransition: false,
})

const tabStore = useTabStore()
</script>

<style scoped></style>
