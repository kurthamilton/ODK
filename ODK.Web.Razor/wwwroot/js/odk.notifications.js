(function() {
    const $dismissButtons = document.querySelectorAll('[data-notification-dismiss]');
    $dismissButtons.forEach($button => {
        $button.addEventListener('click', e => {
            e.preventDefault();

            const $notification = $button.closest('[data-notification]');

            const url = $button.getAttribute('data-notification-dismiss');
            const xhr = new XMLHttpRequest();
            xhr.open('POST', url, true);

            xhr.onreadystatechange = () => {
                if (xhr.readyState === XMLHttpRequest.DONE && xhr.status === 200) {
                    const $container = $notification.closest('[data-notifications]');
                    const tooltip = bootstrap.Tooltip.getOrCreateInstance($button);
                    tooltip.dispose();

                    $notification.remove();

                    const $notifications = $container.querySelectorAll('[data-notification]');
                    if ($notifications.length === 0) {
                        const $hide = $container.querySelectorAll('[data-notifications-hide-if-empty]');
                        const $show = $container.querySelectorAll('[data-notifications-show-if-empty]');

                        $hide.forEach(x => x.classList.add('d-none'));
                        $show.forEach(x => x.classList.remove('d-none'));
                    } else {
                        const $counter = $container.querySelector('[data-notification-count]');
                        const count = $notifications.length > 9 ? '9+' : $notifications.length.toString();
                        $counter.innerHTML = count;
                    }
                }
            };

            xhr.send();
        });
    });
})();