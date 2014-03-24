$(function () {

    var iddocente= document.getElementById("search_term").value;
    setDocente();
    loadTeaching(iddocente, "/sedoc/Teacher/GetLaboresDocente/");

    function setDocente() {
        $("#work-area").html("");
        $("#work-area").append("<table id='TableidTeaching' class='list' style='width:100%'>" +
                            "<thead><tr><th colspan=3>Labores</th></tr>" +
                            "<tr class='sub'><th>Labor</th><th>Tipo De Labor</th><th style='width:130px;'>Acci&oacute;n</th></tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>");
    }

    function loadTeaching(idD, route) {
        $.getJSON(route, { term: idD }, function (data) {
            $.each(data, function (key, val) {
                $("#TableidTeaching  tbody").append("<tr>" +
							"<td>" + val['tipoLabor'] + "</td>" +
                            "<td>" + val['descripcion'] + "</td>" +
                            "<td><a class='edit' href='/sedoc/Teacher/Evaluar/" + val['idlabor'] + "'>Evaluar</a> </td>" +
                            "</tr>");

            });
        });
    }
     
});