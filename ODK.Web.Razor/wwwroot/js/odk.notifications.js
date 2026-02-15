(async function () {
    await load();

    function bindDismiss($container) {
        const $dismissButtons = $container.querySelectorAll('[data-notification-dismiss]');
        $dismissButtons.forEach($button => {
            $button.addEventListener('click', e => {
                e.preventDefault();

                const $notification = $button.closest('[data-notification]');

                const url = $button.getAttribute('data-notification-dismiss');
                fetch(url, { method: 'POST' })
                    .then(() => {
                        const tooltip = bootstrap.Tooltip.getOrCreateInstance($button);
                        tooltip.dispose();
                        $notification.remove();
                        onDismiss($container);
                    });
            });
        });
    }

    function bindDismissAll($container) {
        const $dismissAllButtons = $container.querySelectorAll('[data-notifications-dismiss-all]');
        $dismissAllButtons.forEach($button => {
            $button.addEventListener('click', e => {
                e.preventDefault();

                const url = $button.getAttribute('data-url');
                fetch(url, { method: 'POST' })
                    .then(() => {
                        const $notifications = $container.querySelectorAll('[data-notification]');
                        $notifications.forEach($notification => $notification.remove());
                        onDismiss($container);
                    });
            });
        });
    }

    function bindTabs($container) {
        const $tabContainer = $container.querySelector('[data-notification-group-tabs]');
        if (!$tabContainer) return;

        let $tabs = $tabContainer.querySelectorAll('[data-notification-group-tab]');
        $tabs.forEach($tab => {
            $tab.addEventListener('click', () => onTabClick($container, $tabs, $tab));
        });

        $container.addEventListener('odk:dismiss', () => {
            const $notifications = $container.querySelectorAll('[data-notification]');
            const groups = {
                All: 0
            };

            $notifications.forEach($notification => {
                const group = $notification.getAttribute('data-notification-group');
                if (!groups[group]) groups[group] = 0;
                groups.All++;
                groups[group]++;
            });

            if (groups.All === 0) return;

            $tabs.forEach($tab => {
                const tabGroup = $tab.getAttribute('data-notification-group-tab');
                const count = groups[tabGroup] || 0;
                if (!count) {
                    $tab.remove();
                    $tabs = $tabContainer.querySelectorAll('[data-notification-group-tab]');

                    if ($tabs.length === 2) {
                        // Don't show individual groups if there's only one group + "All"
                        $tabs[1].remove();
                        $tabs = $tabContainer.querySelectorAll('[data-notification-group-tab]');
                    }

                    onTabClick($container, $tabs, $tabs[0]);

                    if ($tabs.length <= 1) return $tabContainer.remove();
                }

                const $count = $tab.querySelector('[data-notification-count]');
                $count.innerHTML = count;
            });
        });
    }

    async function load() {
        const $placeholder = document.querySelector('[data-notifications-load]');
        if (!$placeholder) return;

        const url = $placeholder.getAttribute('data-notifications-load');
        const response = await fetch(url);
        if (!response.ok) return;
        const html = await response.text();
        $placeholder.innerHTML = html;

        onLoad();
    }

    function onChange($container) {
        const $notifications = $container.querySelectorAll('[data-notification]');

        toggleVisibilities($notifications.length === 0);
        updateCounts($container, $notifications);
    }

    function onDismiss($container) {
        onChange($container);

        $container.dispatchEvent(new Event('odk:dismiss'));
    }

    function onLoad() {
        const $container = document.querySelector('[data-notifications]');
        bindDismiss($container);
        bindDismissAll($container);
        bindTabs($container);

        onChange($container);

        window.odk.utils.bindTooltips();
    }

    function onTabClick($container, $tabs, $tab) {
        const $notifications = $container.querySelectorAll('[data-notification]');
        $notifications.forEach($notification => $notification.classList.remove('d-none'));

        $tabs.forEach(x => x.classList.remove('active'));
        $tab.classList.add('active');

        const tabGroup = $tab.getAttribute('data-notification-group-tab');
        if (tabGroup === 'All') return;

        $notifications.forEach($notification => {
            const notificationGroup = $notification.getAttribute('data-notification-group');
            if (notificationGroup === tabGroup) return;
            $notification.classList.add('d-none');
        });
    }

    function toggleVisibilities(empty) {
        const $elementContainers = document.querySelectorAll('[data-notification-components]');
        $elementContainers.forEach($elementContainer => {
            const $hideIfEmpty = $elementContainer.querySelectorAll('[data-notifications-hide-if-empty]');
            const $showIfEmpty = $elementContainer.querySelectorAll('[data-notifications-show-if-empty]');

            if (empty) {
                $hideIfEmpty.forEach(x => x.classList.add('d-none'));
                $showIfEmpty.forEach(x => x.classList.remove('d-none'));
            } else {
                $showIfEmpty.forEach(x => x.classList.add('d-none'));
                $hideIfEmpty.forEach(x => x.classList.remove('d-none'));
            }
        });
    }

    function updateCounts($container, $notifications) {
        const totalCount = $notifications.length;

        const $countBadges = document.querySelectorAll('[data-notification-count-badge]');
        $countBadges.forEach(x => x.innerHTML = totalCount > 9 ? '9+' : totalCount.toString());

        const $counts = $container.querySelectorAll('[data-notification-total-count]');
        $counts.forEach(x => x.innerHTML = totalCount.toString());
    }
})();