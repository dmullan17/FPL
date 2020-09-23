FixturesViewModel = function (data) {
    "use strict";

    var self = this;

    self.Fixtures = ko.observableArray(data.Fixtures);

    self.init = function () {
    };

    self.init();
};