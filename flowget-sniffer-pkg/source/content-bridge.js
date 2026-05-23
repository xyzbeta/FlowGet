// M3U8 Sniffer Content Bridge (ISOLATED world)
// 1. 从 storage 加载 paused/blocklist，通过 postMessage 发送给 MAIN world
// 2. 监听 storage 变化实时同步配置
// 3. 接收 MAIN world 的 M3U8 检测消息，转发给 Service Worker

let config = { paused: false, blocklist: [] };

function sendConfig() {
    window.postMessage({
        __m3u8_config_sync: true,
        paused: config.paused,
        blocklist: config.blocklist
    }, '*');
}

// 异步加载配置
(async () => {
    try {
        const result = await chrome.storage.local.get('settings');
        const s = result.settings || {};
        config = { paused: !!s.paused, blocklist: s.blocklist || [] };
        sendConfig();
    } catch (_) { sendConfig(); }
})();

// 监听配置变更 → 即时同步到 MAIN world
chrome.storage.onChanged.addListener((changes, ns) => {
    if (ns !== 'local' || !changes.settings) return;
    const s = changes.settings.newValue || {};
    config = { paused: !!s.paused, blocklist: s.blocklist || [] };
    sendConfig();
});

// 接收 MAIN world 的检测消息，转发给 Service Worker
window.addEventListener('message', (event) => {
    if (!event.data || !event.data.__m3u8_sniffer) return;
    chrome.runtime.sendMessage({
        type: 'streamDetected',
        url: event.data.url,
        title: event.data.title,
        pageUrl: event.data.pageUrl
    }).catch(() => { /* SW 未就绪 */ });
});
