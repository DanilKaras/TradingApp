var instantForecast = (function () {
    var modalId = '#btc-modal',
        modalWindow = $(modalId),
        instantForecastLink = $('#instant-forecast-link').data('request-url');

    $('#btc-forecast').click(function () {
        instantForecastRequest();
    });

    var instantForecastRequest = function () {
        utils.loaderPageShow(modalId);
        $.ajax({
            url: instantForecastLink,
            type:'GET',
            success: function (data) {
                if (data){
                    buildComponents(data);
                    utils.loaderPageHide(modalId);
                    modalWindow.modal('show');
                }
                else{
                    alert('No BTC data!')
                }
            },
            error: function (error) {
                utils.loaderPageHide();
                bootbox.alert(error.responseJSON.message);
            }
        })
    };

    var buildComponents = function (data) {
        assetName(data.assetName);
        forecastImage(data.forecastPath);
        forecastStats(data.indicator, data.rate, data.change);
        utils.udpateStats(data.callsMadeHisto, data.callsLeftHisto);
        rsi(data.rsi);
    };
    
    
    var assetName = function (asset) {
        modalWindow.find('.modal-title').html(asset);
    };
    
    var forecastImage = function (path) {
        if(path){
            var imgForecast = $('<img />', {
                id: 'instant-forecast',
                src: path,
                class: "img-responsive",
                alt: 'Cinque Terre'
            });
            $('#instant-forecast-place').html(imgForecast);
        }
    };
    
    var forecastStats = function (indicatorVal, rateVal, change) {
        var indicator = indicatorVal;
        var rate = rateVal;
        
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
        
        var ch = change+"%";
        
        $("#change-indicator").html(ch);
        $('#instant-indicator').html(span);
        $('#instant-rate').html(rate);
    };
    var rsi = function (rsi) {
        $('#instant-rsi').html(rsi + '%');
    };
    var callsStats = function (made, left) {
        $('#made-number').html(made);
        $('#left-number').html(left);
    }
})();

