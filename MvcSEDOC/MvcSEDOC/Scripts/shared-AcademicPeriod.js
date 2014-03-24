function loadAcademicPeriods() {
    $.ajax({
        cache: false,
        url: "/sedoc/User/GetAcademicPeriods/",
        type: 'Post',
        dataType: 'json',
        data: null,
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            var period_selected = $("#current_academic_period").attr("ac_selected");
            $.each(data, function (key, val) {
                (period_selected == val['idperiodo'] ? selected = 'selected="selected"' : selected = '');
                $("#current_academic_period").append('<option id="period' + val['idperiodo'] + '" value="' + val['idperiodo'] + '" ' + selected + '  >' + val['anio'] + '.' + val['numeroperiodo'] + '</option>');
            });
        }

    });
}

function changeAcademicPeriod() {
    $("#form_acadperiod").submit();
}

