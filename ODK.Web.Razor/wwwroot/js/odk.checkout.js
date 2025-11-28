(function () {
    bindCheckoutStatusPolling();

    function bindCheckoutStatusPolling() {
        const $container = document.querySelector('[data-odk-checkout]');
        if (!$container) return;

        $container.addEventListener('odk:polling.response', e => {
            console.log(e.detail.json);
        });
    }
})();