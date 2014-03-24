$(function () {
    /************* VARIABLES *********************/
    var searchFacultadVar = "/sedoc/Admin/SearchFacultad/";
    var searchProgramaVar = "/sedoc/Admin/GetFacultadPrograma/";

    var proname = $("#nombre"),
		allFields = $([]).add(proname),
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
        source: searchFacultadVar,
        minLength: 1,       
        select: function (event, ui) {
            $("#work-area").html("");
            $("#work-area").append("<table id='TableQuestionnaire' class='list' style='width:100%'>" +                                                     
                            "</thead></table>" +
                            "<div id='sortdiv' style='width:100%;'>" +
                            "<ul id='sortable'></ul>" +
                            "</div><br/>");
            loadGroups(ui.item.id, "/sedoc/Admin/GetFacultadPrograma/");
            loadSortable(ui.item.id);
        }
    });

    $("#search_term_pro").autocomplete({
        //source: searchVar,
        minLength: 1,
        select: function (event, ui) {
            if (ui.item) {
                setGrupoId(ui);
            }
        }
    });

//    $("#search_term_pro").autocomplete({
//        //source: searchProgramaVar,
//        minLength: 1,
//        select: function (event, ui) {
//            $("#work-area").html("");
//            $("#work-area").append("<table id='TableQuestionnaire' class='list' style='width:100%'>" +
//                            "</thead></table>" +
//                            "<div id='sortdiv' style='width:100%;'>" +
//                            "<ul id='sortable'></ul>" +
//                            "</div><br/>");
//            //loadGroups(ui.item.id, "/sedoc/Admin/GetFacultadPrograma/");
//            loadSortable(ui.item.id);
//        }
//    });


    $('#dialogEditPrograma').dialog({
        autoOpen: false,
        width: 400,
        height: 300,
        modal: true,
        buttons: {
            "Guardar": function () {
                var bValid = true;
                allFields.removeClass("ui-state-error");
                //bValid = bValid && checkLength(proname, "grupo", 3, 50);                

                //if (bValid) {
                allFields.removeClass("ui-state-error");
                SavePrograma("/sedoc/Admin/EditPrograma/", $(this));
                $(this).dialog("close");
                //}
            },
            "Cancelar": function () {
                allFields.removeClass("ui-state-error");
                $(this).dialog("close");
            }
        }
    });

    $("#dialogDeleteProgramaConfirm").dialog({
        resizable: false,
        autoOpen: false,
        height: 140,
        modal: true,
        buttons: {
            "Eliminar": function () {
                DeletePrograma("/sedoc/Admin/DeletePrograma/", $(this));
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
                    url: "/sedoc/Admin/SortProgramas/?stridq=" + idq,
                    type: 'post',
                    dataType: 'json',
                    data: data2send,
                    contentType: 'application/json; charset=utf-8',
                    success: function (data) {
                        if (data != null) {
                            $.each(data, function (key, val) {
                                setEditAction(val['id'], val['label'], val['idf']);
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

    function DeletePrograma(route, jquerydialog) {
        var idp = $('#iddepartamento').val();
        if (idp == "") {
            alert("El programa no puede ser eliminado.");
        } else {
            var prog = '{ "id": ' + idp + ' }';
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: prog,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    deletedResponse(data, jquerydialog);
                }
            });
        }
    }

    function SavePrograma(route, jquerydialog) {
        var prog = getPrograma();
        if (prog != null) {
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: prog,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    savedResponse(data, jquerydialog);
                    setEditAction(data.iddepartamento, data.nombre,data.idfacultad);
                }
            });
        }
    }

    function savedResponse(data, jquerydialog) {
        if (data.iddepartamento != 0) {
            $("#work-area").html("");
            $('#lblEd' + data.iddepartamento).html(data.nombre);
            allFields.val("").removeClass("ui-state-error");
            updateTips("");
            setEditAction(data.iddepartamento, data.nombre);
            jquerydialog.dialog("close");
        } else {
            updateTips("Lo siento!, el registro no se pudo efectuar ... disculpame");
        }
    }

    function deletedResponse(data, jquerydialog) {
        if (data.iddepartamento != 0) {
            $("#work-area").html("");
            $('#btnDel' + data.iddepartamento).parent().parent().remove();
            jquerydialog.dialog("close");
        } else {
            alert("Lo siento!, la eliminaci&oacute;n no se pudo efectuar ... disculpame");
        }
    }

    //    function savedResponse(data, jquerydialog) {
    //        if (data.iddepartamento != 0) {
    //            $('#btnDel' + data.iddepartamento).parent().parent().find("td").eq(1).html(data.nombre);
    //            jquerydialog.dialog("close");
    //        } else {
    //            alert("Lo siento!, el registro no se pudo efectuar ... disculpame");
    //        }
    //    }

    function setEditAction(idP, nameP, idF) {
        $('#btnEdit' + idP).click(function () {
            $('#dialogEditPrograma').dialog('open');
            $("#iddepartamento").val(idP);
            $('#nombre').val(nameP);
            $("#idfacultad").val(idF);
            return false;
        });

        $('#btnDel' + idP).click(function () {
            $('#dialogDeleteProgramaConfirm').dialog('open');
            $('#iddepartamento').val(idP);
            return false;
        });
    }


    function setFacultadId(ui) {
        $("#idfacultad").val(ui.item.id);
        $("#work-area").html("");
    }

    function setGrupoId(ui) {
        $("#iddepartamento").val(ui.item.id);
    }

    function getPrograma() {
        var idp = $("#iddepartamento").val();
        var nom = $("#nombre").val();
        var idf = $("#idfacultad").val();
        return (idp == "" || nom == "" || idf == "") ? null : '{ "iddepartamento": "' + idp + '", "nombre": "' + nom + '","idfacultad": "' + idf + '" }';
    }

    function loadGroups(idF, route) {
        $.getJSON(route, { term: idF }, function (data) {
            $.each(data, function (key, val) {
                $("#sortable").append("<li id='listElement_" + val["id"] + "'><table class=list' style='width:100%;'><tr>" +
							"<td>" + val['label'] + "</td>" +                            
                						"</tr></table></li>");
                $("#work-area").html("");
                $("#work-area").append("<table id='TableQuestionnaire' class='list' style='width:100%'>" +
                            "</thead></table>" +
                            "<div id='sortdiv' style='width:100%;'>" +
                            "<ul id='sortable'></ul>" +
                            "</div><br/>");
                loadSortable(ui.item.id);
                setEditAction(val['id'], val['label'], val['idf']);
            });
        });
    }

});

