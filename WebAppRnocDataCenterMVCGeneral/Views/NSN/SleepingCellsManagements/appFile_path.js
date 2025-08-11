// Global variables
let ossConfigs = [];
let editIndex = null;
let sortAsc = true;
let sortByActive = false;
let sortActiveAsc = true;
let sortByOss = false;
let sortOssAsc = true;
let sortByUser = false;
let sortUserAsc = true;
let sortByHost = false;
let sortHostAsc = true;
let sortByPort = false;
let sortPortAsc = true;
let sortByFilePath = false;
let sortFilePathAsc = true;
let sortByProtocol = false;
let sortProtocolAsc = true;
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
    const tbody = $('#ossTableBody');
    tbody.html(`
        <tr>
            <td colspan="10" class="text-center py-5">
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
    const tbody = $('#ossTableBody');
    tbody.html(`
        <tr>
            <td colspan="10" class="text-center py-5 text-danger">
                <i class="fas fa-exclamation-triangle fa-2x mb-2"></i>
                <div>${message}</div>
                <button class="btn btn-outline-primary btn-sm mt-2" onclick="funGetDataFromAPIFilePath()">
                    <i class="fas fa-redo"></i> Thử lại
                </button>
            </td>
        </tr>
    `);
}

// Render table function
function renderTable() {
    const keyword = $('#search').val().toLowerCase();
    const tbody = $('#ossTableBody');
    
    // If no data, show empty state
    if (!ossConfigs || ossConfigs.length === 0) {
        tbody.html(`
            <tr>
                <td colspan="10" class="text-center py-5 text-muted">
                    <i class="fas fa-server fa-2x mb-2"></i>
                    <div>Chưa có dữ liệu cấu hình OSS</div>
                </td>
            </tr>
        `);
        $('#pagination').empty();
        return;
    }

    tbody.empty();

    // Filter configs based on search
    let filtered = ossConfigs
        .map((e, index) => ({ ...e, _originalIndex: index }))
        .filter(e => 
            e.oss.toLowerCase().includes(keyword) ||
            e.username.toLowerCase().includes(keyword) ||
            e.host.toLowerCase().includes(keyword) ||
            e.protocol.toLowerCase().includes(keyword) ||
            e.filepath.toLowerCase().includes(keyword)
        );

    // Show no results if search doesn't match
    if (filtered.length === 0 && keyword) {
        tbody.html(`
            <tr>
                <td colspan="10" class="text-center py-5 text-muted">
                    <i class="fas fa-search fa-2x mb-2"></i>
                    <div>Không tìm thấy cấu hình phù hợp với "${keyword}"</div>
                </td>
            </tr>
        `);
        $('#pagination').empty();
        return;
    }

    // Apply sorting
    if ($('#sortStt').data('sorted')) {
        filtered.sort((a, b) => sortAsc ? a.sttFilePath - b.sttFilePath : b.sttFilePath - a.sttFilePath);
    } else if (sortByOss) {
        filtered.sort((a, b) => {
            let ossA = a.oss.toLowerCase();
            let ossB = b.oss.toLowerCase();
            return sortOssAsc ? ossA.localeCompare(ossB) : ossB.localeCompare(ossA);
        });
    } else if (sortByUser) {
        filtered.sort((a, b) => {
            let userA = a.username.toLowerCase();
            let userB = b.username.toLowerCase();
            return sortUserAsc ? userA.localeCompare(userB) : userB.localeCompare(userA);
        });
    } else if (sortByHost) {
        filtered.sort((a, b) => {
            let hostA = a.host.toLowerCase();
            let hostB = b.host.toLowerCase();
            return sortHostAsc ? hostA.localeCompare(hostB) : hostB.localeCompare(hostA);
        });
    } else if (sortByPort) {
        filtered.sort((a, b) => sortPortAsc ? a.port - b.port : b.port - a.port);
    } else if (sortByFilePath) {
        filtered.sort((a, b) => {
            let pathA = a.filepath.toLowerCase();
            let pathB = b.filepath.toLowerCase();
            return sortFilePathAsc ? pathA.localeCompare(pathB) : pathB.localeCompare(pathA);
        });
    } else if (sortByProtocol) {
        filtered.sort((a, b) => {
            let protocolA = a.protocol.toLowerCase();
            let protocolB = b.protocol.toLowerCase();
            return sortProtocolAsc ? protocolA.localeCompare(protocolB) : protocolB.localeCompare(protocolA);
        });
    } else if (sortByActive) {
        filtered.sort((a, b) => sortActiveAsc
            ? (a.active === b.active ? 0 : a.active ? -1 : 1)
            : (a.active === b.active ? 0 : a.active ? 1 : -1));
    } else {
        // Default sort by STT
        filtered.sort((a, b) => a.sttFilePath - b.sttFilePath);
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
                <td><div class="stt-cell">${e.sttFilePath}</div></td>
                <td><div class="oss-cell">${e.oss}</div></td>
                <td><div class="user-cell">${e.user}</div></td>
                <td><div class="password-cell">${'●'.repeat(Math.min(e.password.length, 8))}</div></td>
                <td><div class="host-cell">${e.host}</div></td>
                <td><div class="port-cell">${e.port}</div></td>
                <td><div class="filepath-cell">${e.filePath}</div></td>
                <td><div class="protocol-cell">${e.protocol}</div></td>
                <td>
                    <span class="badge ${e.active ? 'bg-success' : 'bg-danger'}">
                        ${e.active ? '<i class="fas fa-check-circle"></i> Active' : '<i class="fas fa-times-circle"></i> Inactive'}
                    </span>
                </td>
                <td>
                    <div class="action-buttons">
                        <button class="icon-btn edit" onclick="editOssConfig(${realIndex})" title="Sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="icon-btn delete" onclick="deleteOssConfig(${realIndex})" title="Xóa">
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
    $('#sttFilePath').val('');
    $('#oss').val('');
    $('#username').val('');
    $('#password').val('');
    $('#host').val('');
    $('#port').val('');
    $('#filePath').val('');
    $('#protocol').val('');
    $('#active').prop('checked', false);
    editIndex = null;
}

// Edit OSS config function
function editOssConfig(index) {
    console.log("Index được truyền vào là:", index);
    const e = ossConfigs[index];
    if (!e) {
        alert("Không tìm thấy cấu hình cần sửa!");
        return;
    }

    $('#sttFilePath').val(e.sttFilePath);
    $('#oss').val(e.oss);
    $('#username').val(e.username);
    $('#password').val(e.password);
    $('#host').val(e.host);
    $('#port').val(e.port);
    $('#filePath').val(e.filepath);
    $('#protocol').val(e.protocol);
    $('#active').prop('checked', e.active);

    editIndex = index;
    $('#modalTitle').html('<i class="fas fa-edit me-2"></i>Sửa Cấu Hình OSS');
    
    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('ossModal'));
    modal.show();
}

// Delete OSS config function
function deleteOssConfig(index) {
    if (confirm('Bạn có chắc chắn muốn xóa cấu hình OSS này?')) {
        const config = ossConfigs[index];
        
        const url = `https://localhost:7232/API/Systems/NSN/SleepingCell/ControllerFilePath/Delete?intID_Destro=${config.id}`;
		
        // Show loading state for this row
        $(`button[onclick="deleteOssConfig(${index})"]`).prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i>');
        
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
            ossConfigs.splice(index, 1);
            
            // Adjust current page if needed
            const totalPages = Math.ceil(ossConfigs.length / rowsPerPage);
            if (currentPage > totalPages && totalPages > 0) {
                currentPage = totalPages;
            }
            
            renderTable();
            
            // Show success message
            showSuccessToast('Cấu hình OSS đã được xóa thành công!');
        })
        .catch(error => {
            console.error('Lỗi khi gọi API Delete:', error);
            alert('Lỗi khi xóa cấu hình: ' + error.message);
            
            // Re-enable button
            $(`button[onclick="deleteOssConfig(${index})"]`).prop('disabled', false).html('<i class="fas fa-trash"></i>');
        });
    }
}

