MyTeamViewModel = function (data) {
    "use strict";

    var self = this;

    self.Picks = ko.observable(data.Picks);
    self.Team = ko.observable(data.Team);
    self.Positions = ko.observableArray(data.Positions);
    self.TotalPoints = ko.observable(data.TotalPoints);
    self.TransferInfo = ko.observable(data.TransferInfo);
    self.CurrentGwId = ko.observable(data.CurrentGwId);


    //self.getColor = ko.pureComputed(function (data) {
    //    return this;
    //});

    self.getColor = function (position) {
        if (position > 11) {
            return "lightgray";
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

        if ((self.CurrentGwId() + 1) == firstFixture.Event || self.CurrentGwId() == firstFixture.Event) {
            if (team.id == firstFixture.team_h) {
                return "GW" + firstFixture.Event + " - " + firstFixture.team_a_name + " (H)";
            }
            else if (team.id == firstFixture.team_a) {
                return "GW" + firstFixture.Event + " - " + firstFixture.team_h_name + " (A)";
            }
        }
        else {
            return "No Game";
        }
    };

    self.CreateGWNetTransfers = function (player) {
        return nFormatter((player.transfers_in_event - player.transfers_out_event), 0)
        //return player.transfers_in_event - player.transfers_out_event;
    }

    self.CalculateFixtureDifficulty = function (team, playerFixtures) {

        //var oppositionStrength = 0;
        var FDR = 0;

        for (var i = 0; i < 5; i++) {

            //if (team.id == team.Fixtures[i].team_h) {
            //    oppositionStrength += team.Fixtures[i].team_h_difficulty;
            //}
            //else if (team.id == team.Fixtures[i].team_a) {
            //    oppositionStrength += team.Fixtures[i].team_a_difficulty;
            //}

            FDR += playerFixtures[i].difficulty;

        }

        //var oppositionStrengthAvg = (oppositionStrength / 5).toFixed(2);
        var fdrAvg = (FDR / 5).toFixed(1);

        return fdrAvg;
    };

    self.FormatValue = function (value, netProfit) {
        var value = parseFloat(value) / 10;
        if (netProfit > 0) {
            return value.toFixed(1) + " (+" + (netProfit / 10) + ")";
        }
        else {
            return value.toFixed(1) + " (" + (netProfit / 10) + ")";
        }
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

    //self.viewGame = function (player) {
    //    $('.ui.special.popup').popup({
    //        inline: true
    //    }).show();

    //    var popupLoading = '<i class="notched circle loading icon green"></i> wait...';
    //    $('.test').popup({
    //        popup: $('.ui.special.popup'),
    //        inline: true,
    //        on: 'click',
    //        exclusive: true,
    //        hoverable: true,
    //        html: popupLoading,
    //        variation: 'wide',
    //        delay: {
    //            show: 400,
    //            hide: 400
    //        },
    //        onShow: function (el) { // load data (it could be called in an external function.)
    //            var popup = this;
    //            popup.html(popupLoading);
    //            $.ajax({
    //                url: 'http://www.example.com/'
    //            }).done(function (result) {
    //                popup.html(result);
    //            }).fail(function () {
    //                popup.html('error');
    //            });
    //        }
    //    }).show();
    //};

    self.init = function () {

        //$('.ui.icon.button').popup();
        //$('.circle.icon').popup();


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