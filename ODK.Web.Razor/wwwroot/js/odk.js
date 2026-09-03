(function () {
    bindAttachTo();
    bindCollapseToggle();
    bindConditionals();
    bindConfirms();
    bindCopyToClipboard();
    bindFeaturePopovers();
    bindForms();
    bindImages();
    bindMenuLinks();
    bindPopovers();
    bindRedirectTimers();
    bindScroll();
    bindSideMenus();
    bindSiteHeader();
    bindToasts();
    bindTooltips();

    window.odk = window.odk || {};
    window.odk.utils = window.odk.utils || {};
    window.odk.utils.bindTooltips = bindTooltips;

    // Headers for AJAX POSTs so they carry the antiforgery token (from the layout meta tag).
    window.odk.antiforgeryHeaders = function () {
        const token = document.querySelector('meta[name="request-verification-token"]')?.content;
        return token ? { 'RequestVerificationToken': token } : {};
    };

    function bindAttachTo() {
        const $elements = document.querySelectorAll('[data-attach-to]');
        $elements.forEach($element => {
            const selector = $element.getAttribute('data-attach-to');
            const $target = document.querySelector(selector);
            $element.removeAttribute('data-attach-to');
            $element.classList.remove('d-none');
            if (!$target) return;
            $target.appendChild($element);
        });
    }

    function bindCollapseToggle() {
        const hiddenClass = 'd-none';
        const $triggers = document.querySelectorAll('[data-collapse-toggle-show]');
        $triggers.forEach($trigger => {
            const showSelector = $trigger.getAttribute('data-collapse-toggle-show');
            const hideSelector = $trigger.getAttribute('data-collapse-toggle-hide');
            const $show = document.querySelector(showSelector);
            const $hide = document.querySelector(hideSelector);
            if (!$show || !$hide) {
                return;
            }

            const $hideTrigger = document.querySelector(`[data-collapse-toggle-hide="${showSelector}"]`);

            $trigger.setAttribute('aria-controls', showSelector);
            $trigger.setAttribute('aria-expanded', !$show.classList.contains(hiddenClass))

            $trigger.addEventListener('click', e => {
                $show.classList.remove(hiddenClass);
                $hide.classList.add(hiddenClass);
                $trigger.setAttribute('aria-expanded', 'true');
                $hideTrigger.setAttribute('aria-expanded', 'false');
            });
        });
    }

    function bindConditionals() {
        const $targets = document.querySelectorAll('[data-if]');
        $targets.forEach($target => {
            const sourceSelector = $target.getAttribute('data-if');
            const $source = document.querySelector(sourceSelector);
            if (!$source) {
                return;
            }

            const conditionalValue = $target.getAttribute('data-if-value');

            const setDisplay = () => {
                const sourceValue = $source.value;
                const visible = sourceValue === conditionalValue;
                if (visible) {
                    $target.classList.remove('d-none');
                } else {
                    $target.classList.add('d-none');
                }
            };

            $source.addEventListener('change', () => setDisplay());
            setDisplay();
        });
    }

    // Replaces the native, blocking confirm() with the shared dialog in _Layout. A form opts in by rendering
    // the _Confirm component, which emits a [data-odk-confirm] marker carrying the message (plus optional
    // -title, -ok and -variant). Intercepting the form's submit rather than a button's click covers every
    // route to submitting - any button, the enter key, requestSubmit - so one confirm guards the whole form.
    // A modal is async, so the submit is cancelled and replayed only once the user accepts.
    function bindConfirms() {
        const $modal = document.getElementById('confirm-modal');
        if (!$modal) return;

        const confirmModal = new bootstrap.Modal($modal);
        const $title = $modal.querySelector('.modal-title');
        const $message = $modal.querySelector('[data-odk-confirm-message]');
        const $accept = $modal.querySelector('[data-odk-confirm-accept]');
        const defaults = {
            title: $title.textContent,
            accept: $accept.textContent,
            variant: 'danger'
        };

        // Forms whose confirmation has been accepted, so the replayed submit passes straight through.
        // One-shot: consumed by that submit, so a later one asks again.
        const accepted = new WeakSet();

        let onAccept = null;

        // Shows the dialog for a form's _Confirm marker, running accept() if the user confirms. Returns
        // false when the form has no marker, so callers can carry on; true means the caller must stop and
        // wait. Exposed as window.odk.confirm for code that submits a form itself (see odk.forms.js), so
        // every confirmation goes through this one dialog.
        // Declared as a const arrow, NOT a function declaration: the minifier rewrites the early return
        // above into an if block, and a hoisted function would then be lifted out of that block and bind
        // its references to whatever shares the mangled name in the enclosing scope.
        const request = ($form, accept) => {
            const $confirm = $form.querySelector('[data-odk-confirm]');
            if (!$confirm) return false;

            onAccept = accept;

            $message.textContent = $confirm.getAttribute('data-odk-confirm');
            $title.textContent = $confirm.getAttribute('data-odk-confirm-title') || defaults.title;
            $accept.textContent = $confirm.getAttribute('data-odk-confirm-ok') || defaults.accept;

            $accept.classList.length = 0;
            $accept.classList.add('btn');
            $accept.className = 'btn btn-'
                + ($confirm.getAttribute('data-odk-confirm-variant') || defaults.variant);

            confirmModal.show();
            return true;
        };

        $modal.addEventListener('hidden.bs.modal', () => onAccept = null);

        $accept.addEventListener('click', () => {
            const accept = onAccept;
            onAccept = null;
            confirmModal.hide();
            if (accept) accept();
        });

        // Delegated (submit bubbles), so forms rendered after load are covered too.
        document.addEventListener('submit', e => {
            const $form = e.target;

            if (accepted.has($form)) {
                accepted.delete($form);
                return;
            }

            if (!$form.querySelector('[data-odk-confirm]')) return;

            e.preventDefault();
            request($form, () => {
                accepted.add($form);
                // requestSubmit (not submit) so validation and submit handlers still run.
                $form.requestSubmit();
                // Still flagged means the submit never fired - validation blocked it - so clear the flag,
                // otherwise the next attempt would skip the confirmation.
                accepted.delete($form);
            });
        });

        window.odk = window.odk || {};
        window.odk.confirm = request;
    }

    function bindCopyToClipboard() {
        const $sources = document.querySelectorAll('[data-copy-to-clipboard]');
        $sources.forEach($source => {
            $source.addEventListener('click', () => {
                const text = $source.getAttribute('data-copy-to-clipboard');
                navigator.clipboard.writeText(text);
            });
        });
    }

    function bindFeaturePopovers() {
        document.addEventListener('click', e => {
            const target = e.target;
            if (!target.hasAttribute('data-feature-hidetip')) {
                return;
            }

            const name = target.getAttribute('data-feature-hidetip');
            const url = `/account/featuretips/${encodeURIComponent(name)}/hide`;

            fetch(url, {
                method: 'POST',
                headers: window.odk.antiforgeryHeaders()
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
            if (!fallbackUrl) {
                image.classList.add('d-none');
                return;
            }

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

    function bindRedirectTimers() {
        const $redirects = document.querySelectorAll('[data-redirect-timer-url][data-redirect-timer-seconds]');
        $redirects.forEach($redirect => {
            let seconds = parseInt($redirect.getAttribute('data-redirect-timer-seconds'));
            const url = $redirect.getAttribute('data-redirect-timer-url');
            const $remaining = $redirect.querySelector('[data-redirect-timer-remaining]');
            const intervalId = $remaining
                ? window.setInterval(() => {
                    seconds--;
                    $remaining.innerHTML = seconds;
                }, 1000)
                : '';
            const timeoutId = window.setTimeout(() => {
                if (intervalId) window.clearInterval(intervalId);
                window.location = url;
            }, seconds * 1000);
        });
    }

    /* The expand toggles on a side menu's sections. The menu renders twice - the page's own column and the
       drawer - so the collapse is driven from the section the toggle sits in rather than through a
       data-bs-target id, which the second copy would duplicate. */
    function bindSideMenus() {
        const $toggles = document.querySelectorAll('[data-side-menu-toggle]');
        $toggles.forEach($toggle => {
            const $section = $toggle.closest('[data-side-menu-section]');
            const $items = $section?.querySelector('[data-side-menu-items]');
            if (!$items) {
                return;
            }

            const collapse = bootstrap.Collapse.getOrCreateInstance($items, { toggle: false });

            $toggle.addEventListener('click', () => collapse.toggle());
            $items.addEventListener('show.bs.collapse', () => $toggle.setAttribute('aria-expanded', 'true'));
            $items.addEventListener('hide.bs.collapse', () => $toggle.setAttribute('aria-expanded', 'false'));
        });
    }

    function bindSiteHeader() {
        const $siteHeader = document.querySelector('[data-site-header]');
        if (!$siteHeader) return;

        const $siteHeaderClass = document.querySelector('[data-site-header-class]');
        if ($siteHeaderClass) {
            const siteHeaderClass = $siteHeaderClass.getAttribute('data-site-header-class');
            if (siteHeaderClass) $siteHeader.setAttribute('class', siteHeaderClass);
        }
    }

    function bindScroll() {
        document.querySelectorAll('[data-scroll-indicator]').forEach($indicator => {
            const $container = $indicator.closest('[data-scroll]');
            if (!$container) return;

            const update = () => {
                const dist = $container.scrollHeight - $container.scrollTop - $container.clientHeight;
                // Hysteresis: show only when there's clearly more below, hide only at the very bottom. A single
                // threshold flickers when a hover or sub-pixel reflow nudges the measurement by a pixel or two.
                if ($indicator.classList.contains('d-none')) {
                    if (dist > 4) $indicator.classList.remove('d-none');
                } else if (dist <= 1) {
                    $indicator.classList.add('d-none');
                }
            };

            $container.addEventListener('scroll', update, { passive: true });
            window.addEventListener('resize', update);

            // Recompute when the offcanvas opens - it may have been measured while hidden (zero size), which
            // is what made the indicator flaky on first show.
            const $offcanvas = $container.closest('.offcanvas');
            if ($offcanvas) $offcanvas.addEventListener('shown.bs.offcanvas', update);

            // Accordions inside the content reshape it; Bootstrap collapse events bubble to the container, so
            // they update the indicator precisely - unlike observing content children, which also fires on the
            // micro-reflows a hover triggers and made the indicator flicker.
            $container.addEventListener('shown.bs.collapse', update);
            $container.addEventListener('hidden.bs.collapse', update);

            // Recompute when the container itself resizes (viewport change, becoming visible). Observe ONLY the
            // container - not its children - so hovering a menu item can't toggle the indicator.
            if (window.ResizeObserver) {
                new ResizeObserver(update).observe($container);
            }

            update();
        });
    }

    function bindToasts() {
        const $toasts = document.querySelectorAll('[data-toast]');

        $toasts.forEach($toast => {
            const autohide = $toast.getAttribute('data-toast-autohide');
            const delay = $toast.getAttribute('data-toast-delay');

            const options = {};
            if (autohide === 'false') options.autohide = false;
            if (delay) options.delay = parseInt(delay);

            const toast = new bootstrap.Toast($toast, options);
            toast.show();
        });
    }

    function bindTooltips() {
        /* Two hooks for the same thing. data-bs-toggle is a single slot, so an element that is already a
           collapse or dropdown trigger cannot declare a tooltip through it - and moving the tooltip to an
           inner span instead shrinks it to that span, leaving the rest of the button hovering silently.
           data-odk-tooltip marks those. Both take their text from data-bs-title. */
        const tooltipTriggerList = document.querySelectorAll(
            '[data-bs-toggle="tooltip"], [data-odk-tooltip]');
        const tooltipList = [...tooltipTriggerList]
            .filter(x => !!x.getAttribute('data-bs-title'))
            .map(x => {
                const tooltip = new bootstrap.Tooltip(x);

                /* A trigger that also does something on click leaves its tooltip behind: whatever the click
                   opens covers the trigger without the pointer moving, so no mouseleave is raised and the
                   tooltip hangs over the new thing. Clicking has answered whatever the tooltip was there to
                   say, so hiding it is right whether or not the trigger does anything else. */
                x.addEventListener('click', () => tooltip.hide());

                return tooltip;
            });
    }
})();