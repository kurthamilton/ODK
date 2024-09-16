(function () {
    window.odk = window.odk || {};

    const parseJwt = (token) => {
        const base64Url = token.split('.')[1];
        const base64 = base64Url
            .replace(/-/g, '+')
            .replace(/_/g, '/');

        const parts = window.atob(base64)
            .split('')
            .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2));

        const jsonPayload = decodeURIComponent(parts.join(''));

        return JSON.parse(jsonPayload);
    };

    const $tokens = document.querySelectorAll('[data-oauth-token]');
    $tokens.forEach($token => {
        const $form = $token.closest('form');
        if (!$form) {
            return;
        }

        $form.addEventListener('oauth', () => {
            const $show = $form.querySelectorAll('[data-oauth-show]');
            $show.forEach(x => x.classList.remove('d-none'));
        });
    });

    window.odk.oauth = {
        parseJwt: parseJwt
    };
})();