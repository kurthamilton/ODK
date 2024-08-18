(function () {
    document.querySelectorAll('[data-paypal-container]').forEach(container => {        
        const amount = container.getAttribute('data-paypal-amount');        
        const type = container.getAttribute('data-paypal-type');

        const $form = container.closest('form');        
        if (!$form) {
            alert('Error setting up PayPal form');
            return;
        }

        const $token = $form.querySelector('[data-paypal-token]');

        const containerId = container.getAttribute('data-paypal-container');

        const style = {
            layout: 'vertical',
            color: 'gold',
            shape: 'rect',
            label: 'paypal'
        };
        
        const buttons = type === 'subscription'
            ? paypal.Buttons({
                style: style,
                createSubscription: (data, actions) => {
                    const planId = container.getAttribute('data-paypal-planid');                    
                    return actions.subscription.create({
                        /* Creates the subscription */
                        plan_id: planId
                    });
                },
                onApprove: (data, actions) => {
                    $token.value = data.subscriptionID;
                    $form.submit();
                }
            })
            : paypal.Buttons({
                style: style,
                createOrder: (data, actions) => {
                    // Set up the transaction
                    return actions.order.create({
                        purchase_units: [{
                            amount: {
                                currency_code: container.getAttribute('data-paypal-currencycode'),
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