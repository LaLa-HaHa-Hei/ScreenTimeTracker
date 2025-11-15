import request from './request'

export async function getProcessById(id: string) {
    return await request.get(`/processes/${id}`)
}

export async function deleteProcessById(id: string) {
    return await request.delete(`/processes/${id}`)
}

export async function getAllProcesses() {
    return await request.get('/processes')
}

export function getProcessIconUrl(id: string): string {
    return `${request.defaults.baseURL}/processes/${id}/icon`
}

export async function updateProcessAlias(id: string, alias: string | null) {
    return await request.put(`/processes/${id}/alias`, { alias })
}

export async function updateProcessAutoUpdate(id: string, autoUpdate: boolean) {
    return await request.put(`/processes/${id}/auto-update`, { autoUpdate })
}

export async function updateProcessIconPath(id: string, iconPath: string | null) {
    return await request.put(`/processes/${id}/icon-path`, { iconPath })
}
