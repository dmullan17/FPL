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

            leagues = await AddBasicInfoToPrivateLeagues(leagues, eventStatus);
            //leagues = await AddPlayerStandingsToLeague(leagues);

            viewModel.SelectedLeague = leagues.classic.FindAll(x => x.league_type == "x").OrderBy(i => i.PlayerCount).First();
            viewModel.IsEventLive = IsEventLive(eventStatus); 
            viewModel.ClassicLeagues = leagues.classic;
            viewModel.H2HLeagues = leagues.h2h;
            viewModel.Cup = leagues.cup;
            viewModel.CurrentGwId = await GetCurrentGameWeekId();

            return View(viewModel);
        }

        [HttpGet]
        [Route("leagues/{id}")]
        public async Task<IActionResult> Index(int id)
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
