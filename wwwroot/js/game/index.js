GameViewModel = function (data) {
    "use strict";

    var self = this;

    self.AllGames = ko.observableArray(data.AllGames);
    self.Game = ko.observable(data.Game);
    self.GameStats = ko.observableArray(data.GameStats);
    self.TotalGameCount = ko.observable(data.TotalGameCount);

    self.init = function () {
    };

    self.init();
};