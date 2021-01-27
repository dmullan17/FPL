LeaguesViewModel = function (data) {
    "use strict";

    var self = this;

    //self.GWTeam = ko.observable(data.GWTeam);
    //self.Team = ko.observable(data.Team);
    //self.TotalPoints = ko.observable(data.TotalPoints);
    //self.GWPoints = ko.observable(data.GWPoints);
    //self.GameweekId = ko.observable(data.GameweekId);
    //self.EventStatus = ko.observable(data.EventStatus);
    //self.EntryHistory = ko.observable(data.EntryHistory);
    //self.IsLive = ko.observable(data.IsLive);
    //self.SelectedPlayer = ko.observable();
    //self.SelectedPlayerStatus = ko.observable();
    self.ClassicLeagues = ko.observableArray(data.ClassicLeagues);
    self.H2HLeagues = ko.observableArray(data.H2HLeagues);
    self.SelectedLeague = ko.observable();
    self.Cup = ko.observable(data.Cup);
    self.IsEventLive = ko.observable(data.IsEventLive);
    self.CurrentGwId = ko.observable(data.CurrentGwId);

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
            if ($.fn.dataTable.isDataTable('#example')) {
                $('#example').DataTable().clear().destroy();
            }
            initialiseDatatable();
            self.SelectedLeague(league);
        }

    };

    self.SelectedLeague.subscribe(function (league) {

        if ($.fn.dataTable.isDataTable('#example')) {
            $('#example').DataTable().clear().destroy();
            self.SelectedLeague(league);
            initialiseDatatable();
            return;
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
        self.SelectedLeague(self.ClassicLeagues()[4]);
        initialiseDatatable();
    };

    self.init();

    /* Formatting function for row details - modify as you need */
    function format(d) {
        // `d` is the original data object for the row
        var team = d.filter(x => x.multiplier > 0);
        var starters = d.filter(x => x.position <= 11);
        var subs = d.filter(x => x.position > 11);
        var gks = d.filter(x => x.player.element_type == 1 && x.multiplier > 0);
        var defs = d.filter(x => x.player.element_type == 2 && x.multiplier > 0);
        var mids = d.filter(x => x.player.element_type == 3 && x.multiplier > 0);
        var fwds = d.filter(x => x.player.element_type == 4 && x.multiplier > 0);

        var starterCells = "";
        var subCells = "";

        for (var i = 0; i < starters.length; i++) {
            starterCells +=
                '<tr>' +
                '<td>' + starters[i].player.web_name + '</td>' +
                '<td>' + starters[i].player.event_points + '</td>' +
                '</tr>'
        }  

        for (var i = 0; i < subs.length; i++) {
            subCells +=
                '<tr>' +
                '<td>' + subs[i].player.web_name + '</td>' +
                '<td>' + subs[i].player.event_points + '</td>' +
                '</tr>'
        }  

        return '<div class="ui grid">' +
                    '<div class="six wide column">' +
                        '<table class="ui very compact small table" style="width: auto !important">' +
                            '<thead>' +
                                '<tr>' +
                                    '<th>Name</th>' +
                                    '<th>Points</th>' +
                                '</tr>' +
                            '</thead>' +
                            '<tbody>' +
                                starterCells +
                            '</tbody>' +
                        '</table>' +
                    '</div>' +
                    '<div class="six wide column">' +
                        '<table class="ui very compact small table" style="width: auto !important">' +
                            '<thead>' +
                                '<tr>' +
                                    '<th>Name</th>' +
                                    '<th>Points</th>' +
                                '</tr>' +
                            '</thead>' +
                            '<tbody>' +
                                subCells +
                            '</tbody>' +
                        '</table>' +
                    '</div>' +
            '</div>';
    }

    function initialiseDatatable() {
        $(document).ready(function () {
            var table = $('#example').DataTable({
                columnDefs: [
                    { orderable: false, targets: "no-sort" }
                ],
                order: [[2, "asc"]],
                responsive: true
            });
        });
    }

    // Add event listener for opening and closing details
    $('#example tbody').on('click', 'td.details-control', function () {
        var tr = $(this).closest('tr');
        var td = $(this).closest('td');
        var row = $('#example').DataTable().row(tr);

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
                data: { teamId: teamId },
                contentType: "application/json",
                beforeSend: function () {
                    tr.removeClass('shown');
                    td.addClass('loading');
                },
                success: function (json, status, xhr) {
                    if (xhr.readyState === 4 && xhr.status === 200) {
                        row.child(format(json.picks)).show();
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