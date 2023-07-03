(function () {
    bindClientSideValidation();
    bindDatePickers();

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
})();