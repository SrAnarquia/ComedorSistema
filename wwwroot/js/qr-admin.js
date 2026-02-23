const btnAdmin = document.getElementById("btnAdmin");
const hotspot = document.getElementById("adminHotspot");

// Activar modo admin
btnAdmin.addEventListener("click", () => {
    document.body.classList.add("admin-mode");

    document.onkeydown = (e) => {
        if (["F5", "Escape"].includes(e.key)) {
            e.preventDefault();
        }
    };
});

// Salir del modo admin (HOTSPOT)
hotspot.addEventListener("click", (e) => {
    e.stopPropagation(); // evita interferencias
    document.body.classList.remove("admin-mode");
    document.onkeydown = null;
});