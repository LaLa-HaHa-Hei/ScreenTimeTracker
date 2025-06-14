<template>
    <div class="w-full">
        <div class="time-dimension-btn-group w-full flex justify-center items-center flex-row gap-2">
            <RouterLink :to="{ name: 'DailyScreenTime' }" active-class="active">日</RouterLink>
            <RouterLink :to="{ name: 'WeeklyScreenTime' }" active-class="active">周</RouterLink>
            <RouterLink :to="{ name: 'MonthlyScreenTime' }" active-class="active">月</RouterLink>
        </div>
        <div>
            <RouterView />
        </div>
    </div>
</template>

<script setup lang="ts">
</script>

<style scoped>
.time-dimension-btn-group a {
    flex: 1;
    display: inline-block;
    max-width: 300px;
    font-weight: 600;
    text-align: center;
    padding: 15px 0;
    text-decoration: none;
    transition: all 0.3s ease;
    border-radius: var(--radius-xl);
}

.time-dimension-btn-group a.active {
    background-color: var(--color-white);
}

.dark .time-dimension-btn-group a.active {
    background-color: var(--color-neutral-800);
}

.time-dimension-btn-group a:hover {
    background-color: var(--color-gray-200);
}

.dark .time-dimension-btn-group a:hover {
    background-color: var(--color-neutral-900);
}
</style>