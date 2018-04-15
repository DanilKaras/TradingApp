var manual = (function () {
    var lessToggle = true,
        moreToggle = false;
    $(document).ready(function () {
        $('#ex13').slider({
            ticks: [0, 240, 480, 720],
            ticks_labels: ['0', '240', '480', '720'],
            ticks_snap_bounds: 30
        });
        //builder.toastrConfig();
        //requests.selecter();
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
    
    $("#update-assets").click(function (){
        requests.updateAssets()
    });
        
    
})();