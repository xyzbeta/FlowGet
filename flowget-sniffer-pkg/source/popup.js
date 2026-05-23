// M3U8 Sniffer Popup — 列表 + 内嵌设置

import { checkSuspicious } from './utils/suspicious.js';

let currentTabId = null;
let settingsVisible = false;

const DEFAULT_SETTINGS = {
    port: 65432, savePath: '',
    paused: false,
    blocklist: [],
    headers: { cookie: true, referer: true, origin: true, userAgent: true, authorization: false }
};

// ====== 初始化 ======

document.addEventListener('DOMContentLoaded', async () => {
    const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
    currentTabId = tab?.id;
    if (!currentTabId) return;

    bindButtons();
    await checkConnection();
    await refreshList();
    // 初始化暂停按钮状态
    const saved = await chrome.storage.local.get('settings');
    settingsCache = saved.settings || { ...DEFAULT_SETTINGS };
    setPauseUI(settingsCache.paused);
    await loadSettingsUI();

    chrome.storage.onChanged.addListener((changes, ns) => {
        if (ns === 'local') {
            if (changes.streams) refreshList();
            if (changes.settings) {
                settingsCache = changes.settings.newValue || { ...DEFAULT_SETTINGS };
                setPauseUI(settingsCache.paused);
            }
        }
    });
});

// ====== 视图切换 ======

let settingsCache = { ...DEFAULT_SETTINGS };

function bindButtons() {
    document.getElementById('btn-toggle').addEventListener('click', toggleSettings);
    document.getElementById('btn-clear').addEventListener('click', onClear);
    document.getElementById('btn-send-all').addEventListener('click', onSendAll);
    document.getElementById('btn-pause').addEventListener('click', onTogglePause);
    document.getElementById('btn-probe').addEventListener('click', onProbe);
    document.getElementById('btn-save').addEventListener('click', onSaveSettings);
    document.getElementById('cfg-pause').addEventListener('click', onTogglePauseFromSettings);
    document.getElementById('btn-add-host').addEventListener('click', onAddCurrentHost);
    // banner 按钮在 checkSuspiciousBanner 中动态绑定
}

function toggleSettings() {
    settingsVisible = !settingsVisible;
    document.getElementById('view-list').style.display = settingsVisible ? 'none' : '';
    document.getElementById('view-settings').style.display = settingsVisible ? '' : 'none';
    document.getElementById('btn-toggle').innerHTML = settingsVisible ? '&#10005;' : '&#9881;';
    if (settingsVisible) loadSettingsUI();
}

// ====== 连接状态 ======

async function checkConnection() {
    setStatus('checking', '检测中...');
    try {
        const resp = await chrome.runtime.sendMessage({ type: 'probe' });
        if (resp?.port) {
            setStatus('connected', `已连接 :${resp.port}`);
        } else {
            setStatus('disconnected', 'FlowGet 未运行');
        }
    } catch {
        setStatus('disconnected', 'FlowGet 未运行');
    }
}

function setStatus(state, text) {
    const bar = document.getElementById('status-bar');
    bar.className = `status-bar ${state}`;
    document.getElementById('status-text').textContent = text;
}

// ====== 流列表 ======

async function refreshList() {
    const resp = await chrome.runtime.sendMessage({ type: 'getStreams', tabId: currentTabId });
    const streams = resp?.streams || [];
    const list = document.getElementById('stream-list');
    const empty = document.getElementById('empty-state');

    list.querySelectorAll('.stream-card').forEach(c => c.remove());

    if (streams.length === 0) {
        empty.style.display = '';
        document.getElementById('btn-send-all').disabled = true;
        return;
    }

    empty.style.display = 'none';
    document.getElementById('btn-send-all').disabled = !streams.some(s => s.status === 'ready');

    // 批量加载发送历史
    sentHistoryCache = await loadSentHistoryCache();

    streams.sort((a, b) => b.timestamp - a.timestamp);

    // 检测可疑域名
    checkSuspiciousBanner(streams);

    for (const stream of streams) {
        list.appendChild(await buildCard(stream));
    }

    // 用完后重置
    sentHistoryCache = null;
}

