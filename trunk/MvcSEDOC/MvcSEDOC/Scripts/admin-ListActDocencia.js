var departamentoSelected;

function openDialog(valor) {
    departamentoSelected = valor;
    $("#dialogListDepartment").dialog('open');
}

function prueba(id) {
    alert(id);
}

var searchVarFac = "/sedoc/Admin/SearchFacultad/";
var searchVar = "/sedoc/Admin/SearchDepartment/";
var nameFac, idfac, nameDep, iddep;

// fija el id y el nombre de la facultad
function setFacultad(ui) {
    nameFac = ui.item.label; // nombre de la facultad
    idfac = ui.item.id;   // id de la facultad     
}

// fija el id y el nombre del departamento
function setDepartamento(ui) {
    nameDep = ui.item.label; // nombre de la departamento
    iddep = ui.item.id;   // id de la departamento     
}

function loadDepartamentos(idF, route) {
    $.getJSON(route, { term: idF }, function (data) {
        // contenido de la tabla           
        $.each(data, function (key, val) {           
            $("#DepartamentFrom" + idF).append("<option value='" + val['id'] + "'>" + val['label'] + "</option>");
        });
    });
}


function loadDept(idFacultad) {
    idfac = idFacultad;
    if (idFacultad != 0) {
        //nameFac = $("#fac" + idfac).attr("facname");
        $("TextBox").html("<select name='departamentos' id='DepartamentFrom" + idfac + "'>" +
                "<option value='0'>Seleccione El Programa</option>" +
                "</select>");
        loadDepartamentos(idFacultad, "/sedoc/Admin/GetFacultadPrograma/");
    }
}

//function loadProfesores(idD, route) {
//    $.getJSON(route, { term: idD }, function (data) {
//        // contenido de la tabla           
//        $.each(data, function (key, val) {
//            $("#ProfesoresFrom" + idD).append("<option value='" + val['id'] + "'>" + val['label'] + "</option>");
//        });
//    });
//}


//function loadProf(idDepartamento) {
//    iddep = idDepartamento;
//    if (idDepartamento != 0) {
//        //nameDep = $("#fac" + iddep).attr("facname");
//        $("TextBox").html("<select  name='profesores' id='ProfesoresFrom" + iddep + "'>" +
//                       "<option value='0'>Seleccione El Profesor</option>" +
//                       "</select>");
//        loadProfesores(idDepartamento, "/sedoc/Admin/GetDepartamentoProfesores/");
//    }
//}

function loadDocentes(idD, route) {
    $.getJSON(route, { term: idD }, function (data) {
        // contenido de la tabla           
        $.each(data, function (key, val) {
            $("#ProfesoresFrom" + idD + " tbody").append("<tr>" +
                    "<td>" + val['label'] + "</td></tr>");
        });
    });
}


// carga  la cabecera de la tabla
function loadTable(idDepartamento) {
    iddep = idDepartamento;
    if (idDepartamento != 0) {
        //nameDep = $("#dep" + iddep).attr("depname");
        $("#work-area").html("<table id='ProfesoresFrom" + iddep + "' class='list' style='width:100%'>" +
                            "<thead><tr><th align='center' colspan=3>Programas De La Facultad De </th></tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>");
        loadDocentes(idDepartamento, "/sedoc/Admin/GetDepartamentoProfesores/");
    }
}

$(function () {
    /*  Buscar facultad*/
    $("#search_facultad").autocomplete({
        source: searchVarFac,
        minLength: 1,
        select: function (event, ui) {
            if (ui.item) {
                setFacultad(ui);
                loadTable(ui.item.id);
            }
        }
    }); 
});