(function () {
    document.querySelectorAll('[data-paypal-container]').forEach(container => {
        const currencyCode = container.getAttribute('data-paypal-currencycode');
        const amount = container.getAttribute('data-paypal-amount');        
        const $token = container.querySelector('[data-paypal-token]');
        const $form = container.closest('form');
        if (!$form) {
            alert('Error setting up PayPal form');
            return;
        }

        const containerId = container.getAttribute('data-paypal-container');

        const buttons = paypal.Buttons({
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
        });
        buttons.render(`[data-paypal-container="${containerId}"]`);
    });    
})();