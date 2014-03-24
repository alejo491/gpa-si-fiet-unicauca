$(function () {

    var iddepto = document.getElementById("search_term").value;
    setDepartament();
    loadTeaching(iddepto, "/sedoc/DepartmentChief/GetValueWork/");

    function setDepartament() {

        $("#work-area").html("");
        $("#work-area").append("<table id='TableidTeaching' class='list' style='width:100%'>" +
                            "<thead><tr align='center'><th colspan=10>Docentes </th></tr>" +
                            "<tr class='sub'>" +
                                "<th>Apellidos</th><th>Nombres</th><th>Social</th><th>Gestión</th><th>Inv</th><th>Des. Prof.</th><th>Docencia</th><th>Trabajo Grado</th><th>Trabajo Inv.</th><th>Total</th>" +
                            "</tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>");
    }

    function loadTeaching(idD, route) {
        //si el valor que llega es -1 entonces las siguientes variables  toman el valor 'no tiene' para mostrar en  pantalla
        var social = "";
        var gestion = "";
        var investigacion = "";
        var dProfesoral = "";
        var docencia = "";
        var trabajoGrado = "";
        var total = "";
        var trabajoInvestigacion = "";
        $.getJSON(route, { term: idD }, function (data) {
            $.each(data, function (key, val) {

                if (val['social'] == null) { social = "-" } else if (val['social'] == "0") { social = "NC" } else { social = val['social'] }
                if (val['gestion'] == null) { gestion = "-" } else if (val['gestion'] == "0") { gestion = "NC" } else { gestion = val['gestion'] }
                if (val['investigacion'] == null) { investigacion = "-" } else if (val['investigacion'] == "0") { investigacion = "NC" } else { investigacion = val['investigacion'] }
                if (val['dProfesoral'] == "-1") { dProfesoral = "-" } else if (val['dProfesoral'] == "0") { dProfesoral = "NC" } else { dProfesoral = val['dProfesoral'] }
                if (val['docencia'] == null) { docencia = "-" } else if (val['docencia'] == "0") { docencia = "NC" } else { docencia = val['docencia'] }
                if (val['trabajoGrado'] == "-1") { trabajoGrado = "-" } else if (val['trabajoGrado'] == "0") { trabajoGrado = "NC" } else { trabajoGrado = val['trabajoGrado'] }
                if (val['trabajoInvestigacion'] == "-1") { trabajoInvestigacion = "-" } else if (val['trabajoInvestigacion'] == "0") { trabajoInvestigacion = "NC" } else { trabajoInvestigacion = val['trabajoInvestigacion'] }
                if (val['total'] == "-1") { total = "-" } else { total = val['total'] }

                $("#TableidTeaching  tbody").append("<tr>" +
							"<td>" + val['apellidos'] + "</td>" +
                            "<td>" + val['nombres'] + "</td>" +
                            "<td>" + social + "</td>" +
                            "<td>" + gestion + "</td>" +
                            "<td>" + investigacion + "</td>" +
                            "<td>" + dProfesoral + "</td>" +
                            "<td>" + docencia + "</td>" +
                            "<td>" + trabajoGrado + "</td>" +
                            "<td>" + trabajoInvestigacion + "</td>" +
                            "<td>" + total + "</td>" +
                            "</tr>");
            });
        });
    }
});