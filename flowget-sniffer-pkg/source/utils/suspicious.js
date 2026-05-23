// 内置可疑/广告/统计域名模式

const SUSPICIOUS_DOMAINS = [
    'analytics', 'ads', 'tracking', 'metrics', 'pixel', 'beacon',
    'stats', 'counter', 'doubleclick', 'adservice', 'adnxs',
    'googlesyndication', 'moatads', 'scorecardresearch',
    'facebook.com/tr', 'tiktok.com/pixel', 'google-analytics.com',
    'hotjar.com', 'clarity.ms', 'segment.io', 'fullstory.com',
    'newrelic.com', 'datadoghq.com', 'sentry.io'
];

/**
 * 检测 URL 是否来自可疑域名
 * @param {string} url
 * @returns {{matched: boolean, keyword: string}|null}
 */
export function checkSuspicious(url) {
    try {
        const host = new URL(url).hostname.toLowerCase();
        for (const pattern of SUSPICIOUS_DOMAINS) {
            if (host.includes(pattern.toLowerCase())) {
                return { matched: true, keyword: pattern };
            }
        }
        return null;
    } catch {
        return null;
    }
}

export { SUSPICIOUS_DOMAINS };
