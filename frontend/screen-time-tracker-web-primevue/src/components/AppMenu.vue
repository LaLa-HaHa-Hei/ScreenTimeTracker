<template>
    <Menu :model="tabStore.tabArray">
        <template #item="{ item }">
            <div
                class="cursor-pointer px-3 py-2"
                :class="{ active: item === tabStore.currentTab }"
                @click.stop="open(item as TabInfoItem)"
            >
                {{ item.title }}
            </div>
        </template>
    </Menu>
</template>

<script lang="ts" setup>
import { useTabStore, type TabInfoItem } from '@/stores/tab'

const tabStore = useTabStore()

function open(item: TabInfoItem) {
    tabStore.currentTab = item
}
</script>

<style scoped>
.active {
    background: var(--p-list-option-selected-background);
    color: var(--p-list-option-selected-color);
}
</style>
