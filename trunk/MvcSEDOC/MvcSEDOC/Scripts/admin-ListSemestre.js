
function loadMaterias(idS, idD, route) {
    $.getJSON(route, { term: idS, depa: idD }, function (data) {
        // contenido de la tabla           
        $.each(data, function (key, val) {
            $("#Materias" + idS + " tbody").append("<tr>" +
                              "<td colspan=3>" + val['nombre'] + "</td>" +
                              "<td><a class='edit' href='/sedoc/Admin/ShowScoresMateria?depa=" + idD + "&tipo=" + val['id'] + "'>Evaluaci&oacute;n</a></td>" +
                              "<td><a class='edit' href='/sedoc/Admin/ShowAutoScoresMateria?depa="+ idD+"&tipo=" + val['id'] + "'>Autoevaluaci&oacute;n</a></td>" +
                              "</tr>");
        });
    });
}

// carga  la cabecera de la tabla
function loadTable(idSemestre, idD) {
    ids = idSemestre;
    if (idSemestre != 0) {
        $("#work-area").html("<table id='Materias" + ids + "' class='list' style='width:100%'>" +
                            "<thead><tr><th align='center' colspan='3'>Materias Del Semestre " + idSemestre + "</th><th colspan='3'>Acciones</th></tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>");
        loadMaterias(idSemestre, idD, "/sedoc/Admin/GetMateriasPrograma/");
    }
}

