$(function () {

    setWorkId();
    var iddocente = document.getElementById("search_term").value;
    loadWork("1", "/sedoc/DepartmentChief/GetTechingWork/");
    var searchVar = "/sedoc/DepartmentChief/searchTeching/"; 

    function setWorkId() {

        $("#work-area").html("");
        $("#work-area").append("<table id='TableidWork' class='list' style='width:100%'>" +
                            "<thead><tr><th colspan=5>Labores</th></tr>" +
                            "<tr class='sub'><th>Id</th>" +
                            "<th>Profesor</th>" +
                            "<th>Tipo Labor</th><th>Labor</th>" +
                            "<th style='width:130px;'>Buscar Jefe</th></tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>");
    }

    function loadWork(idD, route) {
        $.getJSON(route, { term: idD }, function (data) {
            $.each(data, function (key, val) {
                $("#TableidWork  tbody").append("<tr>" +
							"<td  style='width:0;'>" + val['idlabor'] + "</td>" +
                            "<td>" + val['nombres'] + "</td>" +
							"<td>" + val['tipoLabor'] + "</td>" +
                            "<td>" + val['descripcion'] + "</td>" +
                            "<td>" + "<input type='hidden' id='idJL" + val['idlabor'] + "' />" +
                            "<input class='search_Teching' " + " id=" + val['idlabor'] + " />" + "</td>" +
//                            "<td>" + "<input type='hidden' id='idJH" + val['idlabor'] + "' />" +
//                            "<input style='width:30px;' class='search_Teching1' " + " id='idJH1" + val['idlabor'] + "' />" + "</td>" +
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
            //alert(idLabor +" a " + idJefe + " a " + horas);
            if (idJefe != "") {
                //metodo que guarde los datos en la BD
                $.getJSON("/sedoc/DepartmentChief/saveChief/", { idL: idLabor, idJ: idJefe}, function (data) {                                    
                                            if (i == numFilas - 2) {
                                               // alert(numFilas + " " + i);
                                                setTimeout(redireccionar, 1000);
                                            }                  
                });
            }
        }
    }


    function checkRange(o, n, min, max) {
        if (!isNaN(parseInt(o.val())) && checkDigits(o)) {
            var num = parseInt(o.val());
            if (num < min || num > max) {
                o.addClass("ui-state-error");
                updateTips("El rango de la " + n + " debe estar entre " +
					min + " y " + max + ".");
                return false;
            } else {
                return true;
            }
        } else {
            o.addClass("ui-state-error");
            updateTips("El valor de la " + n + " debe ser un numero entre " +
					min + " y " + max + ".");
            return false;
        }
    }
    function checkDigits(o) { //retorna true si solo tiene digitos
        solodigitos = true;
        for (i = 0; i < o.val().length; i++) {
            solodigitos = solodigitos && (o.val().charAt(i) >= '0' && o.val().charAt(i) <= '9');
        }
        return solodigitos;
    }

    function updateTips(t) {
        tips
				.text(t)
				.addClass("ui-state-highlight");
        setTimeout(function () {
            tips.removeClass("ui-state-highlight", 1500);
        }, 500);
    }

    function resultado(data) {
        if (data.respuesta == 0) {
            alert("Lo sentimos, algunos datos no se pueden guardar");
        }
    }
    function redireccionar() {
        //var iddocente = document.getElementById("search_term").value;
        var pagina = "/sedoc/DepartmentChief/AssignWork/";
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


















