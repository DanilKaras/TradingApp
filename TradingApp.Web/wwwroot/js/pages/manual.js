var manual = (function () {
    var lessToggle = true,
        moreToggle = false,
        manualForecastLink = $('#manual-forecast-link').data('request-url'),
        getAssetsLink = $('#assets-list-link').data('request-url');
    
    
    $(document).ready(function () {
        $('#ex13').slider({
            ticks: [0, 240, 480, 720],
            ticks_labels: ['0', '240', '480', '720'],
            ticks_snap_bounds: 30
        });
        builder.toastrConfig();
        getAssets()
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

    $('#make-forecast').click(function () {
        
        var data = wrapData();
        makeForecast(data);
    });
    
    var getAssets = function () {
        utils.loaderShow();
        $.ajax({
            url: getAssetsLink,
            type:'GET',
            success: function (data) {
                bindSelect(data);
                utils.loaderHide();
            },
            error: function (error) {
                alert(error.responseJSON.message);
                utils.loaderHide();
            }
        })
    };

    var makeForecast = function (data) {
        utils.loaderShow();
        $.ajax({
            url: manualForecastLink,
            type:'POST',
            data: data,
            success: function (data) {
                //bindSelect(data);
                utils.loaderHide();
            },
            error: function (error) {
                alert(error.responseJSON.message);
                utils.loaderHide();
            }
        })
    };
        
    var bindSelect = function (data) {
        var jsonData = data;
        var picker = $('#assets');
        for (var i = 0; i < jsonData.length; i++) {
            picker.append('<option value="' + jsonData[i] + '">' + jsonData[i] + '</option>')
        }
        picker.selectpicker('refresh');
    };

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


        asset = $('#assets').find('option:selected').val();
        hourlySeasonality = $('#seasonality-houly').is(':checked');
        dailySeasonality = $('#seasonality-daily').is(':checked');

        periods = $('input[name=period]:checked').val();

        return {
            asset: asset,
            dataHours: dataHours,
            periods: periods,
            hourlySeasonality: hourlySeasonality,
            dailySeasonality: dailySeasonality
        }
    };
})();