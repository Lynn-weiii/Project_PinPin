    var today = new Date();
    var formattedToday = today.toISOString().split('T')[0].replace(/-/g, '/');  // 格式化為 "YYYY/MM/DD"

    $(elementId).daterangepicker({
        "autoApply": true,
        "locale": {
            "format": "YYYY/MM/DD",
            "separator": " - ",
            "applyLabel": "Apply",
            "cancelLabel": "Cancel",
            "fromLabel": "From",
            "toLabel": "To",
            "customRangeLabel": "Custom",
            "weekLabel": "W",
            "daysOfWeek": [
                "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"
            ],
            "monthNames": [
                "Jan", "Feb", "Mar", "Apr", "May", "June",
                "July", "Aug", "Sep", "Oct", "Nov", "Dec"
            ],
            "firstDay": 1
        },
        "minDate": formattedToday,
        "startDate": formattedToday,
        "endDate": formattedToday
    });