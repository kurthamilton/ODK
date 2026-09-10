(function () {
    const $wizards = document.querySelectorAll('[data-wizard]');
    $wizards.forEach($wizard => {
        const $pages = $wizard.querySelectorAll('[data-wizard-page]');

        // Respect the server-rendered active page (the one marked `show`) so a wizard rehydrated
        // mid-flow (e.g. after a cross-request post) doesn't reset to page 0 and block its submit.
        const $shownPage = $wizard.querySelector('[data-wizard-page].show');
        const initialPage = $shownPage
            ? parseInt($shownPage.getAttribute('data-wizard-page'))
            : 0;
        $wizard.setAttribute('data-wizard-active', initialPage);

        const $form = $wizard.closest('[data-wizard-form]') || $wizard.closest('form');
        $form.addEventListener('submit', e => {
            const activePage = getActivePage();
            if (activePage < $pages.length - 1) e.preventDefault();
        });

        $wizard.querySelectorAll('[data-wizard-next]').forEach($button => {
            $button.addEventListener('click', () => goToPage(getActivePage() + 1));
        });

        $wizard.querySelectorAll('[data-wizard-goto]').forEach($button => {
            $button.addEventListener('click', () =>
                goToPage(parseInt($button.getAttribute('data-wizard-goto'))));
        });

        // move to the next page via keyboard carriage return
        $form.addEventListener('keydown', e => {
            if (e.key !== 'Enter') return;

            const $target = e.target;
            if (!($target instanceof HTMLElement)) return;

            // allow normal newline behaviour in textarea
            if ($target.tagName === 'TEXTAREA') return;

            if (getActivePage() >= $pages.length - 1) return;

            e.preventDefault();
            e.stopPropagation();

            goToPage(getActivePage() + 1);
        });

        function getActivePage() {
            return parseInt($wizard.getAttribute('data-wizard-active'));
        }

        async function goToPage(page) {
            const activePage = getActivePage();
            if (page === activePage) return;

            const $page = $wizard.querySelector(`[data-wizard-page="${page}"]`);
            if (!$page) return;

            // Only a move forward is gated; a page already left has validated to get there.
            if (page > activePage && !await validateActivePage()) return;

            /* Set before showing, which is what lets the page being left close: Bootstrap fires
               `show.bs.collapse` on the target and only then hides the accordion's other children, so
               by the time the hide guard below runs the page it names is no longer the active one. */
            $wizard.setAttribute('data-wizard-active', page);
            bootstrap.Collapse.getOrCreateInstance($page, { toggle: false }).show();
        }

        /* Awaited per field, because the validity the service reports synchronously is still the previous
           answer while a rule that asks the server is in flight - a [Remote] duplicate-name check reads
           as valid on the click that started it. The callback is what the result comes back through: the
           promise the service returns for a field resolves false when it is called without one. */
        async function validateActivePage() {
            if ($form.tagName !== 'FORM') return true;

            const v = window.odk.forms.validationService;
            const activePage = getActivePage();

            // The service tracks every element on the page, so this wizard's own are picked out of them.
            const $fields = v.elementUIDs
                .map(elementUID => elementUID.node)
                .filter($field => $wizard.contains($field))
                .filter($field => {
                    const $fieldPage = $field.closest('[data-wizard-page]');
                    return $fieldPage
                        && parseInt($fieldPage.getAttribute('data-wizard-page')) === activePage;
                });

            const results = await Promise.all($fields.map($field =>
                v.validateField($field, () => { })));

            return results.every(valid => valid);
        }

        $pages.forEach($page => {
            const page = parseInt($page.getAttribute('data-wizard-page'));

            /* This page's own event only. Bootstrap's collapse events bubble, so without the guard a
               collapse nested anywhere inside a page's content reaches this handler as though the page
               itself were closing - and it would refuse to close it. */
            $page.addEventListener('hide.bs.collapse', e => {
                if (e.target !== $page) return;

                const activePage = getActivePage();
                if (activePage !== page) return;
                e.preventDefault();
            });
        });
    });
})();
