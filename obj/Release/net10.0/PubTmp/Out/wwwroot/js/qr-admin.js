document.addEventListener("DOMContentLoaded", function () {

    const btnAdmin = document.getElementById("btnAdmin");
    const sidebar = document.getElementById("sidebar");
    const exitTop = document.getElementById("adminExitTop");
    const exitBottom = document.getElementById("adminExitBottom");
    const btnToggle = document.getElementById("btnToggle");

    let topPressed = false;
    let bottomPressed = false;

    /* ===== COLLAPSE SIDEBAR ===== */
    btnToggle.addEventListener("click", function () {
        // Si estamos en modo admin no colapsar
        if (!document.body.classList.contains("admin-mode")) {
            sidebar.classList.toggle("collapsed");
        }
    });

    /* ===== ACTIVAR MODO ADMIN ===== */
    btnAdmin.addEventListener("click", function () {
        document.body.classList.add("admin-mode");
        topPressed = false;
        bottomPressed = false;
    });

    /* ===== BOTONES SECRETOS PARA SALIR ===== */
    exitTop.addEventListener("click", function () {
        topPressed = true;
        checkExit();
    });

    exitBottom.addEventListener("click", function () {
        bottomPressed = true;
        checkExit();
    });

    function checkExit() {
        if (topPressed && bottomPressed) exitAdminMode();
    }

    /* ===== SALIR CON ESCAPE ===== */
    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") exitAdminMode();
    });

    /* ===== FUNCION DE SALIDA ===== */
    function exitAdminMode() {
        document.body.classList.remove("admin-mode");
        topPressed = false;
        bottomPressed = false;
    }

});