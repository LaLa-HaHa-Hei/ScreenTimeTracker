import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/daily-screen-time',
    },
    {
      path: '/daily-screen-time',
      name: 'DailyScreenTime',
      component: () => import('@/views/DailyScreenTime.vue')
    },
    {
      path: '/monthly-screen-time',
      name: 'MonthlyScreenTime',
      component: () => import('@/views/MonthlyScreenTime.vue')
    },
    {
      path: '/weekly-screen-time',
      name: 'WeeklyScreenTime',
      component: () => import('@/views/WeeklyScreenTime.vue')
    }
  ],
})

export default router
