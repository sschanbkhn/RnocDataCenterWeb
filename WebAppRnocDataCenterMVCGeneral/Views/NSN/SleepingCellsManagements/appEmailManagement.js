



// Global variables
let emails = [];
let editIndex = null;
let sortAsc = true;
let sortByActive = false;
let sortActiveAsc = true;
let sortByEmail = false;
let sortEmailAsc = true;
let currentPage = 1;
let rowsPerPage = 10;

// Show success toast
function showSuccessToast(message) {
    const toast = `
        <div class="toast-container position-fixed bottom-0 end-0 p-3">
            <div class="toast show" role="alert">
                <div class="toast-header bg-success text-white">
                    <i class="fas fa-check-circle me-2"></i>
                    <strong class="me-auto">Thành công</strong>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
                </div>
                <div class="toast-body">${message}</div>
            </div>
        </div>
    `;
    $('body').append(toast);
    setTimeout(() => $('.toast-container').remove(), 3000);
}

// Show loading state
function showLoading() {
    const tbody = $('#emailTableBody');
    tbody.html(`
        <tr>
            <td colspan="5" class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <div class="mt-2">Đang tải dữ liệu...</div>
            </td>
        </tr>
    `);
}

// Show error state
function showError(message) {
    const tbody = $('#emailTableBody');
    tbody.html(`
        <tr>
            <td colspan="5" class="text-center py-5 text-danger">
                <i class="fas fa-exclamation-triangle fa-2x mb-2"></i>
                <div>${message}</div>
                <button class="btn btn-outline-primary btn-sm mt-2" onclick="funGetDataFromAPIOutLookEmail()">
                    <i class="fas fa-redo"></i> Thử lại
                </button>
            </td>
        </tr>
    `);
}

// Render table function
function renderTable() {
    const keyword = $('#search').val().toLowerCase();
    const tbody = $('#emailTableBody');
    
    // If no data, show empty state
    if (!emails || emails.length === 0) {
        tbody.html(`
            <tr>
                <td colspan="5" class="text-center py-5 text-muted">
                    <i class="fas fa-inbox fa-2x mb-2"></i>
                    <div>Chưa có dữ liệu email</div>
                </td>
            </tr>
        `);
        $('#pagination').empty();
        return;
    }

    tbody.empty();

    // Filter emails based on search
    let filtered = emails
        .map((e, index) => ({ ...e, _originalIndex: index }))
        .filter(e => e.email.toLowerCase().includes(keyword));

    // Show no results if search doesn't match
    if (filtered.length === 0 && keyword) {
        tbody.html(`
            <tr>
                <td colspan="5" class="text-center py-5 text-muted">
                    <i class="fas fa-search fa-2x mb-2"></i>
                    <div>Không tìm thấy email phù hợp với "${keyword}"</div>
                </td>
            </tr>
        `);
        $('#pagination').empty();
        return;
    }

    // Apply sorting
    if ($('#sortSttEmail').data('sorted')) {
        filtered.sort((a, b) => sortAsc ? a.sttEmail - b.sttEmail : b.sttEmail - a.sttEmail);
    } else if (sortByActive) {
        filtered.sort((a, b) => sortActiveAsc
            ? (a.active === b.active ? 0 : a.active ? -1 : 1)
            : (a.active === b.active ? 0 : a.active ? 1 : -1));
    } else if (sortByEmail) {
        filtered.sort((a, b) => {
            let emailA = a.email.toLowerCase();
            let emailB = b.email.toLowerCase();
            return sortEmailAsc ? emailA.localeCompare(emailB) : emailB.localeCompare(emailA);
        });
    } else {
        // Default sort by STT
        filtered.sort((a, b) => a.sttEmail - b.sttEmail);
    }

    // Pagination
    const start = (currentPage - 1) * rowsPerPage;
    const end = start + rowsPerPage;
    const paginated = filtered.slice(start, end);

    // Render rows
    paginated.forEach((e) => {
        const realIndex = e._originalIndex;
        tbody.append(`
            <tr>
                <td><div class="stt-cell">${e.sttEmail}</div></td>
                <td><div class="email-cell">${e.email}</div></td>
                <td><div class="password-cell">${'●'.repeat(Math.min(e.password.length, 8))}</div></td>
                <td>
                    <span class="badge ${e.active ? 'bg-success' : 'bg-danger'}">
                        ${e.active ? '<i class="fas fa-check-circle"></i> Active' : '<i class="fas fa-times-circle"></i> Inactive'}
                    </span>
                </td>
                <td>
                    <div class="action-buttons">
                        <button class="icon-btn edit" onclick="editEmail(${realIndex})" title="Sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="icon-btn delete" onclick="deleteEmail(${realIndex})" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `);
    });

    renderPagination(filtered.length);
}

