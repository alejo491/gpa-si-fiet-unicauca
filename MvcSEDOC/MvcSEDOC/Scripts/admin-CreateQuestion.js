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

    $('#dialogCreateQuestion').dialog({
        autoOpen: false,
        width: 400,
        height: 300,
        modal: true,
        buttons: {
            "Crear": function () {
                var bValid = true;
                allFields.removeClass("ui-state-error");
                bValid = bValid && checkLength(question, "pregunta", 3, 150);

                if (bValid) {
                    SaveQuestion("/sedoc/Admin/AjaxSetQuestion/", $(this))
                }

            },
            "Cancelar": function () {
                allFields.val("").removeClass("ui-state-error");
                updateTips("");
                $(this).dialog("close");
            }
        }
    });

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
            $("#TableidGroup" + data["idgrupo"] + " tbody").append("<tr>" +
							"<td>" + data["idpregunta"] + "</td>" +
							"<td>" + data["pregunta1"] + "</td>" +
						"</tr>");
            allFields.val("").removeClass("ui-state-error");
            updateTips("");
            jquerydialog.dialog("close");
        } else {
            updateTips("Lo siento!, el registro no se pudo efectuar ... disculpame");
        }
    }

    function getQuestion() {
        var idg = $("#idgrupo").val();
        var preg = $("#pregunta1").val();
        return (preg == "" || idg == "") ? null : '{ "idgrupo": "' + idg + '", "pregunta1": "' + preg + '" }';
    }

    function setQuestionnaireTxt(message) {
        $("#selected_questionnaire_txt").val(message);
        $("#QuestName").text(message);
    }

    function setAddAction(groupId, groupName) {
        $('#btnAdd' + groupId).click(function () {
            $('#dialogCreateQuestion').dialog('open');
            $('#idgrupo').val(groupId);
            $("#GroupName").text(groupName);
            return false;
        });
    }

    function loadGroups(idQ, route) {
        $.getJSON(route, { term: idQ }, function (data) {
            $.each(data, function (key, val) {
                $("#work-area").append("<table id='TableidGroup" + val['id'] + "' class='list' style='width:100%'>" +
                            "<thead><tr><th colspan=2>" + val['label'] + "</th></tr>" +
                            "<tr class='sub'><th style='width:30px;'>id</th><th>Pregunta</th></tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><a href='#' class='add' id='btnAdd" + val['id'] + "'>Agregar</a><br/><br/>");
                setAddAction(val['id'], val['label']);
                loadQuestionsFromGroup(val['id'], "/sedoc/Admin/GetQuestionsFromGroup/");
            });
        });
    }

    function loadQuestionsFromGroup(idG, route) {
        $.getJSON(route, { term: idG }, function (data) {
            $.each(data, function (key, val) {
                $("#TableidGroup" + idG + " tbody").append("<tr>" +
							"<td>" + val['id'] + "</td>" +
							"<td>" + val['label'] + "</td>" +
						"</tr>");
            });
        });
    }


    $("#search_term").autocomplete({
        source: searchVar,
        minLength: 1,
        select: function (event, ui) {
            if (ui.item) {
                setQuestionnaireTxt(ui.item.value);
                $("#work-area").html("");
                loadGroups(ui.item.id, "/sedoc/Admin/GetQuestionnaireGroups/");
            }
        }
    });
});

