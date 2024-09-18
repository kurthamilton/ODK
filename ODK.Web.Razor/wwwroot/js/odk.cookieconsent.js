(function () {
    // https://www.osano.com/cookieconsent/documentation/javascript-api/
    window.cookieconsent.initialise({
        container: document.querySelector('body'),
        content: {
            message: 'This website uses cookies to improve your experience.',
            link: 'Learn more',
            href: '/privacy'
        },
        elements: {
            dismiss: '<a aria-label="dismiss cookie message" tabindex="0" class="btn btn-primary cc-btn cc-dismiss">Got it!</a>',
        },
        revokable: false,        
        law: {
            regionalLaw: false,
        },
        location: false,
    });
})();