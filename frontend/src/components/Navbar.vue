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
        <a @click="clearData()" class="mr-3 cursor-pointer">
          <img :src="config.baseUrl + '/clear.png'" />
        </a>
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
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '@/api'
import { dateToDateOnly } from '@/utils';

const router = useRouter();
const isDark = useDark()
const toggleDark = useToggle(isDark)
const lightTheme = ref(!isDark.value)

const clearData = async () => {
  const date = new Date()
  date.setMonth(date.getMonth() - 3)
  date.setDate(1)
  ElMessageBox.confirm(
    `你确定要删除 3 个月以前（${dateToDateOnly(date)}以前）的所有数据吗`,
    '警告',
    {
      confirmButtonText: '确认',
      cancelButtonText: '取消',
      type: 'warning',
    }
  )
    .then(() => {
      api.DeleteUsagesBeforeDate(dateToDateOnly(date))
      ElMessage({
        message: '成功删除',
        type: 'success',
      })
    })
    .catch(() => { })
}

const goBack = () => {
  router.back()
}
</script>

<style scoped></style>