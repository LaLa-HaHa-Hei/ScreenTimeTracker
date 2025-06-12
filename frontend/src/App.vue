<template>
  <el-container>
    <el-header>
      <Navbar />
    </el-header>
    <el-main>
      <div class="time-dimension-btn-group">
        <RouterLink to="/daily-screen-time" active-class="active">日</RouterLink>
        <RouterLink to="/weekly-screen-time" active-class="active">周</RouterLink>
        <RouterLink to="/monthly-screen-time" active-class="active">月</RouterLink>
      </div>
      <div>
        <RouterView />
      </div>
    </el-main>
  </el-container>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import Navbar from '@/components/Navbar.vue'

</script>

<style scoped>
.time-dimension-btn-group {
  width: 100%;
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  gap: 15px;
}

.time-dimension-btn-group a {
  flex: 1;
  display: inline-block;
  max-width: 300px;
  font-weight: 600;
  text-align: center;
  padding: 15px 0;
  text-decoration: none;
  border-radius: 15px;
  transition: all 0.3s ease;
  color: var(--el-text-color-primary);
}

.time-dimension-btn-group a.active {
  background-color: var(--bg-secondary);
}

.time-dimension-btn-group a:hover {
  background-color: var(--bg-hover);
}
</style>
