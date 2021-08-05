using FPL.Attributes;
using FPL.Contracts;
using FPL.Http;
using FPL.Models;
using FPL.Models.FPL;
using FPL.Models.GWPlayerStats;
using FPL.ViewModels.FPL;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Team = FPL.Models.Team;

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
            ViewData["Title"] = "My Leagues";

            List<Player> allPlayers = await GetAllPlayers();
            List<Team> allTeams = await GetAllTeams();
            List<Game> allGames = await GetAllGames();
            var currentGameweekId = await GetCurrentGameWeekId();
            List<GWPlayer> allGwPlayers = await GetAllGwPlayers(currentGameweekId);
            EventStatus eventStatus = await GetEventStatus();
            List<Game> gwGames = await GetGwGames(currentGameweekId);

            var viewModel = new LeaguesViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            if (Request.Cookies["teamId"] == null) teamId = await GetTeamId();
            else teamId = Convert.ToInt32(Request.Cookies["teamId"]);

            var response = await _httpClient.GetAuthAsync(CreateHandler(handler), $"entry/{teamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var leaguesJSON = JObject.Parse(content);

            JObject leaguesObject = (JObject)leaguesJSON["leagues"];
            Leagues leagues = leaguesObject.ToObject<Leagues>();

            try
            {
                leagues = await AddBasicInfoToPrivateLeagues(gwGames, allPlayers, allTeams, allGames, allGwPlayers, leagues, eventStatus, currentGameweekId);
            }
            catch (Exception e)
            {
                return NotFound();
            }
            //leagues = await AddBasicInfoToPrivateLeagues(gwGames, allPlayers, allTeams, allGames, allGwPlayers, leagues, eventStatus, currentGameweekId);
            //var gwGames = await GetGwFixtures(currentGameweekId);

            viewModel.SelectedLeague = leagues.classic.FindAll(x => x.league_type == "x").OrderBy(i => i.PlayerCount).First();
            viewModel.SelectedLeague.UserTeam = viewModel.SelectedLeague.Standings.results.Find(x => x.entry == teamId);
            viewModel.IsEventLive = IsEventLive(eventStatus);
            viewModel.IsGameLive = IsGameLive(eventStatus);
            viewModel.ClassicLeagues = leagues.classic;
            viewModel.H2HLeagues = leagues.h2h;
            viewModel.Cup = leagues.cup;
            viewModel.CurrentGwId = currentGameweekId;
            viewModel.TeamId = teamId;
            viewModel.EventStatus = eventStatus;
            viewModel.LastUpdated = GetLastTimeLeagueWasUpdated(gwGames);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Classic()
        {
            List<Player> allPlayers = await GetAllPlayers();
            List<Team> allTeams = await GetAllTeams();
            List<Game> allGames = await GetAllGames();
            var currentGameweekId = await GetCurrentGameWeekId();
            List<GWPlayer> allGwPlayers = await GetAllGwPlayers(currentGameweekId);
            EventStatus eventStatus = await GetEventStatus();
            List<Game> gwGames = await GetGwGames(currentGameweekId);

            var viewModel = new LeaguesViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            if (Request.Cookies["teamId"] == null) teamId = await GetTeamId();
            else teamId = Convert.ToInt32(Request.Cookies["teamId"]);

            var response = await _httpClient.GetAuthAsync(CreateHandler(handler), $"entry/{teamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var leaguesJSON = JObject.Parse(content);

            JObject leaguesObject = (JObject)leaguesJSON["leagues"];
            Leagues leagues = leaguesObject.ToObject<Leagues>();

            try
            {
                leagues = await AddBasicInfoToPrivateLeagues(gwGames, allPlayers, allTeams, allGames, allGwPlayers, leagues, eventStatus, currentGameweekId);
            }
            catch (Exception e)
            {

            }
            //leagues = await AddBasicInfoToPrivateLeagues(gwGames, allPlayers, allTeams, allGames, allGwPlayers, leagues, eventStatus, currentGameweekId);
            //var gwGames = await GetGwFixtures(currentGameweekId);

            viewModel.SelectedLeague = leagues.classic.FindAll(x => x.league_type == "x").OrderBy(i => i.PlayerCount).First();
            viewModel.SelectedLeague.UserTeam = viewModel.SelectedLeague.Standings.results.Find(x => x.entry == teamId);
            viewModel.IsEventLive = IsEventLive(eventStatus);
            viewModel.IsGameLive = IsGameLive(eventStatus);
            viewModel.ClassicLeagues = leagues.classic;
            viewModel.H2HLeagues = leagues.h2h;
            viewModel.Cup = leagues.cup;
            viewModel.CurrentGwId = currentGameweekId;
            viewModel.TeamId = teamId;
            viewModel.EventStatus = eventStatus;
            viewModel.LastUpdated = GetLastTimeLeagueWasUpdated(gwGames);

            return View(viewModel);
        }

        [HttpGet]
        [Route("leagues/{id}")]
        public async Task<IActionResult> Index(int id)
        {
            List<Player> allPlayers = await GetAllPlayers();
            List<Team> allTeams = await GetAllTeams();
            List<Game> allGames = await GetAllGames();
            var currentGameweekId = await GetCurrentGameWeekId();
            List<GWPlayer> allGwPlayers = await GetAllGwPlayers(currentGameweekId);
            EventStatus eventStatus = await GetEventStatus();
            List<Game> gwGames = await GetGwGames(currentGameweekId);

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

            leagues = await AddBasicInfoToPrivateLeagues(gwGames, allPlayers, allTeams, allGames, allGwPlayers, leagues, eventStatus, currentGameweekId);
            Classic selectedLeague = await GetPlayerStandingsForClassicLeague(id, currentGameweekId);
            //var gwGames = await GetGwFixtures(currentGameweekId);

            viewModel.SelectedLeague = selectedLeague;
            viewModel.IsEventLive = IsEventLive(eventStatus);
            viewModel.ClassicLeagues = leagues.classic;
            viewModel.H2HLeagues = leagues.h2h;
            viewModel.Cup = leagues.cup;
            viewModel.CurrentGwId = currentGameweekId;
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
            
            if (finishedGames.Count > 0)
            {
                var lastFinishedGame = finishedGames.LastOrDefault();
                lastUpdate = lastFinishedGame.kickoff_time.GetValueOrDefault().AddMinutes(110);
            }

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

        public async Task<Leagues> AddBasicInfoToPrivateLeagues(List<Game> gwGames, List<Player> allPlayers, List<Team> allTeams, List<Game> allGames, List<GWPlayer> allGwPlayers, Leagues leagues, EventStatus eventStatus, int gameweekId)
        {
            var privateClassicLeagues = leagues.classic.FindAll(x => x.league_type == "x");

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

            var smallestLeague = leagues.classic.FindAll(x => x.league_type == "x").OrderBy(i => i.PlayerCount).FirstOrDefault();

            if (smallestLeague == null)
            {
                //return;
            }

            int leagueCount = Convert.ToInt32(smallestLeague.Standings.results.Count);
            List<Pick> players = new List<Pick>();
            List<Transfer> allGwTransfers = new List<Transfer>();

            foreach (var player in smallestLeague.Standings.results)
            {
                GWTeam gwTeam = new GWTeam();
                gwTeam = await PointsController.PopulateGwTeam(gwTeam, gameweekId, player.entry);
                gwTeam = PointsController.AddPlayerSummaryDataToTeam(allPlayers, allTeams, allGames, gwTeam, player.entry, gameweekId);
                gwTeam = await PointsController.AddTransfersToGwTeam(allPlayers, gwTeam, player.entry, gameweekId);
                gwTeam.picks = PointsController.AddPlayerGameweekDataToTeam(gwGames, allGwPlayers, gwTeam.picks, gameweekId);
                gwTeam = PointsController.AddAutoSubs(gwTeam, gwTeam.picks, player.entry, eventStatus, gameweekId);
                player.CompleteEntryHistory = await PointsController.GetCompleteEntryHistory(player.CompleteEntryHistory, player.entry);
                gwTeam.picks = PointsController.AddEstimatedBonusToTeamPicks(gwTeam.picks, eventStatus);
                var teamDetails = await PointsController.GetTeamInfo(player.entry);

                foreach (var transfer in gwTeam.GWTransfers)
                {
                    allGwTransfers.Add(transfer);
                }

                foreach (var p in gwTeam.picks)
                {
                    players.Add(p);
                    CalculatePlayersYetToPlay(gwTeam, p);
                }

                int gwpoints = PointsController.GetGameWeekPoints(gwTeam.picks, eventStatus);
                player.Last5GwPoints = player.CompleteEntryHistory.GetLast5GwPoints();
                //player.total += (gwpoints - player.event_total);
                player.total = ((int)teamDetails.summary_overall_points - (int)teamDetails.summary_event_points) + gwpoints;
                player.event_total = gwpoints;
                //player.event_total += (gwpoints - player.event_total);
                player.GWTeam = gwTeam;

            }

            CalculateRankAndPFF(smallestLeague);
            CalculatePlayersTallyForLeague(smallestLeague, players, leagueCount);

            smallestLeague.PlayersTally = smallestLeague.PlayersTally.ToList();
            smallestLeague.AllGwTransfers = allGwTransfers;

            return leagues;
        }

        public async Task<Classic> GetPlayerStandingsForClassicLeague(int leagueId, int gameweekId)
        {
            List<Player> allPlayers = await GetAllPlayers();
            List<Team> allTeams = await GetAllTeams();
            List<Game> allGames = await GetAllGames();
            EventStatus eventStatus = await GetEventStatus();
            List<GWPlayer> allGwPlayers = await GetAllGwPlayers(gameweekId);
            List<Game> gwGames = await GetGwGames(gameweekId);

            Classic l = new Classic();

            var PointsController = new PointsController(_httpClient);

            var response = await _httpClient.GetAsync($"leagues-classic/{leagueId}/standings");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var leagueJSON = JObject.Parse(content);

            var leagueDetailsJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Object && c.Path.Contains("league"));

            l = leagueDetailsJSON.ToObject<Classic>();

            JObject leagueStandingsObject = (JObject)leagueJSON["standings"];

            l.Standings.has_next = (bool)leagueStandingsObject["has_next"];
            l.Standings.page = (int)leagueStandingsObject["page"];

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
            List<Transfer> allGwTransfers = new List<Transfer>();

            foreach (var player in l.Standings.results)
            {
                GWTeam gwTeam = new GWTeam();
                gwTeam = await PointsController.PopulateGwTeam(gwTeam, gameweekId, player.entry);
                gwTeam = PointsController.AddPlayerSummaryDataToTeam(allPlayers, allTeams, allGames, gwTeam, player.entry, gameweekId);
                gwTeam = await PointsController.AddTransfersToGwTeam(allPlayers, gwTeam, player.entry, gameweekId);
                gwTeam.picks = PointsController.AddPlayerGameweekDataToTeam(gwGames, allGwPlayers, gwTeam.picks, gameweekId);
                gwTeam = PointsController.AddAutoSubs(gwTeam, gwTeam.picks, player.entry, eventStatus, gameweekId);
                player.CompleteEntryHistory = await PointsController.GetCompleteEntryHistory(player.CompleteEntryHistory, player.entry);
                gwTeam.picks = PointsController.AddEstimatedBonusToTeamPicks(gwTeam.picks, eventStatus);
                var teamDetails = await PointsController.GetTeamInfo(player.entry);

                foreach (var p in gwTeam.picks)
                {
                    players.Add(p);
                    CalculatePlayersYetToPlay(gwTeam, p);
                }

                foreach (var transfer in gwTeam.GWTransfers)
                {
                    allGwTransfers.Add(transfer);
                }

                int gwpoints = PointsController.GetGameWeekPoints(gwTeam.picks, eventStatus);
                player.Last5GwPoints = player.CompleteEntryHistory.GetLast5GwPoints();
                //player.total += (gwpoints - player.event_total);
                //player.event_total += (gwpoints - player.event_total);
                player.total = ((int)teamDetails.summary_overall_points - (int)teamDetails.summary_event_points) + gwpoints;
                player.event_total = gwpoints;
                player.GWTeam = gwTeam;

            }

            CalculateRankAndPFF(l);
            CalculatePlayersTallyForLeague(l, players, leagueCount);

            //if l.UserTeam = null && standings.hasnext = true then get logged in users team along with its rank in this league
            l.UserTeam = l.Standings.results.Find(x => x.entry == teamId);

            if (l.UserTeam == null && l.Standings.has_next)
            {
                l.UserTeam = await GetUserTeamIfNotInRetrievedPage(l, PointsController, gwGames, allPlayers, allTeams, allGames, allGwPlayers, eventStatus, gameweekId, l.Standings.results.OrderByDescending(x => x.total).FirstOrDefault().total);
            }

            l.PlayersTally = l.PlayersTally.ToList();
            l.AllGwTransfers = allGwTransfers;

            return l;
        }

        private async Task<Result> GetUserTeamIfNotInRetrievedPage(Classic l, PointsController pointsController, List<Game> gwGames, List<Player> allPlayers, List<Team> allTeams, List<Game> allGames, List<GWPlayer> allGwPlayers, EventStatus eventStatus, int gameweekId, int topOfLeaguePointsTotal)
        {
            var userTeam = new Result();

            HttpClientHandler handler = new HttpClientHandler();

            var response = await _httpClient.GetAuthAsync(CreateHandler(handler), $"entry/{teamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var leaguesJSON = JObject.Parse(content);

            JObject leaguesObject = (JObject)leaguesJSON["leagues"];
            Leagues leagues = leaguesObject.ToObject<Leagues>();

            var selectedLeague = leagues.classic.Find(x => x.id == l.id);

            GWTeam gwTeam = new GWTeam();
            gwTeam = await pointsController.PopulateGwTeam(gwTeam, gameweekId, teamId);
            gwTeam = pointsController.AddPlayerSummaryDataToTeam(allPlayers, allTeams, allGames, gwTeam, teamId, gameweekId);
            gwTeam = await pointsController.AddTransfersToGwTeam(allPlayers, gwTeam, teamId, gameweekId);
            gwTeam.picks = pointsController.AddPlayerGameweekDataToTeam(gwGames, allGwPlayers, gwTeam.picks, gameweekId);
            gwTeam = pointsController.AddAutoSubs(gwTeam, gwTeam.picks, teamId, eventStatus, gameweekId);
            userTeam.CompleteEntryHistory = await pointsController.GetCompleteEntryHistory(userTeam.CompleteEntryHistory, teamId);
            gwTeam.picks = pointsController.AddEstimatedBonusToTeamPicks(gwTeam.picks, eventStatus);
            var teamDetails = await pointsController.GetTeamInfo(teamId);

            foreach (var p in gwTeam.picks)
            {
                CalculatePlayersYetToPlay(gwTeam, p);
            }

            //CalculateRankAndPFF(l);

            int gwpoints = pointsController.GetGameWeekPoints(gwTeam.picks, eventStatus);
            userTeam.Last5GwPoints = userTeam.CompleteEntryHistory.GetLast5GwPoints();
            userTeam.total = (teamDetails.summary_overall_points ?? 0 - teamDetails.summary_event_points ?? 0) + gwpoints;
            userTeam.PointsFromFirst = topOfLeaguePointsTotal - userTeam.total;
            userTeam.event_total = gwpoints;
            userTeam.GWTeam = gwTeam;
            userTeam.rank = selectedLeague.entry_rank;
            userTeam.player_name = teamDetails.player_first_name + ' ' + teamDetails.player_last_name;
            userTeam.entry_name = teamDetails.name;

            return userTeam;
        }

        //called from js
        public async Task<GWTeam> GetPlayersTeam(int teamId, int currentGameWeekId)
        {
            List<Player> allPlayers = await GetAllPlayers();
            List<Team> allTeams = await GetAllTeams();
            List<Game> allGames = await GetAllGames();
            List<GWPlayer> allGwPlayers = await GetAllGwPlayers(currentGameWeekId);
            List<Game> gwGames = await GetGwGames(currentGameWeekId);

            GWTeam gwTeam = new GWTeam();
            EventStatus eventStatus = await GetEventStatus();
            var PointsController = new PointsController(_httpClient);

            gwTeam = await PointsController.PopulateGwTeam(gwTeam, currentGameWeekId, teamId);
            gwTeam = PointsController.AddPlayerSummaryDataToTeam(allPlayers, allTeams, allGames, gwTeam, teamId, currentGameWeekId);
            gwTeam = await PointsController.AddTransfersToGwTeam(allPlayers, gwTeam, teamId, currentGameWeekId);
            gwTeam.picks = PointsController.AddPlayerGameweekDataToTeam(gwGames, allGwPlayers, gwTeam.picks, currentGameWeekId);
            gwTeam = PointsController.AddAutoSubs(gwTeam, gwTeam.picks, teamId, eventStatus, currentGameWeekId);
            gwTeam.picks = PointsController.AddEstimatedBonusToTeamPicks(gwTeam.picks, eventStatus);

            return gwTeam;
        }

        private void CalculateRankAndPFF(Classic league)
        {
            if (league.Standings.results.Count > 0)
            {
                var standingsByLivePointsTotal = league.Standings.results.OrderByDescending(x => x.total).ToList();
                int topOfLeaguePoints = league.Standings.results.OrderByDescending(x => x.total).FirstOrDefault().total;

                foreach (var player in league.Standings.results)
                {
                    player.rank = standingsByLivePointsTotal.IndexOf(player) + 1;
                    player.PointsFromFirst = topOfLeaguePoints - player.total;
                }
            }
        }

        private void CalculatePlayersTallyForLeague(Classic league, List<Pick> players, int leagueCount)
        {
            List<Transfer> allGwTransfers = new List<Transfer>();

            foreach (Result player in league.Standings.results)
            {
                foreach (Transfer transfer in player.GWTeam.GWTransfers)
                {
                    allGwTransfers.Add(transfer);
                }
            }

            foreach (var player in players)
            {
                if (!league.PlayersTally.Any(x => x.Pick.element == player.element))
                {
                    var count = players.FindAll(x => x.element == player.element).Count();
                    var ownership = ((double)count / (double)leagueCount).ToString("0%");

                    var startingCount = players.FindAll(x => x.element == player.element && x.multiplier > 0).Count();
                    var startingSelectionPercentage = ((double)startingCount / (double)leagueCount).ToString("0%");

                    var benchCount = players.FindAll(x => x.element == player.element && x.multiplier == 0).Count();
                    var benchSelectionPercentage = ((double)benchCount / (double)leagueCount).ToString("0%");

                    int captainCount = players.FindAll(x => x.element == player.element && x.is_captain).Count();
                    var captainSelectionPercentage = ((double)captainCount / (double)leagueCount).ToString("0%");

                    int transferInCount = allGwTransfers.FindAll(x => x.PlayerIn.id == player.element).Count();
                    var transferredIn = ((double)transferInCount / (double)leagueCount).ToString("0%");

                    int transferOutCount = allGwTransfers.FindAll(x => x.PlayerOut.id == player.element).Count();
                    var transferredOut = ((double)transferOutCount / (double)leagueCount).ToString("0%");

                    var pt = new PlayerTally()
                    {
                        Pick = player,
                        Count = count,
                        Ownership = ownership,
                        StartingSelection = startingSelectionPercentage,
                        BenchSelection = benchSelectionPercentage,
                        CaptainSelection = captainSelectionPercentage,
                        TransferredOut = transferredOut,
                        TransferredIn = transferredIn
                    };

                    league.PlayersTally.Add(pt);
                }
            }
        }

        //public void CalculatePlayersYetToPlay(Result player, Pick p)
        //{
        //    if (p.multiplier > 0 && p.GWGames.Any(x => x.kickoff_time != null && !x.finished_provisional) && p.player.status != "i")
        //    {
        //        for (var i = 0; i < p.GWPlayer.explain.Count; i++)
        //        {
        //            for (var j = 0; j < p.GWPlayer.explain[i].stats.Count; j++)
        //            {
        //                var g = p.GWGames.Find(x => x.id == p.GWPlayer.explain[i].fixture);

        //                if (p.GWPlayer.explain[i].stats[j].identifier == "minutes" && p.GWPlayer.explain[i].stats[j].value == 0)
        //                {
        //                    if (!g.started ?? true)
        //                    {
        //                        player.PlayersYetToPlay += 1;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

    }
}
