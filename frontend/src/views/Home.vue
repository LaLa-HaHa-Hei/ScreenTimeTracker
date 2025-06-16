<template>
    <div class="w-full">
        <div class="w-full flex justify-center flex-row gap-2">
            <RouterLink :to="{ name: 'DailyScreenTime' }" active-class="active">日</RouterLink>
            <RouterLink :to="{ name: 'WeeklyScreenTime' }" active-class="active">周</RouterLink>
            <RouterLink :to="{ name: 'MonthlyScreenTime' }" active-class="active">月</RouterLink>
            <RouterLink :to="{ name: 'CustomRangeScreenTime' }" active-class="active" class="flex justify-center">
                <el-icon size="1.5em">
                    <Filter />
                </el-icon>
            </RouterLink>
        </div>
        <div class="mt-4 w-full">
            <RouterView />
        </div>
    </div>
</template>

<script setup lang="ts">
</script>

<style scoped>
a {
    flex: 1;
    max-width: 200px;
    font-weight: 600;
    text-align: center;
    padding: 15px 0;
    text-decoration: none;
    transition: all 0.3s ease;
    border-radius: var(--radius-xl);
}

a.active {
    background-color: var(--color-white);
}

.dark a.active {
    background-color: var(--color-neutral-800);
}

a:hover {
    background-color: var(--color-gray-200);
}

.dark a:hover {
    background-color: var(--color-neutral-900);
}
</style>