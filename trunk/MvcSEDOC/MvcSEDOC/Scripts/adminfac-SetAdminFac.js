var docenteSelected;

function openDialog(valor) {
    docenteSelected = valor;
    $("#dialogUpDateAdminFac").dialog('open');
}

function prueba(id) {
    alert(id);
}
var searchVar = "/sedoc/AdminFac/SearchDepartment/";
var nameDept, iddept, idCurrentPeriod;
var  chiefId =-1;
var checked = "";
// fija el id y el nombre del departamento
function setDepartment(ui) {
    nameDept = ui.item.label; // nombre del departamento 
    iddept = ui.item.id;   // id del departamento     
}

function loadTeachers(idD, route) {

    $.getJSON(route, { term: idD }, function (data) {
        // contenido de la tabla           
        $.each(data, function (key, val) {
            checked = "";           
            if (val["chiefId"] != -1) {
                checked = "checked";
                chiefId = val["chiefId"];
            }
            $("#TeachersFrom" + idD + " tbody").append("<tr>" +
                    "<td>" + val['nombre'] + "</td>" +
                    "<td align='center'> <input type= 'radio' name='group1' id='" + val['idusuario'] + "' " + checked + " onClick='openDialog(this.id)' /></td>" +
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
                chiefId = data.idusuario;               
            }            
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
                            "<tr class='sub'colspan=2><th>Docente</th><th align='center' style='width:160px;'>Administrador</th></tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>");       
        getCurrentDepartmentChief("/sedoc/AdminFac/GetCurrentAdminFac/");
        loadTeachers(iddept, "/sedoc/AdminFac/GetTeachersFromDepartment2/");
    }
}


// actualiza el jefe del departamento
function updateCurrentChief(route) {
    var param = '{ "idDepartamento": ' + iddept + ', "idDocentAct": ' + chiefId + ', "idDocentNue": ' + docenteSelected + ' }';
    $.ajax({
        cache: false,
        url: route,
        type: 'POST',
        dataType: 'json',
        data: param,
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            iddept = data.iddepartamento;
            chiefId = data.idusuario;          
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
    $("#dialogUpDateAdminFac").dialog({
        resizable: false,
        autoOpen: false,
        height: 140,
        modal: true,
        buttons: {
            "Cambiar": function () {
                updateCurrentChief("/sedoc/AdminFac/UpdateAdminFac/");
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