(function (Cropper) {
    const containers = document.querySelectorAll('[data-img-container]');
    containers.forEach(container => bindContainer(container));

    function bindContainer(container) {
        const context = {
            container: container,
            dataUrl: container.querySelector('[data-img-dataurl]'),
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

        context.previewContainer = context.preview.closest('[data-img-preview-container]');

        context.preview.addEventListener('load', () => {
            bindCropper(context);
            context.previewContainer.classList.remove('d-none');
        });

        if (context.preview.getAttribute('src') && context.preview.complete) {
            context.previewContainer.classList.remove('d-none');
        }

        context.fileUpload.addEventListener('change', e => {
            const [file] = context.fileUpload.files
            if (!file) {
                return;
            }

            context.preview.src = URL.createObjectURL(file);
            context.preview.classList.remove('d-none');

            setCropData(context, {});
            bindCropper(context);
        });        
    }

    function bindCropper (context) {
        if (context.cropper) {
            context.cropper.destroy();
            context.cropper = null;
        }

        if (!context.resize || !context.preview.getAttribute('src')) {
            return;            
        }        
        
        const options = {
            aspectRatio: context.resize.hasAttribute('data-img-ratio')
                ? context.resize.getAttribute('data-img-ratio') || NaN 
                : 1,
            autoCrop: true,
            autoCropArea: 1, // percentage of default crop area, 0 <= x <= 1
            data: getCropData(context),
            viewMode: 1
        };
        
        context.cropper = new Cropper(context.resize, options);

        handleModals(context);
    }

    function bindForm(context) {
        const form = context.container.closest('form');
        if (!form) {
            return;
        }

        context.cropXInput = context.container.querySelector('[data-img-crop-x]');
        context.cropYInput = context.container.querySelector('[data-img-crop-y]');
        context.cropWidthInput = context.container.querySelector('[data-img-crop-width]');
        context.cropHeightInput = context.container.querySelector('[data-img-crop-height]');        
        context.mimeType = context.container.querySelector('[data-img-type]');

        form.addEventListener('submit', (e) => {
            if (!context.cropper) {
                return;
            }            

            if (!context.cropper.element.src) {
                // no image has been loaded
                return;
            }

            const data = context.cropper.getData(true);

            if (context.dataUrl) {
                let mimeType = context.mimeType;
                const [file] = context.fileUpload.files;
                if (file) {
                    mimeType = file.type;                    
                }

                if (mimeType) {
                    context.dataUrl.value = context.cropper.getCroppedCanvas().toDataURL(mimeType);
                }
            }
            
            setCropData(context, data);
        });
    }

    function getCropData(context) {
        return {
            x: getValue(context.cropXInput),
            y: getValue(context.cropYInput),
            width: getValue(context.cropWidthInput),
            height: getValue(context.cropHeightInput)
        };
    }

    function getValue(el) {
        if (!el) {
            return;
        }

        var value = parseInt(el.value);
        if (isNaN(value)) {
            return;
        }

        return value;
    }

    function handleModals(context) {
        if (context.modalChecked) {
            return;
        }

        context.modalChecked = true;

        context.modal = context.resize.closest('.modal');
        if (!context.modal) {
            return;
        }

        // force cropper to re-bind on modal open to fix layout issues
        context.modal.addEventListener('shown.bs.modal', () => bindCropper(context));
    }    

    function setCropData(context, data) {
        setValue(context.cropXInput, data.x);
        setValue(context.cropYInput, data.y);
        setValue(context.cropWidthInput, data.width);
        setValue(context.cropHeightInput, data.height);
    }

    function setValue(el, value) {
        if (!el) {
            return;
        }

        el.value = value;
    }    
})(Cropper);