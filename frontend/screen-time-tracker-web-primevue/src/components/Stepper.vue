<!-- eslint-disable vue/multi-word-component-names -->
<template>
    <div class="stepper">
        <Button @click="props.onLeftClick" rounded icon="pi pi-angle-left" variant="text" />
        <span>{{ props.text }}</span>
        <Button @click="props.onRightClick" rounded icon="pi pi-angle-right" variant="text" />
    </div>
</template>

<script setup lang="ts">
interface Props {
    text: string
    onLeftClick: () => void
    onRightClick: () => void
}

const props = defineProps<Props>()
</script>

<style scoped>
.stepper {
    display: flex;
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
    border-radius: 9999px;
    border: 1px solid var(--p-content-border-color);
    background: var(--p-form-field-background);
    transition:
        background var(--p-form-field-transition-duration),
        color var(--p-form-field-transition-duration),
        border-color var(--p-form-field-transition-duration),
        box-shadow var(--p-form-field-transition-duration),
        outline-color var(--p-form-field-transition-duration);
}
</style>
