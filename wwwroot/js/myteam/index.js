﻿MyTeamViewModel = function (data) {
    "use strict";

    var self = this;

    self.MyTeam = ko.observable(data.MyTeam);
    //self.Picks = ko.observable(data.Picks);
    self.Team = ko.observable(data.Team);
    self.Positions = ko.observableArray(data.Positions);
    self.TotalPoints = ko.observable(data.TotalPoints);
    //self.TransferInfo = ko.observable(data.TransferInfo);
    self.CurrentGwId = ko.observable(data.CurrentGwId);
    self.IsEventFinished = ko.observable(data.IsEventFinished);
    self.EntryHistory = ko.observable(data.EntryHistory);
    self.CompleteEntryHistory = ko.observable(data.CompleteEntryHistory);
    self.EventStatus = ko.observable(data.EventStatus);

    self.PotentialSub1 = ko.observable();
    self.PotentialSub2 = ko.observable();

    //self.getColor = ko.pureComputed(function (data) {
    //    return this;
    //});

    self.GetFlagClass = function (country) {
        return country.toLowerCase() + " flag";
    }

    self.FormatLargeNumber = function (number) {
        return nFormatter(number, 2)
        //return player.transfers_in_event - player.transfers_out_event;
    }

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

    self.GetTotalPercentileChange = function (entryHistory) {
        var change = entryHistory.LastEventOverallRank - entryHistory.overall_rank;
        var formattedChange = nFormatter(change, 2);

        if (change > 0) {
            return "+" + formattedChange;
        }
        else if (change < 0) {
            return formattedChange;
        }
    }

    self.MakeSub = function (pick, event) {

		if (!self.PotentialSub1()) {
			var allSubButtons = $("button.substitution");
			var position = pick.position;

			allSubButtons.splice(position - 1, 1);

			if (position < 12) {
				for (var i = 0; i < allSubButtons.length; i++) {

					if (i < 10) {
						allSubButtons[i].className += " disabled";
					} else {
						allSubButtons[i].className += " green";
					}

				}
			} else {
				for (var i = 0; i < allSubButtons.length; i++) {

					if (i < 11) {
						allSubButtons[i].className += " green";
					} else {
						allSubButtons[i].className += " disabled";
					}

				}
			}

			self.PotentialSub1(pick);
		}
		else if (self.PotentialSub1().element == pick.element) {
			location.reload();
		}
		else {

			self.PotentialSub2(pick);

			$.ajax({
				url: "/MyTeam/MakeSub",
				type: "POST",
				cache: true,
				data: {
					sub1: self.PotentialSub1().element,
                    sub1Position: self.PotentialSub1().position,
					sub1ElementType: self.PotentialSub1().element_type,
                    isSub1Captain: self.PotentialSub1().is_captain,
					isSub1ViceCaptain: self.PotentialSub1().is_vice_captain,
					sub2: self.PotentialSub2().element,
                    sub2Position: self.PotentialSub2().position,
					sub2ElementType: self.PotentialSub2().element_type
				},
				beforeSend: function () {

				},
				success: function (json, status, xhr) {
					if (xhr.readyState === 4 && xhr.status === 200) {
						location.reload();
						return;
					}
				},
				complete: function (xhr, status) {
					if (xhr.readyState === 4 && xhr.status !== 200) {
					}
				}
			});
		}
    }

    self.GetLastTimeTotalRankWasUpdated = function (eventStatus) {

        var finishedEvents = eventStatus.status.filter(x => x.points == "r");
        var lastFinishedEvent = finishedEvents[finishedEvents.length - 1];
        var day = getDayOfWeek(lastFinishedEvent.date);

        return "Last updated on " + day + " " + lastFinishedEvent.date;
    }

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

    self.GetCurrentGw = function () {
        if (!self.IsEventFinished) {
            return "GW " + (self.CurrentGwId());
        }
        else {
            return "GW " + (self.CurrentGwId() + 1);
        }
    };

    self.GetNextGw = function () {
        if (!self.IsEventFinished) {
            return "GW " + (self.CurrentGwId() + 1);
        }
        else {
            return "GW " + (self.CurrentGwId() + 2);
        }
    };

    self.GetNextPlusOneGw = function () {
        if (!self.IsEventFinished) {
            return "GW " + (self.CurrentGwId() + 2);
        }
        else {
            return "GW " + (self.CurrentGwId() + 3);
        }
    };

    self.GetNextPlusTwoGw = function () {
        if (!self.IsEventFinished) {
            return "GW " + (self.CurrentGwId() + 3);
        }
        else {
            return "GW " + (self.CurrentGwId() + 4);
        }
    };

    self.GetNextPlusThreeGw = function () {
        if (!self.IsEventFinished) {
            return "GW " + (self.CurrentGwId() + 4);
        }
        else {
            return "GW " + (self.CurrentGwId() + 5);
        }
    };


    //self.GetPlayersNextFixture = function (team) {

    //    var fixtures = team.Fixtures.filter(x => x.Event == self.CurrentGwId() + 1 || x.Event == self.CurrentGwId());
    //    var firstFixture = team.Fixtures[0];

    //    if (fixtures.length > 0) {
    //        var gwfixtures = "";
    //        for (var i = 0; i < fixtures.length; i++) {
    //            if (team.id == fixtures[i].team_h) {
    //                gwfixtures += "GW" + fixtures[i].Event + " - " + fixtures[i].team_a_name + " (H) <br/>";
    //            }
    //            else if (team.id == fixtures[i].team_a) {
    //                gwfixtures += "GW" + fixtures[i].Event + " - " + fixtures[i].team_h_name + " (A) <br/>";
    //            }

    //        }
    //        return gwfixtures;
    //    }
    //    else {
    //        return "No Game";
    //    }

    //};

    self.GetCurrentGWGames = function (player) {

        var games = player.GWGames;
        var team = player.player.Team;
        var html = "";

        if (!self.IsEventFinished) {
            for (var i = 0; i < games.length; i++) {
                if (team.id == games[i].team_h) {
                    html += "<div style='" + GetFdrStyle(games[i].team_h_difficulty) + "'>" + games[i].AwayTeam.short_name + " (H)</div>";
                }
                else if (team.id == games[i].team_a) {
                    html += "<div style='" + GetFdrStyle(games[i].team_a_difficulty) + "'>" + games[i].HomeTeam.short_name + " (A)</div>";
                }
            }
        }
        else {
            var fixtures = team.Fixtures.filter(x => x.Event == self.CurrentGwId() + 1);

            if (fixtures.length > 0) {
                for (var i = 0; i < fixtures.length; i++) {
                    if (team.id == fixtures[i].team_h) {
                        html += "<div style='" + GetFdrStyle(fixtures[i].team_h_difficulty) + "'>" + fixtures[i].team_a_short_name + " (H) </div>";
                    }
                    else if (team.id == fixtures[i].team_a) {
                        html += "<div style='" + GetFdrStyle(fixtures[i].team_a_difficulty) + "'>" + fixtures[i].team_h_short_name + " (A) </div>";
                    }

                }
            }
            else {
                html += "N/A";
            }
        }

        return html;

    };

    self.GetNextGWGames = function (team) {

        var html = "";

        if (!self.IsEventFinished) {
            var fixtures = team.Fixtures.filter(x => x.Event == self.CurrentGwId() + 1);
        }
        else {
            var fixtures = team.Fixtures.filter(x => x.Event == self.CurrentGwId() + 2);
        }

        if (fixtures.length > 0) {
            for (var i = 0; i < fixtures.length; i++) {
                if (team.id == fixtures[i].team_h) {
                    //html += trimTeamName(fixtures[i].team_a_name) + " (H) <br/>";
                    html += "<div style='" + GetFdrStyle(fixtures[i].team_h_difficulty) + "'>" + fixtures[i].team_a_short_name + " (H) </div>";
                }
                else if (team.id == fixtures[i].team_a) {
                    //html += trimTeamName(fixtures[i].team_h_name) + " (A) <br/>";
                    html += "<div style='" + GetFdrStyle(fixtures[i].team_a_difficulty) + "'>" + fixtures[i].team_h_short_name + " (A) </div>";
                }

            }
        }
        else {
            html += "N/A";
        }

        return html;

    };

    self.GetNextPlusOneGWGames = function (team) {

        var html = "";

        if (!self.IsEventFinished) {
            var fixtures = team.Fixtures.filter(x => x.Event == self.CurrentGwId() + 2);
        }
        else {
            var fixtures = team.Fixtures.filter(x => x.Event == self.CurrentGwId() + 3);
        }

        if (fixtures.length > 0) {
            for (var i = 0; i < fixtures.length; i++) {
                if (team.id == fixtures[i].team_h) {
                    //html += trimTeamName(fixtures[i].team_a_name) + " (H) <br/>";
                    html += "<div style='" + GetFdrStyle(fixtures[i].team_h_difficulty) + "'>" + fixtures[i].team_a_short_name + " (H) </div>";
                }
                else if (team.id == fixtures[i].team_a) {
                    //html += trimTeamName(fixtures[i].team_h_name) + " (A) <br/>";
                    html += "<div style='" + GetFdrStyle(fixtures[i].team_a_difficulty) + "'>" + fixtures[i].team_h_short_name + " (A) </div>";
                }

            }
        }
        else {
            html += "N/A";
        }

        return html;

    };

    self.GetNextPlusTwoGWGames = function (team) {

        var html = "";

        if (!self.IsEventFinished) {
            var fixtures = team.Fixtures.filter(x => x.Event == self.CurrentGwId() + 3);
        }
        else {
            var fixtures = team.Fixtures.filter(x => x.Event == self.CurrentGwId() + 4);
        }

        if (fixtures.length > 0) {
            for (var i = 0; i < fixtures.length; i++) {
                if (team.id == fixtures[i].team_h) {
                    //html += trimTeamName(fixtures[i].team_a_name) + " (H) <br/>";
                    html += "<div style='" + GetFdrStyle(fixtures[i].team_h_difficulty) + "'>" + fixtures[i].team_a_short_name + " (H) </div>";
                }
                else if (team.id == fixtures[i].team_a) {
                    //html += trimTeamName(fixtures[i].team_h_name) + " (A) <br/>";
                    html += "<div style='" + GetFdrStyle(fixtures[i].team_a_difficulty) + "'>" + fixtures[i].team_h_short_name + " (A) </div>";
                }

            }
        }
        else {
            html += "N/A";
        }

        return html;

    };

    self.GetNextPlusThreeGWGames = function (team) {

        var html = "";

        if (!self.IsEventFinished) {
            var fixtures = team.Fixtures.filter(x => x.Event == self.CurrentGwId() + 4);
        }
        else {
            var fixtures = team.Fixtures.filter(x => x.Event == self.CurrentGwId() + 5);
        }

        if (fixtures.length > 0) {
            for (var i = 0; i < fixtures.length; i++) {
                if (team.id == fixtures[i].team_h) {
                    //html += trimTeamName(fixtures[i].team_a_name) + " (H) <br/>";
                    html += "<div style='" + GetFdrStyle(fixtures[i].team_h_difficulty) + "'>" + fixtures[i].team_a_short_name + " (H) </div>";
                }
                else if (team.id == fixtures[i].team_a) {
                    //html += trimTeamName(fixtures[i].team_h_name) + " (A) <br/>";
                    html += "<div style='" + GetFdrStyle(fixtures[i].team_a_difficulty) + "'>" + fixtures[i].team_h_short_name + " (A) </div>";
                }

            }
        }
        else {
            html += "N/A";
        }

        return html;

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

        $('#total-percentile-statistic').popup({
            popup: '#total-percentile-popup.popup',
            position: 'top center',
            hoverable: true
        });

        $('#buying-power-statistic').popup({
            popup: '#buying-power-popup.popup',
            position: 'top center',
            hoverable: true
        });
        //$('.ui.icon.button').popup();
        //$('.circle.icon').popup();


    };

    self.init();

    function GetFdrStyle(difficulty) {
        var html = "padding: 1px;text-align: center;";

        if (difficulty == 1) {
            html += "background-color: rgb(55, 85, 35); color: white;";
        }
        else if (difficulty == 2) {
            html += "background-color: rgb(1, 252, 122);";
        }
        else if (difficulty == 3) {
            html += "background-color: rgb(231, 231, 231)";
        }
        else if (difficulty == 4) {
            html += "background-color: rgb(255, 23, 81); color: white;";
        }
        else if (difficulty == 5) {
            html += "background-color: rgb(128, 7, 45);  color: white;";
        }

        return html;

    }

    //self.GetFdrBgColour = function (test) {

    //    if (test) {
    //        return "green";
    //    }

    //}

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