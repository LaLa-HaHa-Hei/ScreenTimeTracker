<template>
  <el-page-header title=" " @back="goBack">
    <template #content>
      <div class="flex items-center">
        <span class="text-large font-600 mr-3"> 屏幕使用时间 </span>
      </div>
    </template>
    <template #extra>
      <div class="flex items-center">
        <el-switch v-model="lightTheme" inline-prompt :active-action-icon="Sunny" :inactive-action-icon="Moon"
          @click="toggleDark()" />
      </div>
    </template>
  </el-page-header>
</template>

<script lang="ts" setup>
import { Sunny, Moon } from '@element-plus/icons-vue'
import { useDark, useToggle } from '@vueuse/core'
import { ref } from 'vue'

const isDark = useDark()
const toggleDark = useToggle(isDark)
const lightTheme = ref(!isDark.value)

const goBack = () => {
  window.history.back()
}
</script>

<style scoped>
.page-header {
  font-weight: bold;
  width: 100%;
  padding: 15px 20px;
  border-bottom: 1px solid #eee;
}
</style>