$(function () {
    var searchVar = "/sedoc/Admin/SearchQuestionnaire/";

    var question = $("#pregunta1"),
			idgroup = $("#idgrupo"),
			allFields = $([]).add(question).add(idgroup),
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

    $('#dialogEditQuestion').dialog({
        autoOpen: false,
        width: 400,
        height: 300,
        modal: true,
        buttons: {
            "Guardar": function () {
                var bValid = true;
                allFields.removeClass("ui-state-error");
                bValid = bValid && checkLength(question, "pregunta", 3, 150);

                if (bValid) {
                    SaveQuestion("/sedoc/Admin/EditQuestion/", $(this))
                }
                updateTips("");
                $(this).dialog("close");
            },
            "Cancelar": function () {
                allFields.val("").removeClass("ui-state-error");
                updateTips("");
                $(this).dialog("close");
            }
        }
    });

    $("#dialogDeleteQuestionConfirm").dialog({
        resizable: false,
        autoOpen: false,
        height: 140,
        modal: true,
        buttons: {
            "Eliminar": function () {
                DeleteQuestion("/sedoc/Admin/DeleteQuestion/", $(this));
            },
            Cancelar: function () {
                allFields.val("").removeClass("ui-state-error");
                updateTips("");
                $(this).dialog("close");
            }
        }
    });

    function DeleteQuestion(route, jquerydialog) {
        var idp = $('#idpregunta').val();
        if (idp == "") {
            alert("La pregunta no puede ser eliminada.");
        } else {
            var preg = '{ "id": ' + idp + ' }';
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: preg,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    deletedResponse(data, jquerydialog);
                }
            });
        }
    }
    function SaveQuestion(route, jquerydialog) {
        var question = getQuestion();
        if (question == null) {
            updateTips("La pregunta no debe ser vacía");
        } else {
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: question,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    savedResponse(data, jquerydialog);
                }
            });
        }
    }

    function savedResponse(data, jquerydialog) {
        if (data.idpregunta != 0) {
            $('#lblEdt' + data.idpregunta).html(data.pregunta1);
            allFields.val("").removeClass("ui-state-error");
            updateTips("");
            jquerydialog.dialog("close");
        } else {
            updateTips("Lo siento!, el registro no se pudo efectuar ... disculpame");
        }
    }

    function deletedResponse(data, jquerydialog) {
        if (data.idpregunta != 0) {
            $('#btnDel' + data.idpregunta).parent().parent().remove();
            jquerydialog.dialog("close");
        } else {
            alert("Lo siento!, la eliminaci&oacute;n no se pudo efectuar ... disculpame");
        }
    }

    function getQuestion() {
        var idp = $("#idpregunta").val();
        var preg = $("#pregunta1").val();
        var idg = $("#idgrupo").val();
        return (preg == "" || idp == "" || idg == "") ? null : '{ "idpregunta": "' + idp + '", "pregunta1": "' + preg + '","idgrupo": "' + idg + '" }';
    }

    function setEditAction(idP, txtP, idG) {
        $('#btnEdt' + idP).click(function () {
            $('#dialogEditQuestion').dialog('open');
            $("#idgrupo").val(idG);
            $('#idpregunta').val(idP);
            $("#pregunta1").text($('#lblEdt' + idP).html());
            return false;
        });

        $('#btnDel' + idP).click(function () {
            $('#dialogDeleteQuestionConfirm').dialog('open');
            $('#idpregunta').val(idP);
            return false;
        });
    }

    function setQuestionnaireId(ui) {
        $("#idQuestionnaire").val(ui.item.id);
        $("#work-area").html("");
    }

    function loadGroups(idQ, route) {
        $.getJSON(route, { term: idQ }, function (data) {
            $.each(data, function (key, val) {
                $("#work-area").append("<table id='TableidGroup" + val['id'] + "' class='list' style='width:100%'>" +
                            "<thead><tr><th colspan=2>" + val['label'] + "</th></tr>" +
                            "<tr class='sub'><th>Pregunta</th><th style='width:130px;'>Acci&oacute;n</th></tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>");
                loadQuestionsFromGroup(val["id"], "/sedoc/Admin/GetQuestionsFromGroup/");
            });
        });
    }

    function loadQuestionsFromGroup(idG, route) {
        $.getJSON(route, { term: idG }, function (data) {
            $.each(data, function (key, val) {
                $("#TableidGroup" + idG + " tbody").append("<tr>" +
							"<td id='lblEdt" + val['id']+"' >" + val['label'] + "</td>" +
                            "<td><a href='#' class='edit' id='btnEdt" + val['id'] + "'>Editar</a> |" +
                            "    <a class='delete' href='#' id='btnDel" + val["id"] + "'>Eliminar</a>" +
                            "</td>" +
						"</tr>");
                setEditAction(val['id'], val['label'], val['idg']);
            });
        });
    }

    $("#search_term").autocomplete({
        source: searchVar,
        minLength: 1,
        select: function (event, ui) {
            if (ui.item) {
                setQuestionnaireId(ui);
                loadGroups(ui.item.id, "/sedoc/Admin/GetQuestionnaireGroups/");
            }
        }
    });

});