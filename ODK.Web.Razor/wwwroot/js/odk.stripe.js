(function () {
    document.querySelectorAll('[data-odk-stripe-checkout]').forEach(initCheckout);

    // Stripe Elements driven by a Checkout Session (ui_mode "elements"). The session is created server-side
    // and its client secret rendered into the form; from here Stripe owns only the payment fields, so the
    // rest of the page - layout, submit button, errors - is ours.
    async function initCheckout($form) {
        const clientSecret = $form.getAttribute('data-odk-stripe-checkout');
        const publicKey = $form.getAttribute('data-odk-stripe-key');
        if (!clientSecret || !publicKey || typeof Stripe === 'undefined') return;

        const $error = $form.querySelector('[data-odk-stripe-error]');
        const $paymentElement = $form.querySelector('[data-odk-stripe-payment-element]');
        const $spinner = $form.querySelector('[data-odk-stripe-spinner]');
        const $submit = $form.querySelector('[data-odk-stripe-submit]');

        const checkout = Stripe(publicKey).initCheckoutElementsSdk({
            clientSecret,
            elementsOptions: { appearance: getAppearance() }
        });

        // Stripe tells us when the form is complete enough to confirm; keep the button in step rather than
        // letting someone submit an incomplete card.
        checkout.on('change', session => {
            if ($submit) $submit.disabled = !session.canConfirm;
        });

        checkout.createPaymentElement().mount($paymentElement);

        const { actions } = await checkout.loadActions();

        $form.addEventListener('submit', async e => {
            e.preventDefault();
            setBusy(true);
            showError(null);

            // On success Stripe redirects to the session's return_url, so this only resolves when the
            // payment fails outright (declined card, validation) - or after an off-site authorisation step.
            const result = await actions.confirm();
            if (result.type === 'error') {
                showError(result.error.message);
            }

            setBusy(false);
        });

        function setBusy(busy) {
            if ($submit) $submit.disabled = busy;
            if ($spinner) $spinner.classList.toggle('d-none', !busy);
        }

        function showError(message) {
            if (!$error) return;
            $error.textContent = message || '';
            $error.classList.toggle('d-none', !message);
        }
    }

    // The Payment Element renders in a Stripe-hosted iframe, so our stylesheets can't reach it - the
    // Appearance API is the only way to style it. Values are read from the live CSS custom properties
    // rather than duplicated here, so the form tracks the site's theme (including light/dark, which swaps
    // the same variables) and any future palette change automatically.
    // Set at init only: Stripe fixes the appearance for the element's lifetime, so toggling the theme
    // mid-checkout won't restyle the fields until the page reloads.
    // The Link panel is a known exception, confirmed by Stripe support: it adapts to light/dark from the
    // settings below but keeps its own core design, and its internals (--linkUi-*: black surface, rounded
    // corners) can't be overridden - there's no Link selector in the rules table and no web equivalent of
    // the mobile colorsDark. Don't go looking for a fix; the only lever is the global variables here.
    function getAppearance() {
        const styles = getComputedStyle(document.documentElement);
        const cssVar = name => styles.getPropertyValue(name).trim();
        const dark = document.documentElement.getAttribute('data-bs-theme') === 'dark';

        // Bootstrap leaves --bs-heading-color unset by default, so headings render in the body colour.
        const headingColor = cssVar('--bs-heading-color') || cssVar('--bs-body-color');
        const primaryColor = cssVar('--bs-primary');

        return {
            // Base theme, so anything the variables and rules below don't cover still suits the background.
            theme: dark ? 'night' : 'stripe',
            variables: {
                accordionItemLabelColorText: headingColor,
                accordionItemLabelSelectedColorText: headingColor,
                borderRadius: cssVar('--bs-border-radius'),
                colorBackground: cssVar('--bs-body-bg'),
                colorDanger: cssVar('--bs-danger'),
                // Deliberately NOT the brand colour. Stripe applies a .u-color-primary utility to text such
                // as the collapsed accordion header, which reads as a blue link rather than a heading, and
                // utilities can't be targeted by rules (only the component classes can). So colorPrimary is
                // set to the text colour and the brand colour is re-applied below to the specific places
                // that should carry it.
                colorPrimary: headingColor,
                colorText: cssVar('--bs-body-color'),
                fontFamily: cssVar('--bs-body-font-family'),
                // In px - the CSS variable is in rem, which Stripe resolves against its own root.
                fontSizeBase: getComputedStyle(document.body).fontSize
            },
            rules: {
                // Drop the raised-panel shadow Stripe applies to spaced accordion items (the site's panels
                // are flat), and set the text colour here as well as via accordionItemLabelColorText: that
                // variable doesn't reach a collapsed item's label, which otherwise stays colorPrimary blue.
                '.AccordionItem': {
                    boxShadow: 'none',
                    color: headingColor
                },
                '.Input': {
                    backgroundColor: cssVar('--bs-body-bg'),
                    border: `1px solid ${cssVar('--bs-border-color')}`,
                    boxShadow: 'none'
                },
                '.Input:focus': {
                    borderColor: primaryColor,
                    boxShadow: 'none'
                },
                '.Label': {
                    color: cssVar('--bs-secondary-color')
                },
                // The brand colour, re-applied where it earns its place: the selection and focus accents
                // that colorPrimary would normally provide (see the note on colorPrimary above).
                '.CheckboxInput--checked': {
                    backgroundColor: primaryColor,
                    borderColor: primaryColor
                },
                '.RadioIconInner--checked': {
                    fill: primaryColor
                },
                '.RadioIconOuter--checked': {
                    stroke: primaryColor
                }
            }
        };
    }
})();
