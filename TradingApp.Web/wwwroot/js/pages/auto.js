var auto = (function () {
    var lessToggle = true,
        moreToggle = false,
        autoForecastLink = $('#auto-forecast-link').data('request-url'),
        getLatestAssetsLink = $('#get-latest-assets').data('request-url'),
        getForecastPartLink = $('#show-forecast-elements').data('request-url'),
        updateObservablesLink = $('#update-observables-link').data('request-url');


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
    
    $('#get-latest-assets-link').click(function () {
        latestAssets();
    });
    
    $('#show-positive').click(function(){
        wrapForForecastElements('positive-picker', utils.indicators.positive);
    });

    $('#show-neutral').click(function(){
        wrapForForecastElements('neutral-picker', utils.indicators.neutral);
    });

    $('#show-negative').click(function(){
        wrapForForecastElements('negative-picker', utils.indicators.negative);
    });

    $('#show-strong-positive').click(function(){
        wrapForForecastElements('strong-positive-picker', utils.indicators.superPositive);
    });
    
    $('#update-observable-list').click(function () {
        var observable = getCheckedAssets();
        if(observable && observable.length > 0){
            updateObservableList(observable);
        }
    });
    
    var getCheckedAssets = function () {

        var list = $('.observable').find('input[name=observe]:checked');
        var checked = [];
        if (list.length === 0){
            toastr.warning('No marked coins');
            return;
        }
        
        for(var i = 0; i<list.length; i++){
            checked.push($(list[i]).val());
        }
        return checked;
    };
    
    var updateObservableList = function (observableList) {

         var link = updateObservablesLink;
        
         for(var i = 0; i <observableList.length; i++) {
             if (i === 0) {
                 link += '?observableList=' + observableList[i];
             } else {
                 link += '&observableList=' + observableList[i];
             }
         }
         
        utils.loaderShow();
         
        $.ajax({
            url: link,
            type:'GET',
            success: function (data) {
                utils.loaderHide();
                toastr.success('Observable file has been successfully updated!');
            },
            error: function (error) {
                alert(error.responseJSON.message);
                utils.loaderHide();
            }
        })
    };
    
    var wrapForForecastElements = function (pickerName, indicatorVal) {
        var indicator = indicatorVal;
        var assetName = $('#'+pickerName).find('option:selected').val();
        var periods = $('input[name=period]:checked').val();
        var data = {
            indicator: indicator,
            assetName: assetName,
            periods: periods
        };
        var allow = true;
        for (var prop in data) {
            if(!data[prop] && data[prop]!== 0) allow = false;
        }
        if(allow){
            showForecastElements(data);
        }
    };
    
    var latestAssets = function () {
        $.ajax({
            url: getLatestAssetsLink,
            type:'GET',
            success: function (data) {
                buildLatest(data);
                utils.loaderHide();
            },
            error: function (error) {
                alert(error.responseJSON.message);
                utils.loaderHide();
            }
        })
    };

    var showForecastElements = function (data) {
        $.ajax({
            url: getForecastPartLink,
            type:'GET',
            data: data,
            success: function (data) {
                forecastElementsLoaded(data);
                utils.loaderHide();
                toastr.success("Components has been successfully updated")
            },
            error: function (error) {
                utils.loaderHide();
            }
        })
    };
    
    var makeForecast = function (data) {
        utils.loaderShow();
        $.ajax({
            url: autoForecastLink,
            type:'POST',
            data: data,
            success: function (data) {
                buildAutoResponse(data);
                utils.loaderHide();
                toastr.success("Success!");
                $("#run-auto-forecast").attr("disabled", "disabled");
                $("#btc-forecast").css("pointer-events", "none");
                
                setTimeout(function() {
                    $("#run-auto-forecast").removeAttr("disabled");
                    $("#btc-forecast").css("pointer-events", "auto");
                }, 60000);
                
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
        var readFrom = $('#observable-option').find('option:selected').val();;
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
            dailySeasonality: dailySeasonality,
            readFrom: readFrom
        }
    };
    
    
    var buildAutoResponse = function (data) {
        indicatorPicker(data.strongPositiveAssets, 'strong-positive-picker');
        indicatorPicker(data.positiveAssets, 'positive-picker');
        indicatorPicker(data.neutralAssets, 'neutral-picker');
        indicatorPicker(data.negativeAssets, 'negative-picker');
        reportTable(data.report);
        callsStats(data.callsMadeHisto, data.callsLeftHisto);
    };

    var buildLatest = function (data) {
        indicatorPicker(data.strongPositiveAssets, 'strong-positive-picker');
        indicatorPicker(data.positiveAssets, 'positive-picker');
        indicatorPicker(data.neutralAssets, 'neutral-picker');
        indicatorPicker(data.negativeAssets, 'negative-picker');
        reportTable(data.report);
    };
    
    var indicatorPicker = function (data, pickerId) {
        var picker = $('#'+pickerId);
        picker.empty();
        if(data){
            var jsonData = data;
            for (var i = 0; i < jsonData.length; i++) {
                picker.append('<option value="' + jsonData[i] + '">' + jsonData[i] + '</option>')
            }
        }
        picker.selectpicker('refresh');
    };

    var reportTable = function (data) {
        if (data){
            var $report = '';
            for(var i = 0; i < data.length; i++)
            {
                $report += '<tr>';
                if(data[i].log === utils.logs.negative){
                    $report += '<td class="danger">' + (i+1) + '</td>';
                    $report += '<td class="danger">' +'<label class="observable"><input type="checkbox" value='+data[i].assetName+' name="observe"><span class="label-text"></span></label>'+ '</td>';
                    $report += '<td class="danger">' + data[i].assetName + '</td>';
                    $report += '<td class="danger">' + data[i].log + '</td>';
                    $report += '<td class="danger">' + data[i].rate + '</td>';
                    $report += '<td class="danger">' + data[i].change + '</td>';
                    $report += '<td class="danger">' + data[i].volume + '</td>';
                } else if (data[i].log === utils.logs.positive){
                    $report += '<td class="info">' + (i+1) + '</td>';
                    $report += '<td class="info">' +'<label class="observable"><input type="checkbox" value='+data[i].assetName+' name="observe"><span class="label-text"></span></label>'+ '</td>';
                    $report += '<td class="info">' + data[i].assetName + '</td>';
                    $report += '<td class="info">' + data[i].log + '</td>';
                    $report += '<td class="info">' + data[i].rate + '</td>';
                    $report += '<td class="info">' + data[i].change + '</td>';
                    $report += '<td class="info">' + data[i].volume + '</td>';
                } else if (data[i].log === utils.logs.strongPositive){
                    $report += '<td class="success">' + (i+1) + '</td>';
                    $report += '<td class="success">' +'<label class="observable"><input type="checkbox" value='+data[i].assetName+' name="observe"><span class="label-text"></span></label>'+ '</td>';
                    $report += '<td class="success">' + data[i].assetName + '</td>';
                    $report += '<td class="success">' + data[i].log + '</td>';
                    $report += '<td class="success">' + data[i].rate + '</td>';
                    $report += '<td class="success">' + data[i].change + '</td>';
                    $report += '<td class="success">' + data[i].volume + '</td>';
                } else if (data[i].log === utils.logs.neutral){
                    $report += '<td class="active">' + (i+1) + '</td>';
                    $report += '<td class="active">' +'<label class="observable"><input type="checkbox" value='+data[i].assetName+' name="observe"><span class="label-text"></span></label>'+ '</td>';
                    $report += '<td class="active">' + data[i].assetName + '</td>';
                    $report += '<td class="active">' + data[i].log + '</td>';
                    $report += '<td class="active">' + data[i].rate + '</td>';
                    $report += '<td class="active">' + data[i].change + '</td>';
                    $report += '<td class="active">' + data[i].volume + '</td>';
                } else if (data[i].log === utils.logs.zeroRezults){
                    $report += '<td class="warning">' + (i+1) + '</td>';
                    $report += '<td class="warning">' +'<label class="observable"><input type="checkbox" value='+data[i].assetName+' name="observe"><span class="label-text"></span></label>'+ '</td>';
                    $report += '<td class="warning">' + data[i].assetName + '</td>';
                    $report += '<td class="warning">' + data[i].log + '</td>';
                    $report += '<td class="warning">' + data[i].rate + '</td>';
                    $report += '<td class="warning">' + data[i].change + '</td>';
                    $report += '<td class="warning">' + data[i].volume + '</td>';
                }
                $report += '</tr>';
            }
            var $table = $("#table-report-content");
            $table.find("tr").remove();
            $table.html($report);
        }
    };
    var callsStats = function (made, left) {
        $('#made-number').html(made);
        $('#left-number').html(left);
    };

    var forecastElementsLoaded = function (data) {
        table(data.table);
        imgForecast(data.forecastPath);
        imgComponents(data.componentsPath);
        assetName(data.assetName);
        indicator(data.indicator);
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

})();