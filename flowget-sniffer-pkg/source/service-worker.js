// M3U8 Sniffer Service Worker (无 import，避免 MV3 模块加载问题)

// ====== 内联 storage 工具 ======

async function getStreams() {
    const r = await chrome.storage.local.get('streams');
    return r.streams || {};
}

async function addStream(tabId, stream) {
    const all = await getStreams();
    const key = `tab_${tabId}`;
    if (!all[key]) all[key] = [];
    if (all[key].some(s => s.url === stream.url)) return;
    all[key].push({
        ...stream,
        id: Date.now().toString(36) + Math.random().toString(36).slice(2, 6),
        status: 'ready',
        timestamp: Date.now(),
        tabId
    });
    await chrome.storage.local.set({ streams: all });
}

async function updateStreamStatus(tabId, streamId, status, error) {
    const all = await getStreams();
    const key = `tab_${tabId}`;
    const item = (all[key] || []).find(s => s.id === streamId);
    if (item) {
        item.status = status;
        if (error) item.error = error;
        await chrome.storage.local.set({ streams: all });
    }
}

async function updateStreamMeta(tabId, streamId, meta) {
    const all = await getStreams();
    const key = `tab_${tabId}`;
    const item = (all[key] || []).find(s => s.id === streamId);
    if (item) {
        if (meta.playlistType) item.playlistType = meta.playlistType;
        if (meta.segmentCount != null) {
            item.segmentCount = meta.segmentCount;
            item.estimatedDuration = meta.segmentCount * 5; // 平均5秒/片
        }
        if (meta.bandwidth) item.bandwidth = meta.bandwidth;
        if (meta.resolution) item.resolution = meta.resolution;
        await chrome.storage.local.set({ streams: all });
    }
}

async function getSettings() {
    const r = await chrome.storage.local.get('settings');
    return r.settings || { port: 65432, savePath: '', paused: false, blocklist: [], headers: { cookie: true, referer: true, origin: true, userAgent: true, authorization: false } };
}

async function cleanOldStreams() {
    const all = await getStreams();
    const cutoff = Date.now() - 3600000;
    for (const k of Object.keys(all)) {
        all[k] = all[k].filter(s => s.timestamp > cutoff);
        if (all[k].length === 0) delete all[k];
    }
    await chrome.storage.local.set({ streams: all });
}

// ====== 内联 rest-client 工具 ======

let cachedPort = null;
let lastProbeTime = 0;

async function probePort() {
    const now = Date.now();
    if (cachedPort !== null) return cachedPort;
    if (lastProbeTime > 0 && (now - lastProbeTime) < 30000) return null;
    lastProbeTime = now;
    for (let port = 65432; port > 65400; port--) {
        try {
            const ctrl = new AbortController();
            const t = setTimeout(() => ctrl.abort(), 500);
            await fetch(`http://localhost:${port}/`, { signal: ctrl.signal });
            clearTimeout(t);
            cachedPort = port;
            lastProbeTime = 0;
            return port;
        } catch (_) {}
    }
    return null;
}

function invalidatePort() { cachedPort = null; }

