// FlowGet REST API 客户端
// 负责端口探测和下载请求转发

const PORT_START = 65432;
const PORT_END = 65400;
const PROBE_TIMEOUT_MS = 500;
const PROBE_COOLDOWN_MS = 30000;

let cachedPort = null;
let lastProbeTime = 0;

async function probePort() {
    const now = Date.now();
    if (cachedPort !== null) return cachedPort;
    if (lastProbeTime > 0 && (now - lastProbeTime) < PROBE_COOLDOWN_MS) return null;

    lastProbeTime = now;

    for (let port = PORT_START; port > PORT_END; port--) {
        try {
            const controller = new AbortController();
            const timeout = setTimeout(() => controller.abort(), PROBE_TIMEOUT_MS);
            const resp = await fetch(`http://localhost:${port}/`, {
                signal: controller.signal
            });
            clearTimeout(timeout);
            cachedPort = port;
            lastProbeTime = 0;
            return port;
        } catch (e) {
            continue;
        }
    }
    return null;
}

function getCachedPort() {
    return cachedPort;
}

function invalidatePort() {
    cachedPort = null;
}

async function sendDownload(url, filename, savePath, headers) {
    const port = await probePort();
    if (!port) return { success: false, error: 'FlowGet 未运行或端口不可达' };

    const body = {
        url: url,
        name: !filename || isMeaninglessName(filename) ? null : filename,
        SavePath: savePath || null,
        Headers: headers && Object.keys(headers).length > 0 ? headers : null
    };

    try {
        const resp = await fetch(`http://localhost:${port}/downloadbyurl`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        if (!resp.ok) return { success: false, error: `HTTP ${resp.status}` };
        const result = await resp.json();
        if (result.Code === 0) return { success: true };
        return { success: false, error: result.Message || '未知错误' };
    } catch (e) {
        invalidatePort();
        return { success: false, error: e.message };
    }
}

function isMeaninglessName(name) {
    const withoutExt = name.replace(/\.[^/.]+$/, '');
    if (withoutExt.length < 2) return true;
    if (/^\d+$/.test(withoutExt)) return true;
    if (/^[a-fA-F0-9]{32,64}$/.test(withoutExt)) return true;
    return false;
}

export { probePort, getCachedPort, invalidatePort, sendDownload };
