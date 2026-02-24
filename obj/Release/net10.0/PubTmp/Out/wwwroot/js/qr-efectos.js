document.addEventListener("DOMContentLoaded", function () {

    const alertBox = document.getElementById("alertOk");

    const successAudio = new Audio("/audio/Success.mp3");
    const errorAudio = new Audio("/audio/Alert.mp3");

    successAudio.preload = "auto";
    errorAudio.preload = "auto";

    let lastState = null;

    const observer = new MutationObserver(() => {

        const isVisible = !alertBox.classList.contains("d-none");
        if (!isVisible) return;

        const isSuccess = alertBox.classList.contains("scan-success");
        const isError = alertBox.classList.contains("scan-error");

        const currentState = isSuccess ? "success" : isError ? "error" : null;

        if (!currentState) return;

        // Evita repetir el mismo sonido seguido
        //if (lastState === currentState) return;

        lastState = currentState;

        if (isSuccess) {
            successAudio.currentTime = 0;
            successAudio.play().catch(() => { });
        }

        if (isError) {
            errorAudio.currentTime = 0;
            errorAudio.play().catch(() => { });
        }

    });

    observer.observe(alertBox, {
        attributes: true,
        attributeFilter: ["class"]
    });

});