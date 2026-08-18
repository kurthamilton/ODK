// Draws every state machine diagram on the page, in the theme the rest of the page is using.
//
// Mermaid takes its colours at initialise time and rendering replaces the element's content with an SVG, so
// there is no way to restyle a diagram in place. Each diagram's source is kept aside and the whole set is
// drawn again whenever the theme changes.
(function () {
    const SELECTOR = 'pre.mermaid';

    if (!window.mermaid) return;

    /* Mermaid draws everything it finds on DOMContentLoaded unless told not to, which would race the first
       draw below. Turned off synchronously, before anything is queued. */
    window.mermaid.initialize({ startOnLoad: false });

    const sources = new WeakMap();

    /* Drawing is asynchronous, and it starts by putting the source back in place of the rendered SVG. Two
       overlapping draws therefore have the second pull the element out from under the first, which fails
       inside mermaid - so they are queued rather than allowed to race. Flipping the theme switch twice in
       quick succession is enough to hit it. */
    let queue = Promise.resolve();

    const cssValue = (name, fallback) => {
        const value = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
        return value || fallback;
    };

    const draw = () => {
        const $diagrams = Array.from(document.querySelectorAll(SELECTOR));
        if (!$diagrams.length) {
            return Promise.resolve();
        }

        $diagrams.forEach($diagram => {
            if (!sources.has($diagram)) {
                sources.set($diagram, $diagram.textContent);
            }

            $diagram.textContent = sources.get($diagram);
            $diagram.removeAttribute('data-processed');
        });

        window.mermaid.initialize({
            startOnLoad: false,
            securityLevel: 'strict',
            theme: 'base',
            /* Every colour comes from the same Bootstrap variables the rest of the page uses, so a diagram
               follows a group's theme as well as it follows light and dark. darkMode tells mermaid which way
               to shade the colours it derives from these. */
            themeVariables: {
                darkMode: document.documentElement.getAttribute('data-bs-theme') === 'dark',
                background: cssValue('--bs-body-bg', '#fff'),
                // Mermaid derives this one, and derives it to black in dark mode.
                edgeLabelBackground: cssValue('--bs-body-bg', '#fff'),
                primaryColor: cssValue('--bs-secondary-bg', '#e9ecef'),
                primaryBorderColor: cssValue('--bs-border-color', '#dee2e6'),
                primaryTextColor: cssValue('--bs-body-color', '#212529'),
                lineColor: cssValue('--bs-body-color', '#212529'),
                textColor: cssValue('--bs-body-color', '#212529'),
                fontFamily: cssValue('--bs-body-font-family', 'inherit')
            }
        });

        return window.mermaid.run({ nodes: $diagrams });
    };

    // A failure is logged and dropped: leaving it on the chain would stop every later redraw.
    const render = () => {
        queue = queue.then(draw).catch(error => console.error('Diagram render failed', error));
    };

    render();

    document.addEventListener('odk:theme-changed', render);
})();
