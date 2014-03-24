$(function () {
    /************* VARIABLES *********************/
    var searchVar = "/sedoc/Admin/SearchQuestionnaire/";
    var questionnaireVar = "/sedoc/Admin/GetQuestionnaireGroups/";
    var saveVar = "/sedoc/Admin/CreateQuestionnaire/";

    var typeQuestionnaire = $("#tipoCuestionario"),
        allFields = $([]).add(typeQuestionnaire),
        tips = $("#dialog_validation_msg");


    /****** Inicializaciones JQuery *******/

    $('#dialogCreateQuestionnaire').dialog({
        autoOpen: false,
        width: 400,
        height: 200,
        modal: true,
        buttons: {
            "Crear": function () {
                var bValid = true;
                allFields.removeClass("ui-state-error");
                bValid = bValid && checkLength(typeQuestionnaire, "tipo de Cuestionario", 3, 30);
                if (bValid) {
                    SaveQuestionnaire("/sedoc/Admin/AjaxSetQuestionnaire/", $(this))
                }
            },
            "Cancelar": function () {
                allFields.val("").removeClass("ui-state-error");
                updateTips("");
                $(this).dialog("close");
            }
        }
    });

    $('#btnAddQuestionnaire').click(function () {
        $('#dialogCreateQuestionnaire').dialog('open');
        return false;
    });

    $("#search_term").autocomplete({
        source: searchVar,
        minLength: 1,
        select: function (event, ui) {
            if (ui.item) {
                setQuestionnaireId(ui);
            }
        }
    });

    /**************** FUNCIONES AUXILIARES *****************/

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

    function SaveQuestionnaire(route, jquerydialog) {
       
        var typeQuestionnaire = getQuestionnaire();
        
        if (typeQuestionnaire == null) {
            updateTips("El tipo de cuestionario no debe ser vacio");
        } 
        else {
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: typeQuestionnaire,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    savedResponse(data, jquerydialog);
                }
            });

        }
    }

    function savedResponse(data, jquerydialog) {
        
        if (data.idcuestionario != 0) {
            alert("El cuestionario fue creado con exito");
            jquerydialog.dialog("close");
        } 
        else {
            updateTips("Ya existe un cuestionario con ese nombre");
        }
    }

    function getQuestionnaire() {
        var typeq = $("#tipoCuestionario").val();
        return (typeq == "") ? null : '{ "tipocuestionario" :  "' + typeq + '"}';
    }

    function setQuestionnaireId(ui) {
        $("#idcuestionario").val(ui.item.id);
    }

    function loadGroups(idQ, route) {
        $.getJSON(route, { term: idQ }, function (data) {
            $.each(data, function (key, val) {
                $("#work-area").append("<table id='TableidGroup" + val['id'] + "' class='list' style='width:100%'>" +
                            "<thead><tr><th colspan=3>" + val['label'] + "</th></tr>" +
                            "<tr class='sub'><th style='width:30px;'>id</th><th>Pregunta</th><th style='width:130px;'>Acci&oacute;n</th></tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>");
                loadQuestionsFromGroup(val["id"], "/sedoc/Admin/GetQuestionsFromGroup/");
            });
        });
    }

});