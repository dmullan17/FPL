TeamViewModel = function (data) {
    "use strict";

    var self = this;

    self.AllTeams = ko.observableArray(data.AllTeams);
    self.Team = ko.observable(data.Team);

    self.init = function () {
    };

    self.init();
};