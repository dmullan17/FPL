LeaguesViewModel = function (data) {
    "use strict";

    var self = this,
        standingsSegment = $('#standings-segment'),
        standingsLoader = $('#standings-loader'),
        standingsTable = $('#standings-table'),
        standingsTableBody = $('#standings-table tbody');

    self.ClassicLeagues = ko.observableArray(data.ClassicLeagues);
    self.H2HLeagues = ko.observableArray(data.H2HLeagues);
    self.SelectedLeague = ko.observable(data.SelectedLeague);
    self.Cup = ko.observable(data.Cup);
    self.IsEventLive = ko.observable(data.IsEventLive);
    self.CurrentGwId = ko.observable(data.CurrentGwId);
    self.SelectedLeagueStandings = ko.observableArray(self.SelectedLeague().Standings.results);

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

    self.changeLeague = function (league) {
        //self.SelectedLeague({});
        if (league != self.SelectedLeague()) {
            if ($.fn.dataTable.isDataTable(standingsTable)) {
                standingsTable.DataTable().clear().destroy();
            }
            initialiseDatatable();
            self.SelectedLeague(league);
        }

    };

    self.SelectedLeague.subscribe(function (league) {

        if ($.fn.dataTable.isDataTable(standingsTable)) {
            standingsTable.DataTable().clear().destroy();

            $.ajax({
                url: "/Leagues/GetPlayerStandingsForClassicLeague",
                type: "GET",
                cache: true,
                data: { leagueId: league.id },
                beforeSend: function () {
                    standingsLoader.addClass("active");
                },
                success: function (json, status, xhr) {
                    if (xhr.readyState === 4 && xhr.status === 200) {
                        self.SelectedLeagueStandings(json.Standings.results);
                        initialiseDatatable();
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
                html += activeChips[i] + "</br>";
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
        //$('.menu .item').tab();
        ////self.SelectedLeague(self.ClassicLeagues()[4]);
        for (var i = 0; i < self.ClassicLeagues().length; i++) {
            if (self.ClassicLeagues()[i] = self.SelectedLeague()) {
                self.SelectedLeague(self.ClassicLeagues()[i]);
                break;
            }
        }

        initialiseDatatable();
    };

    self.init();

    /* Formatting function for row details - modify as you need */
    function FormatChildRow(gwTeam) {

        var team = gwTeam.picks;
        var starters = gwTeam.picks.filter(x => x.position <= 11);
        var subs = gwTeam.picks.filter(x => x.position > 11);
        var gks = gwTeam.picks.filter(x => x.player.element_type == 1 && x.multiplier > 0);
        var defs = gwTeam.picks.filter(x => x.player.element_type == 2 && x.multiplier > 0);
        var mids = gwTeam.picks.filter(x => x.player.element_type == 3 && x.multiplier > 0);
        var fwds = gwTeam.picks.filter(x => x.player.element_type == 4 && x.multiplier > 0);
        var autoSubs = gwTeam.automatic_subs;
        var gwTransfers = gwTeam.GWTransfers;

        var teamHtml = "";
        var starterCells = "";
        var subCells = "";

        for (var i = 0; i < starters.length; i++) {
            var icon = "";
            var subIcon = "";

            if (starters[i].player.element_type == 1) {
                icon += "user";
            }
            else {
                icon += "user outline";
            }

            if (starters[i].is_captain) {
                subIcon += "top right corner copyright";
            }

            for (var j = 0; j < autoSubs.length; j++) {
                if (autoSubs[j].element_in == starters[i].player.id) {
                    subIcon += "top right corner sync";
                }
            }

            icon += " icon";
            subIcon += " icon";

            starterCells +=
                '<div class="item">' +
                '<div class="content">' +
            '<h4 class="ui icon header">' +
                '<i class="small icons">' +
            '  <i class="' + icon + '"></i>' +
            '  <i class="' + subIcon + '"></i>' +
            '</i>' +
                '</br>' +
            starters[i].player.web_name +
                '</h4>' +
                '<p style="text-align: center">' + starters[i].GWPlayer.stats.gw_points + '</p>' +
                '</div>' +
                '</div>'
        }  

        for (var i = 0; i < subs.length; i++) {
            var icon = "";
            var subIcon = "";

            if (starters[i].player.element_type == 1) {
                icon += "user";
            }
            else {
                icon += "user outline";
            }

            for (var j = 0; j < autoSubs.length; j++) {
                if (autoSubs[j].element_out == subs[i].player.id) {
                    subIcon += "top right corner sync";
                }
            }

            icon += " disabled icon";
            subIcon += " icon";

            subCells +=
                '<div class="item">' +
                '<div class="content">' +
            '<h4 class="ui icon header">' +
            '<i class="small icons">' +
            '  <i class="' + icon + '"></i>' +
            '  <i class="' + subIcon + '"></i>' +
            '</i>' +
                '</br>' +
            //'<i class="tiny ' + icon + ' disabled icon"></i>' +
            subs[i].player.web_name +
            '</h4>' +
                '<p style="text-align: center">' + subs[i].GWPlayer.stats.gw_points + '</p>' +
                '</div>' +
                '</div>'
        }  

        var transfersHtml = "";

        for (var i = 0; i < gwTransfers.length; i++) {

            transfersHtml +=
                '<div class="item">' +
                    //'<i class="small arrow left icon"></i>' +
            '<span>' + gwTransfers[i].PlayerIn.web_name + ' for ' + gwTransfers[i].PlayerOut.web_name + '</span>' +
            //'<span>' + gwTransfers[i].PlayerOut.web_name + '</span>' +
            //'<i class="small arrow right icon"></i>' +
                '</div>';

        }

        return '<div class="ui grid">' +
                '<div class="thirteen wide column">' +
                    '<div class="ui horizontal list">' + starterCells + '</div>' +
                    '</br>' +
                    '<div class="ui horizontal list">' + subCells + '</div>' +
                '</div>' +
            '<div class="two wide column">' +
                    '<h4 class="ui header">GW Transfers</h4>' +
                    '<div class="ui list">' + transfersHtml + '</div>' +
                    '</br' +
                '</div>' +
                '</div>';

        //for (var i = 0; i < starters.length; i++) {
        //    starterCells +=
        //        '<tr>' +
        //        '<td>' + starters[i].player.web_name + '</td>' +
        //        '<td>' + starters[i].player.element_type + '</td>' +
        //        //'<td>' + starters[i].GWPlayer.stats.EstimatedBonus + '</td>' +
        //        '<td>' + starters[i].GWPlayer.stats.bps + ' (' + starters[i].GWPlayer.stats.BpsRank + ')' + '</td>' +
        //        '<td>' + starters[i].GWPlayer.stats.gw_points + '</td>' +
        //        '</tr>'
        //}  

        //for (var i = 0; i < subs.length; i++) {
        //    subCells +=
        //        '<tr>' +
        //        '<td>' + subs[i].player.web_name + '</td>' +
        //        '<td>' + subs[i].player.element_type + '</td>' +
        //        //'<td>' + subs[i].GWPlayer.stats.EstimatedBonus + '</td>' +
        //        '<td>' + subs[i].GWPlayer.stats.bps + ' (' + subs[i].GWPlayer.stats.BpsRank + ')' + '</td>' +
        //        '<td>' + subs[i].GWPlayer.stats.gw_points + '</td>' +
        //        '</tr>'
        //}  

        //return '<div class="ui horizontal list">' +
        //    starterCells +
        //    '</div>' +
        //    '</br>' +
        //    '<div class="ui horizontal list">' +
        //    subCells +
        //    '</div>';


        //return '<div class="ui fifteen statistics">' +
        //    teamHtml;
        //    '</div>';
    }

    function initialiseDatatable() {
        $(document).ready(function () {
            var table = standingsTable.DataTable({
                columnDefs: [
                    { orderable: false, targets: "no-sort" }
                ],
                order: [[2, "asc"]],
                responsive: true,
                fixedHeader: true
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

};