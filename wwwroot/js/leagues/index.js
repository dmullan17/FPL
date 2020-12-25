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

    self.viewPlayer = function (player) {
        self.SelectedPlayer(player);
        $('.ui.modal').modal().modal('show');
    };

    self.changeLeague = function (league) {
        self.SelectedLeague(league);

    };
   
    self.init = function () {
        //$('.menu .item').tab();
        self.SelectedLeague(self.ClassicLeagues()[3]);
    };

    self.init();

};