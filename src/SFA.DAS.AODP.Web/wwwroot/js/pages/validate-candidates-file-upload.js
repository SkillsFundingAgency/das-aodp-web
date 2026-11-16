document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('uploadForm');
    if (!form) return;

    const fileSection = document.getElementById('fileSection');
    const uploadText = document.getElementById('uploadText');
    const uploadingSection = document.getElementById('uploadingSection');
    const continueButton = document.getElementById('continueButton');
    const fileInput = document.getElementById('fileUpload');

    form.addEventListener('submit', () => {

        if (!fileInput || fileInput.files.length === 0) {
            return;
        }

        fileSection.hidden = true;
        uploadText.hidden = true;

        uploadingSection.style.display = 'block';

        continueButton.disabled = true;
        continueButton.textContent = 'Uploading...';
    }, { once: true });
});