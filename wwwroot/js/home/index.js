HomeViewModel = function (data) {
    "use strict";

    var self = this,
        playersTable = $('#gw-player-table');

    self.GWGames = ko.observableArray(data.GWGames);
    self.AreGamesLive = ko.observable(data.AreGamesLive);
    self.AllGames = ko.observableArray(data.AllGames);
    self.Players = ko.observableArray(data.Players);
    self.CurrentGameweek = ko.observable(data.CurrentGameweek);
    self.GameweekId = ko.observable(data.CurrentGameweek.id);
    self.SelectedPlayer = ko.observable();

    self.GamesInFixtureList = ko.observableArray(self.AllGames().filter(x => x.Event == self.GameweekId()));
    self.CurrentGameweekForFixtureList = ko.observable(data.CurrentGameweek.id);

    self.DoesPlayerHaveStatus = function (playerStatus) {
        if (playerStatus == "i" || playerStatus == "d" || playerStatus == "n" || playerStatus == "s" || playerStatus == "u") {
            return true;
        }
        else {
            return false;
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

    self.GetFixtureDate = function (game, kickoffTime) {
        var html = "";
        html += getDayOfWeek(game.kickoff_time) + " @ " + new Date(game.kickoff_time).toTimeString().split(' ')[0].slice(0, -3) + " <br/>";

        return html;

    }

    self.ShowPreviousFixtures = function () {
        self.CurrentGameweekForFixtureList(self.CurrentGameweekForFixtureList() - 1);
        self.GamesInFixtureList(self.AllGames().filter(x => x.Event == self.CurrentGameweekForFixtureList()));       
    }

    self.ShowNextFixtures = function () {
        self.CurrentGameweekForFixtureList(self.CurrentGameweekForFixtureList() + 1);
        self.GamesInFixtureList(self.AllGames().filter(x => x.Event == self.CurrentGameweekForFixtureList()));
    }

    self.GetOpposition = function (player) {

        var team = player.player.Team;

        //var fixtures = team.Fixtures.filter(x => x.Event == self.GameweekId());
        //var results = team.Results.filter(x => x.Event == self.GameweekId());


        var gwGames = self.GWGames().filter(x => x.Event == self.CurrentGameweek().id && (x.team_h == player.team.id || x.team_a == player.team.id));
        var fixtures = gwGames.filter(x => x.finished_provisional == false);
        var results = gwGames.filter(x => x.finished_provisional == true);


        var html = "";

        if (results.length > 0) {
            for (var i = 0; i < results.length; i++) {
                if (results[i].team_h_score > results[i].team_a_score) {
                    if (player.team.id == results[i].team_h) {
                        html += results[i].AwayTeam.short_name + " (H)<br/>";
                    }
                    else if (player.team.id == results[i].team_a) {
                        html += results[i].HomeTeam.short_name + " (A) <br/>";
                    }
                }
                else if (results[i].team_h_score == results[i].team_a_score) {
                    if (player.team.id == results[i].team_h) {
                        html += results[i].AwayTeam.short_name+ " (H) <br/>";
                    }
                    else if (player.team.id == results[i].team_a) {
                        html += results[i].HomeTeam.short_name + " (A) <br/>";
                    }
                }
                else if (results[i].team_h_score < results[i].team_a_score) {
                    if (player.team.id == results[i].team_h) {
                        html += results[i].AwayTeam.short_name + " (H) <br/>";
                    }
                    else if (player.team.id == results[i].team_a) {
                        html += results[i].HomeTeam.short_name + " (A) <br/>";
                    }
                }
            }
        }
        if (fixtures.length > 0) {
            for (var i = 0; i < fixtures.length; i++) {
                if (player.team.id == fixtures[i].team_h) {
                    html += fixtures[i].AwayTeam.short_name + " (H)<br/>";
                }
                else if (player.team.id == fixtures[i].team_a) {
                    html += fixtures[i].HomeTeam.short_name + " (A)<br/>";
                }
            }
        }
        if (results.length == 0 && fixtures.length == 0) {
            html += "No Game";
        }

        return html;
    };

    self.FormatValue = function (value) {
        var value = parseFloat(value) / 10;
        return value.toFixed(1);
    };

    self.GetGWNetTransfersRaw = function (player) {
        return player.player.transfers_in_event - player.player.transfers_out_event;
    }

    self.CreateGWNetTransfers = function (player) {
        return nFormatter((player.player.transfers_in_event - player.player.transfers_out_event), 0)
        //return player.transfers_in_event - player.transfers_out_event;
    }

    self.viewPlayerGwBreakdown = function (player) {

        //player = player.Pick;
        var gamesWithFplStats = player.explain;
        var gwGames = self.GWGames().filter(x => x.Event == self.CurrentGameweek().id && (x.team_h == player.team.id || x.team_a == player.team.id));

        player.GWGames = gwGames;

        if (gamesWithFplStats.length > 0) {
            for (var i = 0; i < gamesWithFplStats.length; i++) {

                var fixtureId = gamesWithFplStats[i].fixture;

                for (var j = 0; j < gwGames.length; j++) {
                    if (fixtureId == gwGames[j].id) {
                        //add timestamp to game
                        gamesWithFplStats[i].date = new Date(gwGames[j].kickoff_time).getTime();
                    }
                }

            }

            //order by timestamp, this ensures the games in the modal appear in chronological order
            gamesWithFplStats.sort(function (a, b) { return a.date - b.date });

            for (i = 0; i < gamesWithFplStats.length; i++) {

                fixtureId = gamesWithFplStats[i].fixture;

                var gwGame = gwGames.filter(x => x.id == fixtureId);
                var bonus = player.stats.EstimatedBonus[i];

                //add estimated bonus to game stats
                if (!gwGame[0].finished && gwGame[0].started && bonus > 0 && !gamesWithFplStats[i].stats.some(x => x.identifier == "bonus")) {
                    gamesWithFplStats[i].stats.push({ identifier: "bonus", value: bonus, points: bonus })
                }

            }

            self.SelectedPlayer(player);
            $('#player-gw-breakdown-popup').modal().modal('show');
        }

    };

    //using this ensures a players additional points for being a captain doesnt get reflected in the breakdown modal
    self.GetPlayerGwPoints = function (player) {

        //ensuring the gw points shown in the player tally table includes the estimated bonus
        var points = player.stats.total_points;

        for (var i = 0; i < player.GWGames.length; i++) {
            for (var j = 0; j < player.explain.length; j++) {
                if (player.GWGames[i].id == player.explain[j].fixture) {
                    if (!player.GWGames[i].finished) {
                        if (player.stats.EstimatedBonus[i] != null) {
                            points += player.stats.EstimatedBonus[j];
                        }
                    }
                }
            }
        }

        return points;

    }

    self.GetFixtureScore = function (gwGames, fixtureId) {

        var gwGame = gwGames.filter(x => x.id == fixtureId);
        var game = gwGame[0];
        var time = getDayOfWeek(game.kickoff_time) + " @ " + new Date(game.kickoff_time).toTimeString().split(' ')[0].slice(0, -3)

        var html;

        if (game.started && !game.finished_provisional) {
            html = game.HomeTeam.short_name + " " + game.team_h_score + " - " + game.team_a_score + " " + game.AwayTeam.short_name + "<div class=\"ui green label\">Live</div>";
        }
        else if (!game.started) {
            html = game.HomeTeam.short_name + " vs " + game.AwayTeam.short_name + "<div class=\"ui basic label\">" + time + "</div>";
        }
        else if (game.finished_provisional) {
            html = game.HomeTeam.short_name + " " + game.team_h_score + " - " + game.team_a_score + " " + game.AwayTeam.short_name + "<i class=\"green check circle icon\" style=\"font-size: 1rem !important; margin-top: -0.7rem; margin-left: 0.3rem;\"></i>";
        }

        return html;
    }

    self.IsGameStarted = function (gwGames, fixtureId) {
        var gwGame = gwGames.filter(x => x.id == fixtureId)[0];

        if (gwGame.started) {
            return true;
        }
        return false;
    }


    self.init = function () {
        inititalisePlayersDatatable();

        if (self.AreGamesLive()) {
            //auto refresh after a minute of inactivity
            var time = new Date().getTime();
            $(document.body).on("mousemove keypress touchmove", function (e) {
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
    };

    self.init();

    jQuery.extend(jQuery.fn.dataTableExt.oSort, {
        "sort-numbers-with-letters-asc": function (a, b) {
            var el1 = $('<div></div>');
            el1.html(a);

            var el2 = $('<div></div>');
            el2.html(b);

            var aHtml = $('span', el1);
            var bHtml = $('span', el2);

            var first = parseInt(aHtml[0].innerHTML, 10);
            var second = parseInt(bHtml[0].innerHTML, 10);

            return ((first > second) ? 0 : ((first < second) ? -1 : 1));   
        },
        "sort-numbers-with-letters-desc": function (a, b) {
            var el1 = $('<div></div>');
            el1.html(a);

            var el2 = $('<div></div>');
            el2.html(b);

            var aHtml = $('span', el1);
            var bHtml = $('span', el2);

            var first = parseInt(aHtml[0].innerHTML, 10);
            var second = parseInt(bHtml[0].innerHTML, 10);

            return ((first < second) ? 0 : ((first > second) ? -1 : 1));   
        }
    });



    function inititalisePlayersDatatable() {

        // Size of browser viewport.
        var browserH = $(window).height();
        var browserW = $(window).width();

        if (browserW > 1000) {
            $(document).ready(function () {
                var table = playersTable.DataTable({
                    //language: {
                    //    lengthMenu: "Show _MENU_ GW" + self.GameweekId() + " Players",
                    //    info: "Showing _START_ to _END_ of _TOTAL_ GW" + self.GameweekId() + " Players",
                    //    infoEmpty: "Showing 0 to 0 of 0 GW" + self.GameweekId() + " Players",
                    //    infoFiltered: "(filtered from _MAX_ total GW" + self.GameweekId() + " Players)",
                    //},
                    //dom: 'l<"toolbar">frtip',
                    columnDefs: [
                        {
                            type: "sort-numbers-with-letters", targets: "special-sort-1"
                        },
                        {
                            orderable: false, targets: "no-sort"
                        }
                    ],
                    order: [[$('th.default-sort').index(), "desc"]],
                    responsive: true
                });
            });
        }
        else {
            $(document).ready(function () {
                var table = playersTable.DataTable({
                    columnDefs: [
                        {
                            type: "sort-numbers-with-letters", targets: "special-sort-1"
                        },
                        {
                            orderable: false, targets: "no-sort"
                        }
                    ],
                    order: [[$('th.default-sort').index(), "desc"]],
                    responsive: true,
                    scrollX: true,
                    scrollY: true
                });

            });
        }

        //$("div.toolbar").html('Custom tool bar! Text/images etc.');
    }

    //$(window).resize(function () {
    //    //resize just happened, pixels changed

    //    if ($.fn.dataTable.isDataTable(playersTable)) {
    //        playersTable.DataTable().clear().destroy();
    //    }

    //    inititalisePlayersDatatable();
    //});

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

    // Accepts a Date object or date string that is recognized by the Date.parse() method
    function getDayOfWeek(date) {
        const dayOfWeek = new Date(date).getDay();
        return isNaN(dayOfWeek) ? null :
            ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'][dayOfWeek];
    }
};