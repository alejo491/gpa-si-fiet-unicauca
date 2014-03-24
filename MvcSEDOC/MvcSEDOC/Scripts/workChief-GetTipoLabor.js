$(function () {

    var searchVar = "/sedoc/WorkChief/SearchTipoLabor/";


    $('#tipoLabor').change(function () {
        var valor = $('#tipoLabor').val();
        setHeadLaborTable()
        loadLabor(valor, searchVar);
    });

    function setHeadLaborTable() {

        $("#work-area").html("");
        $("#work-area").append("<table id='TableidLabor' class='list' colspan=2 style='width:100%'>" +
                                "<thead>" +
                                    "<tr>" +
                                        "<th style='width:30px;'>Nombre Labor</th>" +
                                        "<th  style='width:30px;'>Acci&oacute;n</th>" +
                                    "</tr>" +
                                "</thead>" +
                                "<tbody></tbody>" +
                                "</table>");
    }

    function loadLabor(valor, searchVar) {
        $.getJSON(searchVar, { term: valor }, function (data) {
            $.each(data, function (key, val) {               
                $("#TableidLabor tbody").append("<tr>" +
                                                    "<td>" + val['descripcion'] + "</td>" +
                                                    "<td><a class='edit' href='/sedoc/WorkChief/ProblemasLabor/" + val['idlabor'] + "'>Ver Detalles</a> </td>" +
                                                "</tr>");
            });
        });
    }

});