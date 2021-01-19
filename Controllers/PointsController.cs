using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FPL.Attributes;
using FPL.Http;
using FPL.Models;
using FPL.Models.FPL;
using FPL.ViewModels;
using FPL.ViewModels.FPL;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Team = FPL.Models.Team;
using FPLTeam = FPL.Models.FPL.Team;
using FPL.Models.GWPlayerStats;
using FPL.Contracts;

namespace FPL.Controllers
{
    [FPLCookie]
    public class PointsController : BaseController
    {
        public PointsController(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentGameweekId = await GetCurrentGameWeekId();

            var viewModel = new GameweekPointsViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            int teamId = await GetTeamId();

            var response = await _httpClient.GetAuthAsync(CreateHandler(handler), $"entry/{teamId}/event/{currentGameweekId}/picks/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var teamPicksJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("picks"))
                .Children<JObject>();

            List<Pick> teamPicks = new List<Pick>();

            foreach (JObject result in teamPicksJSON)
            {
                Pick p = result.ToObject<Pick>();
                teamPicks.Add(p);
            }

            var teamAutoSubsJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("automatic_subs"))
                .Children<JObject>();

            List<AutomaticSub> autoSubs = new List<AutomaticSub>();

            foreach (JObject result in teamAutoSubsJSON)
            {
                AutomaticSub sub = result.ToObject<AutomaticSub>();
                autoSubs.Add(sub);
            }

            var entryHistoryJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Object && c.Path.Contains("entry_history"));

            EntryHistory entryHistory = new EntryHistory();

            entryHistory = entryHistoryJSON.ToObject<EntryHistory>();

            var activeChipsJSON = AllChildren(JObject.Parse(content))
                .First(c => (c.Type == JTokenType.String || c.Type == JTokenType.Null || c.Type == JTokenType.Array) && c.Path.Contains("active_chip"));

            List<string> activeChips = new List<string>();

            if (activeChipsJSON.Type is JTokenType.String)
            {
                var activeChip = activeChipsJSON.ToString();
                activeChips.Add(activeChip);
            }
            else if (activeChipsJSON.Type is JTokenType.Array)
            {
                foreach (JObject result in activeChipsJSON)
                {
                    var ac = result.ToObject<string>();
                    activeChips.Add(ac);
                }
            }

            GWTeam gwTeam = new GWTeam
            {
                picks = teamPicks,
                automatic_subs = autoSubs,
                ActiveChips = activeChips
            };

            teamPicks = await AddPlayerSummaryDataToTeam(teamPicks, teamId);
            teamPicks = await AddPlayerGameweekDataToTeam(teamPicks, currentGameweekId);
            entryHistory = await AddExtraDatatoEntryHistory(entryHistory);
            gwTeam = await AddAutoSubs(gwTeam, teamPicks, teamId);
            EventStatus eventStatus = await GetEventStatus();
            int gwpoints = GetGameWeekPoints(teamPicks, eventStatus);
            FPLTeam teamDetails = await GetTeamInfo();

            foreach (var g in gwTeam.picks)
            {
                if (g.GWGame.started ?? true && g.GWGame.started != null)
                {
                    if (!g.GWGame.finished_provisional)
                    {
                        viewModel.IsLive = true;
                        break;
                    }
                }
            }

            viewModel.GWTeam = gwTeam;
            viewModel.EntryHistory = entryHistory;
            viewModel.EventStatus = eventStatus;
            viewModel.Team = teamDetails;
            viewModel.GWPoints = gwpoints;
            viewModel.TotalPoints = (teamDetails.summary_overall_points - teamDetails.summary_event_points) + gwpoints;
            viewModel.GameweekId = currentGameweekId;

            return View(viewModel);
        }

