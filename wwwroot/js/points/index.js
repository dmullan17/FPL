GameweekPointsViewModel = function (data) {
    "use strict";

    var self = this;

    self.GWTeam = ko.observable(data.GWTeam);
    self.Team = ko.observable(data.Team);
    self.TotalPoints = ko.observable(data.TotalPoints);
    self.GWPoints = ko.observable(data.GWPoints);
    self.GameweekId = ko.observable(data.GameweekId);
    self.CurrentGameweekId = ko.observable(data.CurrentGameweekId);
    self.SelectedGameweek = ko.observable()
    self.EventStatus = ko.observable(data.EventStatus);
    self.EntryHistory = ko.observable(data.EntryHistory);
    self.CompleteEntryHistory = ko.observable(data.EntryHistory);
    self.IsLive = ko.observable(data.IsLive);
    self.GameWeek = ko.observable(data.GameWeek);
    self.SelectedPlayer = ko.observable();
    self.SelectedPlayerStatus = ko.observable();

    self.AllStartedGameWeeks = ko.observableArray(data.AllStartedGameWeeks);

    self.SelectedGameweek.subscribe(function (selectedGameweek) {

        if (selectedGameweek.id != self.GameweekId()) {
            const urlParams = new URLSearchParams(window.location.search);
            const gameweekIdParam = urlParams.get('gameweekId');
            const entryParam = urlParams.get('entry');

            if (gameweekIdParam != null) {
                var url = window.location.href;
                var selectedGameweekUrl = url.replace('gameweekId=' + gameweekIdParam, 'gameweekId=' + selectedGameweek.id);
                window.location.href = selectedGameweekUrl;
            }
            else if (entryParam != null) {
                window.location.href += '&gameweekId=' + selectedGameweek.id;
            }
            else {
                window.location.href += '?gameweekId=' + selectedGameweek.id;
            }
        }

    });

    self.viewPlayer = function (player) {
        self.SelectedPlayer(player);
        $('#player-popup').modal().modal('show');
    };

    self.viewPlayerGwBreakdown = function (player) {

        var games = player.GWPlayer.explain;

        if (games.length > 0) {
            for (var i = 0; i < games.length; i++) {

                var fixtureId = games[i].fixture;

                var gwGame = player.GWGames.filter(x => x.id == fixtureId);
                var bonus = player.GWPlayer.stats.EstimatedBonus[i];

                if (!gwGame[0].finished && bonus > 0) {
                    games[i].stats.push({ identifier: "bonus", value: bonus, points: bonus })
                }

                if (player.is_captain) {
                    for (var j = 0; j < games[i].stats.length; j++) {
                        games[i].stats[j].points = games[i].stats[j].points * player.multiplier;
                    }
                }
            }

            self.SelectedPlayer(player);
            $('#player-gw-breakdown-popup').modal().modal('show');
        }

    };

    self.GetFixtureScore = function (gwGames, fixtureId) {

        var gwGame = gwGames.filter(x => x.id == fixtureId);
        var game = gwGame[0];
        var time = getDayOfWeek(game.kickoff_time) + " @ " + new Date(game.kickoff_time).toTimeString().split(' ')[0].slice(0, -3)

        var html;

        if (game.started && !game.finished_provisional) {
            html = game.HomeTeam.short_name + " " + game.team_h_score + " - " + game.team_a_score + " " + game.AwayTeam.short_name + "<div class=\"ui green basic label\">Live</div>";
        }
        else if (!game.started) {
            html = game.HomeTeam.short_name + " vs " + game.AwayTeam.short_name + "<div class=\"ui basic label\">" + time + "</div>";
        }
        else if (game.finished_provisional) {
            html = game.HomeTeam.short_name + " " + game.team_h_score + " - " + game.team_a_score + " " + game.AwayTeam.short_name + "<i class=\"green check circle icon\" style=\"font-size: 1.6rem !important;\"></i>";
        }

        return html;
    }

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

        if (results.length > 0) {
            for (var i = 0; i < results.length; i++) {
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
            }
        }
        if (fixtures.length > 0) {
            for (var i = 0; i < fixtures.length; i++) {
                if (team.id == fixtures[i].team_h) {
                    html += fixtures[i].team_a_name + " (H)<br/>";
                }
                else if (team.id == fixtures[i].team_a) {
                    html += fixtures[i].team_h_name + " (A)<br/>";
                }
            }
        }
        if (results.length == 0 && fixtures.length == 0) {
            html += "No Game";
        }

        return html;
    };

    self.GetOppositionShortName = function (player) {

        var html = "";
        var gwGames = player.GWGames;
        var team = player.player.Team;

        if (gwGames.length > 0) {
            for (var i = 0; i < gwGames.length; i++) {
                if (team.id == gwGames[i].HomeTeam.id) {
                    html += gwGames[i].AwayTeam.short_name + " (H)<br/>";
                }
                else if (team.id == gwGames[i].AwayTeam.id) {
                    html += gwGames[i].HomeTeam.short_name + " (A) <br/>";
                }
            }
        }
        else {
            html += "No Game";
        }

        return html;
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
                    html += "<span style=\"font-weight: bold\">" + results[i].team_h_score + " - " + results[i].team_a_score + "</span> <span class=\"blinker\">'</span> <br/>";
                }

            }
        }
        if (fixtures.length > 0) {
            for (var i = 0; i < fixtures.length; i++) {
                html += getDayOfWeek(fixtures[i].kickoff_time) + " @ " + new Date(fixtures[i].kickoff_time).toTimeString().split(' ')[0].slice(0, -3) + " <br/>";
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

    self.GetTotalPercentileMovement = function (entryHistory) {

        if (entryHistory.overall_rank > entryHistory.LastEventOverallRank) {
            return "red arrow alternate circle down icon";
        }
        else if (entryHistory.overall_rank < entryHistory.LastEventOverallRank) {
            return "green arrow alternate circle up icon";
        }
        else if (entryHistory.overall_rank == entryHistory.LastEventOverallRank) {
            return "grey circle icon";
        }
        return;

    }

    self.GetGwPercentilePopupHeight = function (gameweekId) {
        if (gameweekId != self.CurrentGameweekId()) {
            return "40px";
        }
    }

    self.GetCaptain = function (gwTeam) {

        //var captain = gwTeam.picks.filter(x => x.is_captain);

        //return captain[0].player.web_name;

        var captain = gwTeam.picks.filter(x => x.is_captain);

        var vice = gwTeam.picks.filter(x => x.is_vice_captain);

        if (captain.length != 0) {
            return captain[0].player.web_name;
        }
        else if (vice.length != 0) {
            return vice[0].player.web_name;
        }
        else {
            return "No Captain";
        }
    }

    self.GetCaptainPointsTotal = function (gwTeam) {

        var captain = gwTeam.picks.filter(x => x.is_captain);

        var vice = gwTeam.picks.filter(x => x.is_vice_captain);

        if (captain.length != 0) {
            return captain[0].GWPlayer.stats.gw_points;
        }
        else if (vice.length != 0) {
            return vice[0].GWPlayer.stats.gw_points;
        }
        else {
            return 0;
        }

    }

    self.GetBenchPointsTotal = function (gwTeam) {

        var benchPicks = gwTeam.picks.filter(x => x.position > 11);
        var total = 0;

        for (var i = 0; i < benchPicks.length; i++) {
            total += benchPicks[i].GWPlayer.stats.gw_points;
        }

        return total;
    }


    self.GetBonus = function (player) {

        //if (player.GWGames.filter(x => !x.finished_provisional && x.started).length > 0) {

        //}
        if (player.GWGames.filter(x => !x.finished && x.started).length > 0) {

            return player.GWPlayer.stats.EstimatedBonus.reduce((a, b) => a + b, 0);

            //for (var i = 0; i < player.GWGames.length; i++) {
            //    var kickoffDate = new Date(player.GWGames[i].kickoff_time);
            //    kickoffDate.setUTCHours(0, 0, 0, 0);

            //    if (self.EventStatus().status[0].event == self.GameweekId() && player.GWGames[i].started) {
            //        for (var j = 0; j < self.EventStatus().status.length; j++) {

            //            var eventStatusDate = new Date(self.EventStatus().status[i].date);
            //            eventStatusDate.setUTCHours(0, 0, 0, 0);

            //            if (kickoffDate.getTime() == eventStatusDate.getTime()) {
            //                if (self.EventStatus().status[j].bonus_added) {
            //                    return player.GWPlayer.stats.bonus;
            //                }
            //                else if (!self.EventStatus().status[j].bonus_added) {
            //                    return player.GWPlayer.stats.EstimatedBonus + "*";
            //                }
            //            }
            //        }
            //    }
            //}
        }
        else {
            return player.GWPlayer.stats.bonus;
        }
    };

    self.GetBonusRank = function (player) {

        for (var i = 0; i < player.GWGames.length; i++) {

            var kickoffDate = new Date(player.GWGames[i].kickoff_time);
            kickoffDate.setUTCHours(0, 0, 0, 0);

            for (var j = 0; j < self.EventStatus().status.length; j++) {

                var eventStatusDate = new Date(self.EventStatus().status[j].date);
                eventStatusDate.setUTCHours(0, 0, 0, 0);

                if (kickoffDate.getTime() == eventStatusDate.getTime() && !player.GWGames[i].finished && player.GWGames[i].started) {
                    if (self.EventStatus().status[j].bonus_added) {
                        return player.GWPlayer.stats.BpsRank;
                    }
                    else if (!self.EventStatus().status[j].bonus_added) {
                        return player.GWPlayer.stats.BpsRank + "*";
                    }
                }
            }

            //if (self.EventStatus().status[0].event == self.GameweekId() && player.GWGames[i].started) {

            //} else {
            //    return player.GWPlayer.stats.BpsRank;
            //}
        }

        return player.GWPlayer.stats.BpsRank;

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

        for (var i = 0; i < player.GWGames.length; i++) {
            var kickoffDate = new Date(player.GWGames[i].kickoff_time);
            kickoffDate.setUTCHours(0, 0, 0, 0);

            if (self.EventStatus().status[0].event == self.GameweekId() && player.GWGames[i].started) {
                for (var i = 0; i < self.EventStatus().status.length; i++) {

                    var eventStatusDate = new Date(self.EventStatus().status[i].date);
                    eventStatusDate.setUTCHours(0, 0, 0, 0);

                    if (kickoffDate.getTime() == eventStatusDate.getTime()) {

                        if (self.EventStatus().status[i].bonus_added) {
                            return player.GWPlayer.stats.gw_points;
                        }
                        else if (!self.EventStatus().status[i].bonus_added) {
                            //if (player.is_captain) {
                            //    return player.GWPlayer.stats.gw_points + (player.GWPlayer.stats.EstimatedBonus * 2) + "*";
                            //}
                            //else {
                            //    return player.GWPlayer.stats.gw_points + player.GWPlayer.stats.EstimatedBonus + "*";
                            //}
                            return player.GWPlayer.stats.gw_points + "*";
                        }
                    }
                }
            } else {
                return player.GWPlayer.stats.gw_points;
            }
        }
        return player.GWPlayer.stats.gw_points;

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

    self.GetGWRank = function (completeEntryHistory) {

        var rank = completeEntryHistory.filter(x => x.event == self.GameweekId()).rank;

        return rank;
    }

    self.GetLastTimeGwAvgWasUpdated = function (eventStatus) {

        var finishedEvents = eventStatus.status.filter(x => x.points == "r");
        var lastFinishedEvent = finishedEvents[finishedEvents.length - 1];
        var day = getDayOfWeek(lastFinishedEvent.date);

        return "Last updated on " + day + " " + lastFinishedEvent.date;
    }

    self.FormatValue = function (value) {
        var value = parseFloat(value) / 10;
        return value.toFixed(1);
    };

    self.GetFlagClass = function (country) {
        return country.toLowerCase() + " flag";
    }


    self.FormatLargeNumber = function (number) {
        return nFormatter(number, 2)
        //return player.transfers_in_event - player.transfers_out_event;
    }

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

        $('.menu .item').tab({});

        $('.gw-percentile-statistic').popup({
            popup: '#gw-percentile-popup.popup',
            position: 'top center',
            hoverable: true
        });

        $('#total-percentile-statistic').popup({
            popup: '#total-percentile-popup.popup',
            position: 'top center',
            hoverable: true
        });

        $('#captain-points-statistic').popup({
            popup: '#captain-points-popup.popup',
            position: 'top center',
            hoverable: true
        });

        $('.pytp-statistic').popup({
            popup: '#pytp-popup.popup',
            position: 'top center',
            hoverable: true
        });

        if (self.GameweekId() == self.CurrentGameweekId()) {
            $('.gw-average-statistic').popup({
                popup: '#gw-average-popup.popup',
                position: 'top center',
                hoverable: true
            });
        }

        var index = self.AllStartedGameWeeks().findIndex(x => x.id === self.GameweekId());
        self.SelectedGameweek(self.AllStartedGameWeeks()[index]);

        $("#flag-icon").attr('title', self.Team().player_region_name)


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

    function nFormatter(num, digits) {
        var si = [
            { value: 1, symbol: "" },
            { value: 1E3, symbol: "k" },
            { value: 1E6, symbol: "M" },
            { value: 1E9, symbol: "G" },
            { value: 1E12, symbol: "T" },
            { value: 1E15, symbol: "P" },
            { value: 1E18, symbol: "E" }
        ];
        var rx = /\.0+$|(\.[0-9]*[1-9])0+$/;
        var i;
        for (i = si.length - 1; i > 0; i--) {
            if (Math.abs(num) >= si[i].value) {
                break;
            }
        }
        return (num / si[i].value).toFixed(digits).replace(rx, "$1") + si[i].symbol;
    }
};