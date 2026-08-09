export interface UserSettings {
    baseUrl: string;
}

export const defaultUserSettings: UserSettings = {
    baseUrl: 'http://127.0.0.1:5124',
};


export const userSettingsStorage = storage.defineItem<UserSettings>('local:userSettings', {
    defaultValue: defaultUserSettings,
});