        [HttpGet]
        [Route("points/{id}")]
        public async Task<IActionResult> Index(int id)
        {
            var viewModel = new GameweekPointsViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            int currentGwId = await GetCurrentGameWeekId();

            if (id > currentGwId)
            {
                return RedirectToAction("Index", new { id = currentGwId });
            }

            int teamId = await GetTeamId();

            var response = await _httpClient.GetAuthAsync(CreateHandler(handler), $"entry/{teamId}/event/{id}/picks/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var teamPicksJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("picks"))
                .Children<JObject>();

            List<Pick> teamPicks = new List<Pick>();

            foreach (JObject result in teamPicksJSON)
            {
                Pick p = result.ToObject<Pick>();
                teamPicks.Add(p);
            }

            var teamAutoSubsJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("automatic_subs"))
                .Children<JObject>();

            List<AutomaticSub> autoSubs = new List<AutomaticSub>();

            foreach (JObject result in teamAutoSubsJSON)
            {
                AutomaticSub sub = result.ToObject<AutomaticSub>();
                autoSubs.Add(sub);
            }

            var entryHistoryJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Object && c.Path.Contains("entry_history"));

            EntryHistory entryHistory = new EntryHistory();

            entryHistory = entryHistoryJSON.ToObject<EntryHistory>();

            var activeChipsJSON = AllChildren(JObject.Parse(content))
                .First(c => (c.Type == JTokenType.String || c.Type == JTokenType.Null || c.Type == JTokenType.Array) && c.Path.Contains("active_chip"));

            List<string> activeChips = new List<string>();

            if (activeChipsJSON.Type is JTokenType.String)
            {
                var activeChip = activeChipsJSON.ToString();
                activeChips.Add(activeChip);
            }
            else if (activeChipsJSON.Type is JTokenType.Array)
            {
                foreach (JObject result in activeChipsJSON)
                {
                    var ac = result.ToObject<string>();
                    activeChips.Add(ac);
                }
            }

            GWTeam gwTeam = new GWTeam
            {
                picks = teamPicks,
                automatic_subs = autoSubs,
                ActiveChips = activeChips
            };

            teamPicks = await AddPlayerSummaryDataToTeam(teamPicks, id);
            teamPicks = await AddPlayerGameweekDataToTeam(teamPicks, id);
            entryHistory = await AddExtraDatatoEntryHistory(entryHistory);
            gwTeam = await AddAutoSubs(gwTeam, teamPicks, id);
            var liveGameCount = gwTeam.picks.FindAll(x => !x.GWGame.finished_provisional).Count();
            EventStatus eventStatus = await GetEventStatus();
            int gwpoints = GetGameWeekPoints(teamPicks, eventStatus);
            FPLTeam teamDetails = await GetTeamInfo();

            if (id == currentGwId)
            {
                viewModel.TotalPoints = (teamDetails.summary_overall_points - teamDetails.summary_event_points) + gwpoints;
            }
            else
            {
                viewModel.TotalPoints = teamDetails.summary_overall_points;
            }

            if (liveGameCount > 0) { viewModel.IsLive = true; }
            viewModel.GWTeam = gwTeam;
            viewModel.EntryHistory = entryHistory;
            viewModel.EventStatus = eventStatus;
            viewModel.Team = teamDetails;
            viewModel.GWPoints = gwpoints;
            viewModel.GameweekId = id;

