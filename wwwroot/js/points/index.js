GameweekPointsViewModel = function (data) {
    "use strict";

    var self = this;

    self.GWTeam = ko.observable(data.GWTeam);
    self.Team = ko.observable(data.Team);
    self.TotalPoints = ko.observable(data.TotalPoints);
    self.GWPoints = ko.observable(data.GWPoints);
    self.GameweekId = ko.observable(data.GameweekId);
    self.EventStatus = ko.observable(data.EventStatus);

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

    self.SubOn = function (player) {
        for (var i = 0; i < self.GWTeam().automatic_subs.length; i++) {
            if (self.GWTeam().automatic_subs[i].element_in == player.element) {
                return true;
            }
        }
        return false;
    };

    self.SubOff = function (player) {
        for (var i = 0; i < self.GWTeam().automatic_subs.length; i++) {
            if (self.GWTeam().automatic_subs[i].element_out == player.element) {
                return true;
            }
        }
        return false;
    };

    self.HomeOrAway = function (player) {

        if (player.GWGame.team_h == player.player.Team.id) {
            return player.GWGame.AwayTeam.name + " (H)"
        }
        else if (player.GWGame.team_a == player.player.Team.id) {
            return player.GWGame.HomeTeam.name + " (A)"
        }
    };

    self.GetPlayerStatusIcon = function (playerStatus) {
        //injured
        if (playerStatus == "i") {
            return "plus circle icon";
        }
        //doubtful
        else if (playerStatus == "d") {
            return "help circle icon";
        }
        //not available or suspended
        else if (playerStatus == "n" || playerStatus == "s") {
            return "warning circle icon";
        }
        //unavailable - left the club
        else if (playerStatus == "u") {
            return "warning sign icon";
        }
        else {
            return null;
        }
    };

    self.DoesPlayerHaveStatus = function (playerStatus) {
        if (playerStatus == "i" || playerStatus == "d" || playerStatus == "n" || playerStatus == "s" || playerStatus == "u") {
            return true;
        }
        else {
            return false;
        }
    };


    self.GetBonus = function (player) {

        var kickoffDate = new Date(player.GWGame.kickoff_time);
        kickoffDate.setUTCHours(0, 0, 0, 0);

        if (self.EventStatus().status[0].event == self.GameweekId() && player.GWGame.started) {
            for (var i = 0; i < self.EventStatus().status.length; i++) {

                var eventStatusDate = new Date(self.EventStatus().status[i].date);
                eventStatusDate.setUTCHours(0, 0, 0, 0);

                if (kickoffDate.getTime() == eventStatusDate.getTime()) {

                    if (self.EventStatus().status[i].bonus_added) {
                        return player.GWPlayer.stats.bonus;
                    }
                    else if (!self.EventStatus().status[i].bonus_added) {
                        return player.GWPlayer.stats.EstimatedBonus + "*";
                    }
                }
            }
        } else {
            return player.GWPlayer.stats.bonus;
        }

    };

    self.GetPoints = function (player) {

        var kickoffDate = new Date(player.GWGame.kickoff_time);
        kickoffDate.setUTCHours(0, 0, 0, 0);

        if (self.EventStatus().status[0].event == self.GameweekId() && player.GWGame.started) {
            for (var i = 0; i < self.EventStatus().status.length; i++) {

                var eventStatusDate = new Date(self.EventStatus().status[i].date);
                eventStatusDate.setUTCHours(0, 0, 0, 0);

                if (kickoffDate.getTime() == eventStatusDate.getTime()) {

                    if (self.EventStatus().status[i].bonus_added) {
                        return player.GWPlayer.stats.gw_points;
                    }
                    else if (!self.EventStatus().status[i].bonus_added) {
                        if (player.is_captain) {
                            return player.GWPlayer.stats.gw_points + (player.GWPlayer.stats.EstimatedBonus * 2) + "*";
                        }
                        else {
                            return player.GWPlayer.stats.gw_points + player.GWPlayer.stats.EstimatedBonus + "*";
                        }
                    }
                }
            }
        } else {
            return player.GWPlayer.stats.gw_points;
        }

    };


    self.GetGWPoints = function (points) {

        var today = new Date();
        today.setUTCHours(0, 0, 0, 0);

        if (self.EventStatus().status[0].event == self.GameweekId() && self.EventStatus().leagues != "Updated") {
            for (var i = 0; i < self.EventStatus().status.length; i++) {

                var eventStatusDate = new Date(self.EventStatus().status[i].date);
                eventStatusDate.setUTCHours(0, 0, 0, 0);

                if (today.getTime() == eventStatusDate.getTime()) {

                    if (self.EventStatus().status[i].bonus_added) {
                        return self.GWPoints();
                    }
                    else if (!self.EventStatus().status[i].bonus_added) {

                        for (var j = 0; j < self.GWTeam().picks.length; j++) {

                            var kickoffDate = new Date(self.GWTeam().picks[j].GWGame.kickoff_time);
                            kickoffDate.setUTCHours(0, 0, 0, 0);

                            if (kickoffDate.getTime() == eventStatusDate.getTime()) {
                                if (self.GWTeam().picks[j].is_captain) {
                                    points = points + (self.GWTeam().picks[j].GWPlayer.stats.EstimatedBonus * 2);
                                }
                                else {
                                    points = points + self.GWTeam().picks[j].GWPlayer.stats.EstimatedBonus;
                                }
                            }
                        }
                        return points + "*";
                    }
                }
            }
        } else {
            return self.GWPoints();
        }

    };

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

    self.FormatValue = function (value) {
        var value = parseFloat(value) / 10;
        return value.toFixed(1);
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