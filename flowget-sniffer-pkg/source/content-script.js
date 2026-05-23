// M3U8 Sniffer Content Script (MAIN world)
// 拦截 fetch 和 XMLHttpRequest，检测 M3U8 请求后通过 postMessage 传出
// 从 ISOLATED world bridge 接收暂停/黑名单配置

(() => {
    const M3U8_RE = /\.m3u8(\?.*)?$/i;

    // 配置（由 bridge 通过 postMessage 同步）
    let _paused = false;
    let _blocklist = [];

    window.addEventListener('message', (event) => {
        if (!event.data?.__m3u8_config_sync) return;
        _paused = !!event.data.paused;
        _blocklist = event.data.blocklist || [];
    });

    function isBlocked() {
        if (_paused) return true;
        const host = location.hostname;
        if (!host || !_blocklist.length) return false;
        return _blocklist.some(domain =>
            host === domain || host.endsWith('.' + domain)
        );
    }

    function notify(url) {
        if (isBlocked()) return;
        console.log('[M3U8 Sniffer] detected:', url);
        window.postMessage({
            __m3u8_sniffer: true,
            type: 'streamDetected',
            url: url,
            title: document.title || '',
            pageUrl: location.href
        }, '*');
    }

    // 拦截 fetch
    const origFetch = window.fetch;
    window.fetch = function(input) {
        const url = typeof input === 'string' ? input : (input?.url || input?.href || '');
        if (M3U8_RE.test(url)) notify(url);
        return origFetch.apply(this, arguments);
    };

    // 拦截 XMLHttpRequest
    const origOpen = XMLHttpRequest.prototype.open;
    XMLHttpRequest.prototype.open = function(method, url) {
        if (M3U8_RE.test(String(url))) notify(String(url));
        return origOpen.apply(this, arguments);
    };
})();
