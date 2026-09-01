// El tema usa el atributo data-bs-theme que Bootstrap interpreta en sus componentes.
// Se guarda en localStorage para conservar la preferencia entre visitas.
(() => {
    const toggle = document.querySelector("[data-theme-toggle]");
    if (!toggle) {
        return;
    }

    const root = document.documentElement;
    const icon = toggle.querySelector("[data-theme-icon]");
    const label = toggle.querySelector("[data-theme-label]");
    const screenReaderLabel = toggle.querySelector("[data-theme-sr-label]");

    function applyTheme(theme, persist) {
        const isDark = theme === "dark";
        root.setAttribute("data-bs-theme", isDark ? "dark" : "light");

        if (persist) {
            localStorage.setItem("biblioteca-theme", isDark ? "dark" : "light");
        }

        if (icon) {
            icon.className = `fa-solid ${isDark ? "fa-sun" : "fa-moon"}`;
        }

        const nextLabel = isDark ? "Cambiar a modo claro" : "Cambiar a modo oscuro";
        if (label) {
            label.textContent = isDark ? "Claro" : "Oscuro";
        }
        if (screenReaderLabel) {
            screenReaderLabel.textContent = nextLabel;
        }
        toggle.setAttribute("title", nextLabel);
        toggle.setAttribute("aria-label", nextLabel);
        toggle.setAttribute("aria-pressed", String(isDark));
    }

    applyTheme(root.getAttribute("data-bs-theme") === "dark" ? "dark" : "light", false);
    toggle.addEventListener("click", () => {
        const currentTheme = root.getAttribute("data-bs-theme") === "dark" ? "dark" : "light";
        applyTheme(currentTheme === "dark" ? "light" : "dark", true);
    });
})();
