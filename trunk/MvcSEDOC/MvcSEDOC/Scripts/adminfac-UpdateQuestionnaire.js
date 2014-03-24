function setEditAction(idQ, tipQ) {
    $('#btnEdit' + idQ).click(function () {
        $("#idcuestionario").val(idQ);
        $("#tipocuestionario").val(entitiesDecode(tipQ));
        $('#dialogEditQuestionnaire').dialog('open');
        return false;
    });

    $('#btnDel' + idQ).click(function () {
        $('#idcuestionario').val(idQ);
        $('#dialogDeleteQuestionnaireConfirm').dialog('open');
        return false;
    });
}

function entitiesDecode(Str) {
    var decoded = $("<div/>").html(Str).text();
    return decoded;
}

$(function () {
    var searchVar = "/sedoc/AdminFac/SearchQuestionnaire/";

    var questionnairetype = $("#tipocuestionario"),
			idquestionnaire = $("#idcuestionario"),
			allFields = $([]).add(questionnairetype).add(idquestionnaire),
			tips = $(".field-validation-valid");

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

    $('#dialogEditQuestionnaire').dialog({
        autoOpen: false,
        width: 400,
        height: 200,
        modal: true,
        buttons: {
            "Guardar": function () {
                var bValid = true;
                allFields.removeClass("ui-state-error");
                bValid = bValid && checkLength(questionnairetype, "tipo de cuestionario", 3, 30);				
                if (bValid) {
                    SaveQuestionnaire("/sedoc/AdminFac/EditQuestionnaire/", $(this))
                }
            },
            "Cancelar": function () {
                allFields.val("").removeClass("ui-state-error");
                updateTips("");
                $(this).dialog("close");
            }
        }
    });

    $("#dialogDeleteQuestionnaireConfirm").dialog({
        resizable: false,
        autoOpen: false,
        height: 140,
        modal: true,
        buttons: {
            "Eliminar": function () {
                DeleteQuestionnaire("/sedoc/AdminFac/DeleteQuestionnaire/", $(this));
            },
            "Cancelar": function () {
                allFields.val("").removeClass("ui-state-error");
                updateTips("");
                $(this).dialog("close");
            }
        }
    });

    function DeleteQuestionnaire(route, jquerydialog) {
        var idQ = $('#idcuestionario').val();
        if (idQ == "") {
            alert("El cuestionario no puede ser eliminado");
        } else {
            var quest = '{ "id": ' + idQ + ' }';
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: quest,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    deletedResponse(data, jquerydialog);
                }
            });
        }
    }
    function SaveQuestionnaire(route, jquerydialog) {
        var questionnaire = getQuestionnaire();
        if (questionnaire == null) {
            updateTips("El cuestionario no debe ser vacío");
        } else {
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: questionnaire,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    savedResponse(data, jquerydialog);
                }
            });
        }
    }

    function savedResponse(data, jquerydialog) {
        if (data.idcuestionario != 0) {
            $("#work-area").html("");
            $('#lblEd' + data.idcuestionario).html(data.tipocuestionario);
            allFields.val("").removeClass("ui-state-error");
            updateTips("");
            setEditAction(data.idcuestionario, data.tipocuestionario);
            jquerydialog.dialog("close");
        } else {
            updateTips("Lo siento!, el registro no se pudo efectuar ... disculpame");
        }
    }

    function deletedResponse(data, jquerydialog) {
        if (data.idcuestionario != 0) {
            $("#work-area").html("");
            $('#btnDel' + data.idcuestionario).parent().parent().remove();
            jquerydialog.dialog("close");
        } else {
            alert("Lo siento!, la eliminaci&oacute;n no se pudo efectuar ... disculpame");
        }
    }

    function getQuestionnaire() {
        var idq = $("#idcuestionario").val();
        var tip = $("#tipocuestionario").val();
        return (idq == "" || tip == "") ? null : '{ "idcuestionario": "' + idq + '", "tipocuestionario": "' + tip + '" }';
    }
});