async function sendDownload(url, filename, savePath, headers) {
    const port = await probePort();
    if (!port) return { success: false, error: 'FlowGet 未运行或端口不可达' };
    const name = (filename && !isMeaninglessName(filename)) ? filename : null;
    try {
        const body = { url, name, SavePath: savePath || null };
        if (headers && Object.keys(headers).length > 0) body.Headers = headers;
        const resp = await fetch(`http://localhost:${port}/downloadbyurl`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        if (!resp.ok) return { success: false, error: `HTTP ${resp.status}` };
        const r = await resp.json();
        return r.Code === 0 ? { success: true } : { success: false, error: r.Message || '未知错误' };
    } catch (e) {
        invalidatePort();
        return { success: false, error: e.message };
    }
}

function isMeaninglessName(name) {
    const w = name.replace(/\.[^/.]+$/, '');
    return w.length < 2 || /^\d+$/.test(w) || /^[a-fA-F0-9]{32,64}$/.test(w);
}

// ====== 消息处理 ======

chrome.runtime.onMessage.addListener((msg, sender, sendResponse) => {
    handleMessage(msg, sender).then(sendResponse);
    return true;
});

async function handleMessage(msg, sender) {
    switch (msg.type) {
        case 'streamDetected': {
            const tabId = sender.tab?.id;
            console.log('[M3U8 Sniffer] streamDetected tab=', tabId, 'url=', msg.url);
            if (tabId == null) return { ok: false };
            const filename = extractFilename(msg.url) || sanitizeTitle(msg.title) || null;
            const streamId = Date.now().toString(36) + Math.random().toString(36).slice(2, 6);
            await addStream(tabId, {
                id: streamId,
                url: msg.url,
                filename,
                referer: msg.pageUrl || '',
                headers: {},
                tabUrl: msg.pageUrl || ''
            });
            await updateBadge(tabId);

            // 异步分析 M3U8（不阻塞响应）
            analyzeM3u8(tabId, streamId, msg.url);

            console.log('[M3U8 Sniffer] saved, filename=', filename);
            return { ok: true };
        }
        case 'getStreams': {
            const all = await getStreams();
            return { streams: all[`tab_${msg.tabId}`] || [], tabId: msg.tabId };
        }
        case 'send': {
            const { tabId, stream } = msg;
            await updateStreamStatus(tabId, stream.id, 'sending');
            const settings = await getSettings();
            const headers = {};
            if (stream.headers) {
                if (settings.headers.cookie && stream.headers.cookie) headers.Cookie = stream.headers.cookie;
                if (settings.headers.referer && stream.headers.referer) headers.Referer = stream.headers.referer;
                if (settings.headers.origin && stream.headers.origin) headers.Origin = stream.headers.origin;
                if (settings.headers.userAgent && stream.headers['user-agent']) headers['User-Agent'] = stream.headers['user-agent'];
                if (settings.headers.authorization && stream.headers.authorization) headers.Authorization = stream.headers.authorization;
            }
            const r = await sendDownload(stream.url, stream.filename, settings.savePath, headers);
            await updateStreamStatus(tabId, stream.id, r.success ? 'sent' : 'error', r.error || null);
            if (r.success) await addToSentHistory(stream.url, stream.filename);
            return { success: r.success, error: r.error, streamId: stream.id };
        }
        case 'sendAll': {
            const all = await getStreams();
            const list = all[`tab_${msg.tabId}`] || [];
            const results = [];
            for (const s of list.filter(x => x.status === 'ready'))
                results.push(await handleMessage({ type: 'send', tabId: msg.tabId, stream: s }, sender));
            return { results };
        }
        case 'deleteStream': {
            const all = await getStreams();
            const list = all[`tab_${msg.tabId}`] || [];
            all[`tab_${msg.tabId}`] = list.filter(s => s.id !== msg.streamId);
            await chrome.storage.local.set({ streams: all });
            if (msg.tabId != null) await updateBadge(msg.tabId);
            return { success: true };
        }
        case 'clear': {
            const all = await getStreams();
            delete all[`tab_${msg.tabId}`];
            await chrome.storage.local.set({ streams: all });
            return { success: true };
        }
        case 'checkUrl': return checkSentHistory(msg.url);
        case 'getSentHistory': return await getSentHistory();
        case 'probe': return { port: await probePort() };
        default: return { error: 'unknown' };
    }
}

// ====== M3U8 分析 ======

async function analyzeM3u8(tabId, streamId, url) {
    try {
        const ctrl = new AbortController();
        const t = setTimeout(() => ctrl.abort(), 5000);
        const resp = await fetch(url, { signal: ctrl.signal });
        clearTimeout(t);
        if (!resp.ok) return;
        const text = await resp.text();
        if (!text) return;

        const meta = {};
        const hasMaster = /^#EXT-X-STREAM-INF/im.test(text);
        const hasExtinf = /^#EXTINF/im.test(text);

        if (hasMaster && !hasExtinf) {
            meta.playlistType = 'master';
        } else if (hasExtinf) {
            meta.playlistType = 'variant';
            const matches = text.match(/^#EXTINF:/gim);
            meta.segmentCount = matches ? matches.length : 0;

            // 尝试提取分辨率/码率
            const bwMatch = text.match(/BANDWIDTH=(\d+)/i);
            if (bwMatch) meta.bandwidth = parseInt(bwMatch[1]);
            const resMatch = text.match(/RESOLUTION=(\d+x\d+)/i);
            if (resMatch) meta.resolution = resMatch[1];
        }
        await updateStreamMeta(tabId, streamId, meta);
    } catch (_) { /* 分析失败静默 */ }
}

// ====== 发送历史 ======

async function getSentHistory() {
    const r = await chrome.storage.local.get('sentHistory');
    return r.sentHistory || [];
}

async function addToSentHistory(url, filename) {
    const hist = await getSentHistory();
    // 去重：同 URL 已存在则更新时间
    const existing = hist.find(h => h.url === url);
    if (existing) {
        existing.sentAt = new Date().toISOString();
        existing.filename = filename;
    } else {
        hist.unshift({ url, filename: filename || '', sentAt: new Date().toISOString() });
        if (hist.length > 200) hist.length = 200;
    }
    await chrome.storage.local.set({ sentHistory: hist });
}

function checkSentHistory(url) {
    // 同步查找需要先 get，这里返回 Promise 供 caller await
    return getSentHistory().then(hist => {
        const found = hist.find(h => h.url === url);
        return found ? { sent: true, sentAt: found.sentAt } : { sent: false };
    });
}

// ====== 文件名提取 ======

function extractFilename(url) {
    try {
        const segs = new URL(url).pathname.split('/').filter(Boolean);
        if (!segs.length) return null;
        const bad = new Set([
            'index','playlist','master','stream','media','chunklist','output',
            'hls','vod','video','dash','live','manifest','segment','seg'
        ]);
        // 从后往前找第一个有意义的段
        for (let i = segs.length - 1; i >= 0; i--) {
            let s = segs[i].split('?')[0];
            const dotIdx = s.lastIndexOf('.');
            if (dotIdx > 0) s = s.substring(0, dotIdx);
            s = s.trim();
            if (!s) continue;
            if (bad.has(s.toLowerCase())) continue;
            if (/^\d+$/.test(s)) continue;                       // 纯数字
            if (/^[a-fA-F0-9]{20,}$/.test(s)) continue;         // 纯 hex hash
            if (/^[a-zA-Z0-9_-]{22,}$/.test(s)) continue;       // base64/uuid 类 ID
            return s;
        }
        return null;
    } catch { return null; }
}

function sanitizeTitle(t) {
    if (!t || t.length < 2) return null;
    // 尝试提取中文/英文有意义的标题
    // 常见模式: "视频名 - 网站名"、"视频名_网站名"、"视频名|网站名"
    let cleaned = t
        .replace(/\s*[-_|—–]\s*(在线观看|高清视频|HD|TS|BD|无水印|抢先版|完整版).*/gi, '')
        .replace(/\s*[-_|—–]\s*[^-_|—–]*$/, '')  // 去掉末尾的网站名
        .replace(/[\\/:*?"<>|]/g, '')             // 去非法字符
        .trim();
    return cleaned.substring(0, 100) || null;
}

async function updateBadge(tabId) {
    try {
        const all = await getStreams();
        const count = (all[`tab_${tabId}`] || []).length;
        chrome.action.setBadgeText({ text: count > 0 ? String(count) : '', tabId });
    } catch (_) {
        // 标签页可能已关闭，忽略
    }
}

// 全局 badge 颜色初始化
chrome.action.setBadgeBackgroundColor({ color: '#1a73e8' });

// ====== 清理 ======

chrome.tabs.onRemoved.addListener(async (tabId) => {
    const all = await getStreams();
    delete all[`tab_${tabId}`];
    await chrome.storage.local.set({ streams: all });
    chrome.action.setBadgeText({ text: '', tabId });
});

cleanOldStreams();
setInterval(cleanOldStreams, 600000);
