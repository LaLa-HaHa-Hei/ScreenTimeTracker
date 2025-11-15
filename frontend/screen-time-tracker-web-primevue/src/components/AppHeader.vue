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
                <Button
                    icon="pi pi-palette"
                    variant="outlined"
                    size="small"
                    class="aspect-square"
                />
            </div>
        </template>
    </Toolbar>
</template>

<script lang="ts" setup>
import Toolbar from 'primevue/toolbar'
import Button from 'primevue/button'
import { useDark, useToggle } from '@vueuse/core'

const isDark = useDark({
    disableTransition: false,
})
const toggleDark = useToggle(isDark)

function openGithub() {
    window.open('https://github.com/LaLa-HaHa-Hei/ScreenTimeTracker', '_blank')
}
</script>
