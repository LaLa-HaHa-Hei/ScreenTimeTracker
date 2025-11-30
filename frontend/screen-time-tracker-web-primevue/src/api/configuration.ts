import type { TrackerSettings, AggregationSettings } from '@/types'
import request from './request'

export async function getTrackerSettings() {
    return await request.get('api/configuration/tracker')
}

export async function updateTrackerSettings(data: TrackerSettings) {
    return await request.put('api/configuration/tracker', data)
}

export async function resetTrackerSettings() {
    return await request.post('api/configuration/tracker/reset')
}

export async function getAggregationSettings() {
    return await request.get('api/configuration/aggregation')
}

export async function updateAggregationSettings(data: AggregationSettings) {
    return await request.put('api/configuration/aggregation', data)
}

export async function resetAggregationSettings() {
    return await request.post('api/configuration/aggregation/reset')
}
