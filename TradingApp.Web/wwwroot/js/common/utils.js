var utils = (function () {
    var toastrConfig = function () {
        toastr.options = {
            "closeButton": false,
            "debug": false,
            "positionClass": "toast-bottom-right",
            "onclick": null,
            "showDuration": "1000",
            "hideDuration": "1000",
            "timeOut": "5000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        }
    };
    
    var group = {
        useButtons: "useButtons",
        useSlider: "useSlider"
    };
    
    var periodGroup = {
        usePeriodSlider: "usePeriodSlider",
        usePeriodToggle: "usePeriodToggle"    
    };

    var pages = {
        settings: 1,
        auto: 2,
        manual: 3
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

    var loaderPageShow = function () {
        App.blockUI({
            animate: 1,
            overlayColor: '#461d6d' 
        })  
    };
    
    var loaderPageHide = function () {
        App.unblockUI();
    };
    
    var loaderBlockShow = function (elementId) {
        App.blockUI({
            target: elementId,
            animate: true
        });
    };

    var loaderBlockHide = function (elementId) {
        App.unblockUI(elementId)
    };
    var udpateStats = function (callsMade, callsLeft) {
        $('#calls-made').html(callsMade);
        $('#calls-left').html(callsLeft);
    };
    
    var tabs = function (page) {
        var navMenu = $('.hor-menu');
        navMenu.find('li.active').removeClass('active');
        switch (page){
            case 1:{
                return navMenu.find("ul li:nth-child(1)").addClass('active');
            }
            case 2:{
                return navMenu.find("ul li:nth-child(2)").addClass('active');
            }
            case 3:{
                return navMenu.find("ul li:nth-child(3)").addClass('active');
            }
            default:
                break;
        }     
        
    };
    return {
        loaderBlockShow: loaderBlockShow,
        loaderBlockHide: loaderBlockHide,
        loaderPageShow: loaderPageShow,
        loaderPageHide: loaderPageHide,
        group: group,
        indicators: indicators,
        logs: logs,
        fixedOutput: fixedOutput,
        modalWindow: modalWindow,
        periodGroup: periodGroup,
        toastrConfig: toastrConfig,
        udpateStats: udpateStats,
        pages: pages,
        tabs: tabs
    };
})();