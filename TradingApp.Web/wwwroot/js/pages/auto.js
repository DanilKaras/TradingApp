var auto = (function () {
    var lessToggle = true,
        moreToggle = false,
        sliderHours = $('#slider-hours'),
        siderPriod = $('#period-slider'),
        collapseAdj = $('#auto-tools .collapse'),
        autoForecastLink = $('#auto-forecast-link').data('request-url'),
        getLatestAssetsLink = $('#get-latest-assets').data('request-url'),
        getForecastPartLink = $('#show-forecast-elements').data('request-url'),
        updateObservablesLink = $('#update-observables-link').data('request-url');


    $(document).ready(function () {
        utils.tabs(utils.pages.auto);
        sliderHours.ionRangeSlider({
            min: 0,
            max: 720,
            grid: true,
            step: 1,
            onStart: function (data) {
                $('#custom-slider').prop("value", data.from);
            },
            onChange: function (data) {
                $('#custom-slider').prop("value", data.from);
            }
        });
        siderPriod.ionRangeSlider({
            min: 0,
            max: 192,
            grid: true,
            step: 1,
            onStart: function (data) {
                $('#custom-periods').prop("value", data.from);
            },
            onChange: function (data) {
                $('#custom-periods').prop("value", data.from);
            }
        });
    });
    $('#trigger-block').click(function () {
        $('#use-buttons').click();
    });

    $('#custom-slider:text').on('input', function () {
        $('#use-slider').click();
        var instance = sliderHours.data("ionRangeSlider");
        var val = this.value;

        instance.update({
            from: val
        });

    });
    
    $('#custom-periods:text').on('input', function () {
        $('#use-period-slider').click();
        var val = this.value;
        var instance = siderPriod.data("ionRangeSlider");

        instance.update({
            from: val
        });
    });
    
    
    siderPriod.on('change', function () {
        $('#use-period-slider').click();
    });
    
    

    sliderHours.on('change', function () {
        $('#use-slider').click();
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
    $('.period-toggle').click(function () {
        $('#use-period-toggles').click();
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
         
        utils.loaderPageShow();
         
        $.ajax({
            url: link,
            type:'GET',
            success: function (data) {
                utils.loaderPageHide();
                toastr.success('Observable file has been successfully updated!');
            },
            error: function (error) {
                bootbox.alert(error.responseJSON.message);
                utils.loaderPageHide();
            }
        })
    };
    
    var wrapForForecastElements = function (pickerName, indicatorVal) {
         
        var indicator = indicatorVal;
        var assetName = $('#'+pickerName).find('option:selected').val();
        var periods = $('input[name=period]:checked').val();
        var tableRow = $("#table-report-content").find("td").filter(function() {
            return $(this).text() === assetName;
        }).closest("tr");
        
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
            showForecastElements(data, tableRow);
        }
    };
    
    var latestAssets = function () {
        $.ajax({
            url: getLatestAssetsLink,
            type:'GET',
            success: function (data) {
                buildLatest(data);
                utils.loaderPageHide();
            },
            error: function (error) {
                bootbox.alert(error.responseJSON.message);
                utils.loaderPageHide();
            }
        })
    };

    var showForecastElements = function (data, containigRow) {
        $.ajax({
            url: getForecastPartLink,
            type:'GET',
            data: data,
            success: function (data) {
                forecastElementsLoaded(data, containigRow);
                utils.loaderPageHide();
                toastr.success("Components has been successfully updated")
            },
            error: function (error) {
                utils.loaderPageHide();
            }
        })
    };
    
    var makeForecast = function (data) {
        collapseAdj.click();
        utils.loaderPageShow();
        $.ajax({
            url: autoForecastLink,
            type:'POST',
            data: data,
            success: function (data) {
                buildAutoResponse(data);
                utils.loaderPageHide();
                toastr.success("Success!");
                $("#run-auto-forecast").attr("disabled", "disabled");
                $("#btc-forecast").css("pointer-events", "none");
                
                setTimeout(function() {
                    $("#run-auto-forecast").removeAttr("disabled");
                    $("#btc-forecast").css("pointer-events", "auto");
                }, 60000);
                
            },
            error: function (error) {
                bootbox.alert(error.responseJSON.message);
                utils.loaderPageHide();
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
                    dataHours = $('#custom-slider').val();
                    break;
                default:
                    break;
            }
        } else {
            dataHours = $('input[name=toggle]:checked').val();
        }
        
        hourlySeasonality = $('#seasonality-houly').is(':checked');
        dailySeasonality = $('#seasonality-daily').is(':checked');

        var selectedForecastPeriod = $('input[name=period-toggles]:checked').val();
        if (selectedForecastPeriod){
            switch(selectedForecastPeriod){
                case utils.periodGroup.usePeriodToggle:
                    periods = $('input[name=period]:checked').val();
                    break;
                case utils.periodGroup.usePeriodSlider:
                    periods = $('#custom-periods').val();
                    break;
                default:
                    break;
            }
        } else{
            periods = $('input[name=period]:checked').val();
        }
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
        utils.udpateStats(data.callsMadeHisto, data.callsLeftHisto);
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
                    var idNegative = 'checkbox' + i;
                    
                    $report += '<td class="danger">' + (i+1) + '</td>';
                    $report += '<td class="danger">' +'<label class="observable mt-checkbox"><input type="checkbox" value='+data[i].assetName+' name="observe"><span></span></label>'+ '</td>';
                    $report += '<td class="danger">' + data[i].assetName + '</td>';
                    $report += '<td class="danger">' + data[i].log + '</td>';
                    $report += '<td class="danger">' + data[i].rate + '</td>';
                    $report += '<td class="danger">' + data[i].change + '</td>';
                    $report += '<td class="danger">' + data[i].volume + '</td>';
                } else if (data[i].log === utils.logs.positive){
                    $report += '<td class="info">' + (i+1) + '</td>';
                    $report += '<td class="info">' +'<label class="observable mt-checkbox"><input type="checkbox" value='+data[i].assetName+' name="observe"><span></span></label>'+ '</td>';
                    $report += '<td class="info">' + data[i].assetName + '</td>';
                    $report += '<td class="info">' + data[i].log + '</td>';
                    $report += '<td class="info">' + data[i].rate + '</td>';
                    $report += '<td class="info">' + data[i].change + '</td>';
                    $report += '<td class="info">' + data[i].volume + '</td>';
                } else if (data[i].log === utils.logs.strongPositive){
                    $report += '<td class="success">' + (i+1) + '</td>';
                    $report += '<td class="success">' +'<label class="observable mt-checkbox"><input type="checkbox" value='+data[i].assetName+' name="observe"><span></span></label>'+ '</td>';
                    $report += '<td class="success">' + data[i].assetName + '</td>';
                    $report += '<td class="success">' + data[i].log + '</td>';
                    $report += '<td class="success">' + data[i].rate + '</td>';
                    $report += '<td class="success">' + data[i].change + '</td>';
                    $report += '<td class="success">' + data[i].volume + '</td>';
                } else if (data[i].log === utils.logs.neutral){
                    $report += '<td class="active">' + (i+1) + '</td>';
                    $report += '<td class="active">' +'<label class="observable mt-checkbox"><input type="checkbox" value='+data[i].assetName+' name="observe"><span></span></label>'+ '</td>';
                    $report += '<td class="active">' + data[i].assetName + '</td>';
                    $report += '<td class="active">' + data[i].log + '</td>';
                    $report += '<td class="active">' + data[i].rate + '</td>';
                    $report += '<td class="active">' + data[i].change + '</td>';
                    $report += '<td class="active">' + data[i].volume + '</td>';
                } else if (data[i].log === utils.logs.zeroRezults){
                    $report += '<td class="warning">' + (i+1) + '</td>';
                    $report += '<td class="warning">' +'<label class="observable mt-checkbox"><input type="checkbox" value='+data[i].assetName+' name="observe"><span></span></label>'+ '</td>';
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
    

    var forecastElementsLoaded = function (data, row) {
        var rate = row.find("td:nth-child(5)").text();
        var change = row.find("td:nth-child(6)").text();
        var volume = row.find("td:nth-child(7)").text();
        table(data.table);
        imgForecast(data.forecastPath);
        imgComponents(data.componentsPath);
        assetName(data.assetName);
        indicator(data.indicator);
        rateFeature(rate);
        changeFeature(change);
        volumeFeature(volume);
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
    
    var changeFeature = function (feature) {
        $('#feature-change').html(feature);
    };

    var volumeFeature = function (feature) {
        $('#feature-volume').html(feature);
    };

    var rateFeature = function (feature) {
        $('#rate-indicator').html(feature);
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