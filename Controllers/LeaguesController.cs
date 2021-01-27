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
        public LeaguesController(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new LeaguesViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            int teamId = await GetTeamId();

            var response = await _httpClient.GetAuthAsync(CreateHandler(handler), $"entry/{teamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var leaguesJSON = JObject.Parse(content);

            JObject leaguesObject = (JObject)leaguesJSON["leagues"];
            Leagues leagues = leaguesObject.ToObject<Leagues>();
            EventStatus eventStatus = await GetEventStatus();

            leagues = await AddPlayerStandingsToLeague(leagues);

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

        public async Task<Leagues> AddPlayerStandingsToLeague(Leagues leagues)
        {
            var currentGameWeekId = await GetCurrentGameWeekId();

            var eventStatus = await GetEventStatus();

            var PointsController = new PointsController(_httpClient);

            //var privateClassicLeagues = leagues.classic.FindAll(x => x.league_type == "x");

            foreach (var l in leagues.classic)
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

                if (l.id == 666321 /*|| l.id == 1018545|| l.id == 421502*/ /*&& eventStatus.leagues != "Updated"*/)
                {
                    foreach (var player in l.Standings.results)
                    {

                        GWTeam gwTeam = new GWTeam();

                        //gwTeam = await GetPlayersTeam(player.entry);
                        //team = await AddGameDataToPlayersTeam(team, currentGameWeekId);
                        gwTeam = await PointsController.PopulateGwTeam(gwTeam, currentGameWeekId, player.entry);
                        gwTeam.picks = await PointsController.AddPlayerSummaryDataToTeam(gwTeam.picks, player.entry);
                        gwTeam.picks = await PointsController.AddPlayerGameweekDataToTeam(gwTeam.picks, currentGameWeekId);
                        gwTeam = await PointsController.AddAutoSubs(gwTeam, gwTeam.picks, player.entry);
                        //gwTeam.EntryHistory = await PointsController.AddExtraDatatoEntryHistory(gwTeam.EntryHistory);
                        player.CompleteEntryHistory = await PointsController.GetCompleteEntryHistory(player.CompleteEntryHistory, player.entry);
                        player.Last5GwPoints = player.CompleteEntryHistory.GetLast5GwPoints();
                        int gwpoints = PointsController.GetGameWeekPoints(gwTeam.picks, eventStatus);

                        player.PlayersYetToPlay = gwTeam.picks.FindAll(x => x.GWPlayer.stats.minutes == 0 && x.multiplier > 0 && x.GWGames.Any(x => x.kickoff_time != null && !x.finished)).Count();
                        player.total += (gwpoints - player.event_total);
                        player.event_total += (gwpoints - player.event_total);
                        player.GWTeam = gwTeam;

                    }

                    var standingsByLivePointsTotal = l.Standings.results.OrderByDescending(x => x.total).ToList();

                    foreach (var player in l.Standings.results)
                    {
                        player.rank = standingsByLivePointsTotal.IndexOf(player) + 1;
                        //player.LiveRank = standingsByLivePointsTotal.IndexOf(player) + 1;
                    }
                }
            }

            return leagues;
        }


        public async Task<GWTeam> GetPlayersTeam(int teamId)
        {
            var currentGameWeekId = await GetCurrentGameWeekId();

            var response = await _httpClient.GetAsync($"entry/{teamId}/event/{currentGameWeekId}/picks/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var playerEntryJSON = JObject.Parse(content);

            var playerGwPicksJSON = AllChildren(playerEntryJSON)
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("picks"))
                .Children<JObject>();

            GWTeam team = new GWTeam();

            foreach (JObject result in playerGwPicksJSON)
            {
                Pick p = result.ToObject<Pick>();
                team.picks.Add(p);

            }

            //var playerEntryHistory = AllChildren(playerEntryJSON)
            //.First(c => c.Type == JTokenType.Object && c.Path.Contains("entry_history"))
            //.Children<JObject>();

            team.EntryHistory = playerEntryJSON["entry_history"].ToObject<EntryHistory>();

            response = await _httpClient.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            content = await response.Content.ReadAsStringAsync();

            var allPlayersJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("elements"))
                .Children<JObject>();

            List<Player> allPlayers = new List<Player>();

            foreach (JObject result in allPlayersJSON)
            {
                Player p = result.ToObject<Player>();

                foreach (Pick pick in team.picks)
                {
                    if (p.id == pick.element)
                    {
                        pick.player = p;
                    }
                }
            }

            return team;

        }

        public async Task<List<Pick>> AddGameDataToPlayersTeam(List<Pick> picks, int gameweekId)
        {
            var response = await _httpClient.GetAsync("fixtures/?event=" + gameweekId);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            List<Game> gwGames = JsonConvert.DeserializeObject<List<Game>>(content);

            foreach (Pick pick in picks)
            {
                foreach (Game g in gwGames)
                {
                    if (pick.player.TeamId == g.team_h)
                    {
                        //pick.GWOppositionName = g.AwayTeam.name;
                        //pick.GWGame = g;
                        pick.GWGames.Add(g);
                        break;
                    }
                    else if (pick.player.TeamId == g.team_a)
                    {
                        //pick.GWOppositionName = g.HomeTeam.name;
                        //pick.GWGame = g;
                        pick.GWGames.Add(g);
                        break;
                    }
                    else
                    {
                        //pick.GWGame = new Game();
                        pick.GWGames.Add(new Game());
                    }
                }
            }

            return picks;
        }
    }
}
