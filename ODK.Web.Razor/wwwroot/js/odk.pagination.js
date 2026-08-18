(function () {
    /* The go-to-page control behind a pagination ellipsis. An ellipsis stands for a run of pages, so it has
       no page to link to and those pages are otherwise unreachable without editing the URL by hand.

       The nav carries the page URL with a {page} placeholder, because the pages in the run cannot all be
       rendered as links - the browser builds the one it needs. Bootstrap owns opening and closing the
       dropdown; this only fills it in. */
    document.querySelectorAll('[data-odk-pagination]').forEach(nav => {
        const urlTemplate = nav.dataset.odkPagination;

        nav.querySelectorAll('[data-odk-pagination-goto]').forEach(panel => {
            const input = panel.querySelector('[data-odk-pagination-page]');
            const first = Number(input.min);
            const last = Number(input.max);

            // A number input accepts anything typed into it - min and max only bind the spinner and browser
            // validation, neither of which is in the way here - so clamp before using the value.
            const wanted = () => {
                const value = Number(input.value);
                return Number.isFinite(value) ? Math.min(Math.max(Math.round(value), first), last) : first;
            };

            panel.querySelectorAll('[data-odk-pagination-step]').forEach(step => {
                step.addEventListener('click', () => {
                    input.value = Math.min(Math.max(wanted() + Number(step.dataset.odkPaginationStep), first), last);
                });
            });

            const go = () => window.location.assign(urlTemplate.replace('{page}', wanted()));

            panel.querySelector('[data-odk-pagination-go]').addEventListener('click', go);

            // Enter is what somebody who has just typed a number will press, and the panel is not a form, so
            // nothing would otherwise happen.
            input.addEventListener('keydown', event => {
                if (event.key === 'Enter') {
                    event.preventDefault();
                    go();
                }
            });

            // Opening the panel to a selected number means a keyboard user can type over it immediately, and
            // the default is only ever a guess at where they want to go.
            panel.closest('.dropdown')?.addEventListener('shown.bs.dropdown', () => {
                input.focus();
                input.select();
            });
        });
    });
})();
