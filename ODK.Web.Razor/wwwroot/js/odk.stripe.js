(function () {
    const $forms = document.querySelectorAll('[data-stripe-checkout]');
    $forms.forEach($form => {
        const publicKey = $form.getAttribute('data-stripe');
        if (!publicKey) return;

        const stripe = Stripe(publicKey);
        const sessionId = $form.getAttribute('data-stripe-checkout');

        const fetchClientSecret = () => Promise.resolve(sessionId);

        stripe.initEmbeddedCheckout({
            fetchClientSecret
        }).then(checkout => {
            // Mount Checkout
            checkout.mount($form);
        });                
    });
})();