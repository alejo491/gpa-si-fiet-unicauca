$(document).ready(function () {
    $("#cal").change(function () {
        valor = $(this).val();
        if (isNaN(valor)) {
            $('#dialogErrorRango').dialog("open");
        }
        else if(valor < 0 || valor > 100) {
            $('#dialogErrorRango').dialog("open");
        }
    });

});

$(function () {
    save();
    //Cuando el evento click es capturado en el enlace guardar abre el dialogo
    function save() {
        $('#save').click(function () {
            if (validarEntero()) {
                $('#dialogSaveAutoEvalaucionConfirm').dialog('open');
            }
            return false;
        });
    }

    function validarEntero() {
        var sonNumeros = true;
        $("input.calif").each(function (index) {
            var valor = $("#cal").val();
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
            else {
                sonNumeros = false;
                $('#dialogValorNulo').dialog("open");
                return;
            }
        });
        return sonNumeros;

    }

    $(function () {
        //adiciona los botones en la ventana del diajogo guardar
        $("#dialogSaveAutoEvalaucionConfirm").dialog({
            resizable: false,
            autoOpen: false,
            height: 140,
            modal: true,
            buttons: {
                "Guardar": function () {
                    loadDateTable();
                    $(this).dialog("close");

                },
                "Cancelar": function () {
                    confirmdialog = true;
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

        $('#dialogValorNulo').dialog({
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

        var iddocente = document.getElementById("search_term").value;
        var idlabor = document.getElementById("idLabor").value;
        var tipolabor = document.getElementById("tipoLabor").value;
        var cal = $("#cal").val();
        var PDesc = $("#PDescripcion").val();
        var PSol = $("#PSolucion").val();
        var RDesc = $("#RDescripcion").val();
        var RSol = $("#RSolucion").val();
        //        //metodo que guarde los datos en la BD
        $.getJSON("/sedoc/Teacher/saveAutoevaluacion/",
                { idL: idlabor, idD: iddocente, val: cal, pdes: PDesc, psol: PSol, rdes: RDesc, rsol: RSol },
                function (data) {
                });
          setTimeout(redireccionar, 1000);
    }

    function resultado(data) {
        if (data.respuesta == 0) {
            alert("Lo siento!, los datos no se pueden guardar... disculpame");
        }
    }

    function redireccionar() {
        var pagina = "/sedoc/Teacher/EvaluarLabor/";
        location.href = pagina;
    }

});


















