﻿$(document).ready(function() {
    utils.toastrConfig();
    
    $('[data-toggle="tooltip"]').tooltip();
});

var site = (function () {
    var updateStats = function (callsMade, callsLeft) {
        $('#cals-made').html(callsMade);
        $('#cals-left').html(callsLeft);
    };
    
    return {
        updateStats : updateStats
    }
})();




    