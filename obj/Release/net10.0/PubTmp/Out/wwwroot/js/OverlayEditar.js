document.addEventListener("DOMContentLoaded", () => {

        // Cargar precio actual
        fetch('/LectorGenerador/GetPrecioActual')
            .then(r => r.json())
            .then(p => document.getElementById("precioActual").innerText = p ?? '0');

    // Abrir overlay
    document.getElementById("btnEditarPrecio")
        .addEventListener("click", () => {
        fetch('/LectorGenerador/EditarPrecioPartial')
            .then(r => r.text())
            .then(html => {
                document.getElementById("contenidoPrecio").innerHTML = html;
                document.getElementById("overlayPrecio").classList.remove("d-none");
                initFormPrecio();
            });
        });
});

    function cerrarOverlay() {
        document.getElementById("overlayPrecio").classList.add("d-none");
}

    function initFormPrecio() {
        document.getElementById("formPrecio").addEventListener("submit", e => {
            e.preventDefault();

            const form = new FormData(e.target);

            fetch('/LectorGenerador/PrecioCobrarUpdate', {
                method: 'POST',
                body: form
            })
                .then(r => r.json())
                .then(() => {
                    cerrarOverlay();
                    location.reload();
                });
        });
}

