FixturesViewModel = function (data) {
    "use strict";

    var self = this;

    self.Fixtures = ko.observableArray(data.Fixtures);
    self.CurrentGameweekId = ko.observable(data.CurrentGameweekId);

    // self.GameweekName =  ko.computed(function () {
    //     return "GameWeek " + self.CurrentGameweekId();
    // });

    self.next = function () {
        self.CurrentGameweekId(self.CurrentGameweekId() + 1);
        window.location = "/fixtures/" + self.CurrentGameweekId();
    };

    self.previous = function () {
        self.CurrentGameweekId(self.CurrentGameweekId() - 1);
        window.location = "/fixtures/" + self.CurrentGameweekId();
    };


    self.init = function () {
        
    };

    self.init();
};