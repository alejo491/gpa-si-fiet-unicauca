$(function () {
    /************* VARIABLES *********************/
    var editnombre = $("#editnombre"),
    //			editfacultad = $("#editfacultad"),
    //			editFields = $([]).add(editnombre).add(editfacultad),
            editFields = $([]).add(editnombre),
			tips = $(".field-validation-valid");

    var createnombre = $("#createnombre"),
    //createfacultad = $("#createfacultad"),
    //createFields = $([]).add(createnombre).add(createfacultad);
            createFields = $([]).add(createnombre);

    /************* INICIALIZACIONES **************/
    function reinitLinks() {
        $("a.edit").click(function () {
            idfacul = $(this).attr("idfacultad");
            $("#idfacultad").val(idfacul);
            $("#editnombre").val($("#lblnombre" + idfacul).html());
            $('#dialogEditFacultad').dialog('open');
            return false;
        });

        //        $("a.delete").click(function () {
        //            idper = $(this).attr("idperiodo");
        //            $("#delidperiod").val(idper);
        //            $('#dialogDeleteAcademicPeriodConfirm').dialog('open');
        //            return false;
        //        });
    }

    reinitLinks();

    $("a.add").click(function () {
        $('#dialogCreateFacultad').dialog('open');
        return false;
    });

    $('#dialogEditFacultad').dialog({
            autoOpen: false,
            width: 400,
            height: 260,
            modal: true,
            buttons: {
                "Guardar": function () {
                    //var bValid = true;
                    editFields.removeClass("ui-state-error");

    //                bValid = bValid && checkRange(editanio, "año", 1000, 9999);
    //                bValid = bValid && checkRange(editperiodo, "numero de periodo", 1, 2);

                    //if (bValid) {
                        UpdateFacultad("/sedoc/Admin/EditFacultad/", $(this))
                    //}
                },
                "Cancelar": function () {
                    editFields.val("").removeClass("ui-state-error");
                    updateTips("");
                    $(this).dialog("close");
                }
            }
        });



    $('#dialogCreateFacultad').dialog({
        autoOpen: false,
        width: 400,
        height: 240,
        modal: true,
        buttons: {
            "Crear": function () {
                var bValid = true;
                createFields.removeClass("ui-state-error");
                updateTips("");

                //                bValid = bValid && checkRange(createanio, "año", 1000, 9999);
                //                bValid = bValid && checkRange(createperiodo, "numero de periodo", 1, 2);

                //if (bValid) {
                    CreateFacultad("/sedoc/Admin/CreateFacultad/", $(this))
               //}
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

    //    function checkDigits(o) { //retorna true si solo tiene digitos
    //        solodigitos = true;
    //        for (i = 0; i < o.val().length; i++) {
    //            solodigitos = solodigitos && (o.val().charAt(i) >= '0' && o.val().charAt(i) <= '9');
    //        }
    //        return solodigitos;
    //    }

    //    function checkRange(o, n, min, max) {
    //        if (!isNaN(parseInt(o.val())) && checkDigits(o)) {
    //            var num = parseInt(o.val());
    //            if (num < min || num > max) {
    //                o.addClass("ui-state-error");
    //                updateTips("El rango de " + n + " debe estar entre " +
    //					min + " y " + max + ".");
    //                return false;
    //            } else {
    //                return true;
    //            }
    //        } else {
    //            o.addClass("ui-state-error");
    //            updateTips("El valor de " + n + " debe ser un numero entre " +
    //					min + " y " + max + ".");
    //            return false;
    //        }
    //    }

    function UpdateFacultad(route, jquerydialog) {
            var fac = getFacultad();
            if (fac != null) {
                var request = $.ajax({
                    cache: false,
                    url: route,
                    type: 'POST',
                    dataType: 'json',
                    data: fac,
                    contentType: 'application/json; charset=utf-8',
                    success: function (data) {
                        if (data) {
                            $("#lblfac" + data.idfacultad).html(data.fac_nombre.toString());
//                            $("#lblNumPer" + data.idperiodo).html(data.numeroperiodo.toString());
                            jquerydialog.dialog("close");
                            alert("Los cambios se grabaron éxitosamente");
                        } else {
                            alert("Lo siento no pude grabar los cambios, comunicate con el administrador");
                        }
                    }
                });
            }
        }

    function getFacultad() {
        var idF = $("#idfacultad").val();
        var nomF = $("#editnombre").val();

        return (idF == "" || nomF == "") ? null : '{ ""idfacultad: "' + idF + '", "nombre": "' + nomF + '" }';
    }

    //    function DeleteAcademicPeriod(route, jquerydialog) {
    //        var idP = $('#delidperiod').val();
    //        if (idP == "") {
    //            alert("El periodo académico no puede ser eliminado");
    //        } else {
    //            var period = '{ "id": ' + idP + ' }';
    //            var request = $.ajax({
    //                cache: false,
    //                url: route,
    //                type: 'POST',
    //                dataType: 'json',
    //                data: period,
    //                contentType: 'application/json; charset=utf-8',
    //                success: function (data) {
    //                    if (data != null) {
    //                        $('#fila' + data.idperiodo).remove();
    //                        jquerydialog.dialog("close");
    //                    } else {
    //                        alert("Lo siento!, la eliminación no se pudo efectuar ... disculpame");
    //                        jquerydialog.dialog("close");
    //                    }
    //                }
    //            });
    //        }
    //    }

    function CreateFacultad(route, jquerydialog) {
        var idfa = getNewIDFacultad();        

        if (idfa != null) {
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: idfa,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    if (data.idfacultad != 0) {
                        $("#datostabla").append("<tr id='fila" + data.idfacultad + "'>" +
							"<td id='lblnombre" + data.idfacultad + "'>" + data.fac_nombre + "</td>" +							
                            "<td>" +
                            "    <a href='#' class='edit' id='btnEdit" + data.idfacultad + "' idfacultad='" + data.idfacultad + "'>Editar</a> |" +
//                            "    <a href='#' class='delete' id='btnDel" + data.idfacultad + "' idfacultad='" + data.idfacultad + "'>Eliminar</a>" +
                            "</td>" +
						"</tr>");
                        reinitLinks()
                        jquerydialog.dialog("close");
                    } else {
                        alert("No pude crear la facultad, por favor contácte al administrador");
                    }
                }
            });
        }
    }

    function getNewIDFacultad() {
        var nomF = $("#createnombre").val();        

        return (nomF == "" ) ? null : '{ "nombre": "' + nomF + '" }';
    }

});