(function () {
    const root = document.querySelector('[data-theme-root]');    

    window.addEventListener('load', () => {
        setButtonVisibility();
    });

    const getButtons = () => {
        const buttons = {
            light: document.querySelector('[data-theme-selector="light"]'),
            dark: document.querySelector('[data-theme-selector="dark"]')
        };

        return buttons.light && buttons.dark
            ? buttons
            : null;
    };

    const getTheme = () => {
        return localStorage.getItem('odk.theme');
    };

    const setButtonVisibility = () => {
        const buttons = getButtons();
        if (!buttons) {
            return;
        }

        const theme = getTheme();
        if (theme === 'light') {
            buttons.dark.classList.remove('d-none');
            buttons.light.classList.add('d-none');
        } else {
            buttons.dark.classList.add('d-none');
            buttons.light.classList.remove('d-none');            
        }
    };

    const setTheme = (theme) => {
        root.setAttribute('data-bs-theme', theme);
        localStorage.setItem('odk.theme', theme);

        const buttons = getButtons();
        if (!buttons) {
            return;
        }

        setButtonVisibility();
    };

    const theme = getTheme();
    if (theme === 'light') {
        setTheme('light');
    } else {
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