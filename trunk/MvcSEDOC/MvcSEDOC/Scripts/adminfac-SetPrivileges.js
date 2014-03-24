var docenteSelected;
function openDialog(valor) {
    docenteSelected = valor;
    $("#dialogUpDateDepartmentChief").dialog('open');
}

function prueba(id) {
    alert(id);
}
var searchVar = "/sedoc/AdminFac/SearchDepartment/";
var nameDept, iddept, idCurrentPeriod, chiefId;

// fija el id y el nombre del departamento
function setDepartment(ui) {
    nameDept = ui.item.label; // nombre del departamento 
    iddept = ui.item.id;   // id del departamento     
}

function loadTeachers(idD, route) {
    $.getJSON(route, { term: idD }, function (data) {
        // contenido de la tabla           
        $.each(data, function (key, val) {
            var checked = "";
            if (val["id"] == chiefId) {
                checked = "checked";
            }

            $("#TeachersFrom" + idD + " tbody").append("<tr>" +
                    "<td>" + val['apellidos'] + "</td>" +
                    "<td>" + val['nombres'] + "</td>" +
                    "<td align='center'> <input type= 'radio' name='group1' id='" + val['id'] + "' " + checked + " onClick='openDialog(this.id)' /></td>" +
						          "</tr>");
        });
    });
}

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
            loadTeachers(iddept, "/sedoc/AdminFac/GetTeachersFromDepartment1/");
        }
    });
}

// carga  la cabecera de la tabla
function loadTable(idDepartamento) {
    iddept = idDepartamento;
    if (idDepartamento != 0) {
        nameDept = $("#dep" + iddept).attr("depname");
        $("#work-area").html("<table id='TeachersFrom" + iddept + "' class='list' style='width:100%'>" +
                            "<thead><tr><th align='center' colspan=3>Docentes del departamento de " + nameDept + "</th></tr>" +
                            "<tr class='sub'><th>Apellidos</th><th>Nombres</th><th align='center' style='width:160px;'>Jefe de Departamento</th></tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>");
        getCurrentDepartmentChief("/sedoc/AdminFac/GetCurrentDepartmentChief/");
    }
}

// obtiene el ultimo periodo academico
function getLastAcademicPeriod(route) {
    $.ajax({
        cache: false,
        url: route,
        type: 'Post',
        dataType: 'json',
        data: null,
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            idCurrentPeriod = data.idperiodo;
            getCurrentDepartmentChief("/sedoc/AdminFac/GetCurrentDepartmentChief/");
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

$(function () {
    /*  Buscar departamento*/
    $("#search_department").autocomplete({
        source: searchVar,
        minLength: 1,
        select: function (event, ui) {
            if (ui.item) {
                setDepartment(ui);
                loadTable(ui.item.id);
            }
        }
    });


    // dialogo para confirmar el cambio de jefe de departamento
    $("#dialogUpDateDepartmentChief").dialog({
        resizable: false,
        autoOpen: false,
        height: 140,
        modal: true,
        buttons: {
            "Cambiar": function () {
                updateCurrentChief("/sedoc/AdminFac/UpdateCurrentChief/");
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