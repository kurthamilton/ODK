window.odk = window.odk || {};
window.odk.forms = window.odk.forms || {};

(function () {
    bindClearables();
    bindClientSideValidation();
    bindColorPickers();
    bindDatePickers();
    bindSubmits();

    function bindClearables() {
        const $containers = document.querySelectorAll('[data-clearable-container]');
        $containers.forEach($container => {
            const $button = $container.querySelector('[data-clearable-button]');
            const $input = $container.querySelector('[data-clearable]');
            if (!$button || !$input) return;

            if (!$input.value) $button.classList.add('d-none');

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
        window.odk.forms.validationService = v;
    }

    function bindColorPickers() {
        const $inputs = document.querySelectorAll('[data-color-picker]');
        $inputs.forEach($input => {
            const required = $input.hasAttribute('data-val-required');
            const picker = new JSColor($input, { format: 'hex', required: required });
        });
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
        document.querySelectorAll('[data-submit]').forEach($button => {
            const targetSelector = $button.getAttribute('data-submit');
            const $target = targetSelector === 'parent'
                ? $button.closest('form')
                : document.querySelector(targetSelector);

            if (!$target) return;

            const confirmMessage = $button.getAttribute('data-submit-confirm');            

            $button.addEventListener('click', () => {
                if (!!confirmMessage && !confirm(confirmMessage)) return;
                if ($target.tagName !== 'FORM') return;

                const v = window.odk.forms.validationService;
                v.validateForm($target);
                if (!v.isValid($target)) return;
                $target.submit();
            });
        });

        document.querySelectorAll('[data-input-change-url]').forEach($input => {
            $input.addEventListener('change', async () => {
                const value = $input.getAttribute('type') === 'checkbox'
                    ? $input.checked
                    : $input.value;
                const url = $input.getAttribute('data-input-change-url')
                    .replace('{value}', value);
                await fetch(url, {
                    method: 'POST'
                });
            });
        });
    }
})();