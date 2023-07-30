(function () {
    const siteKey = document
        .querySelector('[data-recaptcha-sitekey]')
        .getAttribute('data-recaptcha-sitekey');
    
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', e => {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
            fetchScore(form).then(() => {
                form.submit();
            });
        })
    });

    function fetchScore(form) {
        return new Promise(resolve => {
            const input = form.querySelector('[data-recaptcha-token]');
            if (!input || !!input.value) {
                return;
            }

            grecaptcha.ready(function () {
                grecaptcha.execute(siteKey, { action: 'submit' }).then(function (token) {
                    input.value = token;
                    resolve();
                });
            });
        });        
    }
})();