// Cropper.js v2 (ES module). v2 is a web-component rewrite: `new Cropper(img)` replaces the
// <img> with a <cropper-canvas> template, and the crop result is read from the <cropper-selection>
// via $toCanvas(). The only value posted back is the cropped PNG data URL ([data-img-dataurl]).
// NOTE: the cropping UX (aspect ratio, initial selection, canvas sizing) needs in-browser
// verification on the picture pages - it can't be exercised from unit tests.
import Cropper from 'cropperjs';

const containers = document.querySelectorAll('[data-img-container]');
containers.forEach(container => bindContainer(container));

function bindContainer(container) {
    const context = {
        container: container,
        dataUrl: container.querySelector('[data-img-dataurl]'),
        resize: container.querySelector('[data-img-resize]')
    };

    bindFileUpload(context);
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

    if (context.preview.getAttribute('src') && context.preview.complete) {
        context.previewContainer.classList.remove('d-none');
    }

    context.fileUpload.addEventListener('change', () => {
        const [file] = context.fileUpload.files;
        if (!file) {
            return;
        }

        context.previewContainer.classList.remove('d-none');
        setImageSource(context, URL.createObjectURL(file));
    });
}

function bindCropper(context) {
    if (context.cropper) {
        context.cropper.destroy();
        context.cropper = null;
    }

    if (!context.resize || !context.preview || !context.preview.getAttribute('src')) {
        return;
    }

    // Constructing the cropper defines the custom elements and copies the <img> src across.
    const cropper = new Cropper(context.resize);
    context.cropper = cropper;

    const selection = cropper.getCropperSelection();
    if (selection) {
        selection.aspectRatio = getAspectRatio(context);
        selection.initialCoverage = 1;
        selection.addEventListener('change', () => exportSelection(context));
    }

    const image = cropper.getCropperImage();
    if (image) {
        image.$ready(() => {
            selection?.$reset();
            exportSelection(context);
        });
    }

    handleModals(context);
}

function exportSelection(context) {
    if (!context.dataUrl || !context.cropper) {
        return;
    }

    const selection = context.cropper.getCropperSelection();
    if (!selection) {
        return;
    }

    selection.$toCanvas().then(canvas => {
        context.dataUrl.value = canvas.toDataURL('image/png');
    });
}

function getAspectRatio(context) {
    if (!context.resize.hasAttribute('data-img-ratio')) {
        return 1;
    }

    const ratio = parseFloat(context.resize.getAttribute('data-img-ratio'));
    return isNaN(ratio) ? NaN : ratio;
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

function setImageSource(context, src) {
    // Destroy first so the cropper restores the original <img>, then update its src and rebind.
    if (context.cropper) {
        context.cropper.destroy();
        context.cropper = null;
    }

    context.preview.src = src;
    bindCropper(context);
}