async function buildCard(stream) {
    const card = document.createElement('div');
    card.className = 'stream-card';

    const name = stream.filename || '未命名';
    let urlDisplay;
    try {
        const u = new URL(stream.url);
        urlDisplay = u.hostname + u.pathname;
    } catch { urlDisplay = stream.url; }

    // 播放列表类型标签
    let typeTag = '';
    if (stream.playlistType === 'master') {
        typeTag = '<span class="type-tag type-master">⭐ 主列表</span>';
    } else if (stream.playlistType === 'variant') {
        let label = '🎬 子列表';
        if (stream.resolution) label = `🎬 ${stream.resolution.split('x')[1]}p`;
        if (stream.bandwidth) label += `·${Math.round(stream.bandwidth / 1000 / 100) / 10}Mbps`;
        typeTag = `<span class="type-tag type-variant">${label}</span>`;
    }

    // 分片数 & 预估时长
    let metaHtml = '';
    if (stream.segmentCount > 0) {
        const dur = stream.estimatedDuration || stream.segmentCount * 5;
        const min = Math.floor(dur / 60);
        const sec = dur % 60;
        const durStr = min > 0 ? `${min}分${sec}秒` : `${sec}秒`;
        metaHtml = `<div class="stream-meta">🎞 ${stream.segmentCount} 分片 ≈ ${durStr}</div>`;
    }

    // 发送历史检查
    let statusHtml;
    let actionHtml;
    const sent = checkIfSentLocally(stream.url);
    if (sent) {
        const ago = timeAgo(sent.sentAt);
        statusHtml = `<span class="status-sent-before">✅ 已发送 ${ago}</span>`;
        actionHtml = '';
    } else {
        const stMap = {
            ready: { text: '就绪', cls: 'status-ready' },
            sending: { text: '发送中...', cls: 'status-sending' },
            sent: { text: '已发送', cls: 'status-sent' },
            error: { text: '失败', cls: 'status-error' }
        };
        const st = stMap[stream.status] || stMap.ready;
        statusHtml = `<span class="stream-status ${st.cls}">${st.text}</span>`;
        actionHtml = actionBtn(stream);
    }

    card.innerHTML = `
        <div class="stream-name" title="${esc(stream.url)}">
            ${esc(name)}${typeTag}
            <button class="card-del" data-id="${stream.id}" title="删除此条">&#10005;</button>
        </div>
        <div class="stream-url">${esc(urlDisplay)}</div>
        ${metaHtml}
        <div class="stream-action">
            ${statusHtml}
            ${actionHtml}
        </div>
        ${stream.error ? `<div class="error-msg">${esc(stream.error)}</div>` : ''}
    `;

    const delBtn = card.querySelector('.card-del');
    if (delBtn) delBtn.addEventListener('click', (e) => { e.stopPropagation(); onDelete(stream); });
    const sendBtn = card.querySelector('.btn-send, .btn-retry');
    if (sendBtn) sendBtn.addEventListener('click', () => onSend(stream));
    return card;
}

function actionBtn(s) {
    if (s.status === 'ready') return '<button class="btn-send">&#128228; 发送</button>';
    if (s.status === 'error') return '<button class="btn-retry">&#8635; 重试</button>';
    if (s.status === 'sending') return '<button class="btn-send" disabled>发送中...</button>';
    return '';
}

async function onSend(stream) {
    await chrome.runtime.sendMessage({ type: 'send', tabId: currentTabId, stream });
}

async function onSendAll() {
    await chrome.runtime.sendMessage({ type: 'sendAll', tabId: currentTabId });
}

async function onDelete(stream) {
    await chrome.runtime.sendMessage({ type: 'deleteStream', tabId: currentTabId, streamId: stream.id });
    refreshList();
}

async function onClear() {
    await chrome.runtime.sendMessage({ type: 'clear', tabId: currentTabId });
    refreshList();
}

// ====== 设置面板 ======

async function loadSettingsUI() {
    const result = await chrome.storage.local.get('settings');
    const s = result.settings || DEFAULT_SETTINGS;
    settingsCache = s;

    // 暂停开关
    setPauseUI(s.paused);

    // 黑名单
    document.getElementById('cfg-blocklist').value = (s.blocklist || []).join('\n');

    // 当前页面域名
    const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
    try {
        const host = new URL(tab.url).hostname;
        document.getElementById('cfg-current-host').textContent = host;
    } catch { document.getElementById('cfg-current-host').textContent = '-'; }

    // 现有设置
    document.getElementById('cfg-port').value = s.port || 65432;
    document.getElementById('cfg-savepath').value = s.savePath || '';
    document.getElementById('cfg-h-cookie').checked = s.headers?.cookie !== false;
    document.getElementById('cfg-h-referer').checked = s.headers?.referer !== false;
    document.getElementById('cfg-h-ua').checked = s.headers?.userAgent !== false;
    document.getElementById('cfg-h-auth').checked = s.headers?.authorization === true;
}

async function onSaveSettings() {
    settingsCache.paused = settingsCache.paused || false;
    settingsCache.blocklist = document.getElementById('cfg-blocklist').value
        .split('\n').map(s => s.trim()).filter(Boolean);
    settingsCache.port = parseInt(document.getElementById('cfg-port').value) || 65432;
    settingsCache.savePath = document.getElementById('cfg-savepath').value.trim();
    settingsCache.headers = {
        cookie: document.getElementById('cfg-h-cookie').checked,
        referer: document.getElementById('cfg-h-referer').checked,
        origin: document.getElementById('cfg-h-referer').checked,
        userAgent: document.getElementById('cfg-h-ua').checked,
        authorization: document.getElementById('cfg-h-auth').checked
    };
    await chrome.storage.local.set({ settings: settingsCache });
    document.getElementById('save-msg').textContent = '已保存';
    setTimeout(() => document.getElementById('save-msg').textContent = '', 1500);
    setPauseUI(settingsCache.paused);
    checkConnection();
}

