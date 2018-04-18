var instantForecast = (function () {
    var modalWindow = $('#btc-modal'),
        instantForecastLink = $('#instant-forecast-link').data('request-url');

    $('#btc-forecast').click(function () {
        instantForecastRequest();
        
    });

    var instantForecastRequest = function () {
        utils.loaderShow();
        $.ajax({
            url: instantForecastLink,
            type:'GET',
            success: function (data) {
                if (data){
                    buildComponents(data);
                    utils.loaderHide();
                    modalWindow.modal('show');
                }
                else{
                    alert('No BTC data!')
                }
            },
            error: function (error) {
                utils.loaderHide();
                alert(error.responseJSON.message);

            }
        })
    };

    var buildComponents = function (data) {
        assetName(data.assetName);
        forecastImage(data.forecastPath);
        forecastStats(data.indicator, data.rate);
        utils.loaderHide();
        utils.modalWindow.modal('show');
        callsStats(data.callsMadeHisto, data.callsLeftHisto);
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
    
    var forecastStats = function (indicatorVal, rateVal) {
        var indicator = indicatorVal;
        var rate = 'Rate: ' +  rateVal;

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

        $('#instant-indicator').html(span);
        $('#instant-rate').html(rate);
    };
    
    var callsStats = function (made, left) {
        $('#made-number').html(made);
        $('#left-number').html(left);
    }
})();

