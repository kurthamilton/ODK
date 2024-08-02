(function (Cropper) {
    const containers = document.querySelectorAll('[data-img-container]');
    containers.forEach(container => bindContainer(container));

    function bindContainer(container) {
        const context = {
            container: container,
            resize: container.querySelector('[data-img-resize]')
        };

        bindFileUpload(context);
        bindForm(context);
        bindCropper(context);
    }

    function bindFileUpload(context) {
        context.fileUpload = context.container.querySelector('[data-img-input]');
        if (!context.fileUpload) {
            return;
        }

        context.preview = context.container.querySelector('[data-img-preview]');
        if (!context.preview) {
            return;
        }

        context.fileUpload.addEventListener('change', e => {
            const [file] = fileUpload.files
            if (!file) {
                return;
            }

            context.preview.src = URL.createObjectURL(file);
            context.preview.classList.remove('d-none');

            bindCropper(context);
        });        
    }

    function bindCropper (context) {
        if (context.cropper) {
            context.cropper.destroy();
        }

        if (!context.resize) {
            return;            
        }        

        const options = {
            aspectRatio: 1,
            autoCropArea: 1,
            viewMode: 1
        };
        context.cropper = new Cropper(context.resize, options);
    }

    function bindForm(context) {
        const form = context.container.closest('form');
        if (!form) {
            return;
        }

        const cropXInput = context.container.querySelector('[data-img-crop-x]');
        const cropYInput = context.container.querySelector('[data-img-crop-y]');
        const cropWidthInput = context.container.querySelector('[data-img-crop-width]');
        const cropHeightInput = context.container.querySelector('[data-img-crop-height]');
        

        form.addEventListener('submit', (e) => {
            if (!context.cropper) {
                return;
            }            

            const data = context.cropper.getData(true);
            setValue(cropXInput, data.x);
            setValue(cropYInput, data.y);
            setValue(cropWidthInput, data.width);
            setValue(cropHeightInput, data.height);

            // const dataUrl = context.cropper.getCroppedCanvas().toDataURL('image/png');
        });
    }

    function setValue(el, value) {
        if (!el) {
            return;
        }

        el.value = value;
    }
})(Cropper);