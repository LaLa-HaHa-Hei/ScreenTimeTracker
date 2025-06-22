<template>
    <el-page-header class="w-full px-3 py-4 border-b border-gray-400" title=" " @back="goBack">
        <template #content>
            <div class="flex items-center">
                <span>屏幕使用时间</span>
            </div>
        </template>
        <template #extra>
            <div class="flex items-center mx-3">
                <IconClear @click="clearData()" class="mr-3 cursor-pointer" size="1.8em" />
                <el-divider direction="vertical" />
                <el-switch class="mx-3 theme-switch" :model-value="isDark" :active-action-icon="IconMoon"
                    :inactive-action-icon="IconSun" style="--el-switch-on-color: #262626; --el-switch-off-color: #fff"
                    @click="toggleDark()" />
                <a class="mr-3 social-link" href="https://github.com/LaLa-HaHa-Hei/ScreenTimeTracker" target="_blank">
                    <IconGitHub size="1.8em" />
                </a>
                <a class="social-link" href="https://qm.qq.com/q/VEbPwBma40" target="_blank">
                    <IconQQ size="1.8em" />
                </a>
            </div>
        </template>
    </el-page-header>
</template>

<script lang="ts" setup>
import { useDark, useToggle } from '@vueuse/core'
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus'
import api from '@/api'
import { dateToDateOnly } from '@/utils';
import IconClear from './icons/IconClear.vue';
import IconGitHub from './icons/IconGitHub.vue';
import IconSun from './icons/IconSun.vue';
import IconMoon from './icons/IconMoon.vue';
import IconQQ from './icons/IconQQ.vue';

const router = useRouter();
const isDark = useDark()
const toggleDark = useToggle(isDark)

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

<!-- 不能使用 scoped 否则全局的 .dark 类无法查找 -->
<style>
.social-link {
    color: #767676;
}

.social-link :hover {
    color: #213547;
}

.dark .social-link {
    color: #979797;
}

.dark .social-link :hover {
    color: #E1E1E1;
}

/* swtich 图标和圆点颜色 */
.theme-switch .el-switch__core>.el-switch__action {
    color: #262626;
    background-color: #F3F4F6
}

.dark .theme-switch .el-switch__core>.el-switch__action {
    color: #ffffff;
    background-color: #000000
}
</style>