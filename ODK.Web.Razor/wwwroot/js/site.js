(function () {
    bindFeaturePopovers();
    bindForms();
    bindImages();
    bindMenuLinks();
    bindPopovers();
    bindTooltips();    

    function bindFeaturePopovers() {
        document.addEventListener('click', e => {
            const target = e.target;
            if (!target.hasAttribute('data-feature-hidetip')) {
                return;
            }
            
            const name = target.getAttribute('data-feature-hidetip');                        
            const url = `/Account/FeatureTips/${encodeURIComponent(name)}/Hide`;
            
            fetch(url, {
                method: 'POST'
            }).then(() => {
                target.removeAttribute('data-feature-hidetip');

                const tips = document.querySelectorAll('[data-feature-tip]');
                tips.forEach(tip => {
                    const popover = bootstrap.Popover.getInstance(tip);
                    if (!popover) {
                        return;
                    }

                    popover.hide();
                });
            });
        });
    }

    function bindForms() {
        document.querySelectorAll('[data-onchange]').forEach(input => {
            const action = input.getAttribute('data-onchange');
            if (action === 'submit') {                
                input.addEventListener('change', () => {
                    const form = input.closest('form');
                    if (form) {
                        form.submit();
                    }
                });
            }
        });

        document.querySelectorAll('[data-button-for]').forEach(button => {
            button.addEventListener('click', () => {
                const targetSelector = button.getAttribute('data-button-for');
                const target = document.querySelector(targetSelector);
                target.click();
            });
        });

        document.querySelectorAll('[data-select-freetext]').forEach(select => {            
            const targetSelector = select.getAttribute('data-select-freetext');
            const triggerValue = select.getAttribute('data-select-freetext-value');
            const target = document.querySelector(targetSelector);

            function setVisibility() {
                if (select.value === triggerValue) {
                    target.classList.remove('d-none');
                } else {
                    target.classList.add('d-none');
                }
            }

            select.addEventListener('change', () => {
                setVisibility();
            });

            setVisibility();
        });
    }

    function bindImages() {
        const constrainImage = image => {
            image.style.maxWidth = `${image.naturalWidth}px`;
            image.style.maxHeight = `${image.naturalHeight}px`;
        };

        const loadFallback = image => {
            const fallbackUrl = image.getAttribute('data-img-fallback');
            image.src = fallbackUrl;
            image.onerror = null;
        };

        const naturalSizeImages = document.querySelectorAll('[data-img-naturalsize]');
        naturalSizeImages.forEach(image => {
            if (image.complete) {
                constrainImage(image);
            } else {
                image.onload = () => constrainImage(image);
            }            
        });

        const fallbackImages = document.querySelectorAll('[data-img-fallback]');
        fallbackImages.forEach(image => {
            if (image.complete) { 
                if (image.error) {
                    loadFallback(image);
                }
            } else {
                image.onerror = () => loadFallback(image);
            }            
        });
    }    

    function bindMenuLinks() {
        const currentPath = window.location.pathname.toLocaleLowerCase();
        const $links = document.querySelectorAll('[data-menu-link][href]');
        $links.forEach($link => {
            const type = $link.getAttribute('data-menu-link');
            const href = $link.getAttribute('href').toLocaleLowerCase();

            let match = false;
            switch (type) {
                case 'exact':
                    match = currentPath == href;
                    break;
                default:
                    match = currentPath.startsWith(href);
                    break;
            }

            if (match) {
                $link.classList.add('active');
            }
        });
    }

    function bindPopovers() {
        const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
        popoverTriggerList.forEach(element => {
            const content = element.querySelector('[data-popover-content]');
            if (!content) {
                return;
            }

            const html = content.innerHTML;            
            element.setAttribute('data-bs-content', html);
        });

        popoverTriggerList.forEach(element => {
            const options = {};
            if (element.getAttribute('data-popover-sanitize') === 'false') {
                options.sanitize = false;
            }

            const popover = new bootstrap.Popover(element, options);
            if (element.hasAttribute('data-popover-show')) {            
                popover.show();
            }
        });        
    }    

    function bindTooltips() {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
    }    
})();