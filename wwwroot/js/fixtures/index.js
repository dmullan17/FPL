FixturesViewModel = function (data) {
    "use strict";

    var self = this;

    self.Fixtures = ko.observableArray(data.Fixtures);
    self.CurrentGameweekId = ko.observable(data.CurrentGameweekId);
    self.Game = ko.observable();
    self.GameStats = ko.observableArray();

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

    self.viewGame = function (data) {
        self.Game(data);
        self.GameStats(data.stats);
        $('.ui.modal').modal().modal('show');
    };


    self.init = function (data) {
        // $('.ui.modal').modal();

    };

    self.init();
};