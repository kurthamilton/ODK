(function () {
    bindCheckoutStatusPolling();

    function bindCheckoutStatusPolling() {
        const $container = document.querySelector('[data-odk-checkout]');
        if (!$container) return;

        const $statuses = $container.querySelectorAll('[data-odk-checkout-status]');

        hideElements($statuses);

        $container.addEventListener('odk:polling.response', e => {
            hideElements($statuses);
            $statuses.forEach(x => {
                const status = x.getAttribute('data-odk-checkout-status');
                if (status === e.detail.json.status) {
                    x.classList.remove('d-none');

                    const action = x.getAttribute('data-odk-checkout-action');
                    if (action === 'reload') {
                        $container.dispatchEvent(new Event('odk:polling.cancel'));
                        window.location.reload();
                    }
                }
            });
        });
    }

    function hideElements($elements) {
        $elements.forEach(x => x.classList.add('d-none'));
    }
})();