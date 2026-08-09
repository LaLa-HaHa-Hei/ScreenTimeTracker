import { userSettingsStorage } from "@/utils/settings";

export default defineContentScript({
  matches: ['<all_urls>'],
  async main() {
    let userSettings = await userSettingsStorage.getValue();
    userSettingsStorage.watch((newValue) => {
      userSettings = newValue;
    });
    let recordActivityTimer: number | null = null;
    const intervalMs = 1500
    const noHostPlaceholder = "no-host"
    let host = window.location.host || noHostPlaceholder
    let name = extractWebSiteName() || host

    window.addEventListener('focus', startRecordActivity)
    window.addEventListener('blur', stopRecordActivity)

    if (document.hasFocus())
      startRecordActivity()

    function startRecordActivity() {
      if (recordActivityTimer != null)
        return

      recordActivityTimer =
        window.setInterval(async () => {
          try {
            await fetch(
              `${userSettings.baseUrl}/api/screen-time/tracking/track-website-usage/record-activity`,
              {
                method: "POST",
                headers: {
                  "Content-Type": "application/json",
                },
                body: JSON.stringify({
                  host: host,
                  name: name,
                  durationMilliseconds: intervalMs
                }),
              }
            );
          } catch (e) {
          }
        }, intervalMs)
    }


    function stopRecordActivity() {
      if (recordActivityTimer === null)
        return

      clearInterval(recordActivityTimer)

      recordActivityTimer = null
    }

  },
});

function extractWebSiteName(): string | null {
  return (
    getOgSiteName() ||
    getApplicationName() ||
    getJsonLdName()
  );
}

/**
 * Open Graph site name
 * 
 * Example:
 * <meta property="og:site_name" content="GitHub">
 */
function getOgSiteName(): string | null {

  const meta =
    document.querySelector(
      'meta[property="og:site_name"]'
    );

  return meta?.getAttribute("content") ?? null
}

/**
 * application-name
 *
 * Example:
 * <meta name="application-name" content="Notion">
 */
function getApplicationName(): string | null {

  const meta =
    document.querySelector(
      'meta[name="application-name"]'
    );

  return meta?.getAttribute("content") ?? null
}


/**
 * JSON-LD structured data
 *
 * Example:
 * {
 *   "@type":"Organization",
 *   "name":"Example"
 * }
 */
function getJsonLdName(): string | null {
  const scripts =
    document.querySelectorAll(
      'script[type="application/ld+json"]'
    );

  for (const script of scripts) {
    try {
      const json =
        JSON.parse(
          script.textContent || ""
        );

      const items = Array.isArray(json)
        ? json
        : [
          json,
          ...(json["@graph"] ?? [])
        ];

      for (const item of items) {
        const name =
          item?.name ||
          item?.publisher?.name ||
          item?.author?.name;

        if (name && typeof name === "string")
          return name
      }
    } catch {
      // ignore invalid JSON-LD
    }
  }


  return null;
}