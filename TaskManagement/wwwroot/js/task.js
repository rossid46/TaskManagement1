﻿$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { "url": '/admin/task/getall' },
        "columns": [
            { "data": "title", "width": "10%" },
            { "data": "status", "width": "10%" },
            { "data": "applicationUserId", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                            <a href="/admin/task/upsert?id=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i>Update</a>
                            <a onClick=Delete('/admin/task/delete/${data}') class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i>Delete</a>
                            </div>`
                }, "width": "20%"
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You can't go back!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        cancelButtonText: "Cancel",
        confirmButtonText: "Yes, delete!",
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    });
}