// Render pagination
function renderPagination(totalItems) {
    const totalPages = Math.ceil(totalItems / rowsPerPage);
    const pagination = $('#pagination');
    pagination.empty();

    // Previous button
    if (currentPage > 1) {
        pagination.append(`
            <button class="btn btn-outline-primary" onclick="goToPage(${currentPage - 1})">‹</button>
        `);
    }

    // Page numbers
    for (let i = 1; i <= totalPages; i++) {
        if (i === currentPage) {
            pagination.append(`
                <button class="btn btn-primary" onclick="goToPage(${i})">${i}</button>
            `);
        } else {
            pagination.append(`
                <button class="btn btn-outline-primary" onclick="goToPage(${i})">${i}</button>
            `);
        }
    }

    // Next button
    if (currentPage < totalPages) {
        pagination.append(`
            <button class="btn btn-outline-primary" onclick="goToPage(${currentPage + 1})">›</button>
        `);
    }
}

// Go to page function
function goToPage(page) {
    currentPage = page;
    renderTable();
}

// Clear form function
function clearForm() {
    $('#sttEmail').val('');
    $('#email').val('');
    $('#password').val('');
    $('#active').prop('checked', false);
    editIndex = null;
}

// Edit email function
function editEmail(index) {
    console.log("Index được truyền vào là:", index);
    const e = emails[index];
    if (!e) {
        alert("Không tìm thấy email cần sửa!");
        return;
    }

    $('#sttEmail').val(e.sttEmail);
    $('#email').val(e.email);
    $('#password').val(e.password);
    $('#active').prop('checked', e.active);

    editIndex = index;
    $('#modalTitle').html('<i class="fas fa-edit me-2"></i>Sửa Email');
    
    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('emailModal'));
    modal.show();
}

// Delete email function
function deleteEmail(index) {
    if (confirm('Bạn có chắc chắn muốn xóa email này?')) {
        const email = emails[index];
        
        const url = `https://localhost:7232/API/Systems/NSN/SleepingCell/ControllerOutLook/Delete?intID_Destro=${email.id}`;
		
        // Show loading state for this row
        $(`button[onclick="deleteEmail(${index})"]`).prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i>');
        
        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Delete API response:', data);
            emails.splice(index, 1);
            
            // Adjust current page if needed
            const totalPages = Math.ceil(emails.length / rowsPerPage);
            if (currentPage > totalPages && totalPages > 0) {
                currentPage = totalPages;
            }
            
            renderTable();
            
            // Show success message
            const toast = `
                <div class="toast-container position-fixed bottom-0 end-0 p-3">
                    <div class="toast show" role="alert">
                        <div class="toast-header bg-success text-white">
                            <i class="fas fa-check-circle me-2"></i>
                            <strong class="me-auto">Thành công</strong>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
                        </div>
                        <div class="toast-body">Email đã được xóa thành công!</div>
                    </div>
                </div>
            `;
            $('body').append(toast);
            setTimeout(() => $('.toast-container').remove(), 3000);
        })
        .catch(error => {
            console.error('Lỗi khi gọi API Delete:', error);
            alert('Lỗi khi xóa email: ' + error.message);
            
            // Re-enable button
            $(`button[onclick="deleteEmail(${index})"]`).prop('disabled', false).html('<i class="fas fa-trash"></i>');
        });
    }
}

// Get data from API function
function funGetDataFromAPIOutLookEmail() {
    showLoading();
    

	 const url =   "https://localhost:7232/API/Systems/NSN/SleepingCell/ControllerOutLook/GetList";
    fetch(url, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        console.log("API Response:", data);
        
        if (data && Array.isArray(data.objfunCellFaultsOutLookReadGetList)) {
            emails = data.objfunCellFaultsOutLookReadGetList.map(item => ({
                sttEmail: item.sttEmail,
                email: item.email,
                password: item.password,
                id: item.id,
                active: !!item.active
            }));
            
            console.log("Mapped emails:", emails);
            renderTable();
        } else {
            throw new Error("Dữ liệu API không hợp lệ hoặc rỗng");
        }
    })
    .catch(error => {
        console.error('Lỗi khi gọi API GetList:', error);
        showError(`Lỗi khi tải dữ liệu: ${error.message}`);
    });
}

