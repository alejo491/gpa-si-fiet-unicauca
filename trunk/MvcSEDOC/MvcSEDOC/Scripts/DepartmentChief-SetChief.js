$(function () {

    var iddepto = document.getElementById("search_term").value;
    setDepartament();
    loadTeaching(iddepto, "/sedoc/DepartmentChief/GetDepartamentTeaching/");

    function setDepartament() {
        $("#work-area").html("");
        $("#work-area").append("<table id='TableidTeaching' class='list' style='width:100%'>" +
                            "<thead><tr><th colspan='4'>Miembros</th></tr>" +
                            "<tr class='sub'>" +
                                "<th>Apellidos</th>"+
                                "<th>Nombres</th>"+
                                "<th colspan='2' style='width:130px;'>Acciones</th>" +
                            "</tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>");
    }

    function loadTeaching(idD, route) {
        $.getJSON(route, { term: idD }, function (data) {
            $.each(data, function (key, val) {
                $("#TableidTeaching  tbody").append("<tr>" +
							"<td>" + val['ape'] + "</td>" +
                            "<td>" + val['label'] + "</td>" +
                            "<td><a class='edit' href='/sedoc/DepartmentChief/AssignWork/" + val['id'] + "'>Asignar Jefe</a> </td>" +
                            "<td><a class='edit' href='/sedoc/DepartmentChief/AssignWork1/" + val['id'] + "'>Editar Jefe</a> </td>" +
                            "</tr>");

            });
        });
    }


 
});