GameweekPointsViewModel = function (data) {
    "use strict";

    var self = this;

    self.GWTeam = ko.observable(data.GWTeam);
    self.Team = ko.observable(data.Team);
    self.TotalPoints = ko.observable(data.TotalPoints);
    self.GWPoints = ko.observable(data.GWPoints);
    self.GameweekId = ko.observable(data.GameweekId);
    self.EventStatus = ko.observable(data.EventStatus);
    self.EntryHistory = ko.observable(data.EntryHistory);
    self.IsLive = ko.observable(data.IsLive);
    self.SelectedPlayer = ko.observable();
    self.SelectedPlayerStatus = ko.observable();

    self.viewPlayer = function (player) {
        self.SelectedPlayer(player);


        $('.ui.modal').modal().modal('show');
    };

    self.getColor = function (position) {
        if (position > 11) {
            return "lightgray";
        }
        else {
            return null;
        }
    };

    self.GetResult = function (pick, game) {

        var playerTeamId = pick.player.Team.id;

        if (game.team_h_score != null && game.team_a_score != null) {

            if (game.HomeTeam.id == playerTeamId) {

                if (game.team_h_score > game.team_a_score) {
                    return "positive";
                }
                else if (game.team_a_score > game.team_h_score) {
                    return "negative";
                }
                else if (game.team_h_score == game.team_a_score) {
                    return "warning";
                }
            }
            else if (game.AwayTeam.id == playerTeamId) {

                if (game.team_a_score > game.team_h_score) {
                    return "positive";
                }
                else if (game.team_h_score > game.team_a_score) {
                    return "negative";
                }
                else if (game.team_h_score == game.team_a_score) {
                    return "warning";
                }
            }
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

    self.GetOpposition = function (player) {

        var team = player.player.Team;

        var fixtures = team.Fixtures.filter(x => x.Event == self.GameweekId());
        var results = team.Results.filter(x => x.Event == self.GameweekId());
        var html = "";

        //if (player.GWGame.team_h == player.player.Team.id) {
        //    gwfixtures += player.GWGame.AwayTeam.name + " (H)<br/>"
        //}
        //else if (player.GWGame.team_a == player.player.Team.id) {
        //    gwfixtures += player.GWGame.HomeTeam.name + " (A)<br/>";
        //}
        if (results.length > 0) {
            for (var i = 0; i < results.length; i++) {
                //if (player.GWGame.id != results[i].id) {
                if (results[i].team_h_score > results[i].team_a_score) {
                    if (team.id == results[i].team_h) {
                        html += results[i].team_a_name + " (H)<br/>";
                    }
                    else if (team.id == results[i].team_a) {
                        html += results[i].team_h_name + " (A) <br/>";
                    }
                }
                else if (results[i].team_h_score == results[i].team_a_score) {
                    if (team.id == results[i].team_h) {
                        html += results[i].team_a_name + " (H) <br/>";
                    }
                    else if (team.id == results[i].team_a) {
                        html += results[i].team_h_name + " (A) <br/>";
                    }
                }
                else if (results[i].team_h_score < results[i].team_a_score) {
                    if (team.id == results[i].team_h) {
                        html += results[i].team_a_name + " (H) <br/>";
                    }
                    else if (team.id == results[i].team_a) {
                        html += results[i].team_h_name + " (A) <br/>";
                    }
                }
                //}

            }
        }
        if (fixtures.length > 0) {
            for (var i = 0; i < fixtures.length; i++) {
                //if (player.GWGame.id != fixtures[i].id) {
                if (team.id == fixtures[i].team_h) {
                    html += fixtures[i].team_a_name + " (H)<br/>";
                }
                else if (team.id == fixtures[i].team_a) {
                    html += fixtures[i].team_h_name + " (A)<br/>";
                }
                //}
            }
        }
        if (results.length == 0 && fixtures.length == 0) {
            html += "No Game";
        }
        //else {
        //    gwfixtures += "No Game";
        //}

        return html;

        //if (player.GWGame.team_h == player.player.Team.id) {
        //    return player.GWGame.AwayTeam.name + " (H)"
        //}
        //else if (player.GWGame.team_a == player.player.Team.id) {
        //    return player.GWGame.HomeTeam.name + " (A)";
        //}
        //else {
        //    return "No Game";
        //}
    };

    self.GetTimeOrResult = function (player) {


        var team = player.player.Team;

        var fixtures = team.Fixtures.filter(x => x.Event == self.GameweekId());
        var results = team.Results.filter(x => x.Event == self.GameweekId());
        var html = "";

        //if (player.GWGame.team_h == player.player.Team.id) {
        //    gwfixtures += player.GWGame.AwayTeam.name + " (H)<br/>"
        //}
        //else if (player.GWGame.team_a == player.player.Team.id) {
        //    gwfixtures += player.GWGame.HomeTeam.name + " (A)<br/>";
        //}
        if (results.length > 0) {
            for (var i = 0; i < results.length; i++) {
                if (results[i].finished_provisional) {
                    if (results[i].team_h_score > results[i].team_a_score) {
                        if (team.id == results[i].team_h) {
                            html += results[i].team_h_score + " - " + results[i].team_a_score + " <div class=\"result-label\" style=\"background-color: limegreen\">W</div> <br/>";
                        }
                        else if (team.id == results[i].team_a) {
                            html += results[i].team_h_score + " - " + results[i].team_a_score + " <div class=\"result-label\" style=\"background-color: red\">L</div> <br/>";
                        }
                    }
                    else if (results[i].team_h_score == results[i].team_a_score) {
                        if (team.id == results[i].team_h) {
                            html += results[i].team_h_score + " - " + results[i].team_a_score + " <div class=\"result-label\" style=\"background-color: orange\">D</div> <br/>";
                        }
                        else if (team.id == results[i].team_a) {
                            html += results[i].team_h_score + " - " + results[i].team_a_score + " <div class=\"result-label\" style=\"background-color: orange\">D</div> <br/>";
                        }
                    }
                    else if (results[i].team_h_score < results[i].team_a_score) {
                        if (team.id == results[i].team_h) {
                            html += results[i].team_h_score + " - " + results[i].team_a_score + " <div class=\"result-label\" style=\"background-color: red\">L</div> <br/>";
                        }
                        else if (team.id == results[i].team_a) {
                            html += results[i].team_h_score + " - " + results[i].team_a_score + " <div class=\"result-label\" style=\"background-color: limegreen\">W</div> <br/>";
                        }
                    }
                }
                else {
                    html += results[i].team_h_score + " - " + results[i].team_a_score + " <div class=\"ui mini green label\">Live</span> <br/>";
                }

            }
        }
        if (fixtures.length > 0) {
            for (var i = 0; i < fixtures.length; i++) {
                html += getDayOfWeek(fixtures[i].kickoff_time) + " @ " + fixtures[i].kickoff_time.substring(11, 16) + " <br/>";
            }
        }

        return html;

        //var team = player.player.Team;

        //var fixtures = team.Fixtures.filter(x => x.Event == self.GameweekId());
        //var results = team.Results.filter(x => x.Event == self.GameweekId());
        //var gwfixtures = "";

        //if (fixtures.length > 0) {
        //    for (var i = 0; i < fixtures.length; i++) {
        //        gwfixtures += getDayOfWeek(fixtures[i].kickoff_time) + " @ " + fixtures[i].kickoff_time.substring(11, 16) + " <br/>";
        //    }
        //}

        //return gwfixtures;


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

    self.ShowStatusPopup = function (playerStatus) {
        self.SelectedPlayerStatus(playerStatus);

        //$('.ui.icon').popup({
        //    popup: '.special.popup',
        //    on: 'click'
        //});
        $('.special.popup').popup().popup('show');
        //$('.ui.modal').modal().modal('show');

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

    self.GetBonusRank = function (player) {

        var kickoffDate = new Date(player.GWGame.kickoff_time);
        kickoffDate.setUTCHours(0, 0, 0, 0);

        if (self.EventStatus().status[0].event == self.GameweekId() && player.GWGame.started) {
            for (var i = 0; i < self.EventStatus().status.length; i++) {

                var eventStatusDate = new Date(self.EventStatus().status[i].date);
                eventStatusDate.setUTCHours(0, 0, 0, 0);

                if (kickoffDate.getTime() == eventStatusDate.getTime()) {

                    if (self.EventStatus().status[i].bonus_added) {
                        return player.GWPlayer.stats.BpsRank;
                    }
                    else if (!self.EventStatus().status[i].bonus_added) {
                        return player.GWPlayer.stats.BpsRank + "*";
                    }
                }
            }
        } else {
            return player.GWPlayer.stats.BpsRank;
        }

    };

    self.GetTotalPoints = function (totalPoints) {
        var status = self.EventStatus().status;

        if (status.filter(e => e.bonus_added == false).length > 0) {
            return totalPoints + "*";
        }
        else {
            return totalPoints;
        }




        //var kickoffDate = new Date(player.GWGame.kickoff_time);
        //kickoffDate.setUTCHours(0, 0, 0, 0);

        //if (self.EventStatus().status[0].event == self.GameweekId() && player.GWGame.started) {
        //    for (var i = 0; i < self.EventStatus().status.length; i++) {

        //        var eventStatusDate = new Date(self.EventStatus().status[i].date);
        //        eventStatusDate.setUTCHours(0, 0, 0, 0);

        //        if (kickoffDate.getTime() == eventStatusDate.getTime()) {

        //            if (self.EventStatus().status[i].bonus_added) {
        //                return player.GWPlayer.stats.gw_points;
        //            }
        //            else if (!self.EventStatus().status[i].bonus_added) {
        //                if (player.is_captain) {
        //                    return player.GWPlayer.stats.gw_points + (player.GWPlayer.stats.EstimatedBonus * 2) + "*";
        //                }
        //                else {
        //                    return player.GWPlayer.stats.gw_points + player.GWPlayer.stats.EstimatedBonus + "*";
        //                }
        //            }
        //        }
        //    }
        //} else {
        //    return player.GWPlayer.stats.gw_points;
        //}

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

        //var test = self.EventStatus().status.filter(x => x.date == "2021-01-02")[0].date;

        if (self.EventStatus().status[0].event == self.GameweekId() && self.EventStatus().leagues != "Updated") {
            for (var i = 0; i < self.EventStatus().status.length; i++) {

                var eventStatusDate = new Date(self.EventStatus().status[i].date);
                eventStatusDate.setUTCHours(0, 0, 0, 0);

                if (today.getTime() == eventStatusDate.getTime()) {

                    if (self.EventStatus().status[i].bonus_added) {
                        return self.GWPoints();
                    }
                    else if (!self.EventStatus().status[i].bonus_added) {
                        return points + "*";
                    }
                }
                //else {
                //    return self.GWPoints();
                //}
            }
            return self.GWPoints();
        }
        else {
            return self.GWPoints();
        }

        //return self.GWPoints();


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

    // Accepts a Date object or date string that is recognized by the Date.parse() method
    function getDayOfWeek(date) {
        const dayOfWeek = new Date(date).getDay();
        return isNaN(dayOfWeek) ? null :
            ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'][dayOfWeek];
    }

    //self.OnBench = function (entry) {
    //    //return entry.type === 'file' ? 'icon-file' : 'icon-filder';
    //    return;
    //};

    self.init = function () {

        if (self.IsLive()) {
            //auto refresh after a minute of inactivity
            var time = new Date().getTime();
            $(document.body).bind("mousemove keypress", function (e) {
                time = new Date().getTime();
            });

            function refresh() {
                if (new Date().getTime() - time >= 60000)
                    window.location.reload(true);
                else
                    setTimeout(refresh, 10000);
            }

            setTimeout(refresh, 10000);
        }

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