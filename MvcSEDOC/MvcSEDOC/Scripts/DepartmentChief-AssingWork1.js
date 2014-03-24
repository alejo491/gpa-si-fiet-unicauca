$(function () {

    setWorkId();
    //var iddocente = document.getElementById("search_term").value;
    loadWork("1", "/sedoc/DepartmentChief/GetTechingWork1/");
    var searchVar = "/sedoc/DepartmentChief/searchTeching/";

    function setWorkId() {

        $("#work-area").html("");
        $("#work-area").append("<table id='TableidWork' class='list' style='width:100%'>" +
                                "<thead><tr><th colspan=5>Labores</th></tr>" +
                                "<tr class='sub'><th style='width:30px;' type='hidden'>id</th><th>Tipo Labor</th><th ALIGN='CENTER'  style='width:130px;'>Labor</th><th style='width:130px;' ALIGN='CENTER'>Jefe Actual</th><th ALIGN='CENTER'>Nuevo</th></tr></thead>" +
                                "<tbody></tbody>" +
                                "</table><br/>");
        }

    function loadWork(idD, route) {
        $.getJSON(route, { term: idD }, function (data) {
            $.each(data, function (key, val) {
                $("#TableidWork  tbody").append("<tr>" +
							"<td type='hidden'>" + val['idlabor'] + "</td>" +
							"<td>" + val['tipoLabor'] + "</td>" +
                            "<td>" + val['descripcion'] + "</td>" +
//                            "<td >" + "<input style='width:15px;'   value='" + val['horasSemana'] + "' class='search' " + " id='#' disabled />" + "</td>" +
                            "<td>" + "<input   value='" + val['nombres'] + "' class='search' " + " id='#' disabled />" + "</td>" +
                            "<td>" + "<input type='hidden' id='idJL" + val['idlabor'] + "' />" +
                            "<input class='search_Teching' " + " id=" + val['idlabor'] + " />" + "</td>" +
//                            "<td>" + "<input type='hidden' id='idJH" + val['idlabor'] + "' />" +
//                            "<input style='width:15px;' class='search_Teching1' " + " id='idJH1" + val['idlabor'] + "' />" + "</td>" +
                            "</tr>");

            });
            autocompleteTeching();
        });
    }

    function loadDateTable() {
        var tabla = document.getElementById("TableidWork");
        var numFilas = tabla.rows.length;

        for (i = 0; i < numFilas - 2; i++) {
            var idLabor = tabla.tBodies[0].rows[i].cells[0].innerHTML;
            var idJefe = document.getElementById("idJL" + idLabor).value;
            //var horas = document.getElementById("idJH1" + idLabor).value;
            if (idJefe != "") {
                //metodo que guarde los datos en la BD
                $.getJSON("/sedoc/DepartmentChief/saveChief/", { idL: idLabor, idJ: idJefe }, function (data) {
                    if (i == numFilas - 2) {
                        setTimeout(redireccionar, 1000);
                    }
                });
            }
        }
    }

    function resultado(data) {
        if (data.respuesta == 0) {
            alert("Lo sentimos, los datos no pueden ser guardardoso en este momento");
        }
    }
    function redireccionar() {
        //var iddocente = document.getElementById("search_term").value;
        var pagina = "/sedoc/DepartmentChief/AssignWork1/";
        location.href = pagina;
    }

    function autocompleteTeching() {
        $(".search_Teching").autocomplete({
            source: searchVar,
            minLength: 1,
            select: function (event, ui) {
                if (ui.item) {
                    $("#idJL" + $(this).attr("id")).val(ui.item.id);

                }
            }
        });
        save();
    }
    //adiciona los botones en la ventana del diajogo guardar
    $("#dialogSaveGroupConfirm").dialog({
        resizable: false,
        autoOpen: false,
        height: 140,
        modal: true,
        buttons: {
            "Guardar": function () {
                loadDateTable();
                $(this).dialog("close");
            },
            Cancelar: function () {
                $(this).dialog("close");
            }
        }
    });
    //Cuando el evento click es capturado en el enlace guardar abre el dialogo
    function save() {
        $('#save').click(function () {
            $('#dialogSaveGroupConfirm').dialog('open');
            return false;
        });
    }

});












