<template>
    <Listbox
        v-model="tabStore.currentTab"
        :options="tabStore.tabArray"
        optionLabel="title"
        class="w-full"
    />
</template>

<script lang="ts" setup>
import Listbox from 'primevue/listbox'
import { useTabStore } from '@/stores/tab'

const tabStore = useTabStore()
</script>

<style scoped></style>
