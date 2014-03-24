
function loadDepartamentos(idF, route) {
    $.getJSON(route, { term: idF }, function (data) {
        // contenido de la tabla           
        $.each(data, function (key, val) {
            $("#DepartamentFrom" + idF + " tbody").append("<tr>" +
                    "<td>" + val['label'] + "</td>" +
                    "<td><a class='edit' href='/sedoc/Admin/VerDocentes/" + val['id'] + "'>Ver Docentes</a> </td>" +
                    "<td><a class='edit' href='/sedoc/Admin/VerLabores/" + val['id'] + "'>Ver Labores</a> </td>" +
                    "</tr>");
        });
    });
}

// carga  la cabecera de la tabla
function loadTable(idFacultad) {
    idfac = idFacultad;
    if (idFacultad != 0) {
        nameFac = $("#fac" + idfac).attr("facname");
        $("#work-area").html("<table id='DepartamentFrom" + idfac + "' class='list' style='width:100%'>" +
                            "<thead><tr><th align='center' colspan=3>Programas De La Facultad De " + nameFac + "</th></tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>");
        loadDepartamentos(idFacultad, "/sedoc/Admin/GetFacultadPrograma/");
    }
}

