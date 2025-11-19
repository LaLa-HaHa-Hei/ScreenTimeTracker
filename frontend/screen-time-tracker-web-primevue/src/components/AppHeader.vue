<template>
    <Toolbar class="h-full w-full">
        <template #start>
            <div class="flex items-center gap-2">
                <img alt="logo" src="@/assets/logo.svg" width="30" />
                <span class="text-lg font-bold">屏幕使用时间</span>
            </div>
        </template>

        <template #end>
            <div class="flex items-center gap-3">
                <Button
                    icon="pi pi-github"
                    variant="outlined"
                    size="small"
                    class="aspect-square"
                    @click="openGithub"
                />
                <Button
                    :icon="isDark ? 'pi pi-moon' : 'pi pi-sun'"
                    variant="outlined"
                    size="small"
                    class="aspect-square"
                    @click="toggleDark()"
                />
            </div>
        </template>
    </Toolbar>
</template>

<script lang="ts" setup>
import Toolbar from 'primevue/toolbar'
import Button from 'primevue/button'
import { isDark, toggleDark } from '@/composables/darkMode'

function openGithub() {
    window.open('https://github.com/LaLa-HaHa-Hei/ScreenTimeTracker', '_blank')
}
</script>
