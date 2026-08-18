export type ConnectionStatus = 'connected' | 'disconnected'

export const defaultConnectionStatus: ConnectionStatus = 'connected'

export const connectionStatusStorage = storage.defineItem<ConnectionStatus>('local:connectionStatus', {
    defaultValue: defaultConnectionStatus,
});