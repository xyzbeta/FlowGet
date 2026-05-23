// chrome.storage.local 封装

const STORAGE_KEYS = {
    streams: 'streams',
    settings: 'settings'
};

const DEFAULT_SETTINGS = {
    port: 65432,
    savePath: '',
    headers: {
        cookie: true,
        referer: true,
        origin: true,
        userAgent: true,
        authorization: false
    },
    notifyOnSuccess: false
};

async function getStreams() {
    const result = await chrome.storage.local.get(STORAGE_KEYS.streams);
    return result[STORAGE_KEYS.streams] || {};
}

async function getStreamsForTab(tabId) {
    const all = await getStreams();
    return all[`tab_${tabId}`] || [];
}

async function addStream(tabId, stream) {
    const all = await getStreams();
    const key = `tab_${tabId}`;
    if (!all[key]) all[key] = [];

    // 去重
    const exists = all[key].some(s => s.url === stream.url);
    if (exists) return;

    all[key].push({
        ...stream,
        id: Date.now().toString(36) + Math.random().toString(36).slice(2, 6),
        status: 'ready',
        timestamp: Date.now(),
        tabId: tabId
    });
    await chrome.storage.local.set({ [STORAGE_KEYS.streams]: all });
}

async function updateStreamStatus(tabId, streamId, status, error) {
    const all = await getStreams();
    const key = `tab_${tabId}`;
    const list = all[key] || [];
    const item = list.find(s => s.id === streamId);
    if (item) {
        item.status = status;
        if (error) item.error = error;
        await chrome.storage.local.set({ [STORAGE_KEYS.streams]: all });
    }
}

async function clearStreams(tabId) {
    const all = await getStreams();
    delete all[`tab_${tabId}`];
    await chrome.storage.local.set({ [STORAGE_KEYS.streams]: all });
}

async function cleanOldStreams() {
    const all = await getStreams();
    const cutoff = Date.now() - 3600000; // 1小时
    for (const key of Object.keys(all)) {
        all[key] = all[key].filter(s => s.timestamp > cutoff);
        if (all[key].length === 0) delete all[key];
    }
    await chrome.storage.local.set({ [STORAGE_KEYS.streams]: all });
}

async function getSettings() {
    const result = await chrome.storage.local.get(STORAGE_KEYS.settings);
    return result[STORAGE_KEYS.settings] || { ...DEFAULT_SETTINGS };
}

async function saveSettings(settings) {
    await chrome.storage.local.set({ [STORAGE_KEYS.settings]: settings });
}

export {
    getStreamsForTab, addStream, updateStreamStatus, clearStreams, cleanOldStreams,
    getSettings, saveSettings, DEFAULT_SETTINGS
};
