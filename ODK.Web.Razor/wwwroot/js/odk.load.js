// Generic non-blocking component loader: any element with a [data-load] attribute has its inner HTML
// fetched from that URL after the page renders, so the component's server-side work doesn't block the
// main document. A failed load is swallowed - a component must never break the page.
(function () {
    document.querySelectorAll('[data-load]').forEach(loadComponent);

    async function loadComponent($placeholder) {
        const url = $placeholder.getAttribute('data-load');
        if (!url) return;

        try {
            const response = await fetch(url);
            if (!response.ok) return;
            $placeholder.innerHTML = await response.text();
        } catch {
            // Non-blocking: ignore load failures.
        }
    }
})();
