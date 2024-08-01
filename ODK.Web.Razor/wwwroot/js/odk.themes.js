(function () {
    const root = document.querySelector('[data-theme-root]');
    const buttonContainer = document.querySelector('[data-theme-selector-container]');

    const buttons = buttonContainer ? {
        light: buttonContainer.querySelector('[data-theme-selector="light"]'),
        dark: buttonContainer.querySelector('[data-theme-selector="dark"]')
    } : null;

    const setTheme = (theme) => {
        root.setAttribute('data-bs-theme', theme);
        localStorage.setItem('odk.theme', theme);

        if (!buttons) {
            return;
        }

        if (theme === 'dark') {
            buttons.dark.classList.add('d-none');
            buttons.light.classList.remove('d-none');
        } else {
            buttons.dark.classList.remove('d-none');
            buttons.light.classList.add('d-none');
        }
    };

    const theme = localStorage.getItem('odk.theme');
    if (theme === 'dark') {
        setTheme('dark');
    }

    if (buttons) {
        buttons.dark.addEventListener('click', () => {
            setTheme('dark');
        });

        buttons.light.addEventListener('click', () => {
            setTheme('light');
        });
    }
})();