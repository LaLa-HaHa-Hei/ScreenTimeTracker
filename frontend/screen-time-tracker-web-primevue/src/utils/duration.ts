export function formatSeconds(seconds: number) {
    const h = Math.floor(seconds / 3600)
    const m = Math.floor((seconds % 3600) / 60)
    const s = seconds % 60

    let result = ''
    if (h > 0) result += h + '小时'
    if (m > 0) result += m + '分钟'
    if (result === '' || (h === 0 && s !== 0)) result += s + '秒'

    return result
}
