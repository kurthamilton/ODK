// Cropper.js v2 (ES module). v2 is a web-component rewrite: `new Cropper(img)` replaces the
// <img> with a <cropper-canvas> template, and the crop result is read from the <cropper-selection>
// via $toCanvas(). The only value posted back is the cropped PNG data URL ([data-img-dataurl]).
import Cropper from 'cropperjs';

/* Upper bound on the width of the exported crop. The server resizes to its own maximum whichever
   way, so this only has to sit above that maximum - it is here because the crop travels as a base64
   data URL in a form field, and a form value has a length limit that a full-resolution crop of a
   modern camera photo would exceed. */
const MAX_EXPORT_WIDTH = 800;

/* A drag emits a change per pointer move, and each one would otherwise rasterise a crop that the next
   move immediately replaces. Only the trailing edge is worth exporting, and this is short enough not
   to be noticed by someone releasing the pointer and submitting. */
const EXPORT_DEBOUNCE_MS = 100;

/* Gap left between the image and the canvas edges, in canvas pixels. The selection is allowed past the
   image bounds, and this is the room inside the canvas where a selection that has gone past them can
   still draw its border. It cannot be had by unclipping the canvas: <cropper-shade> paints the mask
   outside the selection as an outline the width of the viewport, and the canvas's own overflow:hidden
   is the only thing cutting that back to the cropper. */
const IMAGE_INSET = 40;

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

        // Outline the selection itself. Its dashed border comes from the <cropper-grid> child, which
        // reads well over the image but not over the page behind it, and the selection is allowed past
        // the image bounds.
        selection.outlined = true;
        selection.addEventListener('change', () => scheduleExport(context, EXPORT_DEBOUNCE_MS));
    }

    const canvas = cropper.getCropperCanvas();
    if (canvas) {
        // Releasing the pointer ends the stream of changes, so there is nothing left to debounce and
        // the field should not sit behind the selection the member has settled on.
        canvas.addEventListener('actionend', () => scheduleExport(context, 0));
    }

    const image = cropper.getCropperImage();
    if (image && canvas) {
        /* Deferred past $ready rather than run from it: Cropper centres the image from the inner
           <img>'s load event, and that can land after $ready has already resolved. Centring resets the
           transform, so an inset applied from the callback itself is thrown away again. */
        image.$ready(() => setTimeout(() => {
            fitImage(canvas, image, selection);
            scheduleExport(context, 0);
        }));
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

    /* Every selection change starts its own export, and they settle in whatever order they finish,
       so a slower earlier export must not land on top of a later one. Only the newest may write. */
    const token = (context.exportToken || 0) + 1;
    context.exportToken = token;

    selection.$toCanvas({ width: getExportWidth(context, selection) }).then(canvas => {
        if (context.exportToken !== token) {
            return;
        }

        context.dataUrl.value = canvas.toDataURL('image/png');
    });
}

/* Insets the image within the canvas by IMAGE_INSET, and sizes the initial selection to the image it
   ends up with. */
function fitImage(canvas, image, selection) {
    const canvasBox = canvas.getBoundingClientRect();
    const fittedBox = image.getBoundingClientRect();

    // Whichever axis the fitted image fills is the one that would clip a selection past its bounds, so
    // the smaller of the two scales is what guarantees the gap on both.
    const scale = Math.min(
        (canvasBox.width - (IMAGE_INSET * 2)) / fittedBox.width,
        (canvasBox.height - (IMAGE_INSET * 2)) / fittedBox.height);

    if (scale > 0 && scale < 1) {
        image.$scale(scale);
    }

    if (!selection) {
        return;
    }

    /* The largest box of the locked ratio that fits within the image, centred on it. This is what
       initialCoverage computes, except that it measures the canvas - which now reaches past the picture
       on every side, so it would start the crop with transparent margins baked into it.

       Sized here rather than passed to $change as the image box: $change fits what it is given to the
       ratio by covering, so handing it the image would overhang instead of fit. */
    const insetBox = image.getBoundingClientRect();
    const ratio = selection.aspectRatio;

    let width = insetBox.width;
    let height = insetBox.height;

    if (Number.isFinite(ratio) && ratio > 0) {
        if (width / height > ratio) {
            width = height * ratio;
        }
        else {
            height = width / ratio;
        }
    }

    selection.$change(
        (insetBox.left - canvasBox.left) + ((insetBox.width - width) / 2),
        (insetBox.top - canvasBox.top) + ((insetBox.height - height) / 2),
        width,
        height);
}

function getAspectRatio(context) {
    /* No ratio stated leaves the crop unconstrained. A form that wants one says so, reading it off
       the entity being uploaded. */
    if (!context.resize.hasAttribute('data-img-ratio')) {
        return NaN;
    }

    /* Number() rather than parseFloat(): parseFloat stops at the first character it cannot read, so a
       ratio carrying a decimal comma would parse as just its whole part and lock the crop to that
       instead of failing. An unreadable ratio leaves the crop unconstrained, which is visible. */
    const ratio = Number(context.resize.getAttribute('data-img-ratio'));
    return Number.isFinite(ratio) && ratio > 0 ? ratio : NaN;
}

// Width of the crop in source-image pixels, so what gets posted does not depend on how large the
// cropper happens to be rendered, bounded by MAX_EXPORT_WIDTH.
function getExportWidth(context, selection) {
    const renderedWidth = context.cropper.getCropperImage()?.getBoundingClientRect().width || 0;
    const naturalWidth = context.preview?.naturalWidth || 0;

    const sourceWidth = renderedWidth > 0 && naturalWidth > 0
        ? selection.width * (naturalWidth / renderedWidth)
        : selection.width;

    return Math.max(1, Math.min(Math.round(sourceWidth), MAX_EXPORT_WIDTH));
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

/* Always deferred, whatever the delay: the change event is cancellable, so Cropper emits it before
   assigning the new geometry and the selection still reports its previous position while handlers run.
   $toCanvas() reads that geometry, so an export running synchronously would post the previous crop. */
function scheduleExport(context, delay) {
    clearTimeout(context.exportTimeout);
    context.exportTimeout = setTimeout(() => exportSelection(context), delay);
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
