$(function () {
    var questions = $("input.calif"),
            evalest = $("#calif_est"),
            evalauto = $("#calif_auto"),
			allFields = $([]).add(questions).add(evalest).add(evalauto),
			tips = $("#validation-messages");

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

    var confirmdialog = true;

    $('#dialogEvalSubordinate').dialog({
        autoOpen: false,
        width: 500,
        height: 600,
        modal: true,
        buttons: {
            "Guardar Evaluación": function () {
                confirmdialog = false;
                var bValid = true;
                allFields.removeClass("ui-state-error");

                bValid = bValid && checkRange(evalest, "evaluación de estudiantes", 1, 100);
                bValid = bValid && checkRange(evalauto, "autoevaluación", 1, 100);

                questions.each(function (index) {
                    bValid = bValid && checkRange($(this), "evaluacion", 1, 100);
                });

                if (bValid) {
                    allFields.removeClass("ui-state-error");
                    SaveEval("/sedoc/WorkChief/UpdateEval/", $(this));
                    $(this).dialog("close");
                }
            },
            "Cancelar": function () {
                confirmdialog = true;
                $(this).dialog("close");
            }
        },
        beforeClose: function (event, ui) {
            if (confirmdialog) {
                if (!confirm("¿Seguro desea Cancelar sin guardar ningun cambio?")) {
                    confirmdialog = true;
                    return false;
                }
            }
        }
    });

    function SaveEval(route, jquerydialog) {
        var evaluacion = getEval();
        if (evaluacion != null) {
            var request = $.ajax({
                cache: false,
                url: route,
                type: 'POST',
                dataType: 'json',
                data: evaluacion,
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    if (data.toString() != "-1") {
                        $("#" + $('#idEvaluacion').val()).html(data.toString());
                        $("#Est_" + $('#idEvaluacion').val()).html($("#calif_est").val());
                        $("#Auto_" + $('#idEvaluacion').val()).html($("#calif_auto").val());
                        alert("Evaluación grabada éxitosamente");
                    } else {
                        alert("Lo siento no pude grabar la evaluación, comunicate con el administrador");
                    }

                }
            });
        }
    }

    function getEval() {
        var idL = $("#idLaborEvaluar").val();
        var idD = $("#idDocenteEvaluado").val();
        var idE = $("#idEvaluacion").val();
        var idC = $("#idCuestionario").val();
        var EvEst = $("#calif_est").val();
        var EvAut = $("#calif_auto").val();

        var cal = '[ ';
        $("input.calif").each(function (index) {
            cal += '{ "idgrupo": "' + $(this).attr("idgrupo") + '", "idpregunta": "' + $(this).attr("idpregunta") + '", "calificacion": "' + $(this).val() + '" },';
        });
        cal = cal.substring(0, cal.length - 1) + ' ]';

        return (idL == "" || idD == "" || idE == "" || idC == "" || EvEst == "" || EvAut == "") ? null : '{ "idlabor": "' + idL + '", "iddocente": "' + idD + '","idevaluacion": "' + idE + '", "EvalEst":"' + EvEst + '", "EvalAut":"' + EvAut + '", "idcuestionario": "' + idC + '","calificaciones": ' + cal + ' }';
    }

    $('a.edit').click(function () {
        var ideval = $(this).attr("ideval");
        $("input.calif").val("");
        $('#dialogEvalSubordinate').dialog('open');
        $('#idLaborEvaluar').val($(this).attr("idlabeval"));
        $('#idDocenteEvaluado').val($(this).attr("iddoc"));
        $('#idEvaluacion').val(ideval);
        if ($('#Est_' + ideval).text() != "No tiene") {
            $('#calif_est').val($('#Est_' + ideval).text());
        } else {
            $('#calif_est').val("1");
        }
        if ($('#Auto_' + ideval).text() != "No tiene") {
            $('#calif_auto').val($('#Auto_' + ideval).text());
        } else {
            $('#calif_auto').val("1");
        }
        $('#DocName').html($(this).attr("docname"));
        return false;
    });
});