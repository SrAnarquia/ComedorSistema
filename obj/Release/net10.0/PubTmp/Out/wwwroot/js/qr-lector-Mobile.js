const html5QrCode = new Html5Qrcode("reader");
let scanning = false;
let restarting = false;
let lastBox = { width: 0, height: 0 };

/* ========= MOBILE ========= */
function isMobile() {
    return /android|iphone|ipad|ipod/i.test(navigator.userAgent);
}

/* ========= QR BOX ========= */
function getQrBoxFromFrame() {
    const frame = document.querySelector(".scanner-frame");
    if (!frame) return { width: 250, height: 250 };

    const r = frame.getBoundingClientRect();
    return { width: Math.floor(r.width), height: Math.floor(r.height) };
}

function needsRestart() {
    const box = getQrBoxFromFrame();
    const changed =
        Math.abs(box.width - lastBox.width) > 5 ||
        Math.abs(box.height - lastBox.height) > 5;

    lastBox = box;
    return changed;
}

const config = {
    fps: 10,
    qrbox: () => getQrBoxFromFrame(),
    disableFlip: true
};

/* ========= SUCCESS ========= */
async function onScanSuccess(decodedText) {
    if (scanning) return;
    scanning = true;

    try {
        await fetch("/LectorGenerador/RegistrarLectura", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ codigoQr: decodedText })
        });
    } finally {
        setTimeout(() => scanning = false, 1500);
    }
}

/* ========= START ========= */
async function startScanner() {
    try {
        let cameraConfig;

        if (isMobile()) {
            // ✅ CÁMARA TRASERA (LA BUENA)
            cameraConfig = {
                facingMode: { ideal: "environment" }
            };
        } else {
            const cams = await Html5Qrcode.getCameras();
            if (!cams.length) return;
            cameraConfig = cams[0].id;
        }

        await html5QrCode.start(cameraConfig, config, onScanSuccess);
    } catch (e) {
        console.error(e);
    }
}

/* ========= SAFE RESTART ========= */
async function safeRestart() {
    if (restarting || !needsRestart()) return;
    restarting = true;

    try {
        if (html5QrCode.isScanning) {
            await html5QrCode.stop();
        }
        await startScanner();
    } finally {
        restarting = false;
    }
}

/* ========= INIT ========= */
startScanner();

/* ========= OBSERVER ========= */
const container = document.querySelector(".main-lectura") || document.body;
new ResizeObserver(() => safeRestart()).observe(container);