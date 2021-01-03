using FPL.Attributes;
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
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new LeaguesViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            var client = new FPLHttpClient();

            int teamId = await GetTeamId();

            var response = await client.GetAuthAsync(CreateHandler(handler), $"entry/{teamId}/");

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

            var client = new FPLHttpClient();

            //var privateClassicLeagues = leagues.classic.FindAll(x => x.league_type == "x");

            foreach (var l in leagues.classic)
            {
                var response = await client.GetAsync($"leagues-classic/{l.id}/standings");

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

                var today = DateTime.Now.ToString("yyyy-MM-dd");

                var currentEventDay = eventStatus.status.Find(x => x.date == today);

                if (l.id == 666321  && (currentEventDay.points == "l" || currentEventDay.points == "p" || eventStatus.leagues == "Updating"))
                {
                    foreach (var player in l.Standings.results)
                    {
                        List<Pick> team = new List<Pick>();

                        team = await GetPlayersTeam(player.entry);
                        team = await AddGameDataToPlayersTeam(team, currentGameWeekId);

                        foreach (var pick in team)
                        {
                            if (!pick.GWGame.finished && pick.multiplier > 0)
                            {
                                if (pick.GWGame.started ?? true)
                                {
                                    if (pick.is_captain)
                                    {
                                        player.event_total += (pick.player.event_points * 2);
                                        player.total += (pick.player.event_points * 2);
                                    }
                                    else
                                    {
                                        player.event_total += pick.player.event_points;
                                        player.total += pick.player.event_points;
                                    }
                                }
                            }
                        }
                    }

                    var standingsByLivePointsTotal = l.Standings.results.OrderByDescending(x => x.total).ToList();

                    foreach (var player in l.Standings.results)
                    {
                        player.LiveRank = standingsByLivePointsTotal.IndexOf(player) + 1;
                    }
                }
            }

            return leagues;
        }


        public async Task<List<Pick>> GetPlayersTeam(int teamId)
        {
            var currentGameWeekId = await GetCurrentGameWeekId();

            var client = new FPLHttpClient();

            var response = await client.GetAsync($"entry/{teamId}/event/{currentGameWeekId}/picks/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var playerEntryJSON = JObject.Parse(content);

            var playerGwPicksJSON = AllChildren(playerEntryJSON)
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("picks"))
                .Children<JObject>();

            List<Pick> team = new List<Pick>();

            foreach (JObject result in playerGwPicksJSON)
            {
                Pick p = result.ToObject<Pick>();
                team.Add(p);
            }

            response = await client.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            content = await response.Content.ReadAsStringAsync();

            var allPlayersJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("elements"))
                .Children<JObject>();

            List<Player> allPlayers = new List<Player>();

            foreach (JObject result in allPlayersJSON)
            {
                Player p = result.ToObject<Player>();

                foreach (Pick pick in team)
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
            var client = new FPLHttpClient();

            var response = await client.GetAsync("fixtures/?event=" + gameweekId);

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
                        pick.GWGame = g;
                        break;
                    }
                    else if (pick.player.TeamId == g.team_a)
                    {
                        //pick.GWOppositionName = g.HomeTeam.name;
                        pick.GWGame = g;
                        break;
                    }
                    else
                    {
                        pick.GWGame = new Game();
                    }
                }
            }

            return picks;
        }
    }
}