// ====== 暂停 / 黑名单 ======

async function onTogglePause() {
    settingsCache.paused = !settingsCache.paused;
    await applyPause();
}

async function onTogglePauseFromSettings() {
    settingsCache.paused = !settingsCache.paused;
    setPauseUI(settingsCache.paused);
    await chrome.storage.local.set({ settings: settingsCache });
}

async function applyPause() {
    setPauseUI(settingsCache.paused);
    // 合并 blocklist（从文本框取出，避免丢失未保存的更改）
    const blText = document.getElementById('cfg-blocklist')?.value || '';
    settingsCache.blocklist = blText.split('\n').map(s => s.trim()).filter(Boolean);
    await chrome.storage.local.set({ settings: settingsCache });
}

function setPauseUI(paused) {
    // 列表页开关
    const btn = document.getElementById('btn-pause');
    if (paused) {
        btn.textContent = '▶ 已暂停';
        btn.className = 'pause-btn off';
    } else {
        btn.textContent = '⏸ 暂停嗅探';
        btn.className = 'pause-btn';
    }
    // 设置页开关
    const cfgBtn = document.getElementById('cfg-pause');
    if (paused) {
        cfgBtn.textContent = '已暂停';
        cfgBtn.className = 'toggle-btn off';
    } else {
        cfgBtn.textContent = '已启用';
        cfgBtn.className = 'toggle-btn on';
    }
}

async function onAddCurrentHost() {
    const hostText = document.getElementById('cfg-current-host').textContent;
    if (!hostText || hostText === '-') return;
    let bl = document.getElementById('cfg-blocklist').value
        .split('\n').map(s => s.trim()).filter(Boolean);
    if (!bl.includes(hostText)) {
        bl.push(hostText);
        document.getElementById('cfg-blocklist').value = bl.join('\n');
    }
}

async function onProbe() {
    const status = document.getElementById('port-status');
    status.textContent = '检测中...'; status.className = 'field-hint';
    try {
        const resp = await chrome.runtime.sendMessage({ type: 'probe' });
        if (resp?.port) {
            document.getElementById('cfg-port').value = resp.port;
            status.textContent = `已检测到，端口: ${resp.port}`;
            status.className = 'field-hint ok';
        } else {
            status.textContent = '未检测到运行中的服务';
            status.className = 'field-hint fail';
        }
    } catch {
        status.textContent = '探测失败';
        status.className = 'field-hint fail';
    }
}

// ====== 发送历史检查 ======

let sentHistoryCache = null;

async function loadSentHistoryCache() {
    if (!sentHistoryCache) {
        sentHistoryCache = await chrome.runtime.sendMessage({ type: 'getSentHistory' }) || [];
    }
    return sentHistoryCache;
}

function checkIfSentLocally(url) {
    if (!sentHistoryCache) return null;
    const found = sentHistoryCache.find(h => h.url === url);
    return found ? { sent: true, sentAt: found.sentAt } : null;
}

function timeAgo(iso) {
    const diff = (Date.now() - new Date(iso).getTime()) / 1000;
    if (diff < 60) return '刚刚';
    if (diff < 3600) return Math.round(diff / 60) + '分钟前';
    if (diff < 86400) return Math.round(diff / 3600) + '小时前';
    return Math.round(diff / 86400) + '天前';
}

// ====== 可疑域名提示 ======

function checkSuspiciousBanner(streams) {
    const banner = document.getElementById('suspicious-banner');
    if (!banner) return;

    // 找第一个可疑域名
    for (const s of streams) {
        const result = checkSuspicious(s.url);
        if (result) {
            try {
                const host = new URL(s.url).hostname;
                banner.style.display = 'flex';
                document.getElementById('banner-text').textContent =
                    `⚠ 检测到来自 ${host} 的资源，可能是统计/广告流 (${result.keyword})`;
                const btn = document.getElementById('banner-action');
                btn.textContent = '加入黑名单';
                btn.onclick = async () => {
                    await addToBlocklist(host);
                    banner.style.display = 'none';
                };
            } catch { banner.style.display = 'none'; }
            return;
        }
    }
    banner.style.display = 'none';
}

async function addToBlocklist(host) {
    const result = await chrome.storage.local.get('settings');
    const s = result.settings || settingsCache;
    if (!s.blocklist) s.blocklist = [];
    if (!s.blocklist.includes(host)) {
        s.blocklist.push(host);
        settingsCache.blocklist = s.blocklist;
        await chrome.storage.local.set({ settings: s });
    }
}

function esc(s) {
    const d = document.createElement('div');
    d.textContent = String(s);
    return d.innerHTML;
}
