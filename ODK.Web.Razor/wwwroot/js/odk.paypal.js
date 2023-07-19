(function () {
    document.querySelectorAll('[data-paypal-amount]').forEach(container => {
        const currencyCode = container.getAttribute('data-paypal-currencycode');
        const amount = container.getAttribute('data-paypal-amount');
        const tokenSelector = container.getAttribute('data-paypal-token');
        const $token = document.querySelector(tokenSelector);
        const $form = container.closest('form');
        if (!$form) {
            alert('Error setting up PayPal form');
            return;
        }

        paypal.Buttons({
            style: {
                layout: 'vertical',
                color: 'gold',
                shape: 'rect',
                label: 'paypal'
            },
            createOrder: (data, actions) => {
                // Set up the transaction
                return actions.order.create({
                    purchase_units: [{
                        amount: {
                            currency_code: currencyCode,
                            value: amount
                        }
                    }]
                });
            },
            onApprove: (data, actions) => {
                $token.value = data.orderID;
                $form.submit();
            }
        }).render(`#${container.getAttribute('id')}`);
    });    
})();