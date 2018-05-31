var botPortal = (function () {

    var fireLink = $('#fire-task-link').data('request-url'),
        fireLink2 = $('#fire-task-second-link').data('request-url'),
        stopLink = $('#stop-task-link').data('request-url'),
        triggerLink = $('#trigger-task-link').data('request-url');
    //fire-btn-second;
    $(document).ready(function () {
        utils.tabs(utils.pages.botPortal);
    });
    
    $('#fire-btn').click(function () {
        fireRequest();
    });

    $('#fire-btn-second').click(function () {
        fireRequestSecond();
    });
    
    $('#trigger-btn').click(function () {
        triggerRequest();
    });
    
    $('#stop-btn').click(function () {
        stopRequest();
    });


    var fireRequest = function () {
        $.ajax({
            url: fireLink,
            type:'POST',
            success: function () {
                toastr.success("Forecasts have been fired")
            },
            error: function (error) {
                bootbox.alert(error.responseJSON.message);
            }
        })
    };

    var fireRequestSecond = function () {
        $.ajax({
            url: fireLink2,
            type:'POST',
            success: function () {
                toastr.success("Forecasts have been fired")
            },
            error: function (error) {
                bootbox.alert(error.responseJSON.message);
            }
        })
    };

    var stopRequest = function () {
        $.ajax({
            url: stopLink,
            type:'POST',
            success: function () {
                toastr.success("Forecasts have been stopped")
            },
            error: function (error) {
                bootbox.alert(error.responseJSON.message);
            }
        })
    };

    var triggerRequest = function () {
        $.ajax({
            url: triggerLink,
            type:'POST',
            success: function () {
                toastr.success("Forecasts have been triggered")
            },
            error: function (error) {
                bootbox.alert(error.responseJSON.message);
            }
        })
    };
    
    
    
})();