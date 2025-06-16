import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/daily-screen-time/0',
    },
    {
      path: '/home',
      name: 'Home',
      component: () => import('@/views/Home.vue'),
      children: [
        {
          path: '/daily-screen-time/:daysAgo?',
          name: 'DailyScreenTime',
          component: () => import('@/views/DailyScreenTime.vue')
        },
        {
          path: '/weekly-screen-time/:weeksAgo?',
          name: 'WeeklyScreenTime',
          component: () => import('@/views/WeeklyScreenTime.vue')
        },
        {
          path: '/monthly-screen-time/:monthsAgo?',
          name: 'MonthlyScreenTime',
          component: () => import('@/views/MonthlyScreenTime.vue')
        },
        {
          path: '/custom-range-screen-time/:startDate?/:endDate?/:limit?',
          name: 'CustomRangeScreenTime',
          component: () => import('@/views/CustomRangeScreenTime.vue')
        }
      ]
    },
    {
      path: '/daily-process-usage/:processName/:date',
      name: 'DailyProcessUsage',
      component: () => import('@/views/DailyProcessUsage.vue')
    },
    {
      path: '/weekly-process-usage/:processName/:startDate/:endDate',
      name: 'WeeklyProcessUsage',
      component: () => import('@/views/WeeklyProcessUsage.vue')
    },
    {
      path: '/date-range-process-usage/:processName/:startDate/:endDate',
      name: 'DateRangeProcessUsage',
      component: () => import('@/views/DateRangeProcessUsage.vue')
    }
  ],
})

export default router
