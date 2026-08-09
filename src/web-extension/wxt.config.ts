import { defineConfig } from 'wxt';

// See https://wxt.dev/api/config.html
export default defineConfig({
    vite: () => ({
        server: {
            watch: {
                usePolling: true,
            },
        },
    }),
    dev: {
        server: {
            host: '0.0.0.0',
        },
    },
    manifest: {
        name: "STT Web Extension",
        description: "Collect website usage data for your local app.",
        host_permissions: [
            "<all_urls>"
        ],
        permissions: ['contextMenus','storage'],
        action: {}
    }
});
