const html5QrCode = new Html5Qrcode("reader");
let scanning = false;
let currentCameraId = null;

/* ================= UTILS ================= */
function isMobile() {
    return /Android|iPhone|iPad|iPod/i.test(navigator.userAgent);
}

/* ================= QR BOX ================= */
function getQrBoxFromFrame() {
    const frame = document.querySelector(".scanner-frame");
    if (!frame) return { width: 250, height: 250 };

    const rect = frame.getBoundingClientRect();
    return {
        width: Math.floor(rect.width),
        height: Math.floor(rect.height)
    };
}

const config = {
    fps: 10,
    qrbox: () => getQrBoxFromFrame(),
    disableFlip: true
};

/* ================= MENSAJES ================= */
function mostrarMensaje(texto, tipo) {
    const alertBox = document.getElementById("alertOk");

    alertBox.classList.remove("d-none", "scan-success", "scan-error");
    alertBox.classList.add(tipo === "success" ? "scan-success" : "scan-error");
    alertBox.innerHTML = texto;

    setTimeout(() => {
        alertBox.classList.add("d-none");
        scanning = false;
    }, 2500);
}

/* ================= SCAN SUCCESS ================= */
async function onScanSuccess(decodedText) {
    if (scanning) return;
    scanning = true;

    try {
        const response = await fetch("/LectorGenerador/RegistrarLectura", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ codigoQr: decodedText })
        });

        const data = await response.json();
        let mensaje = data.mensaje;

        if (data.persona) {
            mensaje += `<br><strong>${data.persona}</strong>`;
        }

        mostrarMensaje(mensaje, response.ok ? "success" : "error");
    } catch {
        mostrarMensaje("Error de conexión", "error");
    }
}

/* ================= START SCANNER ================= */
async function startScanner() {
    try {
        let cameraConfig;

        if (isMobile()) {
            // ✅ MOBILE → FORZAR CÁMARA FRONTAL
            cameraConfig = {
                facingMode: { exact: "user" }
            };
        } else {
            // ✅ DESKTOP → LISTAR CÁMARAS
            const devices = await Html5Qrcode.getCameras();
            if (!devices.length) {
                mostrarMensaje("No se detectaron cámaras", "error");
                return;
            }

            const frontCamera = devices.find(d =>
                d.label.toLowerCase().includes("front") ||
                d.label.toLowerCase().includes("user")
            );

            currentCameraId = frontCamera ? frontCamera.id : devices[0].id;
            cameraConfig = currentCameraId;
        }

        await html5QrCode.start(
            cameraConfig,
            config,
            onScanSuccess
        );

    } catch (err) {
        console.error(err);
        mostrarMensaje("Error al iniciar cámara", "error");
    }
}

/* ================= RESTART ================= */
async function restartScanner() {
    if (html5QrCode.isScanning) {
        await html5QrCode.stop();
    }
    await startScanner();
}

/* ================= INIT ================= */
startScanner();

/* ================= RESIZE ================= */
window.addEventListener("resize", restartScanner);

/*const html5QrCode = new Html5Qrcode("reader");
let scanning = false;
let currentCameraId = null;*/

/* ================= QR BOX ================= */
/*
function getQrBoxFromFrame() {
    const frame = document.querySelector(".scanner-frame");
    if (!frame) return { width: 250, height: 250 };

    const rect = frame.getBoundingClientRect();
    return {
        width: Math.floor(rect.width),
        height: Math.floor(rect.height)
    };
}

const config = {
    fps: 10,
    qrbox: () => getQrBoxFromFrame()
};

/* ================= MENSAJES ================= */
function mostrarMensaje(texto, tipo) {
    const alertBox = document.getElementById("alertOk");

    alertBox.classList.remove("d-none", "scan-success", "scan-error");
    alertBox.classList.add(tipo === "success" ? "scan-success" : "scan-error");
    alertBox.innerHTML = texto;

    setTimeout(() => {
        alertBox.classList.add("d-none");
        scanning = false;
    }, 2500);
}

/* ================= SCAN SUCCESS ================= */
async function onScanSuccess(decodedText) {
    if (scanning) return;
    scanning = true;

    try {
        const response = await fetch("/LectorGenerador/RegistrarLectura", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ codigoQr: decodedText })
        });

        const data = await response.json();
        let mensaje = data.mensaje;

        if (data.persona) {
            mensaje += `<br><strong>${data.persona}</strong>`;
        }

        mostrarMensaje(mensaje, response.ok ? "success" : "error");
    } catch {
        mostrarMensaje("Error de conexión", "error");
    }
}

/* ================= START CAMERA ================= */
async function startScanner() {
    const devices = await Html5Qrcode.getCameras();
    if (!devices.length) return;

    const backCamera = devices.find(d =>
        d.label.toLowerCase().includes("back")
    );

    currentCameraId = backCamera ? backCamera.id : devices[0].id;

    await html5QrCode.start(
        currentCameraId,
        config,
        onScanSuccess
    );
}

/* ================= RESTART SCANNER ================= */
async function restartScanner() {
    if (!html5QrCode.isScanning || !currentCameraId) return;

    await html5QrCode.stop();
    await startScanner();
}

/* ================= INIT ================= */
startScanner();

/* ================= RESIZE OBSERVER (🔥 CLAVE) ================= */
const mainContainer = document.querySelector(".main-lectura");

const resizeObserver = new ResizeObserver(() => {
    restartScanner();
});

resizeObserver.observe(mainContainer);

/* ================= SIDEBAR TRANSITION ================= */
const sidebar = document.getElementById("sidebar");

sidebar.addEventListener("transitionend", () => {
    restartScanner();
});

/* ================= WINDOW RESIZE ================= */
window.addEventListener("resize", () => {
    restartScanner();
});

*/