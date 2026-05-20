/**
 * Vehicle Parts System — Client-side JavaScript
 * CS6004NT Group Coursework
 */

// ── Auto-dismiss alerts after 4 seconds ──────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    const alerts = document.querySelectorAll('.alert.alert-success, .alert.alert-danger');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 4000);
    });
});

// ── Confirm delete dialogs ────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-confirm]').forEach(function (el) {
        el.addEventListener('click', function (e) {
            if (!confirm(el.dataset.confirm || 'Are you sure?')) {
                e.preventDefault();
            }
        });
    });
});

// ── Dynamic invoice item rows ─────────────────────────────────────────────────
function addRow() {
    const container = document.getElementById('items');
    if (!container) return;
    const row = container.querySelector('.item-row');
    if (!row) return;
    const clone = row.cloneNode(true);
    clone.querySelectorAll('input').forEach(i => i.value = '');
    clone.querySelectorAll('select').forEach(s => s.selectedIndex = 0);
    container.appendChild(clone);
}

function removeRow(btn) {
    const rows = document.querySelectorAll('.item-row');
    if (rows.length > 1) btn.closest('.item-row').remove();
}

// ── Real-time invoice total calculator ───────────────────────────────────────
document.addEventListener('input', function (e) {
    if (e.target.matches('.item-qty, .item-price')) {
        updateInvoiceTotal();
    }
});

function updateInvoiceTotal() {
    let total = 0;
    document.querySelectorAll('.item-row').forEach(function (row) {
        const qty   = parseFloat(row.querySelector('.item-qty')?.value  || 0);
        const price = parseFloat(row.querySelector('.item-price')?.value || 0);
        total += qty * price;
    });
    const totalEl = document.getElementById('invoiceTotal');
    if (totalEl) {
        const discount = total > 5000 ? total * 0.10 : 0;
        totalEl.textContent = 'Rs. ' + (total - discount).toFixed(2);
        const discountEl = document.getElementById('invoiceDiscount');
        if (discountEl) {
            discountEl.textContent = discount > 0
                ? '10% Loyalty Discount: -Rs. ' + discount.toFixed(2)
                : '';
        }
    }
}

// ── Tooltip initialization ────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltips.forEach(el => new bootstrap.Tooltip(el));
});
