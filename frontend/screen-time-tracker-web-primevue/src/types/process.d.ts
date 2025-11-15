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
