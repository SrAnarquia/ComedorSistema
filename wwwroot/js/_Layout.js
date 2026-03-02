document.addEventListener("DOMContentLoaded", function () {

    const sidebar = document.getElementById("sidebar");
    const btnToggle = document.getElementById("btnToggle");
    const btnAdmin = document.getElementById("btnAdmin");
    const hotspot = document.getElementById("adminHotspot");

    // Toggle expandir / comprimir
    btnToggle.addEventListener("click", function () {
        sidebar.classList.toggle("collapsed");
    });

    /* ===============================
       MODO ADMIN
    =============================== */

    if (btnAdmin) {
        btnAdmin.addEventListener("click", function () {
            document.body.classList.add("admin-mode");
            document.addEventListener("keydown", bloquearTeclas);
        });
    }

    function bloquearTeclas(e) {
        if (e.key === "F5" || e.key === "Escape") {
            e.preventDefault();
        }
    }

    if (hotspot) {
        hotspot.addEventListener("click", function (e) {
            e.stopPropagation();
            document.body.classList.remove("admin-mode");
            document.removeEventListener("keydown", bloquearTeclas);
        });
    }
});
/*document.addEventListener("DOMContentLoaded", function () {

    const sidebar = document.getElementById("sidebar");
    const btnToggle = document.getElementById("btnToggle");
    const btnAdmin = document.getElementById("btnAdmin");
    const hotspot = document.getElementById("adminHotspot");

    // Toggle sidebar
    btnToggle.addEventListener("click", function () {
        sidebar.classList.toggle("collapsed");
    });

    // Activar modo admin
    btnAdmin.addEventListener("click", function () {
        document.body.classList.add("admin-mode");

        document.addEventListener("keydown", bloquearTeclas);
    });

    function bloquearTeclas(e) {
        if (e.key === "F5" || e.key === "Escape") {
            e.preventDefault();
        }
    }

    // Salir modo admin
    hotspot.addEventListener("click", function (e) {
        e.stopPropagation();
        document.body.classList.remove("admin-mode");
        document.removeEventListener("keydown", bloquearTeclas);
    });

});*/