MyTeamViewModel = function (data) {
    "use strict";

    var self = this;

    self.Picks = ko.observable(data.Picks);
    self.Team = ko.observable(data.Team);
    self.Positions = ko.observableArray(data.Positions);
    self.TotalPoints = ko.observable(data.TotalPoints);
    self.TransferInfo = ko.observable(data.TransferInfo);


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

    self.CalculateFixtureDifficulty = function (team, playerFixtures) {

        var oppositionStrength = 0;
        var FDR = 0;

        for (var i = 0; i < 5; i++) {

            if (team.id == team.Fixtures[i].team_h) {
                oppositionStrength += team.Fixtures[i].team_h_difficulty;
            }
            else if (team.id == team.Fixtures[i].team_a) {
                oppositionStrength += team.Fixtures[i].team_a_difficulty;
            }

            FDR += playerFixtures[i].difficulty;

        }

        var oppositionStrengthAvg = (oppositionStrength / 5).toFixed(2);
        var fdrAvg = (FDR / 5).toFixed(2);

        return oppositionStrengthAvg + " (" + fdrAvg + ")";
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
};