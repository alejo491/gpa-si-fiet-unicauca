$(function () {
    /************* VARIABLES *********************/
    var editanio = $("#editanio"),
			editperiodo = $("#editperiodo"),
			editFields = $([]).add(editanio).add(editperiodo),
			tips = $(".field-validation-valid");

    var createanio = $("#createanio"),
			createperiodo = $("#createperiodo"),
			createFields = $([]).add(createanio).add(createperiodo);

    /************* INICIALIZACIONES **************/
    function reinitLinks() {
        $("a.edit").click(function () {
            idper = $(this).attr("idperiodo");
            $("#idperiod").val(idper);
            $("#editanio").val($("#lblAnioPer" + idper).html());
            $("#editperiodo").val($("#lblNumPer" + idper).html());
            $('#dialogEditAcademicPeriod').dialog('open');
            return false;
        });

        $("a.delete").click(function () {
            idper = $(this).attr("idperiodo");
            $("#delidperiod").val(idper);
            $('#dialogDeleteAcademicPeriodConfirm').dialog('open');
            return false;
        });
    }

    reinitLinks();

    $("a.add").click(function () {
        $('#dialogCreateAcademicPeriod').dialog('open');
        return false;
    });

    $('#dialogEditAcademicPeriod').dialog({
        autoOpen: false,
        width: 400,
        height: 260,
        modal: true,
        buttons: {
            "Guardar": function () {
                var bValid = true;
                editFields.removeClass("ui-state-error");

                bValid = bValid && checkRange(editanio, "año", 1000, 9999);
                bValid = bValid && checkRange(editperiodo, "numero de periodo", 1, 2);

                if (bValid) {
                    UpdateAcademicPeriod("/sedoc/AdminFac/EditAcademicPeriod/", $(this))
                }
            },
            "Cancelar": function () {
                editFields.val("").removeClass("ui-state-error");
                updateTips("");
                $(this).dialog("close");
            }
        }
    });

    $("#dialogDeleteAcademicPeriodConfirm").dialog({
        resizable: false,
        autoOpen: false,
        width: 350,
        height: 200,
        modal: true,
        buttons: {
            "Eliminar": function () {
                DeleteAcademicPeriod("/sedoc/AdminFac/DeleteAcademicPeriod/", $(this));
            },
            "Cancelar": function () {
                editFields.val("").removeClass("ui-state-error");
                createFields.val("").removeClass("ui-state-error");
                updateTips("");
                $(this).dialog("close");
            }
        }
    });

    $('#dialogCreateAcademicPeriod').dialog({
        autoOpen: false,
        width: 400,
        height: 240,
        modal: true,
        buttons: {
            "Crear": function () {
                var bValid = true;
                createFields.removeClass("ui-state-error");
                updateTips("");

                bValid = bValid && checkRange(createanio, "año", 1000, 9999);
                bValid = bValid && checkRange(createperiodo, "numero de periodo", 1, 2);

                if (bValid) {
                    CreateAcademicPeriod("/sedoc/AdminFac/CreateAcademicPeriod/", $(this))
                }
            },
            "Cancelar": function () {
                createFields.val("").removeClass("ui-state-error");
                updateTips("");
                $(this).dialog("close");
            }
        }
    });
    /*********** FUNCIONES AUXILIARES ************/
    function updateTips(t) {
        tips
				.text(t)
				.addClass("ui-state-highlight");
        setTimeout(function () {
            tips.removeClass("ui-state-highlight", 1500);
        }, 500);
    }

    function checkLength(o, n, min, max) {
        if (o.val().length > max || o.val().length < min) {
            o.addClass("ui-state-error");
            updateTips("El tamaño de " + n + " debe estar entre " +
					min + " y " + max + ".");
            return false;
        } else {
            return true;
        }
    }

    function checkDigits(o) { //retorna true si solo tiene digitos
        solodigitos = true;
        for (i = 0; i < o.val().length; i++) {
            solodigitos = solodigitos && (o.val().charAt(i) >= '0' && o.val().charAt(i) <= '9');
        }
        return solodigitos;
    }

    function checkRange(o, n, min, max) {
        if (!isNaN(parseInt(o.val())) && checkDigits(o)) {
            var num = parseInt(o.val());
            if (num < min || num > max) {
                o.addClass("ui-state-error");
                updateTips("El rango de " + n + " debe estar entre " +
					min + " y " + max + ".");
                return false;
            } else {
                return true;
            }
        } else {
            o.addClass("ui-state-error");
            updateTips("El valor de " + n + " debe ser un numero entre " +
					min + " y " + max + ".");
            return false;
        }
    }

    function UpdateAcademicPeriod(route, jquerydialog) {
        var period = getPeriod();
        if (period != null) {
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: period,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    if (data) {
                        $("#lblAnioPer" + data.idperiodo).html(data.anio.toString());
                        $("#lblNumPer" + data.idperiodo).html(data.numeroperiodo.toString());
                        jquerydialog.dialog("close");
                        alert("Los cambios se grabaron éxitosamente");
                    } else {
                        alert("Lo siento no pude grabar los cambios, comunicate con el administrador");
                    }
                }
            });
        }
    }

    function getPeriod() {
        var idP = $("#idperiod").val();
        var anP = $("#editanio").val();
        var numP = $("#editperiodo").val();

        return (idP == "" || anP == "" || numP == "") ? null : '{ "idperiodo": "' + idP + '", "anio": "' + anP + '","numeroperiodo": "' + numP + '" }';
    }

    function DeleteAcademicPeriod(route, jquerydialog) {
        var idP = $('#delidperiod').val();
        if (idP == "") {
            alert("El periodo académico no puede ser eliminado");
        } else {
            var period = '{ "id": ' + idP + ' }';
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: period,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    if (data != null) {
                        $('#fila' + data.idperiodo).remove();
                        jquerydialog.dialog("close");
                    } else {
                        alert("Lo siento!, la eliminación no se pudo efectuar ... disculpame");
                        jquerydialog.dialog("close");
                    }
                }
            });
        }
    }

    function CreateAcademicPeriod(route, jquerydialog) {
        var period = getNewPeriod();

        if (period != null) {
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: period,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    if (data.idpregunta != 0) {
                        $("#datostabla").append("<tr id='fila" + data.idperiodo + "'>" +
							"<td id='lblAnioPer" + data.idperiodo + "'>" + data.anio + "</td>" +
							"<td id='lblNumPer" + data.idperiodo + "'>" + data.numeroperiodo + "</td>" +
                            "<td>" +
                            "    <a href='#' class='edit' id='btnEdit" + data.idperiodo + "' idperiodo='" + data.idperiodo + "'>Editar</a> |" +
                            "    <a href='#' class='delete' id='btnDel" + data.idperiodo + "' idperiodo='" + data.idperiodo + "'>Eliminar</a>" +
                            "</td>" +
						"</tr>");
                        reinitLinks()
                        jquerydialog.dialog("close");
                    } else {
                        alert("No pude crear el periodo academico, por favor contácte al administrador");
                    }
                }
            });
        }
    }

    function getNewPeriod() {
        var anP = $("#createanio").val();
        var numP = $("#createperiodo").val();

        return (anP == "" || numP == "") ? null : '{ "anio": "' + anP + '","numeroperiodo": "' + numP + '" }';
    }

});