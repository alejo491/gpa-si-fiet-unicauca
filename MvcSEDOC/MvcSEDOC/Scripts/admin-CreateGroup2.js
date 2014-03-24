$(function () {
    /************* VARIABLES *********************/
    var searchVar = "/sedoc/Admin/SearchQuestionnaire/";
    loadWork("iddocente", "/sedoc/Admin/GetTechingWork1/");

    var iddepto = "search_term";
    setAsignacion();
    //loadTeaching2(iddepto, "/sedoc/Admin/GetAsiganacionPeriodo/");

    function setAsignacion() {
        $("#work-area").html("");
        $("#work-area").append("<form action='/sedoc/Admin/EditarAssignQuestionnaire1' method='post'> <fieldset>" +
                            "<table id='TableidTeaching' class='list' style='width:100%'>" +
                            "<thead><tr><th colspan=4 >Editar Labores - Cuestionarios</th></tr>" +
                            "<tr class='sub'><th ALIGN='CENTER' style='width:30px;'>id</th><th ALIGN='CENTER'>Labor</th><th ALIGN='CENTER'>Asignado</th><th ALIGN='CENTER'>Nuevo</th></tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>" +
                            "<p>" +
                            "<input type='submit' value='Guardar'/>" +
                            "</p>" +
                             "</fieldset>");
    }
    function loadWork(idD, route) {
        $.getJSON(route, { term: idD }, function (data) {
            $.each(data, function (key, val) {
                $("#TableidWork  tbody").append("<tr>" +
							"<td type='hidden'>" + val['idlabor'] + "</td>" +
							"<td>" + val['tipoLabor'] + "</td>" +
                            "<td>" + val['descripcion'] + "</td>" +
                            "<td>" + "<input   value='" + val['nombres'] + "' class='search' " + " id='#' disabled />" + "</td>" +
                            "<td>" + "<input type='hidden' id='idJL" + val['idlabor'] + "' />" +
                            "<input class='search_Teching' " + " id=" + val['idlabor'] + " />" + "</td>" +
                            "</tr>");

            });
            autocompleteTeching();
        });
    }
    function loadWork(idD, route) {
        var cont = -1;
        $.getJSON(route, { term: idD }, function (data) {
            $.each(data, function (key, val) {
                cont = cont + 1;
                if (val['idlabor'] == 1) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                "<td ALIGN='CENTER' >1 </td>" +
					                "<td ALIGN='CENTER'>Social</td>" +
                                     "<td>" + "<input   value='" + val['custionario'] + "' class='search' " + " id='#' disabled />" + "</td>" +
                                    "<td ALIGN='CENTER'><input type='hidden' id='idJL1' /><input name='social' id='search_term' /></td>" +
                                    "</tr>");

                }
                if (val['idlabor'] == 2) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                    "<td ALIGN='CENTER' >2</td>" +
					                    "<td ALIGN='CENTER'>Gestion</td>" +
                                        "<td>" + "<input   value='" + val['custionario'] + "' class='search' " + " id='#' disabled />" + "</td>" +
                                        "<td ALIGN='CENTER'>" +
                                        "<input name='gestion' id='uno' />" +
                                    "</tr>");
                }
                if (val['idlabor'] == 3) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                     "<td ALIGN='CENTER' >3</td>" +
						                "<td ALIGN='CENTER'>Investigacion</td>" +
                                        "<td>" + "<input   value='" + val['custionario'] + "' class='search' " + " id='#' disabled />" + "</td>" +
                                        "<td ALIGN='CENTER'>" +
                                       "<input name='investigacion' id='cinco' />" +
                                    "</tr>");
                }
                if (val['idlabor'] == 4) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                     "<td ALIGN='CENTER' >4</td>" +
						                "<td ALIGN='CENTER'>Trabajo de Grado</td>" +
                                        "<td>" + "<input   value='" + val['custionario'] + "' class='search' " + " id='#' disabled />" + "</td>" +
                                        "<td ALIGN='CENTER'>" +
                                        "<input name='trabajoG' id='tres' />" +
                                    "</tr>");
                }
                if (val['idlabor'] == 5) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                     "<td ALIGN='CENTER' >5</td>" +
						                "<td ALIGN='CENTER'>Desarrollo Profesoral</td>" +
                                        "<td>" + "<input   value='" + val['custionario'] + "' class='search' " + " id='#' disabled />" + "</td>" +
                                        "<td ALIGN='CENTER'>" +
                                        "<input name='desarrolloP' id='cuatro' />" +
                                    "</tr>");
                }
                if (val['idlabor'] == 6) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                       "<td ALIGN='CENTER' >6</td>" +
						                "<td ALIGN='CENTER'>Trabajo de investigacion</td>" +
                                        "<td>" + "<input   value='" + val['custionario'] + "' class='search' " + " id='#' disabled />" + "</td>" +
                                        "<td ALIGN='CENTER'>" +
                                        "<input name='trabajoI' id='dos' />" +
                                    "</tr>");
                }
                if (val['idlabor'] == 7) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                       "<td ALIGN='CENTER' >7</td>" +
						                "<td ALIGN='CENTER'>Docencia</td>" +
                                        "<td>" + "<input   value='" + val['custionario'] + "' class='search' " + " id='#' disabled />" + "</td>" +
                                        "<td ALIGN='CENTER'>" +
                                        "<input name='docencia' id='siete' />" +
                                    "</tr>");
                }
                if (val['idlabor'] == 8) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                       "<td ALIGN='CENTER' >8</td>" +
						                "<td ALIGN='CENTER'>Otras</td>" +
                                        "<td>" + "<input   value='" + val['custionario'] + "' class='search' " + " id='#' disabled />" + "</td>" +
                                        "<td ALIGN='CENTER'>" +
                                        "<input name='otras' id='ocho' />" +
                                    "</tr>");
                }
            });
            autocompleteTeching();
        });

    }

    function autocompleteTeching() {
        $("#search_term").autocomplete({
            source: searchVar,
            minLength: 1,
            select: function (event, ui) {
                if (ui.item) {
                    setQuestionnaireId(ui);
                }
            }
        });
        $("#uno").autocomplete({
            source: searchVar,
            minLength: 1,
            select: function (event, ui) {
                if (ui.item) {
                    setQuestionnaireId(ui);
                }
            }
        });
        $("#dos").autocomplete({
            source: searchVar,
            minLength: 1,
            select: function (event, ui) {
                if (ui.item) {
                    setQuestionnaireId(ui);
                }
            }
        });
        $("#tres").autocomplete({
            source: searchVar,
            minLength: 1,
            select: function (event, ui) {
                if (ui.item) {
                    setQuestionnaireId(ui);
                }
            }
        });
        $("#cuatro").autocomplete({
            source: searchVar,
            minLength: 1,
            select: function (event, ui) {
                if (ui.item) {
                    setQuestionnaireId(ui);
                }
            }
        });
        $("#cinco").autocomplete({
            source: searchVar,
            minLength: 1,
            select: function (event, ui) {
                if (ui.item) {
                    setQuestionnaireId(ui);
                }
            }
        });
        $("#seis").autocomplete({
            source: searchVar,
            minLength: 1,
            select: function (event, ui) {
                if (ui.item) {
                    setQuestionnaireId(ui);
                }
            }
        });
        $("#siete").autocomplete({
            source: searchVar,
            minLength: 1,
            select: function (event, ui) {
                if (ui.item) {
                    setQuestionnaireId(ui);
                }
            }
        });
        $("#ocho").autocomplete({
            source: searchVar,
            minLength: 1,
            select: function (event, ui) {
                if (ui.item) {
                    setQuestionnaireId(ui);
                }
            }
        });
    }

    function setQuestionnaireId(ui) {
        $("#idcuestionario").val(ui.item.id);
    }

});