(function() {
    $(function() {
        var events = $('#events tbody tr').map(function(tr) {
            return {
                url: $('td:eq(0)', tr).html(),
                start: $('td:eq(1)', tr).html(),
                title: $('td:eq(2)', tr).html()
            };
        }).toArray();
        
        $('#calendar').fullCalendar({
            header: {
                left: 'prev,next today',
                center: 'title',
                right: ''
            },
            events: events
        });
    });
})();