var botForecast = (function () {
    var collapseAdj = $('#bot-tools .collapse'),
    botForecastLink = $('#bot-forecast-link').data('request-url'),
    arrangeBotComponentLink = $('#show-arranged-elements').data('request-url'), 
    latestArrangedLink = $('#get-latest-arranged').data('request-url'),
    botAssetsLink = $('#update-bot-assets-link').data('request-url'),
    botPortalLink = $('#bot-redirect-link').data('request-url');
    
    $(document).ready(function () {
        utils.tabs(utils.pages.botForecast);
        $('#trend-select').multiselect();
        $('#border-select').multiselect();
    });


    $('#run-arrange-forecast').click(function(){
        var data = wrapData();
        makeForecast(data);
    });
    $('#get-latest-assets-link').click(function () {
        latestAssets();
    });
    $('#show-buy').click(function(){
        wrapForForecastElements('buy-picker', utils.botArrange.buy);
    });

    $('#show-dontbuy').click(function(){
        wrapForForecastElements('dontbuy-picker', utils.botArrange.dontBuy);
    });

    $('#show-consider').click(function(){
        wrapForForecastElements('consider-picker', utils.botArrange.consider);
    });

    $('#create-bot-list').click(function () {
        var botList = getCheckedAssets();
        if(botList && botList.length > 0){
            createBotList(botList);
        }
    });

    var getCheckedAssets = function () {

        var list = $('.bot-assets').find('input[name=bot-assets]:checked');
        var checked = [];
        if (list.length === 0){
            toastr.warning('No checked coins');
            return;
        }

        for(var i = 0; i<list.length; i++){
            checked.push($(list[i]).val());
        }
        return checked;
    };
    
    var createBotList = function (botList) {

        var link = botAssetsLink;

        for(var i = 0; i <botList.length; i++) {
            if (i === 0) {
                link += '?assetsList=' + botList[i];
            } else {
                link += '&assetsList=' + botList[i];
            }
        }

        utils.loaderPageShow();

        $.ajax({
            url: link,
            type:'GET',
            success: function (data) {
                utils.loaderPageHide();
                toastr.success('Coins for bot has been successfully created!');
                //botPortalLink
                window.location.href = botPortalLink;
            },
            error: function (error) {
                bootbox.alert(error.responseJSON.message);
                utils.loaderPageHide();
            }
        })
    };
    var wrapForForecastElements = function (pickerName, botArrange) {
        var arrange = botArrange;
        var assetName = $('#'+pickerName).find('option:selected').val();
        var tableRow = $("#table-report-content").find("td").filter(function() {
            return $(this).text() === assetName;
        }).closest("tr");

        var data = {
            arrange: arrange,
            assetName: assetName
    };
        var allow = true;
        for (var prop in data) {
            if(!data[prop] && data[prop]!== 0) allow = false;
        }
        if(allow){
            showForecastElements(data, tableRow);
        }
    };
    
    var showForecastElements = function (data, containigRow) {
        $.ajax({
            url: arrangeBotComponentLink,
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
    
    var wrapData = function () {
        var data = {
            rsi: '',
            trend: [],
            border: []
        };
        
        data.rsi = $('#rsi').val();
        data.trend = $('#trend-select').val();
        data.border = $('#border-select').val();
        
        return data;
    };
    var latestAssets = function () {
        $.ajax({
            url: latestArrangedLink,
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
    var makeForecast = function (data) {
        collapseAdj.click();
        utils.loaderPageShow();
        $.ajax({
            url: botForecastLink,
            type:'POST',
            data: data,
            success: function (data) {
                buildAutoResponse(data);
                utils.loaderPageHide();
                toastr.success("Success!");
                $("#run-arrange-forecast").attr("disabled", "disabled");
                $("#btc-forecast").css("pointer-events", "none");

                setTimeout(function() {
                    $("#run-arrange-forecast").removeAttr("disabled");
                    $("#btc-forecast").css("pointer-events", "auto");
                }, 60000);

            },
            error: function (error) {
                bootbox.alert(error.responseJSON.message);
                utils.loaderPageHide();
            }
        })
    };

    var buildAutoResponse = function (data) {
        arrangePicker(data.buy, 'buy-picker');
        arrangePicker(data.consider, 'consider-picker');
        arrangePicker(data.dontBuy, 'dontbuy-picker');

        reportTable(data.report);
        utils.udpateStats(data.callsMadeHisto, data.callsLeftHisto);
    };
    var buildLatest = function (data) {
        arrangePicker(data.buy, 'buy-picker');
        arrangePicker(data.consider, 'consider-picker');
        arrangePicker(data.dontBuy, 'dontbuy-picker');

        reportTable(data.report);
    };
    var arrangePicker = function (data, pickerId) {
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
                if(data[i].botArrange === utils.botArrange.dontBuy){
                    $report += '<td class="danger">' + (i+1) + '</td>';
                    $report += '<td class="danger">' +'<label class="bot-assets mt-checkbox"><input type="checkbox" value='+data[i].assetName+' name="bot-assets"><span></span></label>'+ '</td>';
                    $report += '<td class="danger">' + data[i].assetName + '</td>';
                    $report += '<td class="danger">' + data[i].log + '</td>';
                    $report += '<td class="danger '+specifyWidthClass(data[i].width)+'">' + data[i].width + '</td>';
                    $report += '<td class="danger">' + data[i].rate + '</td>';
                    $report += '<td class="danger">' + data[i].change + '</td>';
                    $report += '<td class="danger">' + data[i].volume + '</td>';
                    $report += '<td class="danger '+ specifyRsiClass(data[i].rsi) +'">' + data[i].rsi + '</td>';
                    $report += '<td class="danger">' + data[i].botArrange + '</td>';
                } else if (data[i].botArrange === utils.botArrange.consider){
                    $report += '<td class="active">' + (i+1) + '</td>';
                    $report += '<td class="active">' +'<label class="bot-assets mt-checkbox"><input type="checkbox" value='+data[i].assetName+' name="bot-assets"><span></span></label>'+ '</td>';
                    $report += '<td class="active">' + data[i].assetName + '</td>';
                    $report += '<td class="active">' + data[i].log + '</td>';
                    $report += '<td class="active '+specifyWidthClass(data[i].width)+'">' + data[i].width + '</td>';
                    $report += '<td class="active">' + data[i].rate + '</td>';
                    $report += '<td class="active">' + data[i].change + '</td>';
                    $report += '<td class="active">' + data[i].volume + '</td>';
                    $report += '<td class="active '+ specifyRsiClass(data[i].rsi) +'">' + data[i].rsi + '</td>';
                    $report += '<td class="active">' + data[i].botArrange + '</td>';
                } else if (data[i].botArrange === utils.botArrange.buy){
                    $report += '<td class="success">' + (i+1) + '</td>';
                    $report += '<td class="success">' +'<label class="bot-assets mt-checkbox"><input type="checkbox" value='+data[i].assetName+' name="bot-assets" checked><span></span></label>'+ '</td>';
                    $report += '<td class="success">' + data[i].assetName + '</td>';
                    $report += '<td class="success">' + data[i].log + '</td>';
                    $report += '<td class="success '+specifyWidthClass(data[i].width)+'">' + data[i].width + '</td>';
                    $report += '<td class="success">' + data[i].rate + '</td>';
                    $report += '<td class="success">' + data[i].change + '</td>';
                    $report += '<td class="success">' + data[i].volume + '</td>';
                    $report += '<td class="success '+ specifyRsiClass(data[i].rsi) +'">' + data[i].rsi + '</td>';
                    $report += '<td class="success">' + data[i].botArrange + '</td>';
                } 
                $report += '</tr>';
            }
            var $table = $("#table-report-content");
            $table.find("tr").remove();
            $table.html($report);
        }
    };

    var specifyRsiClass = function (rsi) {
        var value = rsi.slice(0, -1);
        var $class = '';
        if( value > 0 && value < 50){
            $class = 'italic-text';
        }
        else if(value >= 50 && value <= 70){
            $class = 'bold-text';
        }
        else{
            $class = 'underline-text';
        }
        return $class;
    };

    var specifyWidthClass = function (width) {
        var value = width;
        var $class = 'underline-text';
        if( value === "Medium"){
            $class = '';
        }
        else if(value === "Narrow"){
            $class = 'bold-text';
        }
        else{
            $class = 'italic-text';
        }
        return $class;
    };
    var forecastElementsLoaded = function (data, row) {
        var indicatorVal = row.find("td:nth-child(4)").text();
        var width = row.find("td:nth-child(5)").text();
        var rate = row.find("td:nth-child(6)").text();
        var change = row.find("td:nth-child(7)").text();
        var volume = row.find("td:nth-child(8)").text();
        var rsi = row.find("td:nth-child(9)").text();
        table(data.table);
        imgForecast(data.forecastPath);
        imgComponents(data.componentsPath);
        assetName(data.assetName);
        indicator(indicatorVal);
        arrange(data.arrange);
        rateFeature(rate);
        widthFeature(width);
        changeFeature(change);
        volumeFeature(volume);
        rsiFeature(rsi);
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
    var rsiFeature = function (rsi) {
        $("#feature-rsi").html(rsi);
    };
    var rateFeature = function (feature) {
        $('#rate-indicator').html(feature);
    };

    var widthFeature = function (width) {
        $('#width-indicator').html(width);
    };
    var indicator = function (data) {
        $('#indicator-text').html(data);
    };
    var arrange = function (indicator) {
        var span = '';
        if(indicator === utils.botArrange.buy) {
            span = $('<span />',{
                class:'label label-success',
                html:'Buy'
            });
        }
        else if(indicator === utils.botArrange.consider){
            span = $('<span />',{
                class:'label label-default',
                html:'Consider'
            });
        }
        else if(indicator === utils.botArrange.dontBuy){
            span = $('<span />',{
                class:'label label-danger',
                html:'DontBuy'
            });
        }

        
        $('#arrange-span').html(span);
    };
})();