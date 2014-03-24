$(function () {
    /************* VARIABLES *********************/
    var searchVar = "/sedoc/AdminFac/SearchQuestionnaire/";
    
    var iddepto = "search_term";
    setAsignacion();
    loadTeaching2(iddepto, "/sedoc/AdminFac/GetAsiganacionPeriodo/");

    function setAsignacion() {
        $("#work-area").html("");
        $("#work-area").append("<form action='/sedoc/AdminFac/AssignQuestionnaire1' method='post'> <fieldset>" +
                            "<table id='TableidTeaching' class='list' style='width:100%'>" +
                            "<thead><tr><th colspan=3 >Labores - Cuestionarios</th></tr>" +
                            "<tr class='sub'><th ALIGN='CENTER' style='width:30px;'>id</th><th ALIGN='CENTER'>Labor</th><th ALIGN='CENTER'>Cuestionario</th></tr></thead>" +
                            "<tbody></tbody>" +
                            "</table><br/>" +
                            "<p>" +
                            "<input type='submit' value='Guardar'/>" +
                            "</p>" +
                            "</fieldset>");
    }

    function loadTeaching2(idD, route) {
        var cont = -1;
        $.getJSON(route, { term: idD }, function (data) {
            $.each(data, function (key, val) {
                cont = cont + 1;
                if (cont == 0 && val == 0) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                "<td ALIGN='CENTER' >1 </td>" +
					                "<td ALIGN='CENTER'>Social</td>" +
                                    "<td ALIGN='CENTER'><input type='hidden' id='idJL1' /><input name='social' id='search_term' /></td>" +
                                    "</tr>");

                }
                if (cont == 1 && val == 0) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                    "<td ALIGN='CENTER' >2</td>" +
					                    "<td ALIGN='CENTER'>Gestion</td>" +
                                        "<td ALIGN='CENTER'>" +
                                        "<input name='gestion' id='uno' />" +
                                        "</tr>");
                }
                if (cont == 2 && val == 0) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                        "<td ALIGN='CENTER' >3</td>" +
						                    "<td ALIGN='CENTER'>Investigacion</td>" +
                                            "<td ALIGN='CENTER'>" +
                                            "<input name='investigacion' id='cinco' />" +
                                            "</tr>");
                }
                if (cont == 3 && val == 0) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                        "<td ALIGN='CENTER' >4</td>" +
						                    "<td ALIGN='CENTER'>Trabajo de Grado</td>" +
                                            "<td ALIGN='CENTER'>" +
                                            "<input name='trabajoG' id='tres' />" +
                                            "</tr>");
                }
                if (cont == 4 && val == 0) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                        "<td ALIGN='CENTER' >5</td>" +
						                    "<td ALIGN='CENTER'>Desarrollo Profesoral</td>" +
                                            "<td ALIGN='CENTER'>" +
                                            "<input name='desarrolloP' id='cuatro' />" +
                                            "</tr>");
                }
                if (cont == 5 && val == 0) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                        "<td ALIGN='CENTER' >6</td>" +
						                    "<td ALIGN='CENTER'>Trabajo de investigacion</td>" +
                                            "<td ALIGN='CENTER'>" +
                                            "<input name='trabajoI' id='dos' />" +
                                            "</tr>");
                }
                if (cont == 6 && val == 0) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                        "<td ALIGN='CENTER' >7</td>" +
						                    "<td ALIGN='CENTER'>Docencia</td>" +
                                            "<td ALIGN='CENTER'>" +
                                            "<input name='docencia' id='siete' />" +
                                            "</tr>");
                }
                if (cont == 7 && val == 0) {
                    $("#TableidTeaching  tbody").append("<tr>" +
					                        "<td ALIGN='CENTER' >8</td>" +
						                    "<td ALIGN='CENTER'>Otras</td>" +
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