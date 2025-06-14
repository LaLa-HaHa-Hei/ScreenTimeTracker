export function formatDuration(ms: number) {
    const totalSeconds = Math.floor(ms / 1000);
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;

    let result = '';
    if (hours > 0) result += hours + '小时';
    if (minutes > 0 || hours > 0) result += minutes + '分钟';
    if (hours === 0)
        result += seconds + '秒';

    return result;
}