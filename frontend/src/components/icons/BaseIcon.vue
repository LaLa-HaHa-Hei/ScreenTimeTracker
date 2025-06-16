<template>
    <svg v-bind="svgAttrs" viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg">
        <slot />
    </svg>
</template>

<script setup lang="ts">
import { defineProps, computed } from 'vue'

const props = defineProps<{
    size?: string,
    color?: string
}>()

const svgAttrs = computed(() => {
    const attrs: Record<string, string> = {}
    if (props.size) {
        attrs.width = props.size
        attrs.height = props.size
    }
    if (props.color) {
        attrs.color = props.color
    }
    return attrs
})

</script>