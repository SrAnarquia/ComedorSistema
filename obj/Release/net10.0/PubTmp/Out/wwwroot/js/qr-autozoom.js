const html5QrCode = new Html5Qrcode("reader");

const config = {
    fps: 10,
    qrbox: function (viewfinderWidth, viewfinderHeight) {
        let minEdge = Math.min(viewfinderWidth, viewfinderHeight);
        let qrSize = Math.floor(minEdge * 0.6);
        return { width: qrSize, height: qrSize };
    }
};

function onScanSuccess(decodedText, decodedResult) {

    document.getElementById("alertOk").classList.remove("d-none");

    const video = document.querySelector("#reader video");

    if (video) {
        video.style.transition = "transform 0.3s ease";
        video.style.transform = "scale(1.3)";
    }

    setTimeout(() => {
        document.getElementById("alertOk").classList.add("d-none");
        if (video) {
            video.style.transform = "scale(1)";
        }
    }, 1500);

    console.log("QR Detectado:", decodedText);
}

Html5Qrcode.getCameras().then(devices => {
    if (devices && devices.length) {
        html5QrCode.start(
            devices[0].id,
            config,
            onScanSuccess
        );
    }
}).catch(err => {
    console.error(err);
});