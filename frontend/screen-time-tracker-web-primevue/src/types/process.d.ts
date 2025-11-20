export interface ProcessInfo {
    id: string
    name: string
    alias: string | null
    autoUpdate: boolean
    lastAutoUpdated: string | null
    executablePath: string | null
    iconPath: string | null
    description: string | null
}

export interface ProcessUsageRankEntry {
    processId: string
    processName: string
    processAlias: string | null
    processIconPath: string | null
    totalDuration: number
    percentage: number
}

export interface UpdateProcessRequest {
    alias: string | null
    autoUpdate: boolean
    iconPath: string | null
}
