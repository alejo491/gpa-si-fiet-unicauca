$(function () {
    setAsignacion();

    function setAsignacion() {
        $("#work-area1").html("");
        $("#work-area1").append("<form action='/WorkChief/evaluateSubordinates2' method='post'> <fieldset>" +
                            "<table id='TableidTeaching' class='list' style='width:100%'>" +
                            "<thead><tr><th  >Evaluacion del Jefe de Labor </th></tr>" +
                            "<tbody></tbody>" +
                            "</table><br/>" +
                            "<p>" +
                            "<input type='submit' value='Guardar'/>" +
                            "</p>" +
                             "</fieldset>");
    }

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
            updateTips("El tamaño de aqui " + n + " debe estar entre " +
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
        autoOpen: true,
        width: 500,
        height: 600,
        modal: true,
        buttons: {
            "Guardar Evaluación": function () {
                confirmdialog = false;
                var bValid = true;
                allFields.removeClass("ui-state-error");
                questions.each(function (index) {
                    bValid = bValid && checkRange($(this), "evaluacion", 1, 100);
                });

                if (bValid) {
                    allFields.removeClass("ui-state-error");
                    SaveEval("/sedoc/Admin/UpdateEval/", $(this));
                    $(this).dialog("close");
                }
            },
            "Cancelar": function () {
                confirmdialog = true;
                $(this).dialog("close");
                redireccionar();
            }
        },
        beforeClose: function (event, ui) {
            if (confirmdialog) {
               
            }
        }
    });

    function redireccionar() {
        var pagina = "/sedoc/Admin/AssessLaborChief/";
        location.href = pagina;
    }

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
                        alert("Evaluación grabada éxitosamente");
                        redireccionar();
                    } else {
                        alert("Lo siento no pude grabar la evaluación, comunicate con el administrador");
                        redireccionar();
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

        var EvEst = 1;

        var EvAut = 1;
        
        var cal = '[ ';
        $("input.calif").each(function (index) {
            cal += '{ "idgrupo": "' + $(this).attr("idgrupo") + '", "idpregunta": "' + $(this).attr("idpregunta") + '", "calificacion": "' + $(this).val() + '" },';
        });
        cal = cal.substring(0, cal.length - 1) + ' ]';

        return (idL == "" || idD == "" || idE == "" || idC == "" || EvEst == "" || EvAut == "") ? null : '{ "idlabor": "' + idL + '", "iddocente": "' + idD + '","idevaluacion": "' + idE + '", "EvalEst":"' + EvEst + '", "EvalAut":"' + EvAut + '", "idcuestionario": "' + idC + '","calificaciones": ' + cal + ' }';
    }
        
});