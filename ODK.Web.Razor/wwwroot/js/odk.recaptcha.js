(function () {
    const siteKey = document
        .querySelector('[data-recaptcha-sitekey]')
        .getAttribute('data-recaptcha-sitekey');

    /* A pre-submit step, run by odk.forms.js once a form has validated and before it posts, rather than a
       submit handler of this script's own - a token can only be fetched asynchronously, and a handler that
       replaces the native submit to wait for one silences every other check on the form. Registered for
       every form on the page: a form carrying no token field is a step that does nothing. */
    window.odk.forms.beforeSubmit.push(fetchScore);

    function fetchScore(form) {
        return new Promise(resolve => {
            const input = form.querySelector('[data-recaptcha-token]');
            if (!input || !!input.value) {
                resolve();
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