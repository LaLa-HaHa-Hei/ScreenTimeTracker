import type { DateOnly } from '@/types';

export function dateToDateOnly(date: Date): DateOnly {
    const year = date.getFullYear();
    // 此处不要补0，Asp.Net Core Web API 的 DateOnly 只能识别 YYYY-M-D 格式
    const month = String(date.getMonth() + 1);
    const day = String(date.getDate());
    return `${year}-${month}-${day}` as DateOnly;
}

export function dateOnlyToDate(date: DateOnly): Date {
    const [year, month, day] = date.split('-').map(Number);
    return new Date(year, month - 1, day);
}

export function formatMonthDay(date: Date) {
    return `${date.getMonth() + 1}/${date.getDate()}`;
}

export function getWeek(date: Date) {
    const weekdays = ['周日', '周一', '周二', '周三', '周四', '周五', '周六'];
    return weekdays[date.getDay()];
}