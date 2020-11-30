PlayersViewModel = function (data) {
    "use strict";

    var self = this;

    self.AllPlayers = ko.observableArray(data.AllPlayers);
    self.Player = ko.observable(data.Player);
    self.TotalPlayerCount = ko.observable(data.TotalPlayerCount);
    self.Positions = ko.observableArray(data.Positions);

    self.getColor = function (multipler) {
        if (multipler == 0) {
            return "grey";
        }
        else {
            return null;
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

    self.ConvertToDecimal = function (value) {
        return (value / 10).toFixed(1);
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

    self.CreateNetTransfers = function (player) {
        //return nFormatter((player.transfers_in - player.transfers_out), 0)
        return player.transfers_in - player.transfers_out;
    }

    self.CreateGWNetTransfers = function (player) {
        //return nFormatter((player.transfers_in_event - player.transfers_out_event), 0)
        return player.transfers_in_event - player.transfers_out_event;
    }

    self.CalculateFixtureDifficulty = function (team) {

        var oppositionStrength = 0;
        //var FDR = 0;

        for (var i = 0; i < 5; i++) {

            if (team.id == team.Fixtures[i].team_h) {
                oppositionStrength += team.Fixtures[i].team_h_difficulty;
            }
            else if (team.id == team.Fixtures[i].team_a) {
                oppositionStrength += team.Fixtures[i].team_a_difficulty;
            }

            //FDR += playerFixtures[i].difficulty;

        }

        var fdrAvg = (oppositionStrength / 5).toFixed(1);
        //var fdrAvg = (FDR / 5).toFixed(2);

        return fdrAvg;
    };

    self.GetNextFiveFixtures = function (team) {

        var games = [];

        for (var i = 0; i < 5; i++) {

            if (team.id == team.Fixtures[i].team_h) {
                games.push(' ' + team.Fixtures[i].team_a_name.substring(0, 3) + ' (H)');
            }
            else if (team.id == team.Fixtures[i].team_a) {
                games.push(' ' + team.Fixtures[i].team_h_name.substring(0, 3) + ' (A)');
            }

        }

        return games;
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
        //var percentile = ((value / totalPlayerCount) * 100);
        //return Math.ceil(percentile);
    };

    self.FormatPositionRankType = function (postionId, value) {
        var pos = self.Positions().find(x => x.id == postionId);
        return value + " / " + pos.element_count;
        //var percentile = ((value / pos.element_count) * 100);
        //return Math.ceil(percentile);
        //return percentile;
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

    self.GetGamesPlayed = function (player) {

        if (player.GamesPlayed == 0) {
            return 0;
        }
        else if (player.GamesPlayed != 0) {
            return player.GamesPlayed + "/" + player.Team.Results.length;
        }
        else {
            return player.GamesPlayed + "/" + (player.Team.Results.length + 1);
        }

    };

    self.init = function () {
    };

    self.init();

    /* Custom filtering function which will search data in column four between two values */
    $.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            var max = parseFloat($('#val').val(), 10);
            var val = parseFloat(data[1]) || 0;

            if (isNaN(max) || val <= max) {
                return true;
            }
            return false;
        }
    );

    $(document).ready(function () {

        //$('#allPlayersTable thead th#value').html('<input type="text" placeholder="Search Value" data-index="2" />');

        var table = $('#allPlayersTable').DataTable({
            fixedColumns: {
                leftColumns: 5
            },
            columnDefs: [
                { type: 'natural', targets: "natural-sorter" }
            ],
            scrollX: true,
            //dom: 'lfrtip',
            buttons: [],
            select: true,
            order: [[2, "desc"]]
        });

        $('#val').keyup(function () {
            table.draw();
        });

        // Filter event handler
        //$(table.table().container()).on('keyup', 'thead input', function () {
        //    table
        //        .column(1)
        //        .search(this.value)
        //        .draw();
        //    table.draw();
        //});

        //$('#table tbody').on('click', 'tr', function () {
        //    if ($(this).hasClass('selected')) {
        //        $(this).removeClass('selected');
        //    }
        //    else {
        //        table.$('tr.selected').removeClass('selected');
        //        $(this).addClass('selected');
        //    }
        //});

        //$("table tbody tr").hover(function () {
        //    if ($(this).hasClass('selected')) {
        //        $(this).removeClass('selected');
        //    }
        //    else {
        //        table.$('tr.selected').removeClass('selected');
        //        $(this).addClass('selected');
        //    }
        //});

        new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'collection',
                    text: 'Table control',
                    buttons: [
                        {
                            text: 'Positions',
                            extend: 'collection',
                            buttons: [
                                {
                                    text: 'GK',
                                    action: function (e, dt, node, config) {
                                        if (dt.column(".position").search() == config.text) {
                                            dt.column(".position").search("").draw();
                                        }
                                        else {
                                            dt.column(".position").search(config.text).draw();
                                        }                                    
                                    }
                                },
                                {
                                    text: 'DEF',
                                    action: function (e, dt, node, config) {
                                        if (dt.column(".position").search() == config.text) {
                                            dt.column(".position").search("").draw();
                                        }
                                        else {
                                            dt.column(".position").search(config.text).draw();
                                        }   
                                    }
                                },
                                {
                                    text: 'MID',
                                    action: function (e, dt, node, config) {
                                        if (dt.column(".position").search() == config.text) {
                                            dt.column(".position").search("").draw();
                                        }
                                        else {
                                            dt.column(".position").search(config.text).draw();
                                        }   
                                    }
                                },
                                {
                                    text: 'FWD',
                                    action: function (e, dt, node, config) {
                                        if (dt.column(".position").search() == config.text) {
                                            dt.column(".position").search("").draw();
                                        }
                                        else {
                                            dt.column(".position").search(config.text).draw();
                                        }   
                                    }
                                },
                            ]
                        },
                        {
                            text: 'Toggle salary',
                            action: function (e, dt, node, config) {
                                dt.column(-1).visible(!dt.column(-1).visible());
                            }
                        },
                        {
                            collectionTitle: 'Visibility control',
                            extend: 'colvis',
                            collectionLayout: 'two-column',
                            postfixButtons: ['colvisRestore']
                        }
                    ]
                }
            ]
        });

        table.buttons(1, null).container().prependTo(
            table.table().container()
        );


        //table.columns(2).search("GK").draw();
        //table.columns(1).search("LIV").draw();
    });

    //$(document).on({
    //    mouseenter: function () {
    //        var trIndex = $(this).index() + 1;
    //        $("table.dataTable").each(function (index) {
    //            $(this).find("tr:eq(" + trIndex + ")").addClass("hover")
    //        });
    //    },
    //    mouseleave: function () {
    //        var trIndex = $(this).index() - 1;
    //        $("table.dataTable").each(function (index) {
    //            $(this).find("tr:eq(" + trIndex + ")").removeClass("hover")
    //        });
    //    }
    //}, ".dataTables_wrapper tr");

};