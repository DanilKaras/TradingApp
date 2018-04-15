var settings = (function () {
    var loadExchanges = $('#load-exchanges').data('request-url'),
        updateByExhange = $('#update-byexchange-link').data('request-url');
    
    $(document).ready(function () {
        utils.loaderShow();
        loadExhangesRequest();
    });
    
    $("#update-by-exchange").click(function () {
            var selectedExchange = $('.selectpicker option:selected').val();
            var upperBorder = $('#upper').val();
            var lowerBorder = $('#lower').val();
            var data = {
                upperBorder: upperBorder,
                lowerBorder: lowerBorder,
                lastExchange: selectedExchange,
                btc: '',
                exchanges: null 
            };
            updateByExchangeRequest(data);
            //requests.updateByExchangeRequest(data);
        }
    );

    var loadExhangesRequest = function () {
        $.ajax({
            url: loadExchanges,
            type:'GET',
            success: function (data) {
                buildComponents(data);
                utils.loaderHide();
                
            },
            error: function (error) {
                alert(error.message);
                utils.loaderHide();
            }
        })
    };

    var updateByExchangeRequest = function (data) {
        utils.loaderShow();
        $.ajax({
            url: updateByExhange,
            type:'GET',
            data: data,
            success: function (data) {
                updateComponents(data);
                utils.loaderHide();
            },
            error: function (error) {
                alert(error.message);
                utils.loaderHide();
            }
        })
    };

    var buildComponents = function (data) {
        if (data.exchanges) {
            var jsonData = data.exchanges;
            var picker = $('#exhanges-ddl');
            for (var i = 0; i < jsonData.length; i++) {
                if(jsonData[i] === data.lastExchange){
                    picker.append('<option value="' + jsonData[i] + '" selected>' + jsonData[i] + '</option>')
                }
                else{
                    picker.append('<option value="' + jsonData[i] + '">' + jsonData[i] + '</option>')
                }
            }
            picker.selectpicker('refresh');
        }
        $('#btc-name').val(data.btc);
        $('#upper').val(data.upperBorder);
        $('#lower').val(data.lowerBorder);
    };
    
    var updateComponents = function(data){
        $('#btc-name').val(data.btc);
        $('#upper').val(data.upperBorder);
        $('#lower').val(data.lowerBorder);
    }
    
})();