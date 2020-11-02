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

    self.CreateGWNetTransfers = function (player) {
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

        var fdrAvg = (oppositionStrength / 5).toFixed(2);
        //var fdrAvg = (FDR / 5).toFixed(2);

        return fdrAvg;
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

    self.init = function () {
    };

    self.init();

    $(document).ready(function () {
        $('#allPlayersTable').DataTable();
    });
};