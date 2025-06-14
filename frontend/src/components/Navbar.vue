<template>
  <el-page-header class="w-full px-3 py-4 border-b border-gray-400" title=" " @back="goBack">
    <template #content>
      <div class="flex items-center">
        <span> 屏幕使用时间 </span>
      </div>
    </template>
    <template #extra>
      <div class="flex items-center">
        <el-switch class="mr-3" v-model="lightTheme" :active-action-icon="Sunny" :inactive-action-icon="Moon"
          @click="toggleDark()" />
        <a href="https://github.com/LaLa-HaHa-Hei/ScreenTimeTracker" target="_blank">
          <img class="" :src="config.baseUrl + '/github.png'" />
        </a>
      </div>
    </template>
  </el-page-header>
</template>

<script lang="ts" setup>
import { Sunny, Moon } from '@element-plus/icons-vue'
import { useDark, useToggle } from '@vueuse/core'
import { ref } from 'vue'
import config from '@/config'
import { useRouter } from 'vue-router';

const router = useRouter();
const isDark = useDark()
const toggleDark = useToggle(isDark)
const lightTheme = ref(!isDark.value)

const goBack = () => {
  router.back()
}
</script>

<style scoped></style>