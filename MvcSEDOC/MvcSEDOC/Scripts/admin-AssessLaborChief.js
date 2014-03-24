
$(function () {

    $('#btnGuardar').click(function () {
        if (validarEntero()) {
            $('#dialogSaveConfirm').dialog("open");
        } else {
            $('#dialogError').dialog('open');
        }
        return false;
    });


    $('#dialogSaveConfirm').dialog({
        autoOpen: false,
        width: 400,
        height: 200,
        modal: true,
        buttons: {
            "Guardar": function () {
                Save();
                $(this).dialog("close");
            },
            "Cancelar": function () {
                assessLaborChief();
                $(this).dialog("close");
            }
        }
    });

    $('#dialogError').dialog({
        autoOpen: false,
        width: 400,
        height: 200,
        modal: true,
        buttons: {
            "Aceptar": function () {
                $(this).dialog("close");
            }
        }
    });

    function validarEntero() {
        var sonNumeros = true;

        $("input.calif").each(function (index) {
            // si el imput no esta vacio           
            if ($(this).val() != "") {

                // recupero en valor del input
                var numero = $(this).val();
                //Compruebo si es un valor numérico 
                if (!/^([0-9])*$/.test(numero)) {
                    //entonces (no es numero) fijo la variable sonNumeros en falso y salgo del each                  
                    sonNumeros = false;
                    return;
                } else {
                    if (numero < 0 || numero > 100) {
                        //entonces no es un valor valido, fijo la variable sonNumeros en falso y salgo del each                  
                        sonNumeros = false;
                        return;
                    }
                }
            }
        });
        return sonNumeros;

    }

    function Save() {
        $("input.calif").each(function (index) {
            if ($(this).val() != "") {
                updateEvaluacion($(this).attr("ideval"), $(this).val());
            }
        });
    }

    function updateEvaluacion(idEvaluacion, calificacion) {
        // almacena el valor de la calificacion
        var strParametros = '{"idEvaluacion": ' + idEvaluacion + ', "calificacion": ' + calificacion + ' }';
        $.ajax({
            cache: false,
            url: "/sedoc/Admin/UpdateEvaluacion/",
            type: 'Post',
            dataType: 'json',
            data: strParametros,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
            }
        });
    }

    function assessLaborChief() {
        // carga nuevamente la vista
        $.ajax({
            cache: false,
            url: "/sedoc/Admin/AssessLaborChief/",
            type: 'Post',
            dataType: 'json',
            data: null,
            contentType: 'application/json; charset=utf-8',
            success: function (data) { }
        });
    }

});
/*
function getLaborsId() {
// obtiene los ids de las labores del ultimo periodo academico
$.ajax({
cache: false,
url: "/sedoc/Admin/GetLaborsId/",
type: 'Post',
dataType: 'json',
data: null,
contentType: 'application/json; charset=utf-8',
success: function (data) {
if (validarEntero(data) == true) {
laboresId = data;
$('#dialogSaveConfirm').dialog("open");
} else {
$('#dialogError').dialog('open');
}
}
});
}

function assessLaborChief() {
// carga nuevamente la vista
$.ajax({
cache: false,
url: "/sedoc/Admin/AssessLaborChief/",
type: 'Post',
dataType: 'json',
data: null,
contentType: 'application/json; charset=utf-8',
success: function (data) { }
});
}

*/