$(function () {


    var des = $("#descp").val();
    var tipo = $("#tipo").val();
    //var idlabor = $("#search_term").val();
    var idlabor = document.getElementById("search_term").value;
    setWorkId();
    //loadWork(idlabor, "/sedoc/DepartmentChief/SearchProblemasLabor/");
    loadWork(idlabor,tipo);
    var searchVar = "/sedoc/DepartmentChief/searchTeching/";

    function setWorkId() {
        
        $("#work-area").html("");
        $("#work-area").append("<table id='TableidWork' class='report' style='width:100%'>" +
                            "<thead><tr><th colspan=2>Detalles De  "+ des + "</th></tr></thead>" + 
                            "<tbody></tbody>" +
                            "</table><br/>");
    }

    function loadWork(idL, tip) {
        $.getJSON("/sedoc/DepartmentChief/SearchProblemasLabor/", { id: idL, term: tip }, function (data) {
            $.each(data, function (key, val) {
                $("#TableidWork  tbody").append("<tr><th colspan=4>Docente  " + val['docente'] + "</th></tr>" +
                            "<tr><td colspan=2 aling='center'><strong>Problema<strong></td></tr>" +
                            "<tr class='sub'><td style='width:20%' aling='center'><strong>Descripción</strong></td>" +
							"<td>" + val['problemadescripcion'] + "</td></tr>" +
							"<tr class='sub'><td style='width:20%' aling='center'><strong>Respuesta</strong></td>" +
							"<td >" + val['problemarespuesta'] + "</td></tr>" +
                            "<tr><td colspan=2 ><strong>Solución</strong></td></tr>" +
                            "<tr class='sub'><td style='width:20%' aling='center'><strong>Descripción</strong></td>" +
							"<td >" + val['soluciondescripcion'] + "</td></tr>" +
							"<tr class='sub'><td style='width:20%' aling='center'><strong>Ubicación</strong></td>" +
							"<td >" + val['solucionrespuesta'] + "</td></tr>" +
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
            alert("Lo siento!, los datos no se pueden guardar... disculpame");
        }
    }
    function redireccionar() {
        //var iddocente = document.getElementById("search_term").value;
        var pagina = "/sedoc/DepartmentChief/SetChief/" + $("#depto").val();
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


















