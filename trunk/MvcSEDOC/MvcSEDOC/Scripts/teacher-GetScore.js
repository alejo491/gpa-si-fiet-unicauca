$(function () {

    var searchVar = "/sedoc/Teacher/SearchLabor/";


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
                                    "<th  style='width:30px;'>Calificaci&oacute;n</th>" +
                                "</tr>" +
                                "</thead>" +
                            "<tbody></tbody>" +
                            "</table>");
    }

    function loadLabor(valor, searchVar) {
        $.getJSON(searchVar, { term: valor }, function (data) {
            $.each(data, function (key, val) {
                if (val['calificacion'] == -1) { val['calificacion'] = "No tiene" }
                $("#TableidLabor tbody").append("<tr>" +
                                    "<td>" + val['descripcion'] + "</td>" +
                                    "<td>" + val['calificacion'] + "</td>" +
                                "</tr>");
            });
        });
    }

});