
document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('uploadForm');
if (!form) return;

const fileSection = document.getElementById('fileSection');
const uploadingSection = document.getElementById('uploadingSection');
const continueButton = document.getElementById('continueButton');
const fileInput = document.getElementById('File');

    form.addEventListener('submit', () => {

        if (!fileInput || fileInput.files.length === 0) {
            return;
        }

fileSection.hidden = true;
uploadingSection.style.display = 'block';

if (continueButton) {
    continueButton.disabled = true;
continueButton.textContent = 'Uploading...';
        }
    }, {once: true });
});
