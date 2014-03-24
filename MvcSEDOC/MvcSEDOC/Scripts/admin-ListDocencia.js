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
var nameFac, idfac;

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
            $('#ddlCategoria').append("<option value='"+ val['id']+"'>"+  val['label'] + "</option>");            
        });
    });
}

// carga  la cabecera de la tabla
function loadTable1(idfacultad) {
    idfac = idfacultad;
    if (idfacultad != 0) {
//        nameFac = $("#fac" + idfac).attr("facname");
//        $("#List").html("<DropDownList id='idDepartamento" + id + "' class='list' style='width:100%'>" +
//                            "<thead><tr><th align='center' colspan=3>Programas De La Facultad De " + nameFac + "</th></tr></thead>" +
//                            "<tbody></tbody>" +
//                            "</table><br/>");
        loadDepartamentos(idfacultad, "/sedoc/Admin/GetFacultadPrograma/");
    }
}

// carga  la cabecera de la tabla
function loadTable(idDepartamento) {
    iddep = idDepartamento;
    if (idDepartamento != 0) {
        nameDep = $("#dep" + iddep).attr("depname");
        $("#work-area").html("<table id='DepartamentFrom" + idfac + "' class='list' style='width:100%'>" +
                            "<thead><tr><th align='center' colspan=3>Programas De La Facultad De " + nameFac + "</th></tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>");
        loadDepartamentos(idFacultad, "/sedoc/Admin/GetFacultadPrograma/");
    }
}

$(function lista() {    
    $('#ddlCategoria').append('<option value="1" ">1 Estrella</option>');
    $('#ddlCategoria').append('<option value="2" ">2 Estrellas</option>');
    $('#ddlCategoria').append('<option value="3" ">3 Estrellas</option>');
    $('#ddlCategoria').append('<option value="4" ">4 Estrellas</option>');
    $('#ddlCategoria').append('<option value="5" ">5 Estrellas</option>');              
 }); 

$(function () {
    /*  Buscar facultad*/
    $("#search_facultad").autocomplete({
        source: searchVarFac,
        minLength: 1,
        select: function (event, ui) {
            if (ui.item) {
              setFacultad(ui);
                lista();
            }
        }
    });

$(function () {
    /*  Buscar facultad*/
    $("#search_departamento").autocomplete({
        source: searchVar,
        minLength: 1,
        select: function (event, ui) {
            if (ui.item) {
                setDepartamento(ui);
                lista(ui.item.id);
            }
        }
    });

    
       
});