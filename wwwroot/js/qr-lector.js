let leyendo = false;
let ultimoScan = 0;

const qr = new Html5Qrcode("reader");

qr.start(
    { facingMode: "environment" },
    {
        fps: 8,
        disableFlip: true,
        experimentalFeatures: {
            useBarCodeDetectorIfSupported: true
        }
    },
    async (qrCodeMessage) => {

        if (window.adminActivo) return;

        const ahora = Date.now();
        if (leyendo || ahora - ultimoScan < 120000) return;

        leyendo = true;
        ultimoScan = ahora;

        const res = await fetch('/LectorGenerador/RegistrarLectura', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ codigoQr: qrCodeMessage })
        });

        const data = await res.json();

        const alert = document.getElementById("alertOk");
        alert.innerText = data.mensaje;
        alert.classList.remove("d-none");

        setTimeout(() => {
            leyendo = false;
            alert.classList.add("d-none");
        }, 2000);
    }
);