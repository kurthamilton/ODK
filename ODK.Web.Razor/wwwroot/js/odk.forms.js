(function () {
    bindClientSideValidation();
    bindDatePickers();
    bindSubmits();

    function bindClientSideValidation() {
        const v = new aspnetValidation.ValidationService();
        v.bootstrap();
    }

    function bindDatePickers() {
        document.querySelectorAll('input[data-datepicker]').forEach(input => {
            const datepicker = new Datepicker(input, {
                format: 'dd/mm/yyyy'
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