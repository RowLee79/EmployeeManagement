document.addEventListener("DOMContentLoaded", () => {

    initializeSidebar();

    initializeTheme();

});


function initializeSidebar() {

    const sidebar =
        document.getElementById("sidebar");

    const toggle =
        document.getElementById("sidebarToggle");

    const close =
        document.getElementById("sidebarClose");


    if (!sidebar) {
        return;
    }


    toggle?.addEventListener("click", () => {

        sidebar.classList.toggle("show");

    });


    close?.addEventListener("click", () => {

        sidebar.classList.remove("show");

    });


    document.addEventListener("click", event => {

        if (window.innerWidth > 991) {
            return;
        }

        if (
            sidebar.classList.contains("show") &&
            !sidebar.contains(event.target) &&
            !toggle?.contains(event.target)
        ) {

            sidebar.classList.remove("show");

        }

    });

}


function initializeTheme() {

    const button =
        document.getElementById("themeToggle");

    if (!button) {
        return;
    }


    const savedTheme =
        localStorage.getItem("theme");


    if (savedTheme) {

        document.documentElement
            .setAttribute(
                "data-theme",
                savedTheme
            );

        updateThemeIcon(
            button,
            savedTheme
        );

    }


    button.addEventListener("click", () => {

        const currentTheme =
            document.documentElement
                .getAttribute("data-theme");


        const newTheme =
            currentTheme === "dark"
                ? "light"
                : "dark";


        document.documentElement
            .setAttribute(
                "data-theme",
                newTheme
            );


        localStorage.setItem(
            "theme",
            newTheme
        );


        updateThemeIcon(
            button,
            newTheme
        );

    });

}


function updateThemeIcon(button, theme) {

    const icon =
        button.querySelector("i");

    if (!icon) {
        return;
    }


    icon.className =
        theme === "dark"
            ? "bi bi-sun"
            : "bi bi-moon";

}