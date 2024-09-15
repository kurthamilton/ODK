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
            if (activePage < $pages.length - 1) {
                e.preventDefault();
            }

            const $nextPage = $wizard.querySelector(`[data-wizard-page="${activePage + 1}"]`);
            if ($nextPage) {
                const collapse = bootstrap.Collapse.getInstance($nextPage);                
            }
        });
        
        $pages.forEach($page => {
            const page = parseInt($page.getAttribute('data-wizard-page'));
            
            $page.addEventListener('show.bs.collapse', e => {                                                
                const activePage = parseInt($wizard.getAttribute('data-wizard-active'));

                if (page > activePage) {
                    v.validateForm($form);

                    let pageValid = true;
                    v.elementUIDs.forEach(elementUID => {
                        const node = elementUID.node;
                        const $elementPage = node.closest('[data-wizard-page]');
                        if (!$elementPage) {
                            return;
                        }

                        const elementPage = parseInt($elementPage.getAttribute('data-wizard-page'));
                        if (elementPage !== activePage) {
                            return;
                        }

                        const nodeValid = v.isFieldValid(node);
                        if (!nodeValid) {
                            pageValid = false;
                        }
                    });

                    if (!pageValid) {
                        e.preventDefault();
                        return;
                    }
                }                
                
                $wizard.setAttribute('data-wizard-active', page);
            });

            $page.addEventListener('hide.bs.collapse', e => {
                const activePage = parseInt($wizard.getAttribute('data-wizard-active'));
                if (activePage !== page) {
                    return;
                }
                
                e.preventDefault();
            });
        });
    });
})();