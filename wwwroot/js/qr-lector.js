let leyendo = false;

const qr = new Html5Qrcode("reader");

qr.start(
    { facingMode: "environment" },
    {
        fps: 10,
        // ❌ NO qrbox
        // 👉 así escanea TODA la cámara
        experimentalFeatures: {
            useBarCodeDetectorIfSupported: true
        }
    },
    (qrCodeMessage) => {

        if (leyendo) return;
        leyendo = true;

        fetch('/LectorGenerador/RegistrarLectura', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                codigoQr: qrCodeMessage,
                fechaLectura: new Date()
            })
        })
            .then(r => r.json())
            .then(r => {

                const alert = document.getElementById("alertOk");
                alert.innerText = r.mensaje;
                alert.classList.remove("d-none");

                setTimeout(() => {
                    location.reload();
                }, 3000);
            });
    }
);
