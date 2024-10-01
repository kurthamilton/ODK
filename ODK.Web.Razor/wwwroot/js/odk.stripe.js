(function () {
    const $forms = document.querySelectorAll('[data-stripe-checkout]');
    $forms.forEach($form => {
        const sessionId = $form.getAttribute('data-stripe-checkout');

        const fetchClientSecret = () => Promise.resolve(sessionId);

        const checkout = await stripe.initEmbeddedCheckout({
            fetchClientSecret
        });

        // Mount Checkout
        checkout.mount($form);
    });
})();