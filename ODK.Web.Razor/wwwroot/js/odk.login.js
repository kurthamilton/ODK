(function () {
    const $forms = document.querySelectorAll('[data-oauth-submit]');
    $forms.forEach($form => {
        $form.addEventListener('oauth', () => {
            $form.submit();
        });
    });
})();