var laboresId;
var idLab;

function setEvaluations() {
    if (validarEntero()) {
        $('#dialogSaveConfirm').dialog("open");
    } 
 }

function validarEntero() {
    var sonNumeros = true;
    $("input.calif").each(function (index) {
        var valor = $(this).attr("value");
        idLab = $(this).attr("idlabor");
        // si el imput no esta vacio
        if (valor != "") {
            // recupero en valor del input
            var numero = parseInt(valor);
            //Compruebo si es un valor numérico 
            if (isNaN(numero)) {
                //entonces (no es numero) fijo la variable sonNumeros en falso y salgo del each                  
                sonNumeros = false;
                $('#dialogErrorTexto').dialog("open");
                return;
            } else {
                if (numero < 0 || numero > 100) {
                    //entonces no es un valor valido fijo la variable sonNumeros en falso y salgo del each                  
                    sonNumeros = false;
                    $('#dialogErrorRango').dialog("open");
                    return;
                }
            }
        }
    });
    return sonNumeros;

}


$(function () {
    $('#dialogSaveConfirm').dialog({
        autoOpen: false,
        width: 400,
        height: 200,
        modal: true,
        buttons: {
            "Guardar": function () {
                loadDateTable();
                $(this).dialog("close");
            },
            "Cancelar": function () {
               
                $(this).dialog("close");
            }
        }
    });

    $('#dialogErrorRango').dialog({
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

    $('#dialogErrorTexto').dialog({
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
});

function loadDateTable() {
    var tabla = document.getElementById("LaborTeach");
    var numFilas = tabla.rows.length;

    for (i = 0; i < numFilas - 2; i++) {
        var idLabor = idLab;
        var idDocente = document.getElementById("iddocente").value;
        var valor = document.getElementById("input" + idLabor).value;
              //metodo que guarde los datos en la BD
            $.getJSON("/sedoc/Teacher/saveEval/", { idL: idLabor, idD: idDocente, val: valor }, function (data) {
                if (i == numFilas - 2) {
                    setTimeout(redireccionar, 1000);
                }
            });
      }
}

function redireccionar() {
    //var iddocente = document.getElementById("search_term").value;
    var pagina = "/sedoc/Teacher/index/";
    location.href = pagina;
}








    

   