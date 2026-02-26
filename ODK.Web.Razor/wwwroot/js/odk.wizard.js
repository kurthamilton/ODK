(function () {
    const $wizards = document.querySelectorAll('[data-wizard]');
    $wizards.forEach($wizard => {
        $wizard.setAttribute('data-wizard-active', 0);

        const v = new aspnetValidation.ValidationService();
        v.bootstrap();

        const $pages = $wizard.querySelectorAll('[data-wizard-page]');

        const $form = $wizard.closest('form');
        $form.addEventListener('submit', e => {
            const activePage = parseInt($wizard.getAttribute('data-wizard-active'));
            if (activePage < $pages.length - 1) e.preventDefault();
        });

        // move to the next page via keyboard carriage return
        $form.addEventListener('keydown', (e) => {
            if (e.key !== 'Enter') return;

            const $target = e.target;
            if (!($target instanceof HTMLElement)) return;

            // allow normal newline behaviour in textarea
            if ($target.tagName === 'TEXTAREA') return;

            if (!validateForm()) return;

            const activePage = getActivePage();

            const $nextPage = $wizard.querySelector(`[data-wizard-page="${activePage + 1}"]`);
            if (!$nextPage) return;

            const $activePage = $wizard.querySelector(`[data-wizard-page="${activePage}"]`);
            if (!$activePage) return;

            const activeCollapse = bootstrap.Collapse.getOrCreateInstance($activePage, { toggle: false });
            const nextCollapse = bootstrap.Collapse.getOrCreateInstance($nextPage, { toggle: false });

            activeCollapse.hide();
            nextCollapse.show();

            e.preventDefault();
            e.stopPropagation();
        });

        function getActivePage() {
            return parseInt($wizard.getAttribute('data-wizard-active'));
        }

        function validateForm() {
            v.validateForm($form);

            const activePage = getActivePage();

            let pageValid = true;
            v.elementUIDs.forEach(elementUID => {
                if (!pageValid) return;

                const node = elementUID.node;
                const $elementPage = node.closest('[data-wizard-page]');
                if (!$elementPage) return;

                const elementPage = parseInt($elementPage.getAttribute('data-wizard-page'));
                if (elementPage !== activePage) return;

                const nodeValid = v.isFieldValid(node);
                if (!nodeValid) pageValid = false;
            });

            return pageValid;
        }

        $pages.forEach($page => {
            const page = parseInt($page.getAttribute('data-wizard-page'));

            $page.addEventListener('show.bs.collapse', e => {
                const activePage = getActivePage();

                if (page > activePage) {
                    const pageValid = validateForm();
                    if (!pageValid) e.preventDefault();
                }

                $wizard.setAttribute('data-wizard-active', page);
            });

            $page.addEventListener('hide.bs.collapse', e => {
                const activePage = getActivePage();
                if (activePage !== page) return;
                e.preventDefault();
            });
        });
    });
})();