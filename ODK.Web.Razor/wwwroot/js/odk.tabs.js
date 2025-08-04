(function () {
    bindTabs();

    function bindTabs() {
        const tabGroups = document.querySelectorAll('[data-odk-tabs]');        
        tabGroups.forEach(tabGroup => {
            // data-bs-toggle="tab" only set for JS tabs
            const triggers = tabGroup.querySelectorAll('[data-bs-toggle="tab"]');
            if (triggers.length === 0) {
                return;
            }

            // find active trigger
            let activeTrigger;
            triggers.forEach(trigger => {
                if (activeTrigger) {
                    return;
                }

                if (trigger.classList.contains('active')) {
                    activeTrigger = trigger;
                }
            });
            
            if (activeTrigger) {
                // active tab already set
                return;
            }

            activeTrigger = triggers[0];
            const tabTrigger = new bootstrap.Tab(activeTrigger);
            tabTrigger.show();
        });
    }
})();