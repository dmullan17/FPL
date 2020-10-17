﻿MyTeamViewModel = function (data) {
    "use strict";

    var self = this;

    self.Picks = ko.observable(data.Picks);
    self.Team = ko.observable(data.Team);
    self.Positions = ko.observableArray(data.Positions);
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


    self.GetPlayersNextFixture = function (team) {

        var firstFixture = team.Fixtures[0];

        if (team.id == firstFixture.team_h) {
            return firstFixture.team_a_name + " (H)";
        }
        else if (team.id == firstFixture.team_a) {
            return firstFixture.team_h_name + " (A)";
        }
    };

    self.FormatValue = function (value) {
        var value = parseFloat(value) / 10;
        return value.toFixed(1);
    };

    self.FormatOverallRank = function (value) {
        var totalPlayerCount = 0;
        for (var i = 0; i < self.Positions().length; i++) {
            totalPlayerCount += self.Positions()[i].element_count;
        }
        return value + " / " + totalPlayerCount;
    };

    self.FormatPositionRankType = function (postionId, value) {
        var pos = self.Positions().find(x => x.id == postionId);
        return value + " / " + pos.element_count;
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
        if (position == 1) {
            return "GK";
        }
        else if (position == 2) {
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