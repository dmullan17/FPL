FPLViewModel = function (data) {
    "use strict";

    var self = this;

    self.GWTeam = ko.observable(data.GWTeam);
    self.Team = ko.observable(data.Team);
    self.TotalPoints = ko.observable(data.TotalPoints);


    //self.getColor = ko.pureComputed(function (data) {
    //    return this;
    //});

    self.getColor = function (multipler) {
        if (multipler == 0) {
            return "grey";
        }
        else {
            return null;
        }
    };

    //self.SubOn = function (player) {
    //    for (var i = 0; i < self.GWTeam().automatic_subs.length; i++) {
    //        if (self.GWTeam().automatic_subs[i].element_in == player.element) {
    //            return true;
    //        }
    //        else {
    //            return false;
    //        }
    //    }
    //};

    //self.SubOff = function (player) {
    //    for (var i = 0; i < self.GWTeam().automatic_subs.length; i++) {
    //        if (self.GWTeam().automatic_subs[i].element_out == player.element) {
    //            return true;
    //        }
    //        else {
    //            return false;
    //        }
    //    }
    //};

    self.GetPosition = function (position) {
        if (position == 1)
        {
            return "GK";
        }
        else if (position == 2)
        {
            return "DEF";
        }
        else if (position == 3) {
            return "MID";
        }
        else if (position == 4) {
            return "FWD";
        }
    };

    //self.OnBench = function (entry) {
    //    //return entry.type === 'file' ? 'icon-file' : 'icon-filder';
    //    return;
    //};

    self.init = function () {
        //totalling a players total gw stats
        //for (var i = 0; i < self.Picks().length; i++) {
        //    for (var j = 0; j < self.Picks()[i].GWPlayer.explain.length; j++) {
        //        for (var k = 0; k < self.Picks()[i].GWPlayer.explain[j].stats.length; k++) {
        //            if (self.Picks()[i].GWPlayer.explain[j].stats[k].points != 0) {
        //                self.Picks()[i].GWPlayer.stats.gw_points += self.Picks()[i].GWPlayer.explain[j].stats[k].points;
        //            }
        //        }
        //    }
        //}
    };

    self.init();
};