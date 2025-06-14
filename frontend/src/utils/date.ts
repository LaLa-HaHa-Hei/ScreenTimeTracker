import type { DateOnly } from '@/types';

export function dateToDateOnly(date: Date): DateOnly {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
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