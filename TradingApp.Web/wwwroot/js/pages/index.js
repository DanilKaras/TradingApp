var index = (function () {
    var loadExchanges = $('#load-exchanges').data('request-url'),
        updateByExhange = $('#update-byexchange-link').data('request-url'),
        testTelegram = $('#test-link').data('request-url'),
        testTelegramMessage = $('#test-link-message').data('request-url'),
        block = '#settings-block';
    
    $(document).ready(function () {
        utils.tabs(utils.pages.settings);
        loadExhangesRequest();
    });

    $("#update-by-exchange").click(function () {
            var selectedExchange = $('.selectpicker option:selected').val();
            var upperBorder = $('#upper').val();
            var lowerBorder = $('#lower').val();
            var upperWidth = $('#upper-width').val();
            var lowerWidth = $('#lower-width').val();
            var data = {
                upperBorder: upperBorder,
                lowerBorder: lowerBorder,
                upperWidth: upperWidth,
                lowerWidth: lowerWidth,
                lastExchange: selectedExchange,
                btc: '',
                exchanges: null
            };
            updateByExchangeRequest(data);
        }
    );
    
    $('#test').click(function (){
        testTelegramFunc();
    });
    $('#test-message').click(function () {
       testMessageFunc(); 
    });
    var loadExhangesRequest = function () {
        utils.loaderBlockShow(block);
        $.ajax({
            url: loadExchanges,
            type:'GET',
            success: function (data) {
                buildComponents(data);
                utils.loaderBlockHide(block);
            },
            error: function (error) {
                bootbox.alert(error.responseJSON.message);
                utils.loaderBlockHide(block);
            }
        })
    };
    
    var testTelegramFunc = function () {
        $.ajax({
            url: testTelegram,
            type: 'GET',
            success: function () {
                
            },
            error: function () {
                
            }
        });
    };
    var testMessageFunc = function () {
        $.ajax({
            url: testTelegramMessage,
            type: 'GET',
            success: function () {

            },
            error: function () {

            }
        });
    };
        
    
    
    var updateByExchangeRequest = function (data) {
        utils.loaderBlockShow(block);
        $.ajax({
            url: updateByExhange,
            type:'GET',
            data: data,
            success: function (data) {
                updateComponents(data);
                utils.loaderBlockHide(block);
            },
            error: function (error) {
                bootbox.alert(error.responseJSON.message);
                utils.loaderBlockHide(block);
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
        $('#lower-width').val(data.lowerWidth);
        $('#upper-width').val(data.upperWidth);
    };

    var updateComponents = function(data){
        $('#btc-name').val(data.btc);
        $('#upper').val(data.upperBorder);
        $('#lower').val(data.lowerBorder);
        $('#lower-width').val(data.lowerWidth);
        $('#upper-width').val(data.upperWidth);
    }

})();