(function () {
    const THEME_KEY = 'odk.theme';

    const root = document.querySelector('[data-theme-root]');

    window.addEventListener('load', () => {
        bindEventListeners();
        setButtonVisibility();
        setContrastThemes();
    });

    const bindEventListeners = () => {
        var themeSwitch = getSwitch();
        if (themeSwitch) {
            themeSwitch.addEventListener('change', () => {
                const theme = themeSwitch.checked ? 'dark' : 'light';
                changeTheme(theme);
            });
            return;
        }

        const buttons = getButtons();
        if (!buttons) return;

        [buttons.dark, buttons.light].forEach(button => {
            button.addEventListener('click', e => {
                const theme = button.getAttribute('data-theme-selector');
                changeTheme(theme);
            });
        });
    };

    const getButtons = () => {
        const buttons = {
            light: document.querySelector('[data-theme-selector="light"]'),
            dark: document.querySelector('[data-theme-selector="dark"]')
        };

        return buttons.light && buttons.dark
            ? buttons
            : null;
    };

    const getSwitch = () => document.querySelector('[data-theme-switch]');

    const getTheme = () => {
        // prefer local storage, then system preference, then default to light
        const saved = localStorage.getItem(THEME_KEY);
        if (saved === 'dark' || saved === 'light') return saved;

        const prefersDark = window.matchMedia?.('(prefers-color-scheme: dark)').matches;
        return prefersDark ? 'dark' : 'light';
    };

    const setButtonVisibility = () => {
        const themeSwitch = getSwitch();
        if (themeSwitch) {
            const theme = getTheme();
            themeSwitch.checked = theme === 'dark';
            return;
        }

        const buttons = getButtons();
        if (buttons) {
            const theme = getTheme();
            if (theme === 'light') {
                buttons.dark.classList.remove('d-none');
                buttons.light.classList.add('d-none');
            } else {
                buttons.dark.classList.add('d-none');
                buttons.light.classList.remove('d-none');
            }
        }
    };

    const changeTheme = (theme) => {
        localStorage.setItem(THEME_KEY, theme);
        setTheme(theme);
        setContrastThemes();
    };

    const setContrastThemes = () => {
        const $contrastEls = document.querySelectorAll('[data-theme-contrast]');
        if (!$contrastEls.length) return;

        const theme = root.getAttribute('data-bs-theme');
        const contrastTheme = theme === 'light' ? 'dark' : 'light';
        $contrastEls.forEach($el => $el.setAttribute('data-bs-theme', contrastTheme));
    };

    const setTheme = (theme, persist) => {
        root.setAttribute('data-bs-theme', theme);
        setButtonVisibility();

        document.dispatchEvent(new CustomEvent('odk:theme-changed', {
            detail: theme
        }));
    };

    const theme = getTheme();
    if (theme === 'light') {
        setTheme('light');
    } else {
        setTheme('dark');
    }
})();