// Document ready function
$(document).ready(function() {
    // Initialize data
    funGetDataFromAPIOutLookEmail();

    // Add email button click
    $('#addEmailBtn').click(() => {
        clearForm();
        $('#modalTitle').html('<i class="fas fa-plus-circle me-2"></i>Thêm Email');
        const modal = new bootstrap.Modal(document.getElementById('emailModal'));
        modal.show();
    });

    // Save email button click
    $('#saveEmail').click(() => {
        const sttVal = Number($('#sttEmail').val());
        const emailVal = $('#email').val().trim();
        const passwordVal = $('#password').val().trim();
        const isActive = $('#active').is(':checked');

        // Validation
        if (!Number.isInteger(sttVal) || sttVal < 1) {
            alert('STT Email phải là số nguyên dương');
            return;
        }

        if (!emailVal) {
            alert('Email không được để trống');
            return;
        }

        if (!passwordVal) {
            alert('Password không được để trống');
            return;
        }

        // Check for duplicate STT (except when editing)
        if (editIndex === null && emails.some(e => e.sttEmail === sttVal)) {
            alert('STT Email đã tồn tại!');
            return;
        }

        // Check for duplicate email (except when editing)
        if (editIndex === null && emails.some(e => e.email === emailVal)) {
            alert('Email đã tồn tại!');
            return;
        }

        // Check if only one email can be active
        if (isActive) {
            const existingActiveIndex = emails.findIndex((e, i) => e.active && i !== editIndex);
            if (existingActiveIndex !== -1) {
                alert(`Chỉ được phép có một email active.\nEmail hiện tại đang active: ${emails[existingActiveIndex].email}`);
                return;
            }
        }

        const newEmail = {
            sttEmail: sttVal,
            email: emailVal,
            password: passwordVal,
            active: isActive
        };

        if (editIndex !== null) {
            // Update existing email
            const idBufferForMark = emails[editIndex].id;
            
            const url = `https://localhost:7232/API/Systems/NSN/SleepingCell/ControllerOutLook/Update?intID=${idBufferForMark}&intSTT=${newEmail.sttEmail}&strOutLookEmail=${newEmail.email}&strOutLookPass=${newEmail.password}&strActiveOutlookEmail=${newEmail.active}`;

            // Disable save button and show loading
            $('#saveEmail').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Đang lưu...');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('Update API response:', data);
                
                emails[editIndex] = {
                    ...newEmail,
                    id: idBufferForMark
                };
                
                bootstrap.Modal.getInstance(document.getElementById('emailModal')).hide();
                renderTable();
                
                // Show success toast
                showSuccessToast('Email đã được cập nhật thành công!');
            })
            .catch(error => {
                console.error('Lỗi khi gọi API Update:', error);
                alert('Lỗi khi cập nhật email: ' + error.message);
            })
            .finally(() => {
                // Re-enable save button
                $('#saveEmail').prop('disabled', false).html('<i class="fas fa-save me-2"></i>Lưu');
            });
            
        } else {
            // Add new email
            const url = "https://localhost:7232/API/Systems/NSN/SleepingCell/ControllerOutLook/Insert";

            // Disable save button and show loading
            $('#saveEmail').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Đang lưu...');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(newEmail)
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('Insert API response:', data);
                
                // Refresh data from API to get the new ID
                funGetDataFromAPIOutLookEmail();
                
                bootstrap.Modal.getInstance(document.getElementById('emailModal')).hide();
                
                // Show success toast
                showSuccessToast('Email đã được thêm thành công!');
            })
            .catch(error => {
                console.error('Lỗi khi gọi API Insert:', error);
                alert('Lỗi khi thêm email: ' + error.message);
            })
            .finally(() => {
                // Re-enable save button
                $('#saveEmail').prop('disabled', false).html('<i class="fas fa-save me-2"></i>Lưu');
            });
        }
    });

    // Search input event
    $('#search').on('input', function() {
        currentPage = 1; // Reset to first page when searching
        renderTable();
    });

    // Rows per page change event
    $('#rowsPerPage').change(function() {
        rowsPerPage = parseInt($(this).val());
        currentPage = 1;
        renderTable();
    });

    // Sort by STT Email
    $('#sortSttEmail').click(() => {
        $('#sortSttEmail').data('sorted', true);
        sortByActive = false;
        sortByEmail = false;
        sortAsc = !sortAsc;
        
        // Update icon
        const icon = sortAsc ? 'fa-sort-numeric-down' : 'fa-sort-numeric-up';
        $('#sortSttEmail').html(`<i class="fas ${icon}"></i> STT Email`);
        
        renderTable();
    });

    // Sort by Email
    $('#sortEmail').click(function() {
        sortByEmail = true;
        sortByActive = false;
        $('#sortSttEmail').data('sorted', false);
        sortEmailAsc = !sortEmailAsc;

        // Update icon
        const icon = sortEmailAsc ? 'fa-sort-alpha-down' : 'fa-sort-alpha-up';
        $('#sortEmail').html(`<i class="fas ${icon}"></i> Email Address`);

        renderTable();
    });

    // Sort by Active status
    $('#sortActive').click(() => {
        sortByActive = true;
        sortByEmail = false;
        $('#sortSttEmail').data('sorted', false);
        sortActiveAsc = !sortActiveAsc;
        
        // Update icon
        const icon = sortActiveAsc ? 'fa-toggle-on' : 'fa-toggle-off';
        $('#sortActive').html(`<i class="fas ${icon}"></i> Trạng Thái`);
        
        renderTable();
    });

    // Modal close event to clear form
    $('#emailModal').on('hidden.bs.modal', function() {
        clearForm();
    });
});