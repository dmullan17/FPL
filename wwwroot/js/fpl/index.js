FPLViewModel = function (data) {
    "use strict";

    var self = this;

    self.Picks = ko.observableArray(data.picks);

    self.init = function () {
    };

    self.init();
};