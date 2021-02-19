using FPL.Attributes;
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
            var gameweekId = await GetCurrentGameWeekId();
            var gwGames = await GetGwFixtures(gameweekId);

            viewModel.SelectedLeague = leagues.classic.FindAll(x => x.league_type == "x").OrderBy(i => i.PlayerCount).First();
            viewModel.SelectedLeague.UserTeam = viewModel.SelectedLeague.Standings.results.Find(x => x.entry == teamId);
            viewModel.IsEventLive = IsEventLive(eventStatus);
            viewModel.IsGameLive = IsGameLive(eventStatus);
            viewModel.ClassicLeagues = leagues.classic;
            viewModel.H2HLeagues = leagues.h2h;
            viewModel.Cup = leagues.cup;
            viewModel.CurrentGwId = gameweekId;
            viewModel.TeamId = teamId;
            viewModel.EventStatus = eventStatus;
            viewModel.LastUpdated = GetLastTimeLeagueWasUpdated(gwGames);

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
            var gameweekId = await GetCurrentGameWeekId();
            var gwGames = await GetGwFixtures(gameweekId);

            viewModel.SelectedLeague = selectedLeague;
            viewModel.IsEventLive = IsEventLive(eventStatus);
            viewModel.ClassicLeagues = leagues.classic;
            viewModel.H2HLeagues = leagues.h2h;
            viewModel.Cup = leagues.cup;
            viewModel.CurrentGwId = gameweekId;
            viewModel.TeamId = teamId;
            viewModel.LastUpdated = GetLastTimeLeagueWasUpdated(gwGames);

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

        private DateTime GetLastTimeLeagueWasUpdated(List<Game> gwGames)
        {
            DateTime lastUpdate = new DateTime();

            var finishedGames = gwGames.FindAll(x => x.finished_provisional).ToList();
            var lastFinishedGame = finishedGames.LastOrDefault();
            lastUpdate = lastFinishedGame.kickoff_time.GetValueOrDefault().AddMinutes(110);

            return lastUpdate;
        }

        private bool IsGameLive(EventStatus eventStatus)
        {
            if (eventStatus.status.Any(x => x.points == "l"))
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
            int leagueCount = Convert.ToInt32(smallestLeague.Standings.results.Count);
            List<Pick> players = new List<Pick>();

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

                foreach (var p in gwTeam.picks)
                {
                    players.Add(p);
                    CalculatePlayersYetToPlay(player, p);
                }

                player.total += (gwpoints - player.event_total);
                player.event_total += (gwpoints - player.event_total);
                int topOfLeaguePoints = smallestLeague.Standings.results.FirstOrDefault().total;
                player.PointsFromFirst = topOfLeaguePoints - player.total;
                player.GWTeam = gwTeam;

            }

            var standingsByLivePointsTotal = smallestLeague.Standings.results.OrderByDescending(x => x.total).ToList();

            foreach (var player in smallestLeague.Standings.results)
            {
                player.rank = standingsByLivePointsTotal.IndexOf(player) + 1;
            }

            foreach (var player in players)
            {
                if (!smallestLeague.PlayersTally.Any(x => x.Pick.element == player.element))
                {
                    var count = players.FindAll(x => x.element == player.element).Count();
                    var ownership = ((double)count / (double)leagueCount).ToString("0%");

                    var startingCount = players.FindAll(x => x.element == player.element && x.multiplier > 0).Count();
                    var startingOwnership = ((double)startingCount / (double)leagueCount).ToString("0%");

                    int captainCount = players.FindAll(x => x.element == player.element && x.is_captain).Count();
                    var captainSelection = ((double)captainCount / (double)leagueCount).ToString("0%");

                    var pt = new PlayerTally()
                    {
                        Pick = player,
                        Count = count,
                        Ownership = ownership,
                        StartingOwnership = startingOwnership,
                        CaptainSelection = captainSelection
                    };

                    smallestLeague.PlayersTally.Add(pt);
                }
            }

            smallestLeague.PlayersTally = smallestLeague.PlayersTally.ToList();

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

            int leagueCount = Convert.ToInt32(l.Standings.results.Count);
            List<Pick> players = new List<Pick>();

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

                foreach (var p in gwTeam.picks)
                {
                    players.Add(p);
                    CalculatePlayersYetToPlay(player, p);
                }

                player.total += (gwpoints - player.event_total);
                player.event_total += (gwpoints - player.event_total);
                int topOfLeaguePoints = l.Standings.results.FirstOrDefault().total;
                player.PointsFromFirst = topOfLeaguePoints - player.total;
                player.GWTeam = gwTeam;

            }

            var standingsByLivePointsTotal = l.Standings.results.OrderByDescending(x => x.total).ToList();

            foreach (var player in l.Standings.results)
            {
                player.rank = standingsByLivePointsTotal.IndexOf(player) + 1;
            }

            foreach (var player in players)
            {
                if (!l.PlayersTally.Any(x => x.Pick.element == player.element))
                {
                    var count = players.FindAll(x => x.element == player.element).Count();
                    var ownership = ((double)count / (double)leagueCount).ToString("0%");

                    var startingCount = players.FindAll(x => x.element == player.element && x.multiplier > 0).Count();
                    var startingOwnership = ((double)startingCount / (double)leagueCount).ToString("0%");

                    int captainCount = players.FindAll(x => x.element == player.element && x.is_captain).Count();
                    var captainSelection = ((double)captainCount / (double)leagueCount).ToString("0%");

                    var pt = new PlayerTally()
                    {
                        Pick = player,
                        Count = count,
                        Ownership = ownership,
                        StartingOwnership = startingOwnership,
                        CaptainSelection = captainSelection
                    };

                    l.PlayersTally.Add(pt);
                }
            }

            l.UserTeam = l.Standings.results.Find(x => x.entry == teamId);
            l.PlayersTally = l.PlayersTally.ToList();

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

        public void CalculatePlayersYetToPlay(Result player, Pick p)
        {
            if (p.multiplier > 0 && p.GWGames.Any(x => x.kickoff_time != null && !x.finished_provisional))
            {
                for (var i = 0; i < p.GWPlayer.explain.Count; i++)
                {
                    for (var j = 0; j < p.GWPlayer.explain[i].stats.Count; j++)
                    {
                        var g = p.GWGames.Find(x => x.id == p.GWPlayer.explain[i].fixture);

                        if (p.GWPlayer.explain[i].stats[j].identifier == "minutes" && p.GWPlayer.explain[i].stats[j].value == 0)
                        {
                            if (!g.started ?? true)
                            {
                                player.PlayersYetToPlay += 1;
                            }
                        }
                    }
                }
            }
        }

    }
}
