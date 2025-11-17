import type { UpdateProcessRequest } from '@/types'
import request from './request'

export async function getProcessById(id: string) {
    return await request.get(`/processes/${id}`)
}

export async function deleteProcessById(id: string) {
    return await request.delete(`/processes/${id}`)
}

export async function updateProcessById(id: string, data: UpdateProcessRequest) {
    return await request.put(`/processes/${id}`, data)
}

export async function getAllProcesses() {
    return await request.get('/processes')
}

export function getProcessIconUrl(id: string): string {
    return `${request.defaults.baseURL}/processes/${id}/icon`
}

export async function getProcessHourlyDistribution(id: string, date: string) {
    return await request.get(`/processes/${id}/usage-distribution/hourly?date=${date}`)
}

export async function getProcessDailyDistribution(id: string, startDate: string, endDate: string) {
    return await request.get(
        `/processes/${id}/usage-distribution/daily?startDate=${startDate}&endDate=${endDate}`,
    )
}
