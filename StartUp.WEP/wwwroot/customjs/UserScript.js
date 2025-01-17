$(document).ready(function () {
    getAllData();
    $('.dtr-control').off('click');
});

function getAllData() {
    $("#UserTable").DataTable({
        columnDefs: [{
            "defaultContent": "-", "targets": "_all"
        }],
        processing: true,
        serverSide: true,
        responsive: true, // Enable responsive design
        filter: true,
        fixedHeader: true, // Fix the header when scrolling
        scrollX: true, // Enable horizontal scrolling
        ajax: {
            type: 'POST',
            url: "/UserManagment/GetData", // Use correct URL
            datatype: 'json',
            "dataSrc": function (json) {
                console.log(json); // Log the entire JSON response first
                return json.data?.$values || json.data || []; // Fallback options
            },
            error: function (xhr, status, error) {
                console.error("Error: " + error); // Handle any errors
            }
        },
        columns: [
            { data: "UserName", name: "UserName" },
            { data: "Email", name: "Email" },
            { data: "EmailConfirmed", name: "EmailConfirmed" },
            { data: "CreatedOn", name: "CreatedOn" },
            {
                render: function (data, type, row, meta) {
                    return `
                     <div class="btn-group dropleft">
                         <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                            Actions
                         </button>
                         <div class="dropdown-menu mr-3">
                            <a class="dropdown-item" href="/UserManagment/EditUser/${row.Id}">Edit</a>
                            <a class="dropdown-item" href="/UserManagment/DeleteUser/${row.Id}">Delete</a>
                            <a class="dropdown-item reset-password-btn" data-id="${row.Id}" href="#">Reset Password</a>
                         </div>
                    </div>`;
                },
                searchable: false,
                orderable: false
            }
        ],
        order: [], // Disable default ordering
        dom: "<'row'<'col-sm-4'l><'col-sm-4 d-flex justify-content-center'B><'col-sm-4'f>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-6'i><'col-sm-6'p>>",
        buttons: [
            {
                extend: 'copy',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4], // Define columns to export
                },
            },
            {
                extend: 'excel',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4],
                },
            },
            {
                extend: 'csv',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4],
                },
            },
            {
                extend: 'pdf',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4],
                },
            },
            {
                extend: 'print',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4],
                },
            }
        ]
    });


    // reset password
    $(document).on('click', '.reset-password-btn', function () {
        const userId = $(this).data('id');
        $('#resetUserId').val(userId);
        $('#newPassword').val('');
        $('#confirmPassword').val('');
        $('#resetPasswordModal').modal('show');
    });

    $('#savePasswordBtn').click(function () {
        const userId = $('#resetUserId').val();
        const newPassword = $('#newPassword').val().trim();
        const confirmPassword = $('#confirmPassword').val().trim();

        if (!newPassword || !confirmPassword) {
            toastr.warning('Password fields cannot be empty!', 'Warning');
            return;
        }

        if (newPassword.length < 6) {
            toastr.error('Password must be at least 6 characters long.', 'Validation Error');
            return;
        }

        if (newPassword !== confirmPassword) {
            toastr.warning('Passwords do not match!', 'Warning');
            return;
        }

        $.ajax({
            type: 'POST',
            url: '/Account/AdminResetPassword',
            data: JSON.stringify({
                Id: userId,
                Password: newPassword,
                ConfirmPassword: confirmPassword
            }),
            contentType: 'application/json',
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message, 'Success');
                    $('#resetPasswordModal').modal('hide');
                } else {
                    toastr.error(response.errors.join('<br>'), 'Error');
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred. Please try again.', 'Error');
            }
        });

    });


}
