function setEditAction(idF, nomF) {
    $('#btnEdit' + idF).click(function () {
        $("#idfacultad").val(idF);
        $("#fac_nombre").val(entitiesDecode(nomF));       
        return false;
    });
}

function entitiesDecode(Str) {
    var decoded = $("<div/>").html(Str).text();
    return decoded;
}

$(function () {
        var searchVar = "/sedoc/Admin/SearchFacultad/";

        var nombre = $("#fac_nombre"),
    			idfac = $("#idfacultad"),
    			allFields = $([]).add(nombre).add(idfac),
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


    function savedResponse(data, jquerydialog) {
        if (data.idfacultad != 0) {
            $("#work-area").html("");
            $('#lblEd' + data.idfacultad).html(data.fac_nombre);
            allFields.val("").removeClass("ui-state-error");
            updateTips("");
            setEditAction(data.idfacultad, data.fac_nombre);
            jquerydialog.dialog("close");
        } else {
            updateTips("Lo siento!, el registro no se pudo efectuar ... disculpame");
        }
    }


    function getFacultad() {
        var idF = $("#idfacultad").val();
        var nomF = $("#fac_nombre").val();
        return (idF == "" || nomF == "") ? null : '{ "idfacultad": "' + idF + '", "fac_nombre": "' + nomF + '" }';
    }
});