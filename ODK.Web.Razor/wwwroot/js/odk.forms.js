window.odk = window.odk || {};
window.odk.forms = window.odk.forms || {};

(function () {
    initConfig();
    bindAutoSubmits();
    bindClearables();
    bindClientSideValidation();
    bindColorPickers();
    bindDatePickers();
    bindSubmits();

    // Hydrate window.odk.config from server-rendered data attributes on <html>, so the JS reads
    // server-side values from the DOM rather than the server writing to window.odk inline.
    function initConfig() {
        window.odk.config = window.odk.config || {};
        window.odk.config.datePickerFormat =
            document.documentElement.getAttribute('data-date-format') || window.odk.config.datePickerFormat;
    }

    function bindAutoSubmits() {
        // auto-submit forms on dropdown list and checkbox change
        const $triggers = document.querySelectorAll('[data-autosubmit]');
        $triggers.forEach($trigger => {
            const $form = $trigger.closest('form');
            if (!$form) return;
            // bind selects to odk:changed, which is emitted by the slim-select integration since change events don't bubble
            const eventName = $trigger.tagName === 'SELECT' ? 'odk:changed' : 'change';
            $trigger.addEventListener(eventName, () => $form.submit());
        });
    }

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
                // Clear through flatpickr when present so the localised alt-input display clears too;
                // flatpickr fires 'change' on the original input, which hides the button.
                if ($input._flatpickr) {
                    $input._flatpickr.clear();
                } else {
                    $input.value = '';
                    $input.dispatchEvent(new Event('change'));
                }
            });
        });
    }

    function bindClientSideValidation() {
        const v = new aspnetValidation.ValidationService();

        // Custom [NonNegative] provider (must be registered before bootstrap).
        // Empty values pass - presence is the [Required] provider's job.
        v.addProvider('nonnegative', (value) => {
            if (!value) return true;
            const val = parseFloat(value);
            return isNaN(val) || val >= 0;
        });

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
        const config = (window.odk && window.odk.config) || {};
        const displayDateFormat = config.datePickerFormat || 'd/m/Y';
        const $dateInputs = document.querySelectorAll('input[data-datepicker]');
        $dateInputs.forEach($input => {
            const enableTime = $input.hasAttribute('data-datepicker-time');
            // The posted/parsed value stays a fixed format; only the visible (altInput) display is
            // localised to the viewer's locale via altFormat.
            const postFormat = enableTime ? 'd/m/Y H:i' : 'd/m/Y';
            const altFormat = enableTime ? displayDateFormat + ' H:i' : displayDateFormat;
            flatpickr($input, {
                altFormat,
                altInput: true,
                dateFormat: postFormat,
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

            $button.addEventListener('click', () => {
                if ($target.tagName !== 'FORM') return;

                const submit = () => {
                    const v = window.odk.forms.validationService;
                    v.validateForm($target);
                    if (!v.isValid($target)) return;
                    $target.submit();
                };

                // submit() fires no submit event, so the confirm interception in odk.js can't see it - ask
                // here instead. Returns false when the form has no _Confirm, in which case just submit.
                if (window.odk.confirm?.($target, submit)) return;
                submit();
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
                    method: 'POST',
                    headers: window.odk.antiforgeryHeaders()
                });
            });
        });
    }
})();