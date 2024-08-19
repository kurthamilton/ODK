(function () {
    bindClientSideValidation();
    bindDatePickers();
    bindSubmits();

    function bindClientSideValidation() {
        const v = new aspnetValidation.ValidationService();
        v.bootstrap();
    }

    function bindDatePickers() {
        const $dateInputs = document.querySelectorAll('input[data-datepicker]');
        $dateInputs.forEach($input => {
            const enableTime = $input.hasAttribute('data-datepicker-time');
            const format = enableTime
                ? 'd/m/Y H:i'
                : 'd/m/Y';
            flatpickr($input, {
                dateFormat: format,
                enableTime,
                time_24hr: true
            }); 
        });
    }

    function bindSubmits() {
        document.querySelectorAll('[data-submit]').forEach(button => {
            const targetSelector = button.getAttribute('data-submit');
            const target = document.querySelector(targetSelector);
            const confirmMessage = button.getAttribute('data-submit-confirm');

            if (!target) {
                return;
            }

            button.addEventListener('click', () => {
                if (!!confirmMessage && !confirm(confirmMessage)) {
                    return;
                }

                target.submit();
            });
        });
    }
})();