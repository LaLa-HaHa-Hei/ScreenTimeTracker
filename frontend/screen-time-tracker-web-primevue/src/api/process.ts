import type { UpdateProcessRequest } from '@/types'
import request from './request'

export async function getAllProcesses() {
    return await request.get('api/processes')
}

export async function getProcess(id: string) {
    return await request.get(`api/processes/${id}`)
}

export async function deleteProcess(id: string) {
    return await request.delete(`api/processes/${id}`)
}

export async function updateProcess(id: string, data: UpdateProcessRequest) {
    return await request.put(`api/processes/${id}`, data)
}

export function getProcessIconUrl(id: string): string {
    return `${request.defaults.baseURL}api/processes/${id}/icon`
}

export async function getProcessHourlyDistribution(id: string, date: string) {
    return await request.get(`api/processes/${id}/usage-distribution/hourly?date=${date}`)
}

export async function getProcessDailyDistribution(id: string, startDate: string, endDate: string) {
    return await request.get(
        `api/processes/${id}/usage-distribution/daily?startDate=${startDate}&endDate=${endDate}`,
    )
}
