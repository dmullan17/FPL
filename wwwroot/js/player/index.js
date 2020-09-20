PlayerViewModel = function (data) {
    "use strict";

    var self = this;

    self.Player = ko.observable(data.Player);
    self.TotalPlayerCount = ko.observable(data.TotalPlayerCount);

    self.init = function () {
    };

    self.init();
};