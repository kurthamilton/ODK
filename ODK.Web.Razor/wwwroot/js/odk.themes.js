(function () {
    const root = document.querySelector('[data-theme-root]');    

    const getButtons = () => {
        const buttonContainer = document.querySelector('[data-theme-selector-container]');
        if (!buttonContainer) {
            return;
        }

        return {
            light: buttonContainer.querySelector('[data-theme-selector="light"]'),
            dark: buttonContainer.querySelector('[data-theme-selector="dark"]')
        };
    };

    const setTheme = (theme) => {
        root.setAttribute('data-bs-theme', theme);
        localStorage.setItem('odk.theme', theme);

        const buttons = getButtons();
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

    document.addEventListener('click', e => {
        const target = e.target.closest('[data-theme-selector]');
        if (!target) {
            return;
        }

        const theme = target.getAttribute('data-theme-selector');
        setTheme(theme);
    });    
})();