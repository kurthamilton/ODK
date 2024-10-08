﻿(function () {
    bindClearables();
    bindClientSideValidation();
    bindDatePickers();
    bindSubmits();

    function bindClearables() {
        const $containers = document.querySelectorAll('[data-clearable-container]');
        $containers.forEach($container => {
            const $button = $container.querySelector('[data-clearable-button]');
            const $input = $container.querySelector('[data-clearable]');
            if (!$button || !$input) {
                return;
            }            

            if (!$input.value) {
                $button.classList.add('d-none');
            }

            $input.addEventListener('change', () => {
                if ($input.value) {
                    $button.classList.remove('d-none');
                } else {
                    $button.classList.add('d-none');
                }
            });

            $button.addEventListener('click', () => {
                $input.value = '';
                $input.dispatchEvent(new Event('change'));
            });
        });
    }

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
            const target = targetSelector === 'parent'
                ? button.closest('form')
                : document.querySelector(targetSelector);
            
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