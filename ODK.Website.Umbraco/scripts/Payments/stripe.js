(function () {
    makePayment();
    
    function makePayment() {
        var $paymentId = $('.js-payment-id');
        var $apiKey = $('.js-api-key');

        if ($paymentId.length === 0 || $apiKey.length === 0) {
            return;
        }

        var paymentId = $paymentId.val();
        var apiKey = $apiKey.val();

        var stripe = Stripe(apiKey);

        stripe.redirectToCheckout({
            sessionId: paymentId
        }).then(function (result) {
        });
    }
})();