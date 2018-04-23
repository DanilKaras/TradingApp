var utils = (function () {

    var loaderShow = function () {
        $('#loader').show();
        $('body').append('<div id="overlay"></div>');
    };


    var loaderHide  = function () {
        $('#loader').hide();
        $('#overlay').remove();
    };

    var group = {
        useButtons: "useButtons",
        useSlider: "useSlider"
    };
    
    var periodGroup = {
        usePeriodSlider: "usePeriodSlider",
        usePeriodToggle: "usePeriodToggle"    
    };

    var indicators = {
        positive: 0,
        neutral: 1,
        negative: 2,
        superPositive: 3
    };

    var logs = {
        positive: 'Positive',
        neutral: 'Neutral',
        negative: 'Negative',
        strongPositive: 'StrongPositive',
        zeroRezults: 'ZeroRezults'
    };

    var fixedOutput = function(number) {
        number = number.toString();
        var charNum = number.indexOf(".");
        var beforePoint = number.substring(0, charNum);
        var afterPoint = number.substring(charNum, charNum + 8);
        return beforePoint+ afterPoint;
    };
    var modalWindow = $('#btc-modal');

    return {
        loaderShow: loaderShow,
        loaderHide: loaderHide,
        group: group,
        indicators: indicators,
        logs: logs,
        fixedOutput: fixedOutput,
        modalWindow: modalWindow,
        periodGroup: periodGroup
    };
})();