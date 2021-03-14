﻿LeaguesViewModel = function (data) {
    "use strict";

    var self = this,
        standingsSegment = $('#standings-segment'),
        standingsLoader = $('#standings-loader'),
        standingsTable = $('#standings-table'),
        standingsTableBody = $('#standings-table tbody'),
        standingsTableFooter = $('#standings-table tfoot'),
        playerTallyTable = $('#player-tally-table');

    self.ClassicLeagues = ko.observableArray(data.ClassicLeagues);
    self.H2HLeagues = ko.observableArray(data.H2HLeagues);
    self.SelectedLeague = ko.observable(data.SelectedLeague);
    self.Cup = ko.observable(data.Cup);
    self.IsEventLive = ko.observable(data.IsEventLive);
    self.IsGameLive = ko.observable(data.IsGameLive);
    self.CurrentGwId = ko.observable(data.CurrentGwId);
    self.TeamId = ko.observable(data.TeamId);
    self.UserTeam = ko.observable(data.SelectedLeague.UserTeam);
    self.EventStatus = ko.observable(data.EventStatus);
    self.LastUpdatedTime = ko.observable(data.LastUpdated);

    self.SelectedLeagueStandings = ko.observableArray(self.SelectedLeague().Standings.results);
    self.SelectedLeaguePlayersTally = ko.observableArray(self.SelectedLeague().PlayersTally);

    self.LastUpdated = function () {
        var lastUpdatedTime = new Date(self.LastUpdatedTime()).toLocaleString("en-GB");
        var formattedTime = lastUpdatedTime.slice(0, -3);
        return "Last updated: " + formattedTime;
    }

    self.viewPlayer = function (player) {
        self.SelectedPlayer(player);
        $('.ui.modal').modal().modal('show');
    };

    self.getLeagueClass = function (league) {
        if (league == self.SelectedLeague()) {
            return "active"
        }
        else {
            return null;
        }
    }

    self.GetGwTransfers = function (gwTeam) {

        if (gwTeam.ActiveChips.includes("wildcard")) {
            return gwTeam.GWTransfers.length + ' (0)';
        }
        else {
            return gwTeam.EntryHistory.event_transfers + ' (' + gwTeam.EntryHistory.event_transfers_cost + ')';
        }

    }

    self.GetPlayerTallyGwPoints = function (tally) {

        //ensuring the gw points shown in the player tally table includes the estimated bonus
        var points = tally.Pick.GWPlayer.stats.total_points;

        for (var i = 0; i < tally.Pick.GWGames.length; i++) {
            for (var j = 0; j < tally.Pick.GWPlayer.explain.length; j++) {
                if (tally.Pick.GWGames[i].id == tally.Pick.GWPlayer.explain[j].fixture) {
                    if (!tally.Pick.GWGames[i].finished) {
                        if (tally.Pick.GWPlayer.stats.EstimatedBonus.length > j) {
                            points += tally.Pick.GWPlayer.stats.EstimatedBonus[j];
                        }
                    }
                }
            }
        }

        return points;

    }

    self.changeLeague = function (league) {
        //self.SelectedLeague({});
        if (league != self.SelectedLeague()) {
            if ($.fn.dataTable.isDataTable(standingsTable)) {
                standingsTable.DataTable().clear().destroy();
            }
            initialiseStandingsDatatable();
            self.SelectedLeague(league);
        }

    };

    self.SelectedLeague.subscribe(function (league) {

        if ($.fn.dataTable.isDataTable(standingsTable)) {
            standingsTable.DataTable().clear().destroy();
            playerTallyTable.DataTable().clear().destroy();

            $.ajax({
                url: "/Leagues/GetPlayerStandingsForClassicLeague",
                type: "GET",
                cache: true,
                data: {
                    leagueId: league.id,
                    gameweekId: self.CurrentGwId()
                },
                beforeSend: function () {
                    standingsLoader.addClass("active");
                },
                success: function (json, status, xhr) {
                    if (xhr.readyState === 4 && xhr.status === 200) {
                        self.SelectedLeagueStandings(json.Standings.results);
                        self.SelectedLeaguePlayersTally(json.PlayersTally);
                        self.UserTeam(json.UserTeam);
                        initialiseStandingsDatatable();
                        initialiseTalliesDatatable();
                        return;
                    }
                },
                complete: function (xhr, status) {
                    standingsLoader.removeClass("active");
                    if (xhr.readyState === 4 && xhr.status !== 200) {
                    }
                }
            });
        }
    });

    self.GetRowClass = function (teamId) {
        if (self.TeamId() == teamId && self.SelectedLeague().Standings.results.length <= 10) {
            return "active";
        }
        return;

    }

    self.GetPointsMovement = function (manager) {
        if (manager.rank > manager.last_rank) {
            return "red arrow alternate circle down small icon";
        }
        else if (manager.rank < manager.last_rank) {
            return "green arrow alternate circle up small icon";
        }
        else if (manager.rank == manager.last_rank) {
            return "grey circle small icon";
        }
        return;

    }

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

    self.GetTeamBonusPoints = function (picks) {

        var starters = picks.filter(x => x.multiplier > 0);
        var bonusPoints = 0;

        for (var i = 0; i < starters.length; i++) {
            for (var j = 0; j < starters[i].GWPlayer.stats.EstimatedBonus.length; j++) {
                bonusPoints += starters[i].GWPlayer.stats.EstimatedBonus[j];
            }         
        }

        return bonusPoints;

    }

    self.GetGWBenchPoints = function (picks) {

        var subs = picks.filter(x => x.position > 11);
        var subsPoints = 0;

        for (var i = 0; i < subs.length; i++) {
            subsPoints += subs[i].GWPlayer.stats.gw_points;
        }

        return subsPoints;
    }

    self.GetRank = function (manager) {
        //return live rank if event is live, otherwise return normal rank
        //if (self.IsEventLive()) {
        //    return manager.LiveRank;
        //}
        //else {
        //    return manager.rank;
        //}

        return manager.rank;
    }

    self.GetPlayersYetToPlay = function (manager) {
        //return live rank if event is live, otherwise return normal rank
        if (self.IsEventLive()) {
            return manager.LiveRank;
        }
        else {
            return manager.rank;
        }
    }

    self.FormatNames = function (teamName, managerName) {
        var html = "";
        html += teamName + "</br> <span class=\"manager-name\">" + managerName + "</span>"
        return html;
    }

    self.GetCaptain = function (picks) {
        var captain = picks.filter(x => x.is_captain);
        return captain[0].player.web_name;
    }

    self.GetActiveChips = function (activeChips) {
        var html = "";
        if (activeChips.length == 0) {
            html += "None"
        }
        else {
            for (var i = 0; i < activeChips.length; i++) {
                //html += activeChips[i] + "</br>";
                if (activeChips[i] == "wildcard") {
                    html += "WC</br>";
                    continue;
                }
                else if (activeChips[i] == "freehit") {
                    html += "FH</br>";
                    continue;
                }
                else if (activeChips[i] == "bboost") {
                    html += "BB</br>";
                    continue;
                }
                else if (activeChips[i] == "3xc") {
                    html += "TC</br>";
                    continue;
                }
            }
        }
        return html;
    }

    self.GetChipsUsed = function (chips) {
        var html = "";
        if (chips.length == 0) {
            html += "None"
        }
        else {
            for (var i = 0; i < chips.length; i++) {
                if (chips[i].name == "wildcard") {
                    html += "WC (GW" + chips[i].event + ")</br>";
                    continue;
                }
                else if (chips[i].name == "freehit") {
                    html += "FH (GW" + chips[i].event + ")</br>";
                    continue;
                }
                else if (chips[i].name == "bboost") {
                    html += "BB (GW" + chips[i].event + ")</br>";
                    continue;
                }
                else if (chips[i].name == "3xc") {
                    html += "TC (GW" + chips[i].event + ")</br>";
                    continue;
                }
            }
        }
        return html;
    }

    self.FormatLast5GwPoints = function (last5) {
        var html = "";
        for (var i = 0; i < last5.length; i++) {
            if (i == last5.length - 1) {
                html += last5[i];
            }
            else {
                html += last5[i] + " | ";
            }
        }

        return html;
    }

   
    self.init = function () {
        $('.menu .item').tab({
            //'onLoad': function (tabPath) {
            //    if (tabPath == "second") {
            //        if (!$.fn.dataTable.isDataTable(playerTallyTable)) {
            //            //playerTallyTable.DataTable().clear().destroy();
            //            initialiseTalliesDatatable();
            //        }
            //    }
            //}
        });

        ////self.SelectedLeague(self.ClassicLeagues()[4]);
        for (var i = 0; i < self.ClassicLeagues().length; i++) {
            if (self.ClassicLeagues()[i] = self.SelectedLeague()) {
                self.SelectedLeague(self.ClassicLeagues()[i]);
                break;
            }
        }

        initialiseStandingsDatatable();
        initialiseTalliesDatatable();
    };

    self.init();

    /* Formatting function for row details - modify as you need */
    function FormatChildRow(gwTeam) {

        var starters = gwTeam.picks.filter(x => x.position <= 11);
        var subs = gwTeam.picks.filter(x => x.position > 11);
        var autoSubs = gwTeam.automatic_subs;
        var gwTransfers = gwTeam.GWTransfers;

        var startersHtml = CreateHtmlForPicks(starters, autoSubs);
        var subHtml = CreateHtmlForPicks(subs, autoSubs);

        var transfersHtml = "";

        if (gwTransfers.length > 0) {
            transfersHtml += '<h4 class="ui header" style="margin-bottom: 0.5rem">GW Transfers (' + gwTransfers.length + ')</h4>' +
                '<div class="ui list" style="margin-top: 0rem">' 

            for (var i = 0; i < gwTransfers.length; i++) {
                if (gwTransfers[i].PlayerIn != null && gwTransfers[i].PlayerOut != null) {
                    transfersHtml +=
                        '<div class="item">' +
                        '<span>' + gwTransfers[i].PlayerIn.web_name + ' for ' + gwTransfers[i].PlayerOut.web_name + '</span>' +
                        '</div>';
                }
            }

            transfersHtml += '</div>'
        }

        var autoSubsHtml = "";

        if (autoSubs.length > 0) {

            autoSubsHtml += '<h4 class="ui header" style="margin-bottom: 0.5rem">Auto Subs (' + autoSubs.length + ')</h4>' +
                '<div class="ui list" style="margin-top: 0rem">'

            for (var i = 0; i < autoSubs.length; i++) {

                var playerIn = gwTeam.picks.filter(x => x.element == autoSubs[i].element_in)[0].player.web_name;
                var playerOut = gwTeam.picks.filter(x => x.element == autoSubs[i].element_out)[0].player.web_name;

                autoSubsHtml +=
                    '<div class="item">' +
                    '<span>' + playerIn + ' for ' + playerOut + '</span>' +
                    '</div>';             
            }

            autoSubsHtml += '</div>'

        }

        return '<div class="ui grid">' +
            '<div class="thirteen wide column">' +
            '<div class="ui horizontal list">' + startersHtml + '</div>' +
            '</br>' +
            '<div class="ui horizontal list">' + subHtml + '</div>' +
                '</div>' +
            '<div class="two wide column" style="padding-left: 0; width: 14% !important">' +
            transfersHtml +
            autoSubsHtml +
                '</div>' +
                '</div>';
    }

    function CreateHtmlForPicks(picks, autoSubs) {

        var html = "";

        for (var i = 0; i < picks.length; i++) {
            var icon = "";
            var subIcon = "";
            var subIcon2 = "";
            var captain = "";

            if (picks[i].player.element_type == 1) {
                icon += "user";
            }
            else {
                icon += "user outline";
            }

            if (picks[i].is_captain) {
                captain += "<i class='copyright icon' style='font-size: 0.9rem !important; display: inline !important; margin-left: 2px !important;'></i>";
            }

            for (var j = 0; j < autoSubs.length; j++) {
                if (autoSubs[j].element_in == picks[i].player.id || autoSubs[j].element_out == picks[i].player.id) {
                    subIcon += "top left corner sync";
                }
            }

            if (picks[i].player.status != "a") {
                //injured
                if (picks[i].player.status == "i") {
                    subIcon2 += "top right corner plus circle";
                }
                //doubtful
                else if (picks[i].player.status == "d") {
                    subIcon2 += "top right corner help circle";
                }
                //not available or suspended
                else if (picks[i].player.status == "n" || picks[i].player.status == "s") {
                    subIcon2 += "top right corner warning circle";
                }
                //unavailable - left the club
                else if (picks[i].player.status == "u") {
                    subIcon2 += "top right corner warning sign";
                }
            }

            icon += " icon";
            subIcon += " icon";
            subIcon2 += " icon";

            if (picks.length == 4) {
                icon += " disabled";
            }

            html +=
                '<div class="item">' +
                '<div class="content">' +
                '<h4 class="ui icon header">' +
                '<i class="small icons">' +
                '  <i class="' + icon + '"></i>' +
                '  <i class="' + subIcon + '"></i>' +
                '  <i class="' + subIcon2 + '"></i>' +
                '</i>' +
                '</br>' +
                picks[i].player.web_name + captain +
                '</h5>' +
                '<p style="text-align: center">' + picks[i].GWPlayer.stats.gw_points + '</p>' +
                '</div>' +
                '</div>'
        }  

        return html;

    }

    function initialiseStandingsDatatable() {
        $(document).ready(function () {
            var table = standingsTable.DataTable({
                columnDefs: [
                    { orderable: false, targets: "no-sort" }
                ],
                order: [[$('th.default-sort').index(), "asc"]],
                responsive: true,
                fixedHeader: true
            });

        });
    }

    function initialiseTalliesDatatable() {
        $(document).ready(function () {
            var table = playerTallyTable.DataTable({
                order: [[$('#player-tally-table th.default-sort').index(), "desc"]]

            });

        });
    }

    // Add event listener for opening and closing details
    standingsTableBody.on('click', 'td.details-control', function (data, event) {
        var tr = $(this).closest('tr');
        var td = $(this).closest('td');
        var row = standingsTable.DataTable().row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
            tr.removeClass('shown');
        }
        else {
            // Open this row
            var teamId = row.data()[0];

            $.ajax({
                url: "/Leagues/GetPlayersTeam",
                type: "GET",
                cache: false,
                data: {
                    teamId: teamId,
                    currentGameWeekId: self.CurrentGwId()
                },
                contentType: "application/json",
                beforeSend: function () {
                    tr.removeClass('shown');
                    td.addClass('loading');
                },
                success: function (json, status, xhr) {
                    if (xhr.readyState === 4 && xhr.status === 200) {
                        row.child(FormatChildRow(json)).show();
                        tr.addClass('shown');
                    }
                },
                complete: function (xhr, status) {
                    td.removeClass('loading');
                    if (xhr.readyState === 4 && xhr.status !== 200) {
                    }
                }
            });
        }
    });

    standingsTableFooter.on('click', 'td.details-control', function (data, event) {
        var tr = $(this).closest('tr');
        var td = $(this).closest('td');
        var row = standingsTable.DataTable().row(tr.data());

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
            tr.removeClass('shown');
        }
        else {
            // Open this row

            $.ajax({
                url: "/Leagues/GetPlayersTeam",
                type: "GET",
                cache: false,
                data: {
                    teamId: self.TeamId(),
                    currentGameWeekId: self.CurrentGwId()
                },
                contentType: "application/json",
                beforeSend: function () {
                    tr.removeClass('shown');
                    td.addClass('loading');
                },
                success: function (json, status, xhr) {
                    if (xhr.readyState === 4 && xhr.status === 200) {
                        row.child(FormatChildRow(json)).show();
                        tr.addClass('shown');
                    }
                },
                complete: function (xhr, status) {
                    td.removeClass('loading');
                    if (xhr.readyState === 4 && xhr.status !== 200) {
                    }
                }
            });
        }
    });


};