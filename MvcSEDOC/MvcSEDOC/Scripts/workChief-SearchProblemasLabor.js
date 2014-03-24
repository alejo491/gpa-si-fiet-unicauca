$(function () {

    var des = $("#descp").val();
    var tipo = $("#tipo").val();
    var idlabor = document.getElementById("search_term").value;
    setWorkId();
    loadWork(idlabor,tipo);  

    function setWorkId() {
        
        $("#work-area").html("");
        $("#work-area").append("<table id='TableidWork' class='report' style='width:100%'>" +
                                "<thead><tr><th colspan=2>Detalles De  "+ des + "</th></tr></thead>" + 
                                "<tbody></tbody>" +
                                "</table><br/>");
    }

    function loadWork(idL, tip) {
        $.getJSON("/sedoc/WorkChief/SearchProblemasLabor/", { id: idL, term: tip }, function (data) {
            $.each(data, function (key, val) {
                $("#TableidWork  tbody").append("<tr><th colspan=4>Docente  " + val['docente'] + "</th></tr>" +
                                                "<tr><td colspan=2 aling='center'><strong>Problema</strong></td></tr>" +
                                                "<tr class='sub'><td style='width:20%' aling='center'><strong>Descripción</strong></td>" +
							                    "<td>" + val['problemadescripcion'] + "</td></tr>" +
							                    "<tr class='sub'><td style='width:20%' aling='center'><strong>Respuesta</strong></td>" +
							                    "<td >" + val['problemarespuesta'] + "</td></tr>" +
                                                "<tr><td colspan=2 ><strong>Solución</strong></td></tr>" +
                                                "<tr class='sub'><td style='width:20%' aling='center'><strong>Descripción</strong></td>" +
							                    "<td >" + val['soluciondescripcion'] + "</td></tr>" +
							                    "<tr class='sub'><td style='width:20%' aling='center'><strong>Ubicación</strong></td>" +
							                    "<td >" + val['solucionrespuesta'] + "</td></tr>" +
                                                "</tr>");

            });            
        });
    }

    
});


















