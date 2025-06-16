<template>
    <BaseIcon :size="size" :color="color">
        <path fill="currentColor"
            d="M470.11 130.1C278.1 151.05 128 314.97 128 512c0 212.07 171.93 384 384 384 197.23 0 359.25-149.33 382.29-341.33 3.84-33.69-33.32-60.33-65.81-40.6A230.78 230.78 0 0 1 473.6 320c0-45.23 13.25-87.47 35.86-122.99 19.2-28.5-1.71-69.61-39.74-66.58z">
        </path>
    </BaseIcon>
</template>

<script setup lang="ts">
import BaseIcon from './BaseIcon.vue'

const props = defineProps<{
    size?: string,
    color?: string
}>()
</script>