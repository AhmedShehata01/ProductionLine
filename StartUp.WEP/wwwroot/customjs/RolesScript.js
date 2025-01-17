$(document).ready(function () {
    getAllData();
    $('.dtr-control').off('click');
});

function getAllData() {
    $("#RoleTable").DataTable({
        columnDefs: [{
            "defaultContent": "-", "targets": "_all",
        }],
        processing: true,
        serverSide: true,
        autoWidth: true,
        filter: true,
        ajax: {
            type: 'POST',
            url: "/RoleManagment/GetData",
            datatype: 'json',
            "dataSrc": function (json) {
                console.log(json); // Log the entire JSON response first
                return json.data?.$values || json.data || []; // Fallback options
            },
            error: function (xhr, status, error) {
                console.error("Error: " + error); // Logs any errors to the console
            }
        },
        columns: [
            { data: "Name", "name": "Name" },
            { data: "CreatedOn", "name": "CreatedOn" },
            { data: "IsActive", "name": "IsActive" },
            { data: "IsExternal", "name": "IsExternal" },
            {
                render: function (data, type, row, meta) {
                    return `
                        <div class="btn-group dropleft">
                            <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                               Actions
                            </button>
                            <div class="dropdown-menu mr-3">
                               <a class="dropdown-item" href="/RoleManagment/EditRole/${row.Id}">Edit</a>
                               <a class="dropdown-item" href="/RoleManagment/DeleteRole/${row.Id}">Delete</a>
                            </div>
                       </div>  
                    `;
                },
                searchable: false,
                orderable: false, // Disable sorting for the Action column
            }
        ],
        dom: "<'row'<'col-sm-4'l><'col-sm-4 d-flex justify-content-center'B><'col-sm-4'f>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-6'i><'col-sm-6'p>>",
        buttons: [
            {
                extend: 'copy',
                exportOptions: {
                    columns: [0, 1, 2, 3],
                },
            },
            {
                extend: 'excel',
                exportOptions: {
                    columns: [0, 1, 2, 3],
                },
            },
            {
                extend: 'csv',
                exportOptions: {
                    columns: [0, 1, 2, 3],
                },
            },
            {
                extend: 'pdf',
                exportOptions: {
                    columns: [0, 1, 2, 3],
                },
            },
            {
                extend: 'print',
                exportOptions: {
                    columns: [0, 1, 2, 3],
                },
            }
        ]
    });
}
