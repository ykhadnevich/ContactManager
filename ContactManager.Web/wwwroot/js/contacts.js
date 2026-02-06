// Contact Manager - Client-Side JavaScript
// Handles filtering, sorting, and inline editing

(function() {
    'use strict';

    let originalData = [];
    let isEditMode = false;
    let currentEditRow = null;

    // Initialize on page load
    document.addEventListener('DOMContentLoaded', function() {
        loadOriginalData();
        setupEventListeners();
    });

    function loadOriginalData() {
        const tbody = document.getElementById('contactsTableBody');
        if (!tbody) return;

        originalData = Array.from(tbody.querySelectorAll('tr')).map(row => {
            const marriedText = row.querySelector('[data-field="married"]')?.textContent.trim() || 'No';

            return {
                id: row.getAttribute('data-id'),
                name: row.querySelector('[data-field="name"]')?.textContent || '',
                dateOfBirth: row.querySelector('[data-field="dateOfBirth"]')?.textContent || '',
                age: parseInt(row.querySelector('.age-cell')?.textContent || '0'),
                married: marriedText === 'Yes', // Convert "Yes"/"No" to boolean
                phone: row.querySelector('[data-field="phone"]')?.textContent || '',
                salary: parseCurrency(row.querySelector('[data-field="salary"]')?.textContent || '0'),
                element: row
            };
        });

        console.log('Loaded contacts:', originalData.length);
        console.log('Sample data:', originalData[0]);
    }

    function setupEventListeners() {
        // Search
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.addEventListener('input', filterAndSort);
        }

        // Married filter
        const marriedFilter = document.getElementById('marriedFilter');
        if (marriedFilter) {
            marriedFilter.addEventListener('change', filterAndSort);
        }

        // Sort
        const sortSelect = document.getElementById('sortSelect');
        const sortOrder = document.getElementById('sortOrder');
        if (sortSelect) sortSelect.addEventListener('change', filterAndSort);
        if (sortOrder) sortOrder.addEventListener('change', filterAndSort);

        // Edit and Delete buttons (event delegation)
        const tbody = document.getElementById('contactsTableBody');
        if (tbody) {
            tbody.addEventListener('click', function(e) {
                const editBtn = e.target.closest('.edit-btn');
                const deleteBtn = e.target.closest('.delete-btn');

                if (editBtn) {
                    handleEdit(editBtn);
                } else if (deleteBtn) {
                    handleDelete(deleteBtn);
                }
            });
        }
    }

    function filterAndSort() {
        const searchTerm = document.getElementById('searchInput')?.value.toLowerCase() || '';
        const marriedFilterValue = document.getElementById('marriedFilter')?.value || '';
        const sortBy = document.getElementById('sortSelect')?.value || 'name';
        const sortOrder = document.getElementById('sortOrder')?.value || 'asc';

        console.log('Filtering with:', { searchTerm, marriedFilterValue, sortBy, sortOrder });

        // Filter
        let filtered = originalData.filter(contact => {
            // Search filter
            const matchesSearch = !searchTerm ||
                contact.name.toLowerCase().includes(searchTerm) ||
                contact.phone.toLowerCase().includes(searchTerm);

            // Married filter - FIXED!
            let matchesMarried = true;
            if (marriedFilterValue === 'true') {
                matchesMarried = contact.married === true;
            } else if (marriedFilterValue === 'false') {
                matchesMarried = contact.married === false;
            }
            // If marriedFilterValue is empty string, matchesMarried stays true (show all)

            return matchesSearch && matchesMarried;
        });

        console.log('Filtered results:', filtered.length);

        // Sort
        filtered.sort((a, b) => {
            let aVal = a[sortBy];
            let bVal = b[sortBy];

            // Handle date comparison
            if (sortBy === 'dateOfBirth') {
                aVal = new Date(aVal);
                bVal = new Date(bVal);
            }

            let comparison = 0;
            if (aVal > bVal) comparison = 1;
            if (aVal < bVal) comparison = -1;

            return sortOrder === 'asc' ? comparison : -comparison;
        });

        // Update table
        const tbody = document.getElementById('contactsTableBody');
        tbody.innerHTML = '';
        filtered.forEach(contact => tbody.appendChild(contact.element));

        // Update count
        updateContactCount(filtered.length);
    }

    function updateContactCount(count) {
        const countBadge = document.getElementById('contactCount');
        if (countBadge) {
            countBadge.textContent = count;
        }
    }

    function handleEdit(button) {
        const row = button.closest('tr');

        if (isEditMode && currentEditRow === row) {
            // Save changes
            saveEdit(row);
        } else {
            // Cancel any existing edit
            if (isEditMode && currentEditRow) {
                cancelEdit(currentEditRow);
            }

            // Start editing this row
            startEdit(row, button);
        }
    }

    function startEdit(row, button) {
        isEditMode = true;
        currentEditRow = row;

        // Change button to save
        button.innerHTML = '<i class="bi bi-check"></i>';
        button.classList.remove('btn-success');
        button.classList.add('btn-primary');
        button.title = 'Save';

        // Make cells editable
        const editableCells = row.querySelectorAll('.editable');
        editableCells.forEach(cell => {
            const field = cell.getAttribute('data-field');
            const type = cell.getAttribute('data-type');
            const value = cell.textContent.trim();

            let input;

            if (type === 'date') {
                input = document.createElement('input');
                input.type = 'date';
                input.className = 'form-control form-control-sm';
                input.value = value;
            } else if (type === 'boolean') {
                input = document.createElement('select');
                input.className = 'form-select form-select-sm';
                input.innerHTML = '<option value="true">Yes</option><option value="false">No</option>';
                input.value = value === 'Yes' ? 'true' : 'false';
            } else if (type === 'number') {
                input = document.createElement('input');
                input.type = 'number';
                input.step = '0.01';
                input.className = 'form-control form-control-sm';
                input.value = parseCurrency(value);
            } else {
                input = document.createElement('input');
                input.type = 'text';
                input.className = 'form-control form-control-sm';
                input.value = value;
            }

            input.setAttribute('data-original', value);
            cell.textContent = '';
            cell.appendChild(input);
        });
        
        const deleteBtn = row.querySelector('.delete-btn');
        if (deleteBtn) deleteBtn.disabled = true;
    }

    function saveEdit(row) {
        const id = row.getAttribute('data-id');
        const cells = row.querySelectorAll('.editable');

        const data = { id: parseInt(id) };

        cells.forEach(cell => {
            const field = cell.getAttribute('data-field');
            const input = cell.querySelector('input, select');

            if (input) {
                let value = input.value;

                if (field === 'married') {
                    data[field] = value === 'true';
                } else if (field === 'salary') {
                    data[field] = parseFloat(value);
                } else if (field === 'dateOfBirth') {
                    data[field] = value;
                } else {
                    data[field] = value;
                }
            }
        });
        
        fetch('/Contacts/Update', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(data)
        })
            .then(response => response.json())
            .then(result => {
                if (result.success) {
                    showMessage('success', result.message);
                    location.reload(); // Refresh to update age and data
                } else {
                    showMessage('danger', result.message);
                    cancelEdit(row);
                }
            })
            .catch(error => {
                showMessage('danger', 'An error occurred while saving.');
                console.error('Error:', error);
                cancelEdit(row);
            });
    }

    function finishEdit(row, data) {
        const cells = row.querySelectorAll('.editable');

        cells.forEach(cell => {
            const field = cell.getAttribute('data-field');
            const input = cell.querySelector('input, select');

            if (input) {
                let displayValue = data[field];

                if (field === 'married') {
                    displayValue = data[field] ? 'Yes' : 'No';
                } else if (field === 'salary') {
                    displayValue = formatCurrency(data[field]);
                } else if (field === 'dateOfBirth') {
                    displayValue = data[field];
                }

                cell.textContent = displayValue;
            }
        });
        
        const editBtn = row.querySelector('.edit-btn');
        editBtn.innerHTML = '<i class="bi bi-pencil"></i>';
        editBtn.classList.remove('btn-primary');
        editBtn.classList.add('btn-success');
        editBtn.title = 'Edit';

        // Enable delete button
        const deleteBtn = row.querySelector('.delete-btn');
        if (deleteBtn) deleteBtn.disabled = false;

        isEditMode = false;
        currentEditRow = null;

        // Update original data
        loadOriginalData();
    }

    function cancelEdit(row) {
        const cells = row.querySelectorAll('.editable');

        cells.forEach(cell => {
            const input = cell.querySelector('input, select');
            if (input) {
                const original = input.getAttribute('data-original');
                cell.textContent = original;
            }
        });

        // Reset button
        const editBtn = row.querySelector('.edit-btn');
        editBtn.innerHTML = '<i class="bi bi-pencil"></i>';
        editBtn.classList.remove('btn-primary');
        editBtn.classList.add('btn-success');
        editBtn.title = 'Edit';

        // Enable delete button
        const deleteBtn = row.querySelector('.delete-btn');
        if (deleteBtn) deleteBtn.disabled = false;

        isEditMode = false;
        currentEditRow = null;
    }

    function handleDelete(button) {
        const row = button.closest('tr');
        const id = row.getAttribute('data-id');
        const name = row.querySelector('[data-field="name"]').textContent;

        if (!confirm(`Are you sure you want to delete ${name}?`)) {
            return;
        }
        
        fetch(`/Contacts/Delete?id=${id}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        })
            .then(response => response.json())
            .then(result => {
                if (result.success) {
                    showMessage('success', result.message);
                    row.remove();
                    loadOriginalData();
                    filterAndSort();
                } else {
                    showMessage('danger', result.message);
                }
            })
            .catch(error => {
                showMessage('danger', 'An error occurred while deleting.');
                console.error('Error:', error);
            });
    }

    function showMessage(type, message) {
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
        alertDiv.role = 'alert';
        alertDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        const container = document.querySelector('.container-fluid');
        const firstElement = container.querySelector('.row');
        if (firstElement) {
            container.insertBefore(alertDiv, firstElement);
        } else {
            container.insertBefore(alertDiv, container.firstChild);
        }

        setTimeout(() => alertDiv.remove(), 5000);
    }

    function parseCurrency(value) {
        if (typeof value === 'number') return value;
        return parseFloat(value.replace(/[^0-9.-]+/g, '')) || 0;
    }

    function formatCurrency(value) {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD'
        }).format(value);
    }
})();