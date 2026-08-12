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

        // Custom [EmailAddressFormat] provider. [EmailAddress] alone is far looser than the server's
        // rule - it accepts "a@localhost", "a..b@x.com", "a.@x.com" - so without this a well-formed-
        // looking typo survives to the submit and the whole form comes back rejected. The pattern is
        // rendered by the server rather than written here, so the two checks can't drift apart.
        // Empty values pass - presence is the [Required] provider's job.
        v.addProvider('emailaddressformat', (value, element) => {
            if (!value) return true;

            const pattern = element.getAttribute('data-val-emailaddressformat-pattern');
            if (!pattern) return true;

            return new RegExp(pattern).test(value);
        });

        // Custom [data-val-emailtemplate] provider for the email template editor. Flags a placeholder
        // the send path does not supply - interpolation leaves an unrecognised token exactly as
        // written, so it reaches the member as literal braces with nothing downstream to catch it.
        // The pattern and the list of known placeholders are both rendered by the server rather than
        // written here, so the two checks can't drift apart.
        // Empty values pass - presence is the [Required] provider's job.
        v.addProvider('emailtemplate', (value, element) => {
            if (!value) return true;

            const pattern = element.getAttribute('data-val-emailtemplate-pattern');
            const known = element.getAttribute('data-val-emailtemplate-placeholders');
            if (!pattern || known === null) return true;

            // Lower-cased both sides: the server matches placeholders case-insensitively, so flagging
            // {Group.Name} here would reject a template that renders perfectly well.
            const allowed = new Set(known.toLowerCase().split(',').filter(x => x));

            const unknown = [];
            for (const match of value.matchAll(new RegExp(pattern, 'g'))) {
                const name = match[1].toLowerCase();
                if (!allowed.has(name) && !unknown.includes(match[1])) {
                    unknown.push(match[1]);
                }
            }

            if (unknown.length === 0) return true;

            return `Unknown placeholder${unknown.length > 1 ? 's' : ''}: `
                + unknown.map(x => `{${x}}`).join(', ');
        });

        // Custom [data-val-htmlcontent] provider for the email template editor. The markup rules are a
        // parse for well-formedness plus an allow-list of tags and attributes, none of which can be
        // expressed as a pattern the way the providers above are, so this one asks the server: it posts
        // the value to data-val-htmlcontent-url and returns the promise, which the validation library
        // awaits (true passes, a string is the message to show).
        // Fails open on a network or HTTP error: the same check runs again on submit, so a check that
        // cannot reach the server must not block content the server would have accepted.
        // Empty values pass - presence is the [Required] provider's job.
        v.addProvider('htmlcontent', async (value, element) => {
            if (!value) return true;

            const url = element.getAttribute('data-val-htmlcontent-url');
            if (!url) return true;

            const body = new FormData();
            body.append('content', value);

            try {
                const response = await fetch(url, {
                    method: 'POST',
                    headers: window.odk.antiforgeryHeaders(),
                    body
                });

                if (!response.ok) {
                    // Never a verdict on the content: the check did not run. Warned rather than swallowed
                    // - failing open is silent by design, so without this a broken endpoint looks exactly
                    // like markup that passed.
                    console.warn(`Validation request to ${url} failed: ${response.status}`);
                    return true;
                }

                const result = await response.json();
                return result.valid
                    ? true
                    : result.message || element.getAttribute('data-val-htmlcontent');
            } catch (e) {
                console.warn(`Validation request to ${url} could not be read`, e);
                return true;
            }
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

                // Awaits validateForm rather than reading isValid(): once a provider validates
                // asynchronously (see htmlcontent) the synchronous state is still stale when isValid
                // returns, so the form would submit before the answer arrived. The promise resolves true
                // for a form with no validated fields, which is how the test/restore buttons - which
                // target their own empty forms - still submit.
                const submit = async () => {
                    const v = window.odk.forms.validationService;
                    if (await v.validateForm($target)) {
                        $target.submit();
                    }
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