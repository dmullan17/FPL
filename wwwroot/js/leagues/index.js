﻿LeaguesViewModel = function (data) {
    "use strict";

    var self = this,
        standingsSegment = $('#standings-segment'),
        standingsLoader = $('#standings-loader'),
        entriesLoader = $('#entries-loader'),
        searchedLoader = $('.searched-loader'),
        standingsTable = $('#standings-table'),
        entriesTable = $('#entries-table'),
        standingsTableBody = $('#standings-table tbody'),
        standingsTableFooter = $('#standings-table tfoot'),
        playerTallyTable = $('#player-tally-table'),
        leagueDropdown = $('#league-dropdown'),
        leagueSearchInput = $("#league-search"),
        leagueSearchInput2 = $("#league-search-2"),
        leagueSearchForm2 = $("#league-search-2-form"),
        leagueSearchButton = $("#league-search-button"),
        searchedLeagueName = $("#searched-league-name"),
        leagueSearchAgainButton = $("#league-search-again-button");

    self.ClassicLeagues = ko.observableArray(data.ClassicLeagues);
    self.H2HLeagues = ko.observableArray(data.H2HLeagues);
    self.SelectedLeague = ko.observable(data.SelectedLeague);
    self.SearchedLeague = ko.observable({});
    self.Cup = ko.observable(data.Cup);
    self.IsEventLive = ko.observable(data.IsEventLive);
    self.IsGameLive = ko.observable(data.IsGameLive);
    self.CurrentGwId = ko.observable(data.CurrentGwId);
    self.TeamId = ko.observable(data.TeamId);
    self.EventStatus = ko.observable(data.EventStatus);
    self.LastUpdated = ko.observable();
    self.LastUpdatedTime = ko.observable(data.LastUpdated);
    self.SelectedPlayerFromTally = ko.observable();
    self.SelectedPlayer = ko.observable();
    self.SearchedLeagueId = ko.observable();
    self.SearchedLeagueName = ko.observable();
    self.CurrentLeagueId = ko.observable();
    self.UserTeam = ko.observable();
    self.HideLeagueUI = ko.observable(false);

    self.SelectedLeagueStandings = ko.observableArray();
    self.SelectedLeaguePlayersTally = ko.observableArray();
    self.AllGwTransfersInLeague = ko.observableArray();
    self.ManagersAffiliatedWithSelectedPlayerFromTally = ko.observableArray();

    if (data.SelectedLeague != null) {
        self.UserTeam(data.SelectedLeague.UserTeam);
        self.SelectedLeagueStandings(self.SelectedLeague().Standings.results);
        self.SelectedLeaguePlayersTally(self.SelectedLeague().PlayersTally);
        self.AllGwTransfersInLeague(self.SelectedLeague().AllGwTransfers);
    } else {
        self.HideLeagueUI(true);
    }

    self.GetSelectedLeagueStandingsLoaderText = function (data) {

        if (self.SelectedLeague() != null) {
            return 'Loading ' + self.SelectedLeague().Standings.results.length + ' fantasy teams from ' + self.SelectedLeague().name;
        } else {
            return "";
        }
    }

    self.GetSearchedLeagueStandingsLoaderText = function (data) {

        //if searched league is not null and object is not empty
        if (self.SearchedLeague() != null && Object.keys(self.SearchedLeague()).length > 0) {
            return 'Loading ' + self.SearchedLeague().Standings.results.length + ' fantasy teams from ' + self.SearchedLeague().name;
        } else {
            return "";
        }
    }

    self.SearchLeagueAgain = function () {

        leagueSearchInput.show();
        leagueSearchAgainButton.hide();
        searchedLeagueName.hide();

    }


    self.SearchLeagues = function () {

        //get basic info about searched league
        $.when(self.GetBasicInfoForLeague()).done(function (a1, a2, a3, a4) {
            //if successful load entire searched league
            if (a2 == "success") {
                self.HideLeagueUI(false);
                $.when(self.LoadSearchedLeague()).done(function (b1, b2, b3, b4) {
                    if (b2 == "success") {
                    }
                });
            }
        });

    }

    self.GetBasicInfoForLeague = function () {
        return $.ajax({
            url: "/Leagues/GetBasicInfoForLeague",
            type: "GET",
            cache: false,
            data: {
                leagueId: self.SearchedLeagueId()
            },
            beforeSend: function () {
                leagueSearchInput.removeClass("error");
                leagueSearchInput2.removeClass("error");
                leagueSearchButton.addClass("loading");
            },
            success: function (json, status, xhr) {
                if (xhr.readyState === 4 && xhr.status === 200) {
                    self.SearchedLeague(json);
                    leagueSearchButton.addClass("disabled");
                }
            },
            complete: function (xhr, status) {
                if (xhr.readyState === 4 && xhr.status !== 200) {
                    leagueSearchInput.addClass("error");
                    leagueSearchInput2.addClass("error");
                }
                leagueSearchButton.removeClass("loading");
            }
        });
    }

    self.LastUpdated = ko.computed(function () {

        if (self.IsGameLive()) {
            var lastUpdatedTime = new Date().toLocaleString("en-GB");
            var formattedTime = lastUpdatedTime.slice(0, -3);
            return "Last updated: " + formattedTime;
        } else {
            var lastUpdatedTime = new Date(self.LastUpdatedTime()).toLocaleString("en-GB");
            var formattedTime = lastUpdatedTime.slice(0, -3);
            return "Last updated: " + formattedTime;
        }
    });

    self.LoadSearchedLeague = function () {

        if ($.fn.dataTable.isDataTable(standingsTable)) {
            standingsTable.DataTable().clear().destroy();
            playerTallyTable.DataTable().clear().destroy();

            return $.ajax({
                url: "/Leagues/GetPlayerStandingsForClassicLeague",
                type: "GET",
                cache: false,
                data: {
                    leagueId: self.SearchedLeagueId(),
                    gameweekId: self.CurrentGwId()
                },
                beforeSend: function () {
                    searchedLoader.addClass("active");
                    leagueDropdown.addClass("disabled");
                    standingsTable.hide();
                    playerTallyTable.hide();
                },
                success: function (json, status, xhr) {
                    if (xhr.readyState === 4 && xhr.status === 200) {
                        self.SearchedLeagueName(json.name)
                        searchedLeagueName.show();
                        leagueSearchInput.hide();
                        leagueSearchAgainButton.show();
                        self.SelectedLeagueStandings(json.Standings.results);
                        self.SelectedLeaguePlayersTally(json.PlayersTally);
                        self.AllGwTransfersInLeague(json.AllGwTransfers);
                        self.UserTeam(json.UserTeam);
                        self.CurrentLeagueId(json.id);
                        initialiseStandingsDatatable();
                        initialiseTalliesDatatable();
                        initialiseEntriesTable();
                    }
                },
                complete: function (xhr, status) {
                    searchedLoader.removeClass("active");
                    leagueSearchButton.removeClass("disabled");
                    leagueDropdown.removeClass("disabled");
                    standingsTable.show();
                    playerTallyTable.show();
                    if (xhr.readyState === 4 && xhr.status !== 200) {
                    }
                }
            });
        } else {
            return $.ajax({
                url: "/Leagues/GetPlayerStandingsForClassicLeague",
                type: "GET",
                cache: false,
                data: {
                    leagueId: self.SearchedLeagueId(),
                    gameweekId: self.CurrentGwId()
                },
                beforeSend: function () {
                    searchedLoader.addClass("active");
                    leagueDropdown.addClass("disabled");
                    standingsTable.hide();
                    playerTallyTable.hide();
                },
                success: function (json, status, xhr) {
                    if (xhr.readyState === 4 && xhr.status === 200) {
                        //self.HideLeagueUI(false);
                        self.SearchedLeagueName(json.name)
                        leagueSearchInput.hide();
                        leagueSearchAgainButton.show();
                        self.SelectedLeagueStandings(json.Standings.results);
                        self.SelectedLeaguePlayersTally(json.PlayersTally);
                        self.AllGwTransfersInLeague(json.AllGwTransfers);
                        self.UserTeam(json.UserTeam);
                        self.CurrentLeagueId(json.id);
                        initialiseStandingsDatatable();
                        initialiseTalliesDatatable();
                        initialiseEntriesTable();
                    }
                },
                complete: function (xhr, status) {
                    searchedLoader.removeClass("active");
                    leagueSearchButton.removeClass("disabled");
                    leagueDropdown.removeClass("disabled");
                    standingsTable.show();
                    playerTallyTable.show();
                    if (xhr.readyState === 4 && xhr.status !== 200) {
                    }
                }
            });
        }

    }


    self.reload = function () {
        var leagueId = 0;
        if (self.CurrentLeagueId() != undefined) {
            leagueId = self.CurrentLeagueId();
        } else {
            leagueId = self.SelectedLeague().id;
        }

        if ($.fn.dataTable.isDataTable(standingsTable)) {
            standingsTable.DataTable().clear().destroy();
            playerTallyTable.DataTable().clear().destroy();

            $.ajax({
                url: "/Leagues/GetPlayerStandingsForClassicLeague",
                type: "GET",
                cache: false,
                data: {
                    leagueId: leagueId,
                    gameweekId: self.CurrentGwId()
                },
                beforeSend: function () {
                    if (self.CurrentLeagueId() != undefined) {
                        searchedLoader.addClass("active");
                    } else {
                        standingsLoader.addClass("active");
                        entriesLoader.addClass("active");
                    }

                    leagueDropdown.addClass("disabled");
                    standingsTable.hide();
                    playerTallyTable.hide();
                },
                success: function (json, status, xhr) {
                    if (xhr.readyState === 4 && xhr.status === 200) {
                        self.SelectedLeagueStandings(json.Standings.results);
                        self.SelectedLeaguePlayersTally(json.PlayersTally);
                        self.AllGwTransfersInLeague(json.AllGwTransfers);
                        self.UserTeam(json.UserTeam);
                        self.CurrentLeagueId(json.id);
                        initialiseStandingsDatatable();
                        initialiseTalliesDatatable();
                        initialiseEntriesTable();
                        return;
                    }
                },
                complete: function (xhr, status) {
                    searchedLoader.removeClass("active");
                    leagueSearchButton.removeClass("disabled");
                    standingsLoader.removeClass("active");
                    entriesLoader.removeClass("active");
                    leagueDropdown.removeClass("disabled");
                    standingsTable.show();
                    playerTallyTable.show();
                    if (xhr.readyState === 4 && xhr.status !== 200) {
                    }
                }
            });
        }

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

        if (gwTeam.ActiveChips.includes("wildcard") || gwTeam.ActiveChips.includes("freehit")) {
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
                        if (tally.Pick.GWPlayer.stats.EstimatedBonus[i] != null) {
                            points += tally.Pick.GWPlayer.stats.EstimatedBonus[j];
                        }
                    }
                }
            }
        }

        return points;

    }

    //using this ensures a picks additional points for being a captain doesnt get reflected in the breakdown modal
    self.GetPlayerGwPoints = function (pick) {

	    //ensuring the gw points shown in the player tally table includes the estimated bonus
	    var points = pick.GWPlayer.stats.total_points;

	    for (var i = 0; i < pick.GWGames.length; i++) {
		    for (var j = 0; j < pick.GWPlayer.explain.length; j++) {
			    if (pick.GWGames[i].id == pick.GWPlayer.explain[j].fixture) {
				    if (!pick.GWGames[i].finished) {
					    if (pick.GWPlayer.stats.EstimatedBonus[i] != null) {
						    points += pick.GWPlayer.stats.EstimatedBonus[j];
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
            initialiseEntriesTable();
            self.SelectedLeague(league);
        }

    };

    var localCache = {
        data: {},
        remove: function (url) {
            delete localCache.data[url];
        },
        exist: function (url) {
            return localCache.data.hasOwnProperty(url) && localCache.data[url] !== null;
        },
        get: function (url) {
            console.log('Getting in cache for url' + url);
            return localCache.data[url];
        },
        set: function (url, cachedData, callback) {
            localCache.remove(url);
            localCache.data[url] = cachedData;
            if ($.isFunction(callback)) callback(cachedData);
        }
    };

    $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
        if (options.cache) {
            var success = originalOptions.success || $.noop,
                url = originalOptions.url + "/" + originalOptions.data.leagueId;
            //remove jQuery cache as we have our own localCache
            options.cache = false;
            options.beforeSend = function () {
                if (localCache.exist(url) && !self.IsGameLive()) {
                    success(localCache.get(url));
                    return false;
                }
                standingsLoader.addClass("active");
                entriesLoader.addClass("active");
                leagueDropdown.addClass("disabled");
                standingsTable.hide();
                playerTallyTable.hide();
                return true;
            };
            options.success = function (data, textStatus) {
                localCache.set(url, data, success);
            };
            options.complete = function (data, textStatus) {
                standingsLoader.removeClass("active");
                entriesLoader.removeClass("active");
                leagueDropdown.removeClass("disabled");
                standingsTable.show();
                playerTallyTable.show();
                //localCache.set(url, data, complete);
            };
        }
    });

    //$.ajaxPrefilter(function (options, originalOptions, jqXHR) {
    //    if (options.cache) {
    //        var success = originalOptions.success || $.noop,
    //            url = originalOptions.url + "/" + originalOptions.data.leagueId;

    //        options.cache = false; //remove jQuery cache as we have our own localStorage
    //        options.beforeSend = function () {
    //            if (localStorage.getItem(url)) {
    //                success(localStorage.getItem(url));
    //                return false;
    //            }
    //            return true;
    //        };
    //        options.success = function (data, textStatus) {
    //            var responseData = data;
    //            localStorage.setItem(url, responseData);
    //            if ($.isFunction(success)) success(responseData); //call back to original ajax call
    //        };
    //    }
    //});

    self.SelectedLeague.subscribe(function (league) {

        if (self.SelectedLeague() == self.SearchedLeague()) {
            return;
        }

        //if 'get league by id' option selected, show league search input allowing user to enter id
        if (league.id == -1) {
            leagueSearchForm2.show();
            leagueSearchForm2.css("display", "inline-block");
            //leagueSearchInput2.show()
            self.HideLeagueUI(true);
            return;
        }

        leagueSearchForm2.hide();
        //leagueSearchInput2.hide();
        self.HideLeagueUI(false);

        if ($.fn.dataTable.isDataTable(standingsTable) || $.fn.dataTable.isDataTable(entriesTable)) {
            standingsTable.DataTable().clear().destroy();
            playerTallyTable.DataTable().clear().destroy();
            entriesTable.DataTable().clear().destroy();

            $.ajax({
                url: "/Leagues/GetPlayerStandingsForClassicLeague",
                type: "GET",
                cache: true,
                data: {
                    leagueId: league.id,
                    gameweekId: self.CurrentGwId()
                },
                //beforeSend: function () {
                //    standingsLoader.addClass("active");
                //},
                success: function (json, status, xhr) {
                    //if (xhr.readyState === 4 && xhr.status === 200) {
                        self.SelectedLeagueStandings(json.Standings.results);
                        self.SelectedLeaguePlayersTally(json.PlayersTally);
                        self.AllGwTransfersInLeague(json.AllGwTransfers);
                        self.UserTeam(json.UserTeam);
                        self.CurrentLeagueId(json.id);
                        initialiseStandingsDatatable();
                        initialiseTalliesDatatable();
                        initialiseEntriesTable();
                        return;
                    //}
                }
                //complete: function (xhr, status) {
                //    standingsLoader.removeClass("active");
                //    if (xhr.readyState === 4 && xhr.status !== 200) {
                //    }
                //}
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
        if (manager.last_rank != 0) {
            if (manager.rank > manager.last_rank) {
                return "red arrow alternate circle down small icon";
            }
            else if (manager.rank < manager.last_rank) {
                return "green arrow alternate circle up small icon";
            }
            else if (manager.rank == manager.last_rank) {
                return "grey circle small icon";
            }
        } else {
            return "grey circle small icon";
        }
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

        return nFormatter(manager.rank, 1);
    }

    self.GetOverallRank = function (manager) {

        return nFormatter(manager.GWTeam.OverallRank, 1);
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

    self.FormatNames = function (player) {
        var html = "";
        var url = window.location.href;
        var selectedPlayerUrl = url.replace(window.location.pathname, '/points?entry=' + player.entry);
        var name = "";

        if (player.player_name == null) {
            name = player.player_first_name + " " + player.player_last_name;
        } else {
            name = player.player_name
        }

        html += "<a class=\"team-name\" href=" + selectedPlayerUrl + " target=\"_blank\"" + ">" + player.entry_name + "</a></br > <span class=\"manager-name\">" + name + "</span>"
        return html;
    }

    self.ViewTeam = function (data) {
        var playerId = data.entry;
        var url = window.location.href;
        var selectedPlayerUrl = url.replace(window.location.pathname, '/points?entry=' + playerId);

        window.open(selectedPlayerUrl);
    }

    self.GetCaptain = function (picks) {
        var captain = picks.filter(x => x.is_captain);

        if (captain.length == 1) {
            return captain[0].player.web_name;
        }
        else {
            return "No Captain";
        }
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

    self.IsAllGwGamesFinished = function (gwGames) {

        var isAllGwGamesFinished = gwGames.every(function (item) {
            return item.finished_provisional == true;
        });

        return isAllGwGamesFinished;
    }

    self.FireStartingSelectionModal = function (data) {
        var playerId = data.Pick.element;
        var managers = [];

        for (var i = 0; i < self.SelectedLeagueStandings().length; i++) {

            if (self.SelectedLeagueStandings()[i].GWTeam.picks.filter(x => x.element == playerId && x.multiplier > 0).length > 0) {
                managers.push(self.SelectedLeagueStandings()[i]);
            }
        }

        self.SelectedPlayerFromTally(data);
        self.ManagersAffiliatedWithSelectedPlayerFromTally(managers);

        $('.ui.starting-selection.modal').modal('show');
    }

    self.FireBenchSelectionModal = function (data) {
        var playerId = data.Pick.element;
        var managers = [];

        for (var i = 0; i < self.SelectedLeagueStandings().length; i++) {

            if (self.SelectedLeagueStandings()[i].GWTeam.picks.filter(x => x.element == playerId && x.multiplier == 0).length > 0) {
                managers.push(self.SelectedLeagueStandings()[i]);
            }
        }

        self.SelectedPlayerFromTally(data);
        self.ManagersAffiliatedWithSelectedPlayerFromTally(managers);

        $('.ui.bench-selection.modal').modal('show');
    }

    self.FireCaptainSelectionModal = function (data) {
        var playerId = data.Pick.element;
        var managers = [];

        for (var i = 0; i < self.SelectedLeagueStandings().length; i++) {

            if (self.SelectedLeagueStandings()[i].GWTeam.picks.filter(x => x.element == playerId && x.is_captain).length > 0) {
                managers.push(self.SelectedLeagueStandings()[i]);
            }
        }

        self.SelectedPlayerFromTally(data);
        self.ManagersAffiliatedWithSelectedPlayerFromTally(managers);

        $('.ui.captain-selection.modal').modal('show');
    }

    self.FireTransferredInSelectionModal = function (data) {
        var playerId = data.Pick.element;
        var managers = [];

        for (var i = 0; i < self.SelectedLeagueStandings().length; i++)
        {
            if (self.SelectedLeagueStandings()[i].GWTeam.picks.filter(x => x.element == playerId).length > 0)
            {
                for (var j = 0; j < self.AllGwTransfersInLeague().length; j++)
                {
                    if (self.AllGwTransfersInLeague()[j].element_in == playerId && self.AllGwTransfersInLeague()[j].entry == self.SelectedLeagueStandings()[i].entry) {
                        managers.push(self.SelectedLeagueStandings()[i]);
                    }
                }
            }
        }

        self.SelectedPlayerFromTally(data);
        self.ManagersAffiliatedWithSelectedPlayerFromTally(managers);

        $('.ui.transferred-in-selection.modal').modal('show');
    }

    self.FireTransferredOutSelectionModal = function (data) {
        var playerId = data.Pick.element;
        var managers = [];

        for (var i = 0; i < self.SelectedLeagueStandings().length; i++) {
            if (self.SelectedLeagueStandings()[i].GWTeam.picks.filter(x => x.element == playerId).length == 0) {
                for (var j = 0; j < self.AllGwTransfersInLeague().length; j++) {
                    if (self.AllGwTransfersInLeague()[j].element_out == playerId && self.AllGwTransfersInLeague()[j].entry == self.SelectedLeagueStandings()[i].entry) {
                        managers.push(self.SelectedLeagueStandings()[i]);
                    }
                }
            }
        }

        self.SelectedPlayerFromTally(data);
        self.ManagersAffiliatedWithSelectedPlayerFromTally(managers);

        $('.ui.transferred-out-selection.modal').modal('show');
    }

    self.viewPlayerGwBreakdown = function (player) {

        player = player.Pick;
        var gamesWithFplStats = player.GWPlayer.explain;
        var gwGames = player.GWGames;

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

                var gwGame = player.GWGames.filter(x => x.id == fixtureId);
                var bonus = player.GWPlayer.stats.EstimatedBonus[i];

                //add estimated bonus to game stats
                if (!gwGame[0].finished && gwGame[0].started && bonus > 0 && !gamesWithFplStats[i].stats.some(x => x.identifier == "bonus")) {
                    gamesWithFplStats[i].stats.push({ identifier: "bonus", value: bonus, points: bonus })
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
        $('.menu .item').tab({
            'onLoad': function (tabPath) {
                if (tabPath == "second") {
                    if ($.fn.dataTable.isDataTable(playerTallyTable)) {
                        // Sort by gw points and then re-draw
                        //needed to fix th column width bug
                        //https://datatables.net/forums/discussion/35868/scrollx-and-width-of-headers-problem
                        playerTallyTable.DataTable()
                            .order([8, 'desc'])
                            .draw();
                    }
                }
            }
        });

        //$('#league-dropdown').dropdown();

        ////self.SelectedLeague(self.ClassicLeagues()[4]);
        for (var i = 0; i < self.ClassicLeagues().length; i++) {
            if (self.ClassicLeagues()[i] = self.SelectedLeague()) {
                self.SelectedLeague(self.ClassicLeagues()[i]);
                break;
            }
        }

        if (!self.HideLeagueUI()) {
            self.ClassicLeagues().push({ name: "Get League By Id", id: -1, Standings: {results: []} });
            initialiseStandingsDatatable();
            initialiseTalliesDatatable();
            initialiseEntriesTable();
        }

        $("#th-bonus-points").attr('title', 'This is the hover-over text');

        $('#mobile-standings-last-updated').popup({
            popup: '#standings-last-updated-popup.popup',
            position: 'top center',
            hoverable: true
        });
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

    /* Formatting function for row details - modify as you need */
    function FormatChildRow(gwTeam) {

        var starters = gwTeam.picks.filter(x => x.position <= 11);
        var subs = gwTeam.picks.filter(x => x.position > 11);
        var autoSubs = gwTeam.automatic_subs;
        var gwTransfers = gwTeam.GWTransfers;

        var transfersHtml = CreateHtmlForTransfers(gwTransfers);
        var autoSubsHtml = CreateHtmlForAutoSubs(autoSubs, gwTeam);

        var browserW = $(window).width();

        if (browserW > 992) {
            var startersHtml = CreateHtmlForPicksDesktop(starters, autoSubs);
            var subHtml = CreateHtmlForPicksDesktop(subs, autoSubs);        

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
        } else {

            var startersHtml = CreateHtmlForPicksMobile(starters, autoSubs);
            var subHtml = CreateHtmlForPicksMobile(subs, autoSubs);

            return  '<div class="ui grid">' +
                        '<div class="three wide column">' +
                            '<h4 class="ui header" style="margin-bottom: 0!important;">Starters</h4>' +
                            '<div class="ui list" style="margin-top: 0.5rem!important;">' + startersHtml + '</div>' +
                            '</br>' +
                        '</div>' +
                        '<div class="three wide column">' +
                            '<h4 class="ui header" style="margin-bottom: 0!important;">Subs</h4>' +
                '<div class="ui list" style="margin-top: 0.5rem!important;">' + subHtml + '</div>' +
                autoSubsHtml +
                            '</br>' +
                        '</div>' +
                        '<div class="four wide column" style="width: 22% !important">' +
                            transfersHtml +
                        '</div>' +
                        //'<div class="four wide column" style="width: 22% !important">' +
                        //    autoSubsHtml +
                        //'</div>' +
                    '</div>';
        }

    }

    function CreateHtmlForTransfers(gwTransfers) {
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

        return transfersHtml;
    }

    function CreateHtmlForAutoSubs(autoSubs, gwTeam) {
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

        return autoSubsHtml;
    }

    function CreateHtmlForPicksDesktop(picks, autoSubs) {

        var html = "";

        for (var i = 0; i < picks.length; i++) {
            var icon = "";
            var subIcon = "";
            var subIcon2 = "";
            var subIcon3 = "";
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

            //var isAllGwGamesFinished = picks[i].GWGames.every(function (item) {
            //    return item.finished_provisional == true;
            //});

            var isAllGwGamesFinished = self.IsAllGwGamesFinished(picks[i].GWGames);

            if (isAllGwGamesFinished) {
                subIcon3 += "green bottom right corner check circle";
            }

            icon += " icon";
            subIcon += " icon";
            subIcon2 += " icon";
            subIcon3 += " icon";

            if (picks.length == 4) {
                icon += " disabled";
            }

            html +=
                '<div class="item">' +
                '<div class="content">' +
                '<h4 class="ui icon header truncate-text">' +
                '<i class="small icons">' +
                '  <i class="' + icon + '"></i>' +
                '  <i class="' + subIcon + '"></i>' +
            '  <i class="' + subIcon2 + '"></i>' +
            '  <i class="' + subIcon3 + '"></i>' +
                '</i>' +
                '</br>' +
                picks[i].player.web_name + captain +
                '</h5>' +
                '<p style="text-align: center" data-bind="click: $root.viewPlayerGwBreakdown.bind(' + picks[i] + ')">' + picks[i].GWPlayer.stats.gw_points + '</p>' +
                '</div>' +
                '</div>'
        }  

        return html;

    }

    function CreateHtmlForPicksMobile(picks, autoSubs) {
        var html = "";

        for (var i = 0; i < picks.length; i++) {

            var subIcon = "";
            var subIcon2 = "";
            var subIcon3 = "";
            var captain = "";

            if (picks[i].is_captain) {
                captain += "<i class='copyright icon' style='display: inline-block'></i>";
            }

            //for (var j = 0; j < autoSubs.length; j++) {
            //    if (autoSubs[j].element_in == picks[i].player.id || autoSubs[j].element_out == picks[i].player.id) {
            //        subIcon += "<i class='sync icon' style='display: inline-block'></i>";
            //    }
            //}

            //if (picks[i].player.status != "a") {
            //    //injured
            //    if (picks[i].player.status == "i") {
            //        subIcon2 += "<i class='sync circle icon' style='display: inline-block'></i>";
            //    }
            //    //doubtful
            //    else if (picks[i].player.status == "d") {
            //        subIcon2 += "<i class='help circle icon' style='display: inline-block'></i>";
            //    }
            //    //not available or suspended
            //    else if (picks[i].player.status == "n" || picks[i].player.status == "s") {
            //        subIcon2 += "<i class='warning circle icon' style='display: inline-block'></i>";
            //    }
            //    //unavailable - left the club
            //    else if (picks[i].player.status == "u") {
            //        subIcon2 += "<i class='warning sign icon' style='display: inline-block'></i>";
            //    }
            //}


            var isAllGwGamesFinished = self.IsAllGwGamesFinished(picks[i].GWGames);

            if (isAllGwGamesFinished) {
                subIcon3 += "<i class='green check circle icon' style='display: inline-block'></i>";
            }

            var name = truncateText(picks[i].player.web_name, 12);

            html +=
                '<div class="item">' +

            name +
            ' (' + picks[i].GWPlayer.stats.gw_points + ')' +
            captain +
            //subIcon +
            //subIcon2 +
            subIcon3 +
                '</div>'

        }

        return html;

    }

    function truncateText(string, cutoff) {
        if (string.length > cutoff) {
            return string.substring(0, cutoff) + '...';
        }
        return string;
    }

    function initialiseStandingsDatatable() {
        // Size of browser viewport.
        var browserH = $(window).height();
        var browserW = $(window).width();

        if (browserW > 992) {
            $(document).ready(function () {
                var table = standingsTable.DataTable({
                    columnDefs: [
                        { orderable: false, targets: "no-sort" }
                    ],
                    order: [[$('th.default-sort').index(), "asc"]],
                    responsive: true,
                    fixedHeader: true,
                    dom:
                        "<'ui unstackable grid'" +
                        "<'row p-b-0-4'" +
                        "<'five wide column'l>" +
                        "<'right floated five wide column'f>" +
                        ">" +
                        "<'row dt-table padding-top-zero'" +
                        "<'sixteen wide column'tr>" +
                        ">" +
                        "<'row padding-top-zero'" +
                        "<'five wide column'i>" +
                        "<'right aligned eleven wide column'p>" +
                        ">" +
                        ">",
                    columnDefs: [
                        {
                            type: "sort-numbers-with-letters", targets: "special-sort-1"
                        },
                        {
                            orderable: false, targets: "no-sort"
                        }
                    ],
                    //scrollX: true,
                    //scrollY: true
                });

            });
        }
        else {
            $(document).ready(function () {
                var table = standingsTable.DataTable({
                    columnDefs: [
                        { orderable: false, targets: "no-sort" }
                    ],
                    order: [[$('th.default-sort').index(), "asc"]],
                    responsive: true,
                    scrollX: true,
                    scrollY: true,
                    searching: true,
                    dom:
                        "<'ui unstackable grid'" +//1
                        "<'row'" +//2
                        "<'five wide column'l>" +
                        //"<'#gw-player-table-heading-mobile.six wide center aligned column ui header'>" +
                        "<'right floated six wide column'f>" +
                        ">" +//2
                        "<'row dt-table padding-top-zero'" +//3
                        "<'sixteen wide column'tr>" +
                        ">" +//3
                        //"<'row'" +
                        "<'row padding-top-zero'" +//4
                        "<'sixteen wide column'i>" +
                        "<'sixteen wide column'p>" +
                        ">" +//4
                        //">" +
                        ">",//1
                    language: {
                        search: '',
                        searchPlaceholder: 'Search',
                        lengthMenu: "_MENU_"
                    },
                    pagingType: "simple",
                    columnDefs: [
                        {
                            type: "sort-numbers-with-letters", targets: "special-sort-1"
                        },
                        {
                            orderable: false, targets: "no-sort"
                        }
                    ],

                });

            });
        }

    }

    function initialiseTalliesDatatable() {
	    // Size of browser viewport.
	    var browserH = $(window).height();
	    var browserW = $(window).width();

	    if (browserW > 992) {
		    $(document).ready(function () {
			    var table = playerTallyTable.DataTable({
				    fixedColumns: {
					    leftColumns: 1
				    },
				    order: [[$('#player-tally-table th.default-sort').index(), "desc"]],
				    columnDefs: [
					    { orderable: false, targets: "no-sort" }
				    ],
				    responsive: true,
                    scrollX: true,
                    dom:
                        "<'ui unstackable grid'" +
                        "<'row p-b-0-4'" +
                        "<'five wide column'l>" +
                        "<'right floated five wide column'f>" +
                        ">" +
                        "<'row dt-table padding-top-zero'" +
                        "<'sixteen wide column'tr>" +
                        ">" +
                        "<'row padding-top-zero'" +
                        "<'five wide column'i>" +
                        "<'right aligned eleven wide column'p>" +
                        ">" +
                        ">",
				    //scrollY: true
			    });

		    });
	    } else {
		    $(document).ready(function () {
			    var table = playerTallyTable.DataTable({
				    fixedColumns: {
					    leftColumns: 1
				    },
				    order: [[$('#player-tally-table th.default-sort').index(), "desc"]],
				    columnDefs: [
					    { orderable: false, targets: "no-sort" }
				    ],
				    responsive: true,
				    scrollX: true,
                    dom:
                        "<'ui unstackable grid'" +//1
                        "<'row'" +//2
                        "<'five wide column'l>" +
                        //"<'#gw-player-table-heading-mobile.six wide center aligned column ui header'>" +
                        "<'right floated six wide column'f>" +
                        ">" +//2
                        "<'row dt-table padding-top-zero'" +//3
                        "<'sixteen wide column'tr>" +
                        ">" +//3
                        //"<'row'" +
                        "<'row padding-top-zero'" +//4
                        "<'sixteen wide column'i>" +
                        "<'sixteen wide column'p>" +
                        ">" +//4
                        //">" +
                        ">",//1
                    language: {
                        search: '',
                        searchPlaceholder: 'Search',
                        lengthMenu: "_MENU_"
                    },
                    pagingType: "simple"
			    });

		    });
	    }
    }

    function initialiseEntriesTable() {
        $(document).ready(function () {
            var table = entriesTable.DataTable();
        });
    }

    // Accepts a Date object or date string that is recognized by the Date.parse() method
    function getDayOfWeek(date) {
        const dayOfWeek = new Date(date).getDay();
        return isNaN(dayOfWeek) ? null :
            ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'][dayOfWeek];
    }

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

//document.onreadystatechange = function (e) {
//    $('#loader').show();
//    $('.main-grid').hide();
//    if (document.readyState === 'complete') {
//        //dom is ready, window.onload fires later

//    }
//};
//window.onload = function (e) {
//    //document.readyState will be complete, it's one of the requirements for the window.onload event to be fired
//    //do stuff for when everything is loaded
//    $('#loader').hide();
//    $('.main-grid').show();
//};

