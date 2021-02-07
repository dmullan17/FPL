﻿using FPL.Attributes;
using FPL.Contracts;
using FPL.Http;
using FPL.Models;
using FPL.Models.FPL;
using FPL.ViewModels.FPL;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FPL.Controllers
{
    [FPLCookie]
    public class LeaguesController : BaseController
    {
        private static int teamId;

        public LeaguesController(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new LeaguesViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            if (Request.Cookies["teamId"] == null)
            {
                teamId = await GetTeamId();
            }
            else
            {
                teamId = Convert.ToInt32(Request.Cookies["teamId"]);
            }

            var response = await _httpClient.GetAuthAsync(CreateHandler(handler), $"entry/{teamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var leaguesJSON = JObject.Parse(content);

            JObject leaguesObject = (JObject)leaguesJSON["leagues"];
            Leagues leagues = leaguesObject.ToObject<Leagues>();
            EventStatus eventStatus = await GetEventStatus();

            leagues = await AddBasicInfoToPrivateLeagues(leagues, eventStatus);
            //leagues = await AddPlayerStandingsToLeague(leagues);

            viewModel.SelectedLeague = leagues.classic.FindAll(x => x.league_type == "x").OrderBy(i => i.PlayerCount).First();
            viewModel.IsEventLive = IsEventLive(eventStatus); 
            viewModel.ClassicLeagues = leagues.classic;
            viewModel.H2HLeagues = leagues.h2h;
            viewModel.Cup = leagues.cup;
            viewModel.CurrentGwId = await GetCurrentGameWeekId();
            viewModel.TeamId = teamId;

            return View(viewModel);
        }

        [HttpGet]
        [Route("leagues/{id}")]
        public async Task<IActionResult> Index(int id)
        {
            var viewModel = new LeaguesViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            if (Request.Cookies["teamId"] == null)
            {
                teamId = await GetTeamId();
            }
            else
            {
                teamId = Convert.ToInt32(Request.Cookies["teamId"]);
            }

            var response = await _httpClient.GetAuthAsync(CreateHandler(handler), $"entry/{teamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var leaguesJSON = JObject.Parse(content);

            JObject leaguesObject = (JObject)leaguesJSON["leagues"];
            Leagues leagues = leaguesObject.ToObject<Leagues>();
            EventStatus eventStatus = await GetEventStatus();

            leagues = await AddBasicInfoToPrivateLeagues(leagues, eventStatus);
            Classic selectedLeague = await GetPlayerStandingsForClassicLeague(id);
            //leagues = await AddPlayerStandingsToLeague(leagues);

            viewModel.SelectedLeague = selectedLeague;
            viewModel.IsEventLive = IsEventLive(eventStatus);
            viewModel.ClassicLeagues = leagues.classic;
            viewModel.H2HLeagues = leagues.h2h;
            viewModel.Cup = leagues.cup;
            viewModel.CurrentGwId = await GetCurrentGameWeekId();

            return View(viewModel);
        }

        private bool IsEventLive(EventStatus eventStatus)
        {
            if (eventStatus.leagues != "Updated")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Leagues> AddBasicInfoToPrivateLeagues(Leagues leagues, EventStatus eventStatus)
        {
            var privateClassicLeagues = leagues.classic.FindAll(x => x.league_type == "x");

            var currentGameWeekId = await GetCurrentGameWeekId();

            var PointsController = new PointsController(_httpClient);

            foreach (var l in privateClassicLeagues)
            {
                var response = await _httpClient.GetAsync($"leagues-classic/{l.id}/standings");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var leagueJSON = JObject.Parse(content);

                JObject leagueStandingsObject = (JObject)leagueJSON["standings"];

                var leaguePlayersJSON = AllChildren(leagueStandingsObject)
                    .First(c => c.Type == JTokenType.Array && c.Path.Contains("results"))
                    .Children<JObject>();

                foreach (JObject result in leaguePlayersJSON)
                {
                    Result r = result.ToObject<Result>();
                    l.Standings.results.Add(r);
                }

                l.PlayerCount = l.Standings.results.Count();
            }

            var smallestLeague = leagues.classic.FindAll(x => x.league_type == "x").OrderBy(i => i.PlayerCount).First();

            foreach (var player in smallestLeague.Standings.results)
            {

                GWTeam gwTeam = new GWTeam();
                gwTeam = await PointsController.PopulateGwTeam(gwTeam, currentGameWeekId, player.entry);
                gwTeam = await PointsController.AddPlayerSummaryDataToTeam(gwTeam, player.entry, currentGameWeekId);
                gwTeam.picks = await PointsController.AddPlayerGameweekDataToTeam(gwTeam.picks, currentGameWeekId);
                gwTeam = await PointsController.AddAutoSubs(gwTeam, gwTeam.picks, player.entry);
                //gwTeam.EntryHistory = await PointsController.AddExtraDatatoEntryHistory(gwTeam.EntryHistory);
                player.CompleteEntryHistory = await PointsController.GetCompleteEntryHistory(player.CompleteEntryHistory, player.entry);
                player.Last5GwPoints = player.CompleteEntryHistory.GetLast5GwPoints();
                int gwpoints = PointsController.GetGameWeekPoints(gwTeam.picks, eventStatus);

                var captain = gwTeam.picks.Find(x => x.is_captain);
                smallestLeague.Captains.Add(captain);

                foreach (var p in gwTeam.picks)
                {
                    smallestLeague.Players.Add(p);
                }

                player.PlayersYetToPlay = gwTeam.picks.FindAll(x => x.GWPlayer.stats.minutes == 0 && x.multiplier > 0 && x.GWGames.Any(y => y.kickoff_time != null && !y.finished_provisional)).Count();
                player.total += (gwpoints - player.event_total);
                player.event_total += (gwpoints - player.event_total);
                player.GWTeam = gwTeam;

            }

            var standingsByLivePointsTotal = smallestLeague.Standings.results.OrderByDescending(x => x.total).ToList();

            foreach (var player in smallestLeague.Standings.results)
            {
                player.rank = standingsByLivePointsTotal.IndexOf(player) + 1;
            }

            foreach (var captain in smallestLeague.Captains)
            {
                if (!smallestLeague.CaptainsTally.Any(x => x.Name == captain.player.web_name))
                {
                    var count = smallestLeague.Captains.FindAll(x => x.element == captain.element).Count();
                    var pt = new PlayerTally()
                    {
                        Name = captain.player.web_name,
                        Count = count
                    };

                    smallestLeague.CaptainsTally.Add(pt);
                }
            }

            foreach (var player in smallestLeague.Players)
            {
                if (!smallestLeague.PlayersTally.Any(x => x.Name == player.player.web_name))
                {
                    var count = smallestLeague.Players.FindAll(x => x.element == player.element).Count();
                    var pt = new PlayerTally()
                    {
                        Name = player.player.web_name,
                        Count = count
                    };

                    smallestLeague.PlayersTally.Add(pt);
                }
            }

            smallestLeague.CaptainsTally = smallestLeague.CaptainsTally.OrderByDescending(x => x.Count).ToList();
            smallestLeague.PlayersTally = smallestLeague.PlayersTally.OrderByDescending(x => x.Count).ToList();

            return leagues;
        }

        public async Task<Classic> GetPlayerStandingsForClassicLeague(int leagueId)
        {
            Classic l = new Classic();

            var currentGameWeekId = await GetCurrentGameWeekId();

            var eventStatus = await GetEventStatus();

            var PointsController = new PointsController(_httpClient);

            var response = await _httpClient.GetAsync($"leagues-classic/{leagueId}/standings");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var leagueJSON = JObject.Parse(content);

            var leagueDetailsJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Object && c.Path.Contains("league"));

            l = leagueDetailsJSON.ToObject<Classic>();

            JObject leagueStandingsObject = (JObject)leagueJSON["standings"];

            var leaguePlayersJSON = AllChildren(leagueStandingsObject)
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("results"))
                .Children<JObject>();

            foreach (JObject result in leaguePlayersJSON)
            {
                Result r = result.ToObject<Result>();
                l.Standings.results.Add(r);
            }

            foreach (var player in l.Standings.results)
            {

                GWTeam gwTeam = new GWTeam();
                gwTeam = await PointsController.PopulateGwTeam(gwTeam, currentGameWeekId, player.entry);
                gwTeam = await PointsController.AddPlayerSummaryDataToTeam(gwTeam, player.entry, currentGameWeekId);
                gwTeam.picks = await PointsController.AddPlayerGameweekDataToTeam(gwTeam.picks, currentGameWeekId);
                gwTeam = await PointsController.AddAutoSubs(gwTeam, gwTeam.picks, player.entry);
                //gwTeam.EntryHistory = await PointsController.AddExtraDatatoEntryHistory(gwTeam.EntryHistory);
                player.CompleteEntryHistory = await PointsController.GetCompleteEntryHistory(player.CompleteEntryHistory, player.entry);
                player.Last5GwPoints = player.CompleteEntryHistory.GetLast5GwPoints();
                int gwpoints = PointsController.GetGameWeekPoints(gwTeam.picks, eventStatus);

                var captain = gwTeam.picks.Find(x => x.is_captain);
                l.Captains.Add(captain);

                foreach (var p in gwTeam.picks)
                {
                    l.Players.Add(p);
                }

                player.PlayersYetToPlay = gwTeam.picks.FindAll(x => x.GWPlayer.stats.minutes == 0 && x.multiplier > 0 && x.GWGames.Any(x => x.kickoff_time != null && !x.finished)).Count();
                player.total += (gwpoints - player.event_total);
                player.event_total += (gwpoints - player.event_total);
                player.GWTeam = gwTeam;

            }

            var standingsByLivePointsTotal = l.Standings.results.OrderByDescending(x => x.total).ToList();

            foreach (var player in l.Standings.results)
            {
                player.rank = standingsByLivePointsTotal.IndexOf(player) + 1;
            }

            foreach (var captain in l.Captains)
            {
                if (!l.CaptainsTally.Any(x => x.Name == captain.player.web_name))
                {
                    var count = l.Captains.FindAll(x => x.element == captain.element).Count();
                    var pt = new PlayerTally()
                    {
                        Name = captain.player.web_name,
                        Count = count
                    };

                    l.CaptainsTally.Add(pt);
                }
            }

            foreach (var player in l.Players)
            {
                if (!l.PlayersTally.Any(x => x.Name == player.player.web_name))
                {
                    var count = l.Players.FindAll(x => x.element == player.element).Count();
                    var pt = new PlayerTally()
                    {
                        Name = player.player.web_name,
                        Count = count
                    };

                    l.PlayersTally.Add(pt);
                }
            }


            l.CaptainsTally = l.CaptainsTally.OrderByDescending(x => x.Count).ToList();
            l.PlayersTally = l.PlayersTally.OrderByDescending(x => x.Count).ToList();

            return l;
        }

        public async Task<GWTeam> GetPlayersTeam(int teamId, int currentGameWeekId)
        {
            GWTeam gwTeam = new GWTeam();
            EventStatus eventStatus = await GetEventStatus();
            var PointsController = new PointsController(_httpClient);

            gwTeam = await PointsController.PopulateGwTeam(gwTeam, currentGameWeekId, teamId);
            gwTeam = await PointsController.AddPlayerSummaryDataToTeam(gwTeam, teamId, currentGameWeekId);
            gwTeam.picks = await PointsController.AddPlayerGameweekDataToTeam(gwTeam.picks, currentGameWeekId);
            gwTeam = await PointsController.AddAutoSubs(gwTeam, gwTeam.picks, teamId);
            //gwTeam.picks = await PointsController.AddPlayerSummaryDataToTeam(gwTeam.picks, teamId);
            gwTeam.picks = PointsController.AddEstimatedBonusToTeamPicks(gwTeam.picks, eventStatus);

            return gwTeam;
        }

    }
}
