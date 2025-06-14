export type DateOnly = `${number}-${number}-${number}`;

export interface ProcessUsage {
    processName: string;
    durationMs: number;
}

export interface DateUsage {
    date: DateOnly;
    durationMs: number;
}