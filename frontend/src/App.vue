<template>
    <el-container>
        <el-header>
            <Navbar />
        </el-header>
        <el-main>
            <RouterView />
        </el-main>
    </el-container>
</template>

<script setup lang="ts">
import Navbar from '@/components/Navbar.vue'
</script>

<style scoped></style>
