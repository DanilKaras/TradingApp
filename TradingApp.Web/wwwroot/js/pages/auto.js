var auto = (function () {
    var lessToggle = true,
        moreToggle = false,
        autoForecastLink = $('#auto-forecast-link').data('request-url');


    $(document).ready(function () {
        $('#ex13').slider({
            ticks: [0, 240, 480, 720],
            ticks_labels: ['0', '240', '480', '720'],
            ticks_snap_bounds: 30
        });
        builder.toastrConfig();
    });
    $('#trigger-block').click(function () {
        $('#use-buttons').click();
    });


    $('#custom-slider:text').on('input', function () {
        $('#use-slider').click();
        var $val = this.value;
        $('#ex13').slider('setValue', $val);

    });

    $('#ex13').on('change', function () {
        $('#use-slider').click();
        $('#custom-slider').val('');
    });

    $('#custom-slider').focus(function () {
        var $input = $('#custom-slider');

        $input.keydown(function (e) {

            if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
                (e.keyCode === 65 && (e.ctrlKey === true || e.metaKey === true)) ||
                (e.keyCode >= 35 && e.keyCode <= 40)) {
                return;
            }

            if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                e.preventDefault();
            }
        });

        $input.keyup(function(){

            if($input.val() > 2000){
                $input.val(2000)
            }
        });
    });

    $('#run-auto-forecast').click(function(){
        var data = wrapData();
        makeForecast(data);
    });

    var makeForecast = function (data) {
        utils.loaderShow();
        $.ajax({
            url: autoForecastLink,
            type:'POST',
            data: data,
            success: function (data) {
                utils.loaderHide();
                toastr.success("Success!")
            },
            error: function (error) {
                alert(error.responseJSON.message);
                utils.loaderHide();
            }
        })
    };
    
    $('.rb-less').click(function () {
        if (!lessToggle && moreToggle){
            seasonalityDisable();
            $('.per-24').click();
        }
        lessToggle = true;
        moreToggle = false;
    });

    $('.rb-more').click(function () {
        if (lessToggle && !moreToggle){
            seasonalityEnable();
            $('.per-72').click();
        }
        lessToggle = false;
        moreToggle = true;
    });
    var seasonalityEnable = function () {
        var $daily = $('#seasonality-daily');
        var $hourly = $('#seasonality-houly');
        if (!$daily.is(':checked')){
            $daily.click();
        }
        if (!$hourly.is(':checked')){
            $hourly.click();
        }
    };

    var seasonalityDisable = function () {
        var $daily = $('#seasonality-daily');
        var $hourly = $('#seasonality-houly');
        if ($daily.is(':checked')){
            $daily.click();
        }
        if ($hourly.is(':checked')){
            $hourly.click();
        }
    };

    var wrapData = function () {


        var hourlySeasonality = false;
        var dailySeasonality = false;
        var asset = '';
        var selectedGroup = '';
        var dataHours = 0;
        var periods = 0;
        var postData = '';

        selectedGroup = $('input[name=radio]:checked').val();
        if(selectedGroup){
            switch (selectedGroup) {
                case utils.group.useButtons:
                    dataHours = $('input[name=toggle]:checked').val();
                    break;
                case utils.group.useSlider:
                    var $custom = $('#custom-slider').val();
                    if ($custom && $custom !== 0) {
                        dataHours = $custom;
                    }
                    else {
                        dataHours = $('#ex13').slider('getValue');
                    }
                    break;
                default:
                    break;
            }
        } else {
            dataHours = $('input[name=toggle]:checked').val();
        }
        
        hourlySeasonality = $('#seasonality-houly').is(':checked');
        dailySeasonality = $('#seasonality-daily').is(':checked');

        periods = $('input[name=period]:checked').val();

        return {

            dataHours: dataHours,
            periods: periods,
            hourlySeasonality: hourlySeasonality,
            dailySeasonality: dailySeasonality
        }
    };
    
})();