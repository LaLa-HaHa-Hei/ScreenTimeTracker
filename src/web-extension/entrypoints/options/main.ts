import { userSettingsStorage } from "@/utils/settings";

const userSettings = await userSettingsStorage.getValue();

const baseUrlInputElement = document.getElementById('baseUrl') as HTMLInputElement;
baseUrlInputElement.value = userSettings.baseUrl;

baseUrlInputElement.addEventListener('keydown', (e) => {
    if (e.key === 'Enter') {
        baseUrlInputElement.blur();
    }
});

baseUrlInputElement.addEventListener('blur', (e) => {
    const baseUrl = baseUrlInputElement.value;
    userSettingsStorage.setValue({ baseUrl });
});
