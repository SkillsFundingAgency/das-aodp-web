document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('rolloverForm');
    const allSection = document.getElementById('allSection');
    const loadingContainer = document.getElementById('loadingContainer');

    const day = document.getElementById('maxApprovalEndDateDay');
    const month = document.getElementById('maxApprovalEndDateMonth');
    const year = document.getElementById('maxApprovalEndDateYear');

    loadingContainer.style.display = 'none';

    form.addEventListener('submit', () => {

        if (!day.value.trim() || !month.value.trim() || !year.value.trim()) {
            return;
        }

        const d = Number(day.value);
        const m = Number(month.value);
        const y = Number(year.value);

        if (!Number.isInteger(d) || !Number.isInteger(m) || !Number.isInteger(y)) {
            return;
        }

        allSection.style.display = 'none';
        loadingContainer.style.display = 'block';

    }, { once: true });
});