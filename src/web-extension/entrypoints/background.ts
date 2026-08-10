import { fileTypeFromBuffer } from "file-type"
import { userSettingsStorage } from "@/utils/settings";

const isChrome = import.meta.env.BROWSER === 'chrome';
const isFirefox = import.meta.env.BROWSER === 'firefox';

if (!isChrome && !isFirefox)
    throw new Error('Only Chrome and Firefox are supported')

type Icon = {
    extension: string,
    data: string
}

type Website = {
    id: string
}

const noHostPlaceholder = "no-host"

// host -> favicon
const faviconCache = new Map<string, Icon | null>()

export default defineBackground(() => {
    let userSettings = defaultUserSettings
    userSettingsStorage.getValue().then((val) => {
        userSettings = val;
    });
    userSettingsStorage.watch((newValue) => {
        userSettings = newValue;
    });

    // 单击扩展图标
    (isChrome ? browser.action : browser.browserAction).onClicked.addListener(() => {
        browser.tabs.create({
            url: userSettings.baseUrl,
        });
    });

    // 右键扩展图标的菜单
    browser.runtime.onInstalled.addListener(async () => {
        await browser.contextMenus.removeAll();
        browser.contextMenus.create({
            id: 'open-settings',
            title: 'Open Settings',
            contexts: isChrome ? ['action'] : ['browser_action']
        });

        browser.contextMenus.create({
            id: 'open-web-ui',
            title: 'Open Web UI',
            contexts: isChrome ? ['action'] : ['browser_action']
        });
    });
    browser.contextMenus.onClicked.addListener((info) => {
        if (info.menuItemId === 'open-settings') {
            browser.runtime.openOptionsPage()
        }

        if (info.menuItemId === 'open-web-ui') {
            browser.tabs.create({
                url: userSettings.baseUrl,
            })
        }
    })

    browser.tabs.onActivated.addListener(async (activeInfo) => {
        let tab: Browser.tabs.Tab
        // 可能事件刚触发标签页就被关闭导致获取失败
        try {
            tab = await browser.tabs.get(activeInfo.tabId);
        } catch {
            return
        }
        if (!tab.url)
            return;

        const host = new URL(tab.url).host || noHostPlaceholder

        const website = await getWebsiteByHost(host)
        if (website === null)
            return

        let icon: Icon | null = null
        if (faviconCache.has(host))
            icon = faviconCache.get(host) || null
        else if (!tab.favIconUrl) {
            icon = null
            faviconCache.set(host, null)
        }
        else {
            icon = await getIcon(tab.favIconUrl)
            faviconCache.set(host, icon)
        }

        await refreshWebsiteMetadata(website.id, icon)
    });

    async function getWebsiteByHost(host: string): Promise<Website | null> {
        try {
            const response = await fetch(
                `${userSettings.baseUrl}/api/screen-time/websites/by-host/${encodeURIComponent(host)}`
            );

            if (!response.ok)
                return null

            const website = await response.json();
            return website
        } catch (e) {
            return null
        }
    }

    async function refreshWebsiteMetadata(websiteId: string, icon: Icon | null) {
        try {
            await fetch(
                `${userSettings.baseUrl}/api/screen-time/websites/${websiteId}/refresh-metadata`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        icon,
                    }),
                }
            );
        }
        catch (e) {
        }
    }
})

async function getIcon(iconUrl: string): Promise<Icon | null> {
    try {
        const response = await fetch(iconUrl)

        if (!response.ok) {
            throw new Error(
                `Failed to fetch icon: ${response.status}`
            )
        }

        const buffer = await response.arrayBuffer()

        const result = await fileTypeFromBuffer(buffer)

        // xml 格式的图片就是 svg 格式，svg 无法通过二进制特征识别
        const extension = (!result?.ext || result.ext === "xml") ? "svg" : result.ext

        return {
            extension: extension,
            data: arrayBufferToBase64(buffer)
        }
    }
    catch (e) {
        console.log("getIcon failed: ", e)
        return null
    }
}

function arrayBufferToBase64(buffer: ArrayBuffer): string {
    let binary = '';
    const bytes = new Uint8Array(buffer);

    for (let i = 0; i < bytes.byteLength; i++) {
        binary += String.fromCharCode(bytes[i]!);
    }

    return btoa(binary);
}
