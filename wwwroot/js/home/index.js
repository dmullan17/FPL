HomeViewModel = function (data) {
    "use strict";

    var self = this;

    self.Gameweeks = ko.observableArray(data.Gameweeks);
    self.CurrentGameweek = ko.observable(data.CurrentGameweek);

    self.init = function () {
    };

    self.init();
};