export interface TrackerSettings {
    pollingIntervalMilliseconds: number
    processInfoStaleThresholdHours: number
    processIconDirPath: string
    enableIdleDetection: boolean
    idleTimeoutSeconds: number
}

export interface AggregationSettings {
    pollingIntervalMinutes: number
}
