$(function () {
    /************* VARIABLES *********************/
    var searchQuestionnaireVar = "/sedoc/AdminFac/SearchQuestionnaire/";
    var searchGroupVar = "/sedoc/AdminFac/SearchGroup/";

    var groupname = $("#nombre"),
            grouppercent = $("#porcentaje"),
			allFields = $([]).add(groupname).add(grouppercent),
			tips = $(".validateTips");
    /*****************FUNCIONES AUXILIARES*****************************/

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
            updateTips("El tamaño del " + n + " debe estar entre " +
					min + " y " + max + ".");
            return false;
        } else {
            return true;
        }
    }

    function checkRange(o, n, min, max) {
        if (!isNaN(parseFloat(o.val()))) {
            var num = parseFloat(o.val());
            if (num < min || num > max) {
                o.addClass("ui-state-error");
                updateTips("El rango del " + n + " debe estar entre " +
					min + " y " + max + ".");
                return false;
            } else {
                return true;
            }
        } else {
            o.addClass("ui-state-error");
            updateTips("El valor del " + n + " debe ser un numero entre " +
					min + " y " + max + ".");
            return false;
        }
    }

    /****** Inicializaciones JQuery *******/

    $("#search_term").autocomplete({
        source: searchQuestionnaireVar,
        minLength: 1,
        select: function (event, ui) {
            $("#work-area").html("");
            $("#work-area").append("<table id='TableQuestionnaire' class='list' style='width:100%'>" +
                            "<thead><tr><th colspan=3>" + ui.item.value + "</th></tr>" +
                            "<tr class='sub'>" +
                                "<th>Grupo</th>" +
                                "<th style='width:130px;'>Acci&oacute;n</th></tr>" +
                            "</thead></table>" +
                            "<div id='sortdiv' style='width:100%;'>" +
                            "<ul id='sortable'></ul>" +
                            "</div><br/>");
            loadGroups(ui.item.id, "/sedoc/AdminFac/GetQuestionnaireGroups/");
            loadSortable(ui.item.id);
        }
    });

    $('#dialogEditGroup').dialog({
        autoOpen: false,
        width: 400,
        height: 300,
        modal: true,
        buttons: {
            "Guardar": function () {
                var bValid = true;
                allFields.removeClass("ui-state-error");
                bValid = bValid && checkLength(groupname, "grupo", 3, 250);
                bValid = bValid && checkRange(grouppercent, "porcentaje", 1, 100);

                if (bValid) {
                    allFields.removeClass("ui-state-error");
                    SaveGroup("/sedoc/AdminFac/EditGroup/", $(this));
                    $(this).dialog("close");
                }
            },
            "Cancelar": function () {
                allFields.removeClass("ui-state-error");
                $(this).dialog("close");
            }
        }
    });

    $("#dialogDeleteGroupConfirm").dialog({
        resizable: false,
        autoOpen: false,
        height: 140,
        modal: true,
        buttons: {
            "Eliminar": function () {
                DeleteGroup("/sedoc/AdminFac/DeleteGroup/", $(this));
            },
            Cancelar: function () {
                $(this).dialog("close");
            }
        }
    });

    function loadSortable(idq) {
        $("#sortable").sortable({
            placeholder: "ui-state-highlight",
            update: function (event, ui) {
                var items = $("#sortable").sortable("toArray");
                var arr = [];
                $.each(items, function (key, val) {
                    var idval = val.replace("listElement_", "");
                    var obj = { id: idval }
                    arr.push(obj);
                });
                var data2send = JSON.stringify(arr);
                $.ajax({
                    cache: false,
                    url: "/sedoc/AdminFac/SortGroups/?stridq=" + idq,
                    type: 'post',
                    dataType: 'json',
                    data: data2send,
                    contentType: 'application/json; charset=utf-8',
                    success: function (data) {
                        if (data != null) {
                            $.each(data, function (key, val) {
                                setEditAction(val['id'], val['label'], val['pos'], val['idc'], val['porc']);
                            });
                        } else {
                            alert("Lo siento!, ocurrio un error ... disculpame")
                        }
                    }
                });
            }
        });
        $("#sortable").disableSelection();
    }


    /**************** FUNCIONES AUXILIARES *****************/
    function DeleteGroup(route, jquerydialog) {
        var idg = $('#idgrupo').val();
        if (idg == "") {
            alert("El grupo no puede ser eliminado.");
        } else {
            var grup = '{ "id": ' + idg + ' }';
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: grup,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    deletedResponse(data, jquerydialog);
                }
            });
        }
    }

    function SaveGroup(route, jquerydialog) {
        var group = getGroup();
        if (group != null) {
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: group,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    savedResponse(data, jquerydialog);
                    setEditAction(data.idgrupo, data.nombre, data.orden, data.idcuestionario, data.porcentaje);
                }
            });
        }
    }

    function deletedResponse(data, jquerydialog) {
        if (data.idgrupo != 0) {
            $('#btnDel' + data.idgrupo).parent().parent().parent().parent().remove();
            jquerydialog.dialog("close");
        } else {
            alert("Lo siento!, la eliminaci&oacute;n no se pudo efectuar ... disculpame");
        }
    }

    function savedResponse(data, jquerydialog) {
        if (data.idgroup != 0) {
            $('#lblEdt' + data.idgrupo).html(data.nombre);
            allFields.val("").removeClass("ui-state-error");
            updateTips("");
            jquerydialog.dialog("close");

//            $('#btnDel' + data.idgrupo).parent().parent().find("td").eq(1).html(data.nombre);
//            jquerydialog.dialog("close");
        } else {
            alert("Lo siento!, el registro no se pudo efectuar ... disculpame");
        }
    }

    function setEditAction(idG, nameG, orden, idQ, porc) {
        $('#btnEdit' + idG).click(function () {
            $('#dialogEditGroup').dialog('open');
            $("#idgrupo").val(idG);
            $('#nombre').val(nameG);
            $("#orden").val(orden);
            $("#idcuestionario").val(idQ);
            $("#porcentaje").val(porc);
            return false;
        });

        $('#btnDel' + idG).click(function () {
            $('#dialogDeleteGroupConfirm').dialog('open');
            $('#idgrupo').val(idG);
            return false;
        });
    }

    function setQuestionnaireId(ui) {
        $("#idQuestionnaire").val(ui.item.id);
        $("#work-area").html("");
    }

    function getGroup() {
        var idg = $("#idgrupo").val();
        var nom = $("#nombre").val();
        var idq = $("#idcuestionario").val();
        var ord = $("#orden").val();
        var por = $("#porcentaje").val();
        return (idg == "" || nom == "" || idq == "" || ord == "" || por == "") ? null : '{ "idgrupo": "' + idg + '", "nombre": "' + nom + '","idcuestionario": "' + idq + '","orden": "' + ord + '","porcentaje": "' + por + '" }';
    }

    function loadGroups(idQ, route) {
        $.getJSON(route, { term: idQ }, function (data) {
            $.each(data, function (key, val) {
                $("#sortable").append("<li id='listElement_" + val["id"] + "'><table class=list' style='width:100%;'><tr>" +
							"<td id='lblEdt" + val['id'] + "'>" + val['label'] + "</td>" +
                            "<td style='width:130px;'><a href='#' class='edit' id='btnEdit" + val['id'] + "'>Editar</a> |" +
                            "    <a class='delete' href='#' id='btnDel" + val["id"] + "'>Eliminar</a>" +
                            "</td>" +
						"</tr>");
                setEditAction(val['id'], val['label'], val['pos'], val['idc'], val['porc']);
            });
        });
    }

});

