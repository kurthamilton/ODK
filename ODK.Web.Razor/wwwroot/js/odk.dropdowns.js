(function(bootstrap) {
    const CLASS_NAME = 'has-child-dropdown-show';

    bootstrap.Dropdown.prototype.toggle = function(_original) {
        return function () {
            const openChildren = document.querySelectorAll('.' + CLASS_NAME);            
            openChildren.forEach(x => x.classList.remove(CLASS_NAME));
            let dd = this._element.closest('.dropdown').parentNode.closest('.dropdown');
            for (; dd && dd !== document; dd = dd.parentNode.closest('.dropdown')) {
                dd.classList.add(CLASS_NAME);
            }
            return _original.call(this);
        }
    }(bootstrap.Dropdown.prototype.toggle);

    document.querySelectorAll('.dropdown').forEach(dd => {
        dd.addEventListener('hide.bs.dropdown', e => {
            if (dd.classList.contains(CLASS_NAME)) {
                dd.classList.remove(CLASS_NAME);
                e.preventDefault();
            }
            e.stopPropagation(); // do not need pop in multi level mode
        });
    });

    // for hover
    // NB - opening a child menu means the top-level parent needs two clicks to close.     
    document.querySelectorAll('.dropdown-hover, .dropdown-hover-all .dropdown').forEach(dd => {        
        dd.addEventListener('mouseenter', e => {
            const toggle = e.target.querySelector(':scope>[data-bs-toggle="dropdown"]');
            if (toggle.classList.contains('show')) {
                return;                
            }

            bootstrap.Dropdown.getOrCreateInstance(toggle).toggle();
            dd.classList.add(CLASS_NAME);
            bootstrap.Dropdown.clearMenus(e);
        });
        dd.addEventListener('mouseleave', e => {            
            const toggle = e.target.querySelector(':scope>[data-bs-toggle="dropdown"]');
            if (toggle.classList.contains('show')) {                
                const instance = bootstrap.Dropdown.getOrCreateInstance(toggle);
                instance.toggle();
            }
        });
    });
})(bootstrap);
