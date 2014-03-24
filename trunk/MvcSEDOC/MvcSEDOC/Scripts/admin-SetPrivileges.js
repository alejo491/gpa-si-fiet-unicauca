var searchVar = "/sedoc/Admin/SearchDepartment/";
var nameDept, iddept, idCurrentPeriod, chiefId;
var docenteSelected;

function openDialog(valor,idD) {
    docenteSelected = valor;
    iddept = idD;
    //obtiene el jefe actual
    getCurrentDepartmentChief("/sedoc/Admin/GetCurrentDepartmentChief/");   
    $("#dialogUpDateDepartmentChief").dialog('open');
}

$(function () {

    // dialogo para confirmar el cambio de jefe de departamento
    $("#dialogUpDateDepartmentChief").dialog({
        resizable: false,
        autoOpen: false,
        height: 320,
        modal: true,
        buttons: {
            "Cambiar": function () {
                updateCurrentChief("/sedoc/Admin/UpdateCurrentChief/");
                $(this).dialog("close");
            },
            "Cancelar": function () {
                if (chiefId == -1) {
                    document.getElementById(docenteSelected).checked = false;
                } else {
                    document.getElementById(chiefId).checked = true;
                }
                $(this).dialog("close");
            }
        }
    });

});

// obtiene el actual jefe de ese departamento
function getCurrentDepartmentChief(route) {
    var strDepto = '{"iddepartamento": ' + iddept + ' }';
    $.ajax({
        cache: false,
        url: route,
        type: 'Post',
        dataType: 'json',
        data: strDepto,
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            if (data == null) {
                chiefId = -1;
            } else {
                chiefId = data.iddocente;
            }            
        }
    });
}

// actualiza el jefe del departamento
function updateCurrentChief(route) {
    var worksValue = 1;
    if ($('input[id$="chkDocencia"]').is(':checked')) worksValue = worksValue * 3;
    if ($('input[id$="chkDesarrollo"]').is(':checked')) worksValue = worksValue * 5;
    if ($('input[id$="chkTrabajoGradoInv"]').is(':checked')) worksValue = worksValue * 7;
    if ($('input[id$="chkTrabajoGrado"]').is(':checked')) worksValue = worksValue * 11;
    if ($('input[id$="chkInvestigacion"]').is(':checked')) worksValue = worksValue * 13;
    if ($('input[id$="chkGestion"]').is(':checked')) worksValue = worksValue * 17;
    if ($('input[id$="chkSocial"]').is(':checked')) worksValue = worksValue * 19;
    if ($('input[id$="chkOtras"]').is(':checked')) worksValue = worksValue * 23;

    var param = '{ "idDepartamento": ' + iddept + ', "idDocentAct": ' + chiefId + ', "idDocentNue": ' + docenteSelected + ', "laboresCalc": ' + worksValue + '}';
    $.ajax({
        cache: false,
        url: route,
        type: 'POST',
        dataType: 'json',
        data: param,
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            iddept = data.iddepartamento;
            chiefId = data.iddocente;
            idCurrentPeriod = data.idperiodo;
        }
    });
}

