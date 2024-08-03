function setImageError(img) {
    img.onerror = function () { };
    img.error = true;
}