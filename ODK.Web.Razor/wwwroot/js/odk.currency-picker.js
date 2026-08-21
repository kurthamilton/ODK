(function () {
    bindCurrencyPickers();

    function bindCurrencyPicker($picker) {
        const id = $picker.getAttribute('data-currency-picker');
        const $options = document.querySelector(`[data-currency-picker-options="${id}"]`);
        if (!$options) {
            return;
        }

        const tooltipSelector = '[data-bs-toggle="tooltip"]';

        const $value = $picker.querySelector('[data-currency-picker-value]');
        const $label = $picker.querySelector('[data-currency-picker-label]');
        const $search = $options.querySelector('[data-currency-picker-search]');
        const $rows = $options.querySelectorAll('[data-currency-picker-option]');
        const $modal = $options.closest('.modal');

        // bindTooltips (odk.js) only ran over the markup present on load, so the label's replacement brings
        // its own tooltip and the one it displaces has to go. A title-less element is skipped there too.
        const bindTooltip = () => {
            const $tooltip = $label.querySelector(tooltipSelector);
            if ($tooltip && $tooltip.getAttribute('data-bs-title')) {
                new bootstrap.Tooltip($tooltip);
            }
        };

        const disposeTooltip = () => {
            const $tooltip = $label.querySelector(tooltipSelector);
            if ($tooltip) {
                bootstrap.Tooltip.getInstance($tooltip)?.dispose();
            }
        };

        const select = $row => {
            $value.value = $row.getAttribute('data-currency-id');

            // The picked row's own rendering of the currency, so the field reads the same as the table.
            disposeTooltip();
            $label.innerHTML = $row.querySelector('[data-currency-picker-option-label]').innerHTML;
            bindTooltip();

            $rows.forEach($x => $x.classList.toggle('table-active', $x === $row));

            $value.dispatchEvent(new Event('change', { bubbles: true }));
            bootstrap.Modal.getOrCreateInstance($modal).hide();
        };

        $rows.forEach($row => {
            $row.addEventListener('click', () => select($row));
            $row.addEventListener('keydown', e => {
                if (e.key !== 'Enter' && e.key !== ' ') {
                    return;
                }

                // Space scrolls the dialog otherwise, and Enter inside a form would submit it.
                e.preventDefault();
                select($row);
            });
        });

        if ($search) {
            // The dialog is rendered inside the form the picker belongs to, so Enter here would submit it.
            $search.addEventListener('keydown', e => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                }
            });

            $search.addEventListener('input', () => {
                const search = $search.value.trim().toLowerCase();
                $rows.forEach($row => {
                    const option = $row.getAttribute('data-currency-picker-option').toLowerCase();
                    $row.classList.toggle('d-none', !!search && !option.includes(search));
                });
            });

            $modal.addEventListener('shown.bs.modal', () => $search.focus());
        }
    }

    function bindCurrencyPickers() {
        const $pickers = document.querySelectorAll('[data-currency-picker]');
        $pickers.forEach($picker => bindCurrencyPicker($picker));
    }
})();
