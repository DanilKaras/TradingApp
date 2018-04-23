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
        $('#ex10').slider({
            ticks: [0, 48, 96, 144, 192],
            ticks_labels: ['0', '48', '96', '144', '192'],
            ticks_snap_bounds: 1
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


    $('#custom-periods:text').on('input', function () {
        $('#use-period-slider').click();
        var $val = this.value;
        $('#ex10').slider('setValue', $val);
    });

    $('#ex10').on('change', function () {
        $('#use-period-slider').click();
        $('#custom-periods').val('');
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
                buildComponents(data);
                utils.loaderHide();
                toastr.success("Success!")
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
        var selectedForecastPeriod = '';
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

        selectedForecastPeriod = $('input[name=period-toggles]:checked').val();
        if (selectedForecastPeriod){
            switch(selectedForecastPeriod){
                case utils.periodGroup.usePeriodToggle:
                    periods = $('input[name=period]:checked').val();
                    break;
                case utils.periodGroup.usePeriodSlider:
                    var $period = $('#custom-periods').val();
                    if ($period && $period !== 0) {
                        periods = $period;
                    }
                    else {
                        periods = $('#ex10').slider('getValue');
                    }
                    break;
                default:
                    break;    
            }
        } else{
            periods = $('input[name=period]:checked').val();
        }
        
        return {
            asset: asset,
            dataHours: dataHours,
            periods: periods,
            hourlySeasonality: hourlySeasonality,
            dailySeasonality: dailySeasonality
        }
    };
    
    var buildComponents = function (data) {
        table(data.table);
        imgForecast(data.forecastPath);
        imgComponents(data.componentsPath);
        assetName(data.assetName);
        indicator(data.indicator);
        callsStats(data.callsMadeHisto, data.callsLeftHisto);
        features(data.volume, data.change);
    };

    var features = function (volume, change) {
        var vol = "Volume: " + volume + " BTC";
        
        //var parts = change.split('.');
        var ch ="Change: " + change +"%";
        $("#feature-volume").html(vol);
        $("#feature-change").html(ch);
    };
    
    var table = function (table) {
        if (table){
            var $content = '';
            for(var i = 0; i < table.length; i++)
            {
                $content += "<tr><td>" +
                    table[i].id+"</td><td>"+
                    table[i].ds+"</td><td>"+
                    utils.fixedOutput(table[i].yhat)+"</td><td>"+
                    utils.fixedOutput(table[i].yhatLower)+"</td><td>"+
                    utils.fixedOutput(table[i].yhatUpper)+"</td></tr>";
            }
            $('#table-content').html($content);
        }
        else{
            toastr.error("no data from server")
        }
    };

    var imgForecast = function (picPath) {
        if(picPath){
            var imgForecast = $('<img />', {
                id: 'forecast',
                src: picPath,
                class: "img-responsive",
                alt: 'Cinque Terre'
            });
            $('#forecast-place').html(imgForecast);
        }
        else{
            error("No forecast image")
        }
    };

    var imgComponents = function (picPath) {
        if(picPath){
            var imgComponents = $('<img />', {
                id: 'components',
                src: picPath,
                class: "img-responsive",
                alt: 'Cinque Terre'
            });
            $('#components-place').html(imgComponents);
        } else {
            toastr.error("No components image");
        }
    };

    var assetName = function (assetName) {
        if(assetName){
            $('#asset-name').html(assetName);
        }else{
            toastr.error("No asset name");
        }

    };

    var indicator = function (indicator) {
        var span = '';
        if(indicator === utils.indicators.positive) {
            span = $('<span />',{
                class:'label label-info',
                html:'Positive'
            });
        }
        else if(indicator === utils.indicators.neutral){
            span = $('<span />',{
                class:'label label-default',
                html:'Neutral'
            });
        }
        else if(indicator === utils.indicators.negative){
            span = $('<span />',{
                class:'label label-danger',
                html:'Negative'
            });
        }
        else if (indicator === utils.indicators.superPositive){
            span = $('<span />',{
                class:'label label-success',
                html:'Strong Positive'
            });
        }
        $('#indicator-text').html(span);
    };
    
    var callsStats = function (made, left) {
        $('#made-number').html(made);
        $('#left-number').html(left);
    }
})();