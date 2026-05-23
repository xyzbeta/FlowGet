// M3U8 Sniffer 设置页面

import { probePort } from './lib/rest-client.js';
import { getSettings, saveSettings, DEFAULT_SETTINGS } from './utils/storage.js';

// ====== 初始化 ======

document.addEventListener('DOMContentLoaded', async () => {
    const settings = await getSettings();
    loadSettings(settings);
    bindEvents();
});

function loadSettings(settings) {
    document.getElementById('port').value = settings.port || DEFAULT_SETTINGS.port;
    document.getElementById('savePath').value = settings.savePath || '';
    document.getElementById('h-cookie').checked = settings.headers.cookie !== false;
    document.getElementById('h-referer').checked = settings.headers.referer !== false;
    document.getElementById('h-user-agent').checked = settings.headers.userAgent !== false;
    document.getElementById('h-authorization').checked = settings.headers.authorization === true;
}

// ====== 事件绑定 ======

function bindEvents() {
    document.getElementById('btn-save').addEventListener('click', onSave);
    document.getElementById('btn-probe').addEventListener('click', onProbe);
}

async function onSave() {
    const settings = {
        port: parseInt(document.getElementById('port').value) || 65432,
        savePath: document.getElementById('savePath').value.trim(),
        headers: {
            cookie: document.getElementById('h-cookie').checked,
            referer: document.getElementById('h-referer').checked,
            origin: document.getElementById('h-referer').checked,
            userAgent: document.getElementById('h-user-agent').checked,
            authorization: document.getElementById('h-authorization').checked
        }
    };
    await saveSettings(settings);
    showToast('设置已保存');
}

async function onProbe() {
    const statusEl = document.getElementById('port-status');
    statusEl.textContent = '检测中...';
    statusEl.className = 'status-text';

    const port = await probePort();
    if (port) {
        document.getElementById('port').value = port;
        statusEl.textContent = `已检测到运行中的服务，端口: ${port}`;
        statusEl.className = 'status-text status-ok';
    } else {
        statusEl.textContent = '未检测到 FlowGet，请确认已启动';
        statusEl.className = 'status-text status-fail';
    }
}

function showToast(text) {
    const toast = document.getElementById('toast');
    toast.textContent = text;
    toast.classList.add('show');
    setTimeout(() => toast.classList.remove('show'), 2000);
}