            return View(viewModel);

        }

        public int GetGameWeekPoints(List<Pick> teamPicks, EventStatus eventStatus)
        {
            int gwpoints = 0;
            bool bonusAdded = true;

            //needs to be sorted

            foreach (var status in eventStatus.status)
            {
                if (status.date == DateTime.Now.ToString("yyyy-MM-dd") && !status.bonus_added)
                {
                    bonusAdded = false;
                }
            }

            foreach (Pick pick in teamPicks)
            {
                while (pick.multiplier > 0)
                {
                    if (!bonusAdded && !pick.GWGame.finished)
                    {
                        if (pick.is_captain)
                        {
                            gwpoints += pick.GWPlayer.stats.gw_points + (pick.GWPlayer.stats.EstimatedBonus * 2);
                        }
                        else
                        {
                            gwpoints += pick.GWPlayer.stats.gw_points + pick.GWPlayer.stats.EstimatedBonus;
                        }               
                    }
                    else
                    {
                        gwpoints += pick.GWPlayer.stats.gw_points;
                    }
                    break;
                }
            }

            return gwpoints;
        }

        private async Task<EntryHistory> AddExtraDatatoEntryHistory(EntryHistory entryHistory)
        {
            var response = await _httpClient.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var totalPlayersJson = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Integer && c.Path.Contains("total_players"));

            //EntryHistory entryHistory = new EntryHistory();

            int totalPlayers = totalPlayersJson.ToObject<int>();

            var rankPercentile = 0m;

            if (entryHistory.rank != null)
            {
                rankPercentile = Math.Round(((decimal)entryHistory.rank / (decimal)totalPlayers) * 100m, 0);
            }

            if (rankPercentile == 0)
            {
                entryHistory.RankPercentile = 1;
            }
            else
            {
                entryHistory.RankPercentile = Convert.ToInt32(rankPercentile);
            }

            return entryHistory;
        }

        public async Task<GWTeam> AddAutoSubs(GWTeam gwTeam, List<Pick> picks, int teamId)
        {
            EventStatus eventStatus = await GetEventStatus();
            //int teamId = await GetTeamId();
            var lastEvent = eventStatus.status.LastOrDefault();
            var starters = picks.FindAll(x => x.position < 12);
            var startersWhoDidNotPlay = picks.FindAll(x => x.position < 12 && x.GWPlayer.stats.minutes == 0 && x.GWGame.finished_provisional);
            var subsWhoPlayed = picks.FindAll(x => x.position > 12 && x.GWPlayer.stats.minutes > 0);

            if (lastEvent.bonus_added && eventStatus.leagues != "Updating")
            {
                return gwTeam;
            }
            else
            {
                if (startersWhoDidNotPlay.Count > 0 && subsWhoPlayed.Count > 0)
                {
                    for (var i = 0; i < startersWhoDidNotPlay.Count; i++)
                    {
                        if (startersWhoDidNotPlay[i].player.element_type == 1)
                        {
                            if (subsWhoPlayed.Find(x => x.player.element_type == 1) != null)
                            {
                                AutomaticSub autoSub = new AutomaticSub
                                {
                                    element_out = startersWhoDidNotPlay[i].element,
                                    element_in = subsWhoPlayed.Find(x => x.player.element_type == 1).element,
                                    @event = eventStatus.status[0].@event
                                };
                                gwTeam.automatic_subs.Add(autoSub);
                                break;
                            }
                            else
                            {
                                break;
                            }

                        }

                        if (startersWhoDidNotPlay[i].player.element_type == 2)
                        {
                            var startingDefenders = starters.FindAll(x => x.player.element_type == 2);

                            for (var k = 0; k < subsWhoPlayed.Count; k++)
                            {
                                if (subsWhoPlayed[k].player.element_type == 2)
                                {
                                    var autoSub = MakeOutfieldAutoSub(startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    break;
                                }

                                if (subsWhoPlayed[k].player.element_type == 3 && startingDefenders.Count > 3)
                                {
                                    var autoSub = MakeOutfieldAutoSub(startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    break;

                                }

                                if (subsWhoPlayed[k].player.element_type == 4 && startingDefenders.Count > 3)
                                {
                                    var autoSub = MakeOutfieldAutoSub(startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    break;
                                }
                            }
                        }

                        if (startersWhoDidNotPlay[i].player.element_type == 3)
                        {
                            for (var k = 0; k < subsWhoPlayed.Count; k++)
                            {
                                if (subsWhoPlayed[k].player.element_type == 2)
                                {
                                    var autoSub = MakeOutfieldAutoSub(startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    break;
                                }

                                if (subsWhoPlayed[k].player.element_type == 3)
                                {
                                    var autoSub = MakeOutfieldAutoSub(startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    break;

                                }

                                if (subsWhoPlayed[k].player.element_type == 4)
                                {
                                    var autoSub = MakeOutfieldAutoSub(startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    break;
                                }
                            }
                        }

                        if (startersWhoDidNotPlay[i].player.element_type == 4)
                        {
                            for (var k = 0; k < subsWhoPlayed.Count; k++)
                            {
                                if (subsWhoPlayed[k].player.element_type == 2)
                                {
                                    var autoSub = MakeOutfieldAutoSub(startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    break;
                                }

                                if (subsWhoPlayed[k].player.element_type == 3)
                                {
                                    var autoSub = MakeOutfieldAutoSub(startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    break;

                                }

                                if (subsWhoPlayed[k].player.element_type == 4)
                                {
                                    var autoSub = MakeOutfieldAutoSub(startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    break;
                                }
                            }
                        }

                    }
                }


            }

            gwTeam.picks = picks.OrderBy(x => x.position).ToList();

            return gwTeam;

        }

        private async Task<FPLTeam> GetTeamInfo()
        {
            HttpClientHandler handler = new HttpClientHandler();

            handler = CreateHandler(handler);

            int teamId = await GetTeamId();

            var response = await _httpClient.GetAuthAsync(handler, $"entry/{teamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            FPLTeam teamDetails = JsonConvert.DeserializeObject<FPLTeam>(content);

            return teamDetails;

        }

        public async Task<List<Pick>> AddPlayerSummaryDataToTeam(List<Pick> teamPicks, int teamId)
        {
            var response = await _httpClient.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var allPlayersJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("elements"))
                .Children<JObject>();

            List<Player> allPlayers = new List<Player>();

            foreach (JObject result in allPlayersJSON)
            {
                Player p = result.ToObject<Player>();

                foreach (Pick pick in teamPicks)
                {
                    if (p.id == pick.element)
                    {
                        pick.player = p;
                    }
                }
            }

            var allTeamsJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("teams"))
                .Children<JObject>();

            List<Team> allTeams = new List<Team>();

            foreach (JObject result in allTeamsJSON)
            {
                Team t = result.ToObject<Team>();
                allTeams.Add(t);

                foreach (Pick pick in teamPicks)
                {
                    if (t.id == pick.player.TeamId)
                    {
                        pick.player.Team = t;
                    }
                }
            }

            var response1 = await _httpClient.GetAsync("fixtures/");

            response1.EnsureSuccessStatusCode();

            var content1 = await response1.Content.ReadAsStringAsync();

            List<Game> games = JsonConvert.DeserializeObject<List<Game>>(content1);

            List<Game> fixtures = games.FindAll(x => x.started == false);
            List<Game> results = games.FindAll(x => x.started == true);

            foreach (Pick pick in teamPicks)
            {
                pick.player.Team.Fixtures = new List<Game>();
                pick.player.Team.Results = new List<Game>();

                foreach (Game fixture in fixtures)
                {
                    if (pick.player.TeamId == fixture.team_h || pick.player.TeamId == fixture.team_a)
                    {
                        pick.player.Team.Fixtures.Add(fixture);
                    }
                }

                foreach (Game playerFixture in pick.player.Team.Fixtures)
                {
                    foreach (Team t in allTeams)
                    {
                        if (playerFixture.team_h == t.id)
                        {
                            playerFixture.team_h_name = t.name;
                        }

                        if (playerFixture.team_a == t.id)
                        {
                            playerFixture.team_a_name = t.name;
                        }
                    }
                }

                foreach (Game result in results)
                {
                    if (pick.player.TeamId == result.team_h || pick.player.TeamId == result.team_a)
                    {
                        pick.player.Team.Results.Add(result);
                    }

                    Stat totalBps = result.stats[9];

                    List<PlayerStat> homeBps = totalBps.h;
                    List<PlayerStat> awayBps = totalBps.a;
                    List<PlayerStat> allPlayersInGameBps = homeBps.Concat(awayBps).ToList();
                    allPlayersInGameBps = allPlayersInGameBps.OrderByDescending(x => x.value).ToList();

                    for (var i = 0; i < allPlayersInGameBps.Count; i++)
                    {
                        if (pick.element == allPlayersInGameBps[i].element)
                        {
                            //currently only calculates players total bps ranking - need to find away to get average
                            pick.player.AvgBpsRank += allPlayersInGameBps.IndexOf(allPlayersInGameBps[i]) + 1;
                        }
                    }
                }

                foreach (Game playerResult in pick.player.Team.Results)
                {
                    foreach (Team t in allTeams)
                    {
                        if (playerResult.team_h == t.id)
                        {
                            playerResult.team_h_name = t.name;
                        }

                        if (playerResult.team_a == t.id)
                        {
                            playerResult.team_a_name = t.name;
                        }
                    }
                }
            }

            var response2 = await _httpClient.GetAsync($"entry/{teamId}/transfers/");

            response2.EnsureSuccessStatusCode();

            var content2 = await response2.Content.ReadAsStringAsync();

            List<Transfer> transfers = JsonConvert.DeserializeObject<List<Transfer>>(content2);

            return teamPicks;
        }

        public async Task<List<Pick>> AddPlayerGameweekDataToTeam(List<Pick> teamPicks, int gameweekId)
        {
            //get player stats specific to the gameweek
            var response = await _httpClient.GetAsync("event/" + gameweekId + "/live/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var allPlayersJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("elements"))
                .Children<JObject>();

            List<GWPlayer> allGwPlayers = new List<GWPlayer>();

            foreach (JObject result in allPlayersJSON)
            {
                GWPlayer p = result.ToObject<GWPlayer>();
                allGwPlayers.Add(p);
            }

            foreach (GWPlayer gwplayer in allGwPlayers)
            {
                foreach (Pick pick in teamPicks)
                {
                    if (pick.element == gwplayer.id)
                    {
                        pick.GWPlayer = gwplayer;
                    }
                }
            }

            var response1 = await _httpClient.GetAsync("fixtures/?event=" + gameweekId);

            response1.EnsureSuccessStatusCode();

            var content1 = await response1.Content.ReadAsStringAsync();

            List<Game> gwGames = JsonConvert.DeserializeObject<List<Game>>(content1);

            gwGames = await PopulateGameListWithTeams(gwGames);
            List<Game> startedGames = gwGames.FindAll(x => x.started == true);

            foreach (Pick pick in teamPicks)
            {
                foreach (Game g in gwGames)
                {
                    if (pick.player.TeamId == g.team_h)
                    {
                        pick.GWOppositionName = g.AwayTeam.name;
                        pick.GWGame = g;
                        break;
                    }
                    else if (pick.player.TeamId == g.team_a)
                    {
                        pick.GWOppositionName = g.HomeTeam.name;
                        pick.GWGame = g;
                        break;
                    }
                    else
                    {
                        pick.GWGame = new Game();
                    }
                }

                foreach (Game g in startedGames)
                {
                    Stat totalBps = g.stats[9];
                    List<PlayerStat> homeBps = totalBps.h;
                    List<PlayerStat> awayBps = totalBps.a;
                    List<PlayerStat> allPlayersInGameBps = homeBps.Concat(awayBps).ToList();
                    allPlayersInGameBps = allPlayersInGameBps.OrderByDescending(x => x.value).ToList();
                    List<PlayerStat> topPlayersByBps = allPlayersInGameBps.Take(4).ToList();
                    var IsBpsEqual = topPlayersByBps.FindAll(n => n.value == pick.GWPlayer.stats.bps).Count > 1;

                    for (var i = 0; i < allPlayersInGameBps.Count; i++)
                    {
                        if (pick.element == allPlayersInGameBps[i].element)
                        {
                            if (IsBpsEqual)
                            {
                                if (topPlayersByBps[0].value == pick.GWPlayer.stats.bps && topPlayersByBps[1].value == pick.GWPlayer.stats.bps)
                                {
                                    pick.GWPlayer.stats.BpsRank.Add(1);
                                    pick.GWPlayer.stats.EstimatedBonus += 3;
                                }
                                else if (topPlayersByBps[1].value == pick.GWPlayer.stats.bps && topPlayersByBps[2].value == pick.GWPlayer.stats.bps)
                                {
                                    pick.GWPlayer.stats.BpsRank.Add(2);
                                    pick.GWPlayer.stats.EstimatedBonus += 2;
                                }
                                else if (topPlayersByBps[2].value == pick.GWPlayer.stats.bps && topPlayersByBps[3].value == pick.GWPlayer.stats.bps)
                                {
                                    pick.GWPlayer.stats.BpsRank.Add(3);
                                    pick.GWPlayer.stats.EstimatedBonus += 1;
                                }
                                break;
                            } 
                            else
                            {
                                pick.GWPlayer.stats.BpsRank.Add(allPlayersInGameBps.IndexOf(allPlayersInGameBps[i]) + 1);
                                break;
                            }
                        }
                    }

                    for (var i = 0; i < topPlayersByBps.Count; i++)
                    {
                        if (topPlayersByBps[i].element == pick.element && !IsBpsEqual)
                        {
                            if (i == 3)
                            {
                                pick.GWPlayer.stats.EstimatedBonus += 0;
                            }
                            else
                            {
                                pick.GWPlayer.stats.EstimatedBonus += 3 - i;
                            }
                            break;
                        }
                    }
                }
            }
            //get gameweek dreamteam data
            //var response1 = await client.GetAsync("dream-team/" + gameweekId + "/");

            //response1.EnsureSuccessStatusCode();

            //var content1 = await response1.Content.ReadAsStringAsync();

            //RootGWDreamTeam gwDreamTeam = JsonConvert.DeserializeObject<RootGWDreamTeam>(content1);

            //RootGWDreamTeam gwDreamTeam = JsonConvert.DeserializeObject<RootGWDreamTeam>(content1);

            //totalling a players total gw stats
            for (var i = 0; i < teamPicks.Count; i++)
            {
                if (teamPicks[i].GWPlayer.stats.minutes != 0)
                {
                    for (var j = 0; j < teamPicks[i].GWPlayer.explain.Count; j++)
                    {
                        for (var k = 0; k < teamPicks[i].GWPlayer.explain[j].stats.Count; k++)
                        {
                            if (teamPicks[i].GWPlayer.explain[j].stats[k].points != 0)
                            {
                                if (teamPicks[i].is_captain)
                                {
                                    teamPicks[i].GWPlayer.stats.gw_points += teamPicks[i].GWPlayer.explain[j].stats[k].points * teamPicks[i].multiplier;
                                }
                                else
                                {
                                    teamPicks[i].GWPlayer.stats.gw_points += teamPicks[i].GWPlayer.explain[j].stats[k].points;
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                }
                else if (teamPicks[i].GWGame.started ?? true)
                {
                    //if captain didnt play assign double points to vice
                    if (teamPicks[i].is_captain)
                    {
                        var vc = teamPicks.Find(x => x.is_vice_captain);
                        if (vc.GWGame.minutes != 0)
                        {
                            teamPicks[i].is_captain = false;
                            vc.is_captain = true;
                            vc.is_vice_captain = false;
                            vc.GWPlayer.stats.gw_points += vc.GWPlayer.stats.gw_points;
                        }
                    }
                }

            }
            return teamPicks;
        }

        private AutomaticSub MakeOutfieldAutoSub(Pick starter, Pick sub, int eventId, int teamId)
        {
            AutomaticSub autoSub = new AutomaticSub();

            autoSub.element_out = starter.element;
            autoSub.element_in = sub.element;
            autoSub.@event = eventId;
            autoSub.entry = teamId;
            var starterPosition = starter.position;
            var subPosition = sub.position;
            starter.position = subPosition;
            starter.multiplier = 0;
            sub.position = starterPosition;
            sub.multiplier = 1;

            return autoSub;
        }

        public async Task<List<Pick>> GetPicks(List<Pick> picks, int teamId)
        {
            return picks;
        }
    }
}
