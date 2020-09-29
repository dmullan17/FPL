FixturesViewModel = function (data) {
    "use strict";

    var self = this,
    buttonNext = $("#button-next"),
    buttonPrevious = $("#button-previous");

    self.Fixtures = ko.observableArray(data.Fixtures);
    self.CurrentGameWeek = ko.observable(data.CurrentGameWeek);
    self.CurrentGameweekId = ko.observable(data.CurrentGameweekId);
    self.LiveGames = ko.observableArray(data.LiveGames);
    self.Game = ko.observable();
    self.GameStats = ko.observableArray();

    // self.GameweekName =  ko.computed(function () {
    //     return "GameWeek " + self.CurrentGameweekId();
    // });

    self.IsGameLive = ko.computed(function () {

        if (self.LiveGames().length != 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    });

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
        if (self.CurrentGameweekId() == 1)
        {
            buttonPrevious.addClass("disabled");
        }
        else if (self.CurrentGameweekId() == 38)
        {
            buttonNext.addClass("disabled");
        }

        if (self.LiveGames().length != 0)
        {
            for (var i = 0; i <= self.LiveGames().length - 1; i++)
            {
                var game = self.LiveGames()[i];
                var date = new Date(game.kickoff_time);
                var milliseconds = date.getTime();

                for (var j = 0; j <= self.Fixtures().length - 1; j++)
                {
                    if (self.Fixtures()[j].id == game.id && game.minutes == 0)
                    {
                        var diff = (Date.now() - milliseconds) / 1000;
                        diff /= 60;
                        diff = Math.abs(Math.round(diff));

                        if (diff >= 48 && diff <= 63) {
                            self.Fixtures()[j].minutes = diff;
                            self.Fixtures()[j].is_half_time = true;
                        }

                        if (diff > 63) {
                            self.Fixtures()[j].minutes = diff - 15;
                            self.Fixtures()[j].is_half_time = false;
                        }
                    }

                }
            }
        }

    };

    self.init();
};