// Get data from API function
function funGetDataFromAPIFilePath() {
    showLoading();
    
    const url = "https://localhost:7232/API/Systems/NSN/SleepingCell/ControllerFilePath/GetList";
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
        // objfunSleeping_CellFilePathReadGetList la thay doi theo tung API
        if (data && Array.isArray(data.objfunSleeping_CellFilePathReadGetList)) {
            ossConfigs = data.objfunSleeping_CellFilePathReadGetList.map(item => ({
                sttFilePath: item.sttFilePath,
                oss: item.oss,
                username: item.username,
                password: item.password,
                host: item.host,
                port: item.port,
                filePath: item.filePath,
                protocol: item.protocol,
                id: item.id,
                active: !!item.active
            }));
            
            console.log("Mapped OSS configs:", ossConfigs);
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
    funGetDataFromAPIFilePath();

    // Add OSS config button click
    $('#addOssBtn').click(() => {
        clearForm();
        $('#modalTitle').html('<i class="fas fa-plus-circle me-2"></i>Thêm Cấu Hình OSS');
        const modal = new bootstrap.Modal(document.getElementById('ossModal'));
        modal.show();
    });

    // Save OSS config button click
    $('#saveOss').click(() => {
        const sttVal = Number($('#sttFilePath').val());
        const ossVal = $('#oss').val().trim();
        const userVal = $('#username').val().trim();
        const passwordVal = $('#password').val().trim();
        const hostVal = $('#host').val().trim();
        const portVal = Number($('#port').val());
        const filePathVal = $('#filePath').val().trim();
        const protocolVal = $('#protocol').val().trim();
        const isActive = $('#active').is(':checked');

        // Validation
        if (!Number.isInteger(sttVal) || sttVal < 1) {
            alert('STT phải là số nguyên dương');
            return;
        }

        if (!ossVal) {
            alert('OSS không được để trống');
            return;
        }

        if (!userVal) {
            alert('User không được để trống');
            return;
        }

        if (!passwordVal) {
            alert('Password không được để trống');
            return;
        }

        if (!hostVal) {
            alert('Host không được để trống');
            return;
        }

        if (!Number.isInteger(portVal) || portVal < 1 || portVal > 65535) {
            alert('Port phải là số nguyên từ 1 đến 65535');
            return;
        }

        if (!filePathVal) {
            alert('File Path không được để trống');
            return;
        }

        if (!protocolVal) {
            alert('Protocol không được để trống');
            return;
        }

        // Check for duplicate STT (except when editing)
        if (editIndex === null && ossConfigs.some(e => e.sttFilePath === sttVal)) {
            alert('STT đã tồn tại!');
            return;
        }

        // Check for duplicate OSS (except when editing)
        if (editIndex === null && ossConfigs.some(e => e.oss === ossVal)) {
            alert('OSS đã tồn tại!');
            return;
        }

        // Check if only one config can be active
        if (isActive) {
            const existingActiveIndex = ossConfigs.findIndex((e, i) => e.active && i !== editIndex);
            if (existingActiveIndex !== -1) {
                alert(`Chỉ được phép có một cấu hình OSS active.\nCấu hình hiện tại đang active: ${ossConfigs[existingActiveIndex].oss}`);
                return;
            }
        }

        const newConfig = {
            sttFilePath: sttVal,
            oss: ossVal,
            username: userVal,
            password: passwordVal,
            host: hostVal,
            port: portVal,
            filePath: filePathVal,
            protocol: protocolVal,
            active: isActive
        };


        if (editIndex !== null) {
            // Update existing config
            const idBufferForMark = ossConfigs[editIndex].id;
            
            const url = `https://localhost:7232/API/Systems/NSN/SleepingCell/ControllerFilePath/Update?intID=${idBufferForMark}&intSTT=${newConfig.sttFilePath}&strOSS=${newConfig.oss}&strUser=${newConfig.username}&strPassword=${newConfig.password}&strHost=${newConfig.host}&intPort=${newConfig.port}&strFilePath=${newConfig.filepath}&strProtocol=${newConfig.protocol}&strActive=${newConfig.active}`;

            // Disable save button and show loading
            $('#saveOss').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Đang lưu...');

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
                
                ossConfigs[editIndex] = {
                    ...newConfig,
                    id: idBufferForMark
                };
                
                bootstrap.Modal.getInstance(document.getElementById('ossModal')).hide();
                renderTable();
                
                // Show success toast
                showSuccessToast('Cấu hình OSS đã được cập nhật thành công!');
            })
            .catch(error => {
                console.error('Lỗi khi gọi API Update:', error);
                alert('Lỗi khi cập nhật cấu hình: ' + error.message);
            })
            .finally(() => {
                // Re-enable save button
                $('#saveOss').prop('disabled', false).html('<i class="fas fa-save me-2"></i>Lưu');
            });
            
        } else {
            // Add new config
            const url = "https://localhost:7232/API/Systems/NSN/SleepingCell/ControllerFilePath/Insert";
						 
            // Disable save button and show loading
            $('#saveOss').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Đang lưu...');

            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(newConfig)
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
                funGetDataFromAPIFilePath();
                
                bootstrap.Modal.getInstance(document.getElementById('ossModal')).hide();
                
                // Show success toast
                showSuccessToast('Cấu hình OSS đã được thêm thành công!');
            })
            .catch(error => {
                console.error('Lỗi khi gọi API Insert:', error);
                alert('Lỗi khi thêm cấu hình: ' + error.message);
            })
            .finally(() => {
                // Re-enable save button
                $('#saveOss').prop('disabled', false).html('<i class="fas fa-save me-2"></i>Lưu');
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

    // Sort by STT
    $('#sortStt').click(() => {
        $('#sortStt').data('sorted', true);
        sortByOss = false;
        sortByUser = false;
        sortByHost = false;
        sortByPort = false;
        sortByFilePath = false;
        sortByProtocol = false;
        sortByActive = false;
        sortAsc = !sortAsc;
        
        // Update icon
        const icon = sortAsc ? 'fa-sort-numeric-down' : 'fa-sort-numeric-up';
        $('#sortStt').html(`<i class="fas ${icon}"></i> STT`);
        
        renderTable();
    });

    // Sort by OSS
    $('#sortOss').click(function() {
        sortByOss = true;
        sortByUser = false;
        sortByHost = false;
        sortByPort = false;
        sortByFilePath = false;
        sortByProtocol = false;
        sortByActive = false;
        $('#sortStt').data('sorted', false);
        sortOssAsc = !sortOssAsc;

        // Update icon
        const icon = sortOssAsc ? 'fa-sort-alpha-down' : 'fa-sort-alpha-up';
        $('#sortOss').html(`<i class="fas ${icon}"></i> OSS`);

        renderTable();
    });

    // Sort by User
    $('#sortUser').click(function() {
        sortByUser = true;
        sortByOss = false;
        sortByHost = false;
        sortByPort = false;
        sortByFilePath = false;
        sortByProtocol = false;
        sortByActive = false;
        $('#sortStt').data('sorted', false);
        sortUserAsc = !sortUserAsc;

        // Update icon
        const icon = sortUserAsc ? 'fa-sort-alpha-down' : 'fa-sort-alpha-up';
        $('#sortUser').html(`<i class="fas ${icon}"></i> USER`);

        renderTable();
    });

    // Sort by Host
    $('#sortHost').click(function() {
        sortByHost = true;
        sortByOss = false;
        sortByUser = false;
        sortByPort = false;
        sortByFilePath = false;
        sortByProtocol = false;
        sortByActive = false;
        $('#sortStt').data('sorted', false);
        sortHostAsc = !sortHostAsc;

        // Update icon
        const icon = sortHostAsc ? 'fa-sort-alpha-down' : 'fa-sort-alpha-up';
        $('#sortHost').html(`<i class="fas ${icon}"></i> HOST`);

        renderTable();
    });

    // Sort by Port
    $('#sortPort').click(function() {
        sortByPort = true;
        sortByOss = false;
        sortByUser = false;
        sortByHost = false;
        sortByFilePath = false;
        sortByProtocol = false;
        sortByActive = false;
        $('#sortStt').data('sorted', false);
        sortPortAsc = !sortPortAsc;

        // Update icon
        const icon = sortPortAsc ? 'fa-sort-numeric-down' : 'fa-sort-numeric-up';
        $('#sortPort').html(`<i class="fas ${icon}"></i> PORT`);

        renderTable();
    });

    // Sort by FilePath
    $('#sortFilePath').click(function() {
        sortByFilePath = true;
        sortByOss = false;
        sortByUser = false;
        sortByHost = false;
        sortByPort = false;
        sortByProtocol = false;
        sortByActive = false;
        $('#sortStt').data('sorted', false);
        sortFilePathAsc = !sortFilePathAsc;

        // Update icon
        const icon = sortFilePathAsc ? 'fa-sort-alpha-down' : 'fa-sort-alpha-up';
        $('#sortFilePath').html(`<i class="fas ${icon}"></i> FILEPATH`);

        renderTable();
    });

    // Sort by Protocol
    $('#sortProtocol').click(function() {
        sortByProtocol = true;
        sortByOss = false;
        sortByUser = false;
        sortByHost = false;
        sortByPort = false;
        sortByFilePath = false;
        sortByActive = false;
        $('#sortStt').data('sorted', false);
        sortProtocolAsc = !sortProtocolAsc;

        // Update icon
        const icon = sortProtocolAsc ? 'fa-sort-alpha-down' : 'fa-sort-alpha-up';
        $('#sortProtocol').html(`<i class="fas ${icon}"></i> PROTOCOL`);

        renderTable();
    });

    // Sort by Active status
    $('#sortActive').click(() => {
        sortByActive = true;
        sortByOss = false;
        sortByUser = false;
        sortByHost = false;
        sortByPort = false;
        sortByFilePath = false;
        sortByProtocol = false;
        $('#sortStt').data('sorted', false);
        sortActiveAsc = !sortActiveAsc;
        
        // Update icon
        const icon = sortActiveAsc ? 'fa-toggle-on' : 'fa-toggle-off';
        $('#sortActive').html(`<i class="fas ${icon}"></i> ACTIVE`);
        
        renderTable();
    });

    // Modal close event to clear form
    $('#ossModal').on('hidden.bs.modal', function() {
        clearForm();
    });
	
	// code tu file html
	//
	
	//
	//
	document.addEventListener('DOMContentLoaded', function() {
            // Add button functionality
            document.getElementById('addOssBtn').addEventListener('click', function() {
                document.getElementById('modalTitle').innerHTML = '<i class="fas fa-plus-circle me-2"></i>Thêm Cấu Hình OSS';
                // Clear all form fields
                document.getElementById('sttFilePath').value = '';
                document.getElementById('oss').value = '';
                document.getElementById('username').value = '';
                document.getElementById('password').value = '';
                document.getElementById('host').value = '';
                document.getElementById('port').value = '';
                document.getElementById('filePath').value = '';
                document.getElementById('protocol').value = '';
                document.getElementById('active').checked = false;
                
                // Show modal
                var modal = new bootstrap.Modal(document.getElementById('ossModal'));
                modal.show();
            });
			
			
			
			<!-- ========== PHẦN 16: JAVASCRIPT EDIT VÀ DELETE FUNCTIONS ========== -->
            // Edit button functionality
            document.querySelectorAll('.icon-btn.edit').forEach(function(btn) {
                btn.addEventListener('click', function() {
                    document.getElementById('modalTitle').innerHTML = '<i class="fas fa-edit me-2"></i>Chỉnh Sửa Cấu Hình OSS';
                    
                    // Get row data
                    var row = this.closest('tr');
                    var stt = row.querySelector('.stt-cell').textContent;
                    var oss = row.querySelector('.oss-cell').textContent;
                    var user = row.querySelector('.user-cell').textContent;
                    var host = row.querySelector('.host-cell').textContent;
                    var port = row.querySelector('.port-cell').textContent;
                    var filePath = row.querySelector('.filepath-cell').textContent;
                    var protocol = row.querySelector('.protocol-cell').textContent;
                    
                    // Populate form fields
                    document.getElementById('sttFilePath').value = stt;
                    document.getElementById('oss').value = oss;
                    document.getElementById('username').value = user;
                    document.getElementById('password').value = '';
                    document.getElementById('host').value = host;
                    document.getElementById('port').value = port;
                    document.getElementById('filePath').value = filePath;
                    document.getElementById('protocol').value = protocol;
                    
                    // Show modal
                    var modal = new bootstrap.Modal(document.getElementById('ossModal'));
                    modal.show();
                });
            });

            // Delete button functionality
            document.querySelectorAll('.icon-btn.delete').forEach(function(btn) {
                btn.addEventListener('click', function() {
                    if (confirm('Bạn có chắc chắn muốn xóa cấu hình OSS này?')) {
                        this.closest('tr').remove();
                    }
                });
            });
							
							
							
							
							<!-- ========== PHẦN 16: JAVASCRIPT EDIT VÀ DELETE FUNCTIONS ========== -->
            // Edit button functionality
            document.querySelectorAll('.icon-btn.edit').forEach(function(btn) {
                btn.addEventListener('click', function() {
                    document.getElementById('modalTitle').innerHTML = '<i class="fas fa-edit me-2"></i>Chỉnh Sửa Cấu Hình OSS';
                    
                    // Get row data
                    var row = this.closest('tr');
                    var stt = row.querySelector('.stt-cell').textContent;
                    var oss = row.querySelector('.oss-cell').textContent;
                    var user = row.querySelector('.user-cell').textContent;
                    var host = row.querySelector('.host-cell').textContent;
                    var port = row.querySelector('.port-cell').textContent;
                    var filePath = row.querySelector('.filepath-cell').textContent;
                    var protocol = row.querySelector('.protocol-cell').textContent;
                    
                    // Populate form fields
                    document.getElementById('sttFilePath').value = stt;
                    document.getElementById('oss').value = oss;
                    document.getElementById('username').value = user;
                    document.getElementById('password').value = '';
                    document.getElementById('host').value = host;
                    document.getElementById('port').value = port;
                    document.getElementById('filePath').value = filePath;
                    document.getElementById('protocol').value = protocol;
                    
                    // Show modal
                    var modal = new bootstrap.Modal(document.getElementById('ossModal'));
                    modal.show();
                });
            });

            // Delete button functionality
            document.querySelectorAll('.icon-btn.delete').forEach(function(btn) {
                btn.addEventListener('click', function() {
                    if (confirm('Bạn có chắc chắn muốn xóa cấu hình OSS này?')) {
                        this.closest('tr').remove();
                    }
                });
            });
			
			
			
			<!-- ========== PHẦN 17: JAVASCRIPT SEARCH VÀ SAVE FUNCTIONS ========== -->
            // Search functionality
            document.getElementById('search').addEventListener('input', function() {
                var searchTerm = this.value.toLowerCase();
                var rows = document.querySelectorAll('#ossTableBody tr');
                
                rows.forEach(function(row) {
                    var oss = row.querySelector('.oss-cell').textContent.toLowerCase();
                    var user = row.querySelector('.user-cell').textContent.toLowerCase();
                    var host = row.querySelector('.host-cell').textContent.toLowerCase();
                    var protocol = row.querySelector('.protocol-cell').textContent.toLowerCase();
                    
                    if (oss.includes(searchTerm) || user.includes(searchTerm) || 
                        host.includes(searchTerm) || protocol.includes(searchTerm)) {
                        row.style.display = '';
                    } else {
                        row.style.display = 'none';
                    }
                });
            });

            // Save functionality
            document.getElementById('saveOss').addEventListener('click', function() {
                var stt = document.getElementById('sttFilePath').value;
                var oss = document.getElementById('oss').value;
                var user = document.getElementById('username').value;
                var password = document.getElementById('password').value;
                var host = document.getElementById('host').value;
                var port = document.getElementById('port').value;
                var filePath = document.getElementById('filePath').value;
                var protocol = document.getElementById('protocol').value;
                var active = document.getElementById('active').checked;

                // Validation
                if (!stt || !oss || !user || !password || !host || !port || !protocol) {
                    alert('Vui lòng điền đầy đủ thông tin bắt buộc!');
                    return;
                }

                if (port < 1 || port > 65535) {
                    alert('Port phải trong khoảng 1-65535!');
                    return;
                }

                console.log('Saving OSS configuration:', { 
                    stt, oss, user, password, host, port, filePath, protocol, active 
                });
                
                var modal = bootstrap.Modal.getInstance(document.getElementById('ossModal'));
                modal.hide();
                
                alert('Cấu hình OSS đã được lưu thành công!');
            });
			
			
			
			
			<!-- ========== PHẦN 18: JAVASCRIPT SORT VÀ ĐÓNG FILE ========== -->
            // Sort functionality with enhanced features
            let sortDirection = {}; // Store sort direction for each column

            document.querySelectorAll('.sortable').forEach(function(header) {
                header.addEventListener('click', function() {
                    const columnId = this.id;
                    const columnIndex = Array.from(this.parentNode.children).indexOf(this);
                    const tbody = document.getElementById('ossTableBody');
                    const rows = Array.from(tbody.querySelectorAll('tr'));
                    
                    // Toggle sort direction
                    sortDirection[columnId] = sortDirection[columnId] === 'asc' ? 'desc' : 'asc';
                    
                    // Clear all sort icons
                    document.querySelectorAll('.sortable i').forEach(icon => {
                        icon.className = icon.className.replace(/fa-sort-.*/, 'fa-sort');
                    });
                    
                    // Add new sort icon
                    const icon = this.querySelector('i');
                    if (sortDirection[columnId] === 'asc') {
                        icon.className = icon.className.replace('fa-sort', 'fa-sort-up');
                    } else {
                        icon.className = icon.className.replace('fa-sort', 'fa-sort-down');
                    }
                    
                    // Sort rows based on column
                    rows.sort((a, b) => {
                        let aValue, bValue;
                        
                        switch(columnId) {
                            case 'sortStt':
                                aValue = parseInt(a.querySelector('.stt-cell').textContent);
                                bValue = parseInt(b.querySelector('.stt-cell').textContent);
                                break;
                            case 'sortOss':
                                aValue = a.querySelector('.oss-cell').textContent.toLowerCase();
                                bValue = b.querySelector('.oss-cell').textContent.toLowerCase();
                                break;
                            case 'sortUser':
                                aValue = a.querySelector('.user-cell').textContent.toLowerCase();
                                bValue = b.querySelector('.user-cell').textContent.toLowerCase();
                                break;
                            case 'sortHost':
                                aValue = a.querySelector('.host-cell').textContent.toLowerCase();
                                bValue = b.querySelector('.host-cell').textContent.toLowerCase();
                                break;
                            case 'sortPort':
                                aValue = parseInt(a.querySelector('.port-cell').textContent);
                                bValue = parseInt(b.querySelector('.port-cell').textContent);
                                break;
                            case 'sortFilePath':
                                aValue = a.querySelector('.filepath-cell').textContent.toLowerCase();
                                bValue = b.querySelector('.filepath-cell').textContent.toLowerCase();
                                break;
                            case 'sortProtocol':
                                aValue = a.querySelector('.protocol-cell').textContent.toLowerCase();
                                bValue = b.querySelector('.protocol-cell').textContent.toLowerCase();
                                break;
                            case 'sortActive':
                                aValue = a.querySelector('.badge').textContent.toLowerCase();
                                bValue = b.querySelector('.badge').textContent.toLowerCase();
                                break;
                            default:
                                return 0;
                        }
                        
                        // Compare values
                        let comparison = 0;
                        if (typeof aValue === 'number' && typeof bValue === 'number') {
                            comparison = aValue - bValue;
                        } else {
                            comparison = aValue.localeCompare(bValue);
                        }
                        
                        return sortDirection[columnId] === 'asc' ? comparison : -comparison;
                    });
                    
                    // Update table
                    tbody.innerHTML = '';
                    rows.forEach(row => tbody.appendChild(row));
                    
                    // Update row numbers if not sorting by STT
                    if (columnId !== 'sortStt') {
                        updateRowNumbers();
                    }
                });
            });

            // Function to update row numbers
            function updateRowNumbers() {
                const allRows = document.querySelectorAll('#ossTableBody tr');
                allRows.forEach((row, index) => {
                    const sttCell = row.querySelector('.stt-cell');
                    if (sttCell) {
                        sttCell.textContent = index + 1;
                    }
                });
            }
        });
	
	
	
	
	
});