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
        private static int teamId;

        public PointsController(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new GameweekPointsViewModel();

            List<Player> allPlayers = await GetAllPlayers();
            List<Team> allTeams = await GetAllTeams();
            List<Game> allGames = await GetAllGames();
            var currentGameweekId = await GetCurrentGameWeekId();
            List<GWPlayer> allGwPlayers = await GetAllGwPlayers(currentGameweekId);
            EventStatus eventStatus = await GetEventStatus();
            List<Game> gwGames = await GetGwGames(currentGameweekId);
            if (Request.Cookies["teamId"] == null) teamId = await GetTeamId();
            else teamId = Convert.ToInt32(Request.Cookies["teamId"]);   

            GWTeam gwTeam = new GWTeam();
            gwTeam = await PopulateGwTeam(gwTeam, currentGameweekId, teamId);
            gwTeam = AddPlayerSummaryDataToTeam(allPlayers, allTeams, allGames, gwTeam, teamId, currentGameweekId);
            gwTeam = await AddTransfersToGwTeam(allPlayers, gwTeam, teamId, currentGameweekId);
            gwTeam.picks = AddPlayerGameweekDataToTeam(gwGames, allGwPlayers, gwTeam.picks, currentGameweekId);
            gwTeam.EntryHistory = await AddExtraDatatoEntryHistory(gwTeam.EntryHistory);
            gwTeam = AddAutoSubs(gwTeam, gwTeam.picks, teamId, eventStatus);
            gwTeam.picks = AddEstimatedBonusToTeamPicks(gwTeam.picks, eventStatus);
            int gwpoints = GetGameWeekPoints(gwTeam.picks, eventStatus);
            FPLTeam teamDetails = await GetTeamInfo(teamId);

            foreach (var pick in gwTeam.picks)
            {
                foreach (var game in pick.player.Team.Results)
                {
                    if ((game.started ?? true) && game.started != null && game.Event == currentGameweekId)
                    {
                        if (!game.finished_provisional)
                        {
                            viewModel.IsLive = true;
                            break;
                        }
                    }
                }
            }

            viewModel.GWTeam = gwTeam;
            viewModel.EntryHistory = gwTeam.EntryHistory;
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

            List<Player> allPlayers = await GetAllPlayers();
            List<Team> allTeams = await GetAllTeams();
            List<Game> allGames = await GetAllGames();
            var currentGameweekId = await GetCurrentGameWeekId();
            List<GWPlayer> allGwPlayers = await GetAllGwPlayers(id);
            EventStatus eventStatus = await GetEventStatus();
            List<Game> gwGames = await GetGwGames(id);
            if (Request.Cookies["teamId"] == null) teamId = await GetTeamId();
            else teamId = Convert.ToInt32(Request.Cookies["teamId"]);
      
            if (id > currentGameweekId)
            {
                return RedirectToAction("Index", new { id = currentGameweekId });
            }

            GWTeam gwTeam = new GWTeam();
            gwTeam = await PopulateGwTeam(gwTeam, id, teamId);
            gwTeam = AddPlayerSummaryDataToTeam(allPlayers, allTeams, allGames, gwTeam, teamId, id);
            gwTeam = await AddTransfersToGwTeam(allPlayers, gwTeam, teamId, id);
            gwTeam.picks = AddPlayerGameweekDataToTeam(gwGames, allGwPlayers, gwTeam.picks, id);
            gwTeam.EntryHistory = await AddExtraDatatoEntryHistory(gwTeam.EntryHistory);
            //gwTeam = AddAutoSubs(gwTeam, gwTeam.picks, id, eventStatus);
            var liveGameCount = gwTeam.picks.FindAll(x => !x.GWGames.Any(x => x.finished_provisional)).Count();
            //gwTeam.picks = AddEstimatedBonusToTeamPicks(gwTeam.picks, eventStatus);
            int gwpoints = GetGameWeekPoints(gwTeam.picks, null);
            FPLTeam teamDetails = await GetTeamInfo(teamId);

            if (id == currentGameweekId)
            {
                viewModel.TotalPoints = (teamDetails.summary_overall_points - teamDetails.summary_event_points) + gwpoints;
            }
            else
            {
                viewModel.TotalPoints = teamDetails.summary_overall_points;
            }

            if (liveGameCount > 0) { viewModel.IsLive = true; }
            viewModel.GWTeam = gwTeam;
            viewModel.EntryHistory = gwTeam.EntryHistory;
            viewModel.EventStatus = eventStatus;
            viewModel.Team = teamDetails;
            viewModel.GWPoints = gwpoints;
            viewModel.GameweekId = id;

            return View(viewModel);

        }

        public async Task<GWTeam> PopulateGwTeam(GWTeam gwTeam, int gameweekId, int teamId)
        {
            HttpClientHandler handler = new HttpClientHandler();

            var response = await _httpClient.GetAuthAsync(CreateHandler(handler), $"entry/{teamId}/event/{gameweekId}/picks/");

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

            gwTeam.picks = teamPicks;
            gwTeam.automatic_subs = autoSubs;
            gwTeam.ActiveChips = activeChips;
            gwTeam.EntryHistory = entryHistory;

            return gwTeam;
        }

        public async Task<CompleteEntryHistory> GetCompleteEntryHistory(CompleteEntryHistory completeEntryHistory, int teamId)
        {
            HttpClientHandler handler = new HttpClientHandler();

            var response = await _httpClient.GetAuthAsync(CreateHandler(handler), $"entry/{teamId}/history/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var currentSeasonEntryHistoryJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("current"))
                .Children<JObject>();

            List<EntryHistory> currentSeasonEntryHistory = new List<EntryHistory>();
            int totalTransfers = 0;
            int totalTransferCost = 0;

            foreach (JObject result in currentSeasonEntryHistoryJSON)
            {
                EntryHistory eh = result.ToObject<EntryHistory>();
                currentSeasonEntryHistory.Add(eh);
                totalTransfers += eh.event_transfers;
                totalTransferCost += eh.event_transfers_cost;
            }

            var chipsUsedJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("chips"))
                .Children<JObject>();

            List<BasicChip> chipsUsed = new List<BasicChip>();

            foreach (JObject result in chipsUsedJSON)
            {
                BasicChip bc = result.ToObject<BasicChip>();
                chipsUsed.Add(bc);
            }

            completeEntryHistory.CurrentSeasonEntryHistory = currentSeasonEntryHistory;
            completeEntryHistory.ChipsUsed = chipsUsed;
            completeEntryHistory.TotalTransfersMade = totalTransfers;
            completeEntryHistory.TotalTransfersCost = totalTransferCost;

            return completeEntryHistory;
        }

        public int GetGameWeekPoints(List<Pick> teamPicks, EventStatus eventStatus)
        {
            int gwpoints = 0;
            //bool bonusAdded = true;

            ////needs to be sorted

            //foreach (var status in eventStatus.status)
            //{
            //    if (status.date == DateTime.Now.ToString("yyyy-MM-dd") && !status.bonus_added)
            //    {
            //        bonusAdded = false;
            //        break;
            //    }
            //}

            foreach (Pick pick in teamPicks.Where(x => x.multiplier > 0))
            {
                gwpoints += pick.GWPlayer.stats.gw_points;

                //while (pick.multiplier > 0)
                //{
                //    //if (!bonusAdded && !pick.GWGames.Any(x => x.finished))
                //    //{
                //    //    if (pick.is_captain)
                //    //    {
                //    //        gwpoints += pick.GWPlayer.stats.gw_points;
                //    //        //gwpoints += pick.GWPlayer.stats.gw_points * pick.multiplier;
                //    //    }
                //    //    else
                //    //    {
                //    //        gwpoints += pick.GWPlayer.stats.gw_points;
                //    //    }               
                //    //}
                //    //else
                //    //{
                //    //    gwpoints += pick.GWPlayer.stats.gw_points;
                //    //}
                //    //break;
                //}
            }

            return gwpoints;
        }

        public List<Pick> AddEstimatedBonusToTeamPicks(List<Pick> teamPicks, EventStatus eventStatus)
        {
            var picksWhosGameHasStarted = new List<Pick>();

            //get picks whos gw games has started
            foreach (Pick pick in teamPicks)
            {
                var games = (pick.GWGames.FindAll(x => x.started ?? true).ToList());

                if (games.Count > 0)
                {
                    picksWhosGameHasStarted.Add(pick);              
                }              
            }

            //add estimated bonus to picks whos games have started if bonus hasn't been applied by FPL yet
            foreach (Pick pick in picksWhosGameHasStarted)
            {
                if (!pick.GWGames.FindAll(x => x.started ?? true).LastOrDefault().finished)
                {
                    if (pick.is_captain)
                    {
                        pick.GWPlayer.stats.gw_points += (pick.GWPlayer.stats.EstimatedBonus.LastOrDefault() * pick.multiplier);
                    }
                    else
                    {
                        pick.GWPlayer.stats.gw_points += pick.GWPlayer.stats.EstimatedBonus.LastOrDefault();
                    }
                }
            }

            return teamPicks;
        }

        public async Task<EntryHistory> AddExtraDatatoEntryHistory(EntryHistory entryHistory)
        {
            var response = await _httpClient.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var totalPlayersJson = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Integer && c.Path.Contains("total_players"));

            //EntryHistory entryHistory = new EntryHistory();

            int totalPlayers = totalPlayersJson.ToObject<int>();

            var gwRankPercentile = 0m;
            var overallRankPercentile = 0m;

            if (entryHistory.rank != null)
            {
                gwRankPercentile = Math.Round(((decimal)entryHistory.rank / (decimal)totalPlayers) * 100m, 2);
            }

            if (entryHistory.overall_rank != null)
            {
                overallRankPercentile = Math.Round(((decimal)entryHistory.overall_rank / (decimal)totalPlayers) * 100m, 2);
            }

            if (gwRankPercentile < 1)
            {
                entryHistory.GwRankPercentile = 1;
            }
            else
            {
                entryHistory.GwRankPercentile = Convert.ToInt32(gwRankPercentile + 1);
            }

            if (overallRankPercentile < 1)
            {
                entryHistory.TotalRankPercentile = 1;
            }
            else
            {
                entryHistory.TotalRankPercentile = Convert.ToInt32(overallRankPercentile + 1);
            }

            return entryHistory;
        }

        public GWTeam AddAutoSubs(GWTeam gwTeam, List<Pick> picks, int teamId, EventStatus eventStatus)
        {
            var picksWhoHaveAGame = picks.FindAll(x => x.GWGames.Count > 0).ToList();
            var lastEvent = eventStatus.status.LastOrDefault();
            var starters = picksWhoHaveAGame.FindAll(x => x.position < 12);
            var startersWhoDidNotPlay = picksWhoHaveAGame.FindAll(x => x.position < 12 && x.GWPlayer.stats.minutes == 0 && (x.GWGames.LastOrDefault().finished_provisional || x.GWGames.Count == 0 || x.player.status == "i" || x.player.status == "u"));
            //RemoveIfMultiGamesInGW(startersWhoDidNotPlay);
            var subsWhoPlayed = picksWhoHaveAGame.FindAll(x => x.position > 11 && x.GWPlayer.stats.minutes > 0);
            var subsYetToPlay = picksWhoHaveAGame.FindAll(x => x.position > 11 && x.GWPlayer.stats.minutes == 0 && (x.player.status != "u" && x.player.status != "i") && x.GWGames.Any(y => !y.finished_provisional));

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
                        bool IsSubAdded = false;
                        subsWhoPlayed = subsWhoPlayed.FindAll(x => x.multiplier == 0);
                        if (startersWhoDidNotPlay[i].player.element_type == 1 && !IsSubAdded)
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
                                IsSubAdded = true;
                                break;
                            }
                            else
                            {
                                break;
                            }

                        }

                        if (startersWhoDidNotPlay[i].player.element_type == 2 && !IsSubAdded)
                        {
                            var startingDefenders = starters.FindAll(x => x.player.element_type == 2);

                            for (var k = 0; k < subsWhoPlayed.Count; k++)
                            {
                                if (subsWhoPlayed[k].player.element_type == 2)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;
                                }

                                if (subsWhoPlayed[k].player.element_type == 3 && startingDefenders.Count > 3)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;

                                }

                                if (subsWhoPlayed[k].player.element_type == 4 && startingDefenders.Count > 3)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;
                                }
                            }
                        }

                        if (startersWhoDidNotPlay[i].player.element_type == 3 && !IsSubAdded)
                        {
                            for (var k = 0; k < subsWhoPlayed.Count; k++)
                            {
                                if (subsWhoPlayed[k].player.element_type == 2)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;
                                }

                                if (subsWhoPlayed[k].player.element_type == 3)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;

                                }

                                if (subsWhoPlayed[k].player.element_type == 4)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;
                                }
                            }
                        }

                        if (startersWhoDidNotPlay[i].player.element_type == 4 && !IsSubAdded)
                        {
                            for (var k = 0; k < subsWhoPlayed.Count; k++)
                            {
                                if (subsWhoPlayed[k].player.element_type == 2)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;
                                }

                                if (subsWhoPlayed[k].player.element_type == 3)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;

                                }

                                if (subsWhoPlayed[k].player.element_type == 4)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsWhoPlayed[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;
                                }
                            }
                        }

                    }
                }
                else if (startersWhoDidNotPlay.Count > 0 && subsYetToPlay.Count > 0)
                {
                    for (var i = 0; i < startersWhoDidNotPlay.Count; i++)
                    {
                        bool IsSubAdded = false;
                        subsYetToPlay = subsYetToPlay.FindAll(x => x.multiplier == 0);
                        if (startersWhoDidNotPlay[i].player.element_type == 1 && !IsSubAdded)
                        {
                            if (subsYetToPlay.Find(x => x.player.element_type == 1) != null)
                            {
                                AutomaticSub autoSub = new AutomaticSub
                                {
                                    element_out = startersWhoDidNotPlay[i].element,
                                    element_in = subsYetToPlay.Find(x => x.player.element_type == 1).element,
                                    @event = eventStatus.status[0].@event
                                };
                                gwTeam.automatic_subs.Add(autoSub);
                                IsSubAdded = true;
                                break;
                            }
                            else
                            {
                                break;
                            }

                        }

                        if (startersWhoDidNotPlay[i].player.element_type == 2 && !IsSubAdded)
                        {
                            var subDefenders = subsYetToPlay.FindAll(x => x.player.element_type == 2);

                            for (var k = 0; k < subsYetToPlay.Count; k++)
                            {
                                if (subsYetToPlay[k].player.element_type == 2)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsYetToPlay[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;
                                }

                                if (subsYetToPlay[k].player.element_type == 3 && subDefenders.Count > 3)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsYetToPlay[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;

                                }

                                if (subsYetToPlay[k].player.element_type == 4 && subDefenders.Count > 3)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsYetToPlay[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;
                                }
                            }
                        }

                        if (startersWhoDidNotPlay[i].player.element_type == 3 && !IsSubAdded)
                        {
                            for (var k = 0; k < subsYetToPlay.Count; k++)
                            {
                                if (subsYetToPlay[k].player.element_type == 2)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsYetToPlay[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;
                                }

                                if (subsYetToPlay[k].player.element_type == 3)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsYetToPlay[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;

                                }

                                if (subsYetToPlay[k].player.element_type == 4)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsYetToPlay[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;
                                }
                            }
                        }

                        if (startersWhoDidNotPlay[i].player.element_type == 4 && !IsSubAdded)
                        {
                            for (var k = 0; k < subsYetToPlay.Count; k++)
                            {
                                if (subsYetToPlay[k].player.element_type == 2)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsYetToPlay[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;
                                }

                                if (subsYetToPlay[k].player.element_type == 3)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsYetToPlay[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
                                    break;

                                }

                                if (subsYetToPlay[k].player.element_type == 4)
                                {
                                    var autoSub = MakeOutfieldAutoSub(picks, startersWhoDidNotPlay[i], subsYetToPlay[k], eventStatus.status[0].@event, teamId);
                                    gwTeam.automatic_subs.Add(autoSub);
                                    IsSubAdded = true;
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



        //public void RemoveIfMultiGamesInGW(List<Pick> startersWhoDidNotPlay)
        //{
        //    var startersWhoDidNotPlayButHaveAnotherGame = startersWhoDidNotPlay.FindAll(x => x.GWGames.Count > 1);

        //    if (startersWhoDidNotPlayButHaveAnotherGame.Count > 0)
        //    {
        //        foreach (var t in startersWhoDidNotPlayButHaveAnotherGame)
        //        {
        //            startersWhoDidNotPlay.Remove(t);
        //        }
        //    }
        //}

        private async Task<FPLTeam> GetTeamInfo(int teamId)
        {
            HttpClientHandler handler = new HttpClientHandler();

            handler = CreateHandler(handler);

            var response = await _httpClient.GetAuthAsync(handler, $"entry/{teamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            FPLTeam teamDetails = JsonConvert.DeserializeObject<FPLTeam>(content);

            return teamDetails;

        }

        public async Task<GWTeam> AddTransfersToGwTeam(List<Player> allPlayers, GWTeam gwTeam, int teamId, int gameweekId)
        {
            var response = await _httpClient.GetAsync($"entry/{teamId}/transfers/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            List<Transfer> transfers = JsonConvert.DeserializeObject<List<Transfer>>(content);

            //populate transfers with player in and out
            foreach (var transfer in transfers)
            {
                transfer.PlayerIn = allPlayers.Find(x => x.id == transfer.element_in);
                transfer.PlayerOut = allPlayers.Find(x => x.id == transfer.element_out);
            }

            gwTeam.GWTransfers = transfers.Where(x => x.@event == gameweekId).ToList();

            return gwTeam;
        }

        public GWTeam AddPlayerSummaryDataToTeam(List<Player> allPlayers, List<Team> allTeams, List<Game> allGames, GWTeam gwTeam, int teamId, int currentGwId)
        {
            List<Game> allFixtures = allGames.FindAll(x => x.started == false);
            List<Game> allResults = allGames.FindAll(x => x.started == true);

            foreach (var pick in gwTeam.picks)
            {
                pick.player = allPlayers.Find(x => x.id == pick.element);
                pick.player.Team = allTeams.Find(x => x.id == pick.player.TeamId);

                pick.player.Team.Fixtures = new List<Game>();
                pick.player.Team.Results = new List<Game>();

                foreach (Game fixture in allFixtures.Where(x => pick.player.TeamId == x.team_h || pick.player.TeamId == x.team_a))
                {
                    pick.player.Team.Fixtures.Add(fixture);
                }

                foreach (Game playerFixture in pick.player.Team.Fixtures)
                {
                    //playerFixture.HomeTeam = allTeams.Find(x => x.id == playerFixture.team_h);
                    //playerFixture.AwayTeam = allTeams.Find(x => x.id == playerFixture.team_a);
                    playerFixture.team_h_name = allTeams.Find(x => x.id == playerFixture.team_h).name;
                    playerFixture.team_a_name = allTeams.Find(x => x.id == playerFixture.team_a).name;
                }

                foreach (Game result in allResults.Where(x => pick.player.TeamId == x.team_h || pick.player.TeamId == x.team_a))
                {
                    pick.player.Team.Results.Add(result);

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
                    //playerFixture.HomeTeam = allTeams.Find(x => x.id == playerFixture.team_h);
                    //playerFixture.AwayTeam = allTeams.Find(x => x.id == playerFixture.team_a);

                    playerResult.team_h_name = allTeams.Find(x => x.id == playerResult.team_h).name;
                    playerResult.team_a_name = allTeams.Find(x => x.id == playerResult.team_a).name;
                }
            }

            //gwTeam.GWTransfers = transfers.FindAll(x => x.@event == currentGwId && (x.PlayerIn != null && x.PlayerOut != null)).ToList();
            //gwTeam.picks = teamPicks;

            return gwTeam;
        }

        public List<Pick> AddPlayerGameweekDataToTeam(List<Game> gwGames, List<GWPlayer> allGwPlayers, List<Pick> teamPicks, int gameweekId)
        {
            //var response = await _httpClient.GetAsync("fixtures/?event=" + gameweekId);

            //response.EnsureSuccessStatusCode();

            //var content = await response.Content.ReadAsStringAsync();

            //List<Game> gwGames = JsonConvert.DeserializeObject<List<Game>>(content);

            //gwGames = await PopulateGameListWithTeams(gwGames);
            List<Game> startedGames = gwGames.FindAll(x => x.started == true);

            foreach (Pick pick in teamPicks)
            {
                pick.GWPlayer = allGwPlayers.Find(x => x.id == pick.element);

                foreach (Game g in gwGames.Where(x => x.team_h == pick.player.TeamId || x.team_a == pick.player.TeamId))
                {
                    if (pick.player.TeamId == g.team_h)
                    {
                        pick.GWOppositionName = g.AwayTeam.name;
                        pick.GWGames.Add(g);
                        continue;
                    }
                    else if (pick.player.TeamId == g.team_a)
                    {
                        pick.GWOppositionName = g.HomeTeam.name;
                        pick.GWGames.Add(g);
                        continue;
                    }
                }

                foreach (Game g in startedGames.Where(x => x.team_h == pick.player.TeamId || x.team_a == pick.player.TeamId))
                {
                    Stat totalBps = g.stats[9];
                    List<PlayerStat> homeBps = totalBps.h;
                    List<PlayerStat> awayBps = totalBps.a;
                    List<PlayerStat> allPlayersInGameBps = homeBps.Concat(awayBps).ToList();
                    allPlayersInGameBps = allPlayersInGameBps.OrderByDescending(x => x.value).ToList();
                    List<PlayerStat> topPlayersByBps = allPlayersInGameBps.Take(4).ToList();
                    var playerBps = 0;
                    var player = new PlayerStat();

                    if (pick.player.TeamId == g.HomeTeam.id)
                    {
                        //playerBps = pick.GWGames.Find(x => x.id == g.id).stats[9].h.Find(y => y.element == pick.element).value;
                        var homeStats = pick.GWGames.Find(x => x.id == g.id).stats[9].h;
                        player = homeStats.FirstOrDefault(x => x.element == pick.element);
                    }
                    else if (pick.player.TeamId == g.AwayTeam.id)
                    {
                        var awayStats = pick.GWGames.Find(x => x.id == g.id).stats[9].a;
                        player = awayStats.FirstOrDefault(x => x.element == pick.element);
                    }

                    //if pick did not play
                    if (player is null)
                    {
                        pick.GWPlayer.stats.EstimatedBonus.Add(0);
                        pick.GWPlayer.stats.BpsRank.Add(0);
                        continue;
                    }
                    else
                    {
                        if (player.element != 0)
                        {
                            playerBps = player.value;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    //if any players in top bps list have equal bps
                    var IsBpsEqual = topPlayersByBps.FindAll(n => n.value == playerBps).Count > 1;

                    for (var i = 0; i < allPlayersInGameBps.Count; i++)
                    {
                        if (pick.element == allPlayersInGameBps[i].element)
                        {
                            pick.GWPlayer.stats.BpsRank.Add(allPlayersInGameBps.IndexOf(allPlayersInGameBps[i]) + 1);
                            break;
                            //if (IsBpsEqual)
                            //{
                            //    if (topPlayersByBps[0].value == playerBps && topPlayersByBps[1].value == playerBps)
                            //    {
                            //        pick.GWPlayer.stats.BpsRank.Add(1);
                            //        pick.GWPlayer.stats.EstimatedBonus.Add(3);
                            //    }
                            //    else if (topPlayersByBps[1].value == playerBps && topPlayersByBps[2].value == playerBps)
                            //    {
                            //        pick.GWPlayer.stats.BpsRank.Add(2);
                            //        pick.GWPlayer.stats.EstimatedBonus.Add(2);
                            //    }
                            //    else if (topPlayersByBps[2].value == playerBps && topPlayersByBps[3].value == playerBps)
                            //    {
                            //        pick.GWPlayer.stats.BpsRank.Add(3);
                            //        pick.GWPlayer.stats.EstimatedBonus.Add(1);
                            //    }
                            //    break;
                            //} 
                            //else
                            //{
                            //    pick.GWPlayer.stats.BpsRank.Add(allPlayersInGameBps.IndexOf(allPlayersInGameBps[i]) + 1);
                            //    break;
                            //}
                        }
                    }

                    pick.GWPlayer.stats.EstimatedBonus.Clear();

                    if (topPlayersByBps.Any(x => x.element == pick.element))
                    {
                        for (var i = 0; i < topPlayersByBps.Count; i++)
                        {
                            if (IsBpsEqual)
                            {
                                if (topPlayersByBps[0].value == playerBps && topPlayersByBps[1].value == playerBps)
                                {
                                    //pick.GWPlayer.stats.BpsRank.Add(1);
                                    pick.GWPlayer.stats.EstimatedBonus.Add(3);
                                }
                                else if (topPlayersByBps[1].value == playerBps && topPlayersByBps[2].value == playerBps)
                                {
                                    //pick.GWPlayer.stats.BpsRank.Add(2);
                                    pick.GWPlayer.stats.EstimatedBonus.Add(2);
                                }
                                else if (topPlayersByBps[2].value == playerBps && topPlayersByBps[3].value == playerBps)
                                {
                                    //pick.GWPlayer.stats.BpsRank.Add(3);
                                    pick.GWPlayer.stats.EstimatedBonus.Add(1);
                                }
                                break;
                            }
                            else if (topPlayersByBps[i].element == pick.element && !IsBpsEqual)
                            {
                                if (i == 3)
                                {
                                    pick.GWPlayer.stats.EstimatedBonus.Add(0);
                                }
                                else
                                {
                                    pick.GWPlayer.stats.EstimatedBonus.Add(3 - i);
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        pick.GWPlayer.stats.EstimatedBonus.Add(0);
                    }
                }
            }

            foreach (var pick in teamPicks.FindAll(x => x.GWGames.Count > 0).ToList())
            {
                pick.GWPlayer.stats.gw_points = 0;

                if (pick.GWPlayer.stats.minutes != 0)
                {
                    for (var j = 0; j < pick.GWPlayer.explain.Count; j++)
                    {
                        for (var k = 0; k < pick.GWPlayer.explain[j].stats.Count; k++)
                        {
                            //if (pick.GWPlayer.explain[j].stats[k].points != 0)
                            //{
                            if (pick.is_captain)
                            {
                                pick.GWPlayer.stats.gw_points += pick.GWPlayer.explain[j].stats[k].points * pick.multiplier;
                            }
                            else
                            {
                                pick.GWPlayer.stats.gw_points += pick.GWPlayer.explain[j].stats[k].points;
                            }
                            //}
                            //else
                            //{

                            //}
                        }
                    }
                }
                else if (pick.GWGames.LastOrDefault().started ?? true && pick.is_captain)
                {
                    //if captain didnt play assign double points to vice
                    if (pick.is_captain)
                    {
                        var vc = teamPicks.Find(x => x.is_vice_captain);
                        if (vc != null)
                        {
                            if (vc.GWGames.LastOrDefault().started ?? true)
                            {
                                if (vc.GWGames.Any(x => x.minutes != 0) || vc.GWPlayer.stats.minutes > 0)
                                {
                                    pick.is_captain = false;
                                    vc.is_captain = true;
                                    vc.is_vice_captain = false;
                                    vc.GWPlayer.stats.gw_points = vc.GWPlayer.stats.gw_points * pick.multiplier;
                                }
                            }
                            else
                            {
                                pick.is_captain = false;
                                vc.is_captain = true;
                                vc.is_vice_captain = false;
                                vc.GWPlayer.stats.gw_points = vc.GWPlayer.stats.gw_points * pick.multiplier;
                            }
                        }
                    }
                }

            }
            //totalling a players total gw stats
            //for (var i = 0; i < teamPicks.FindAll(x => x.GWGames.Count > 0).ToList().Count; i++)
            //{
            //    teamPicks[i].GWPlayer.stats.gw_points = 0;

            //    if (teamPicks[i].GWPlayer.stats.minutes != 0)
            //    {
            //        for (var j = 0; j < teamPicks[i].GWPlayer.explain.Count; j++)
            //        {
            //            for (var k = 0; k < teamPicks[i].GWPlayer.explain[j].stats.Count; k++)
            //            {
            //                //if (teamPicks[i].GWPlayer.explain[j].stats[k].points != 0)
            //                //{
            //                if (teamPicks[i].is_captain)
            //                {
            //                    teamPicks[i].GWPlayer.stats.gw_points += teamPicks[i].GWPlayer.explain[j].stats[k].points * teamPicks[i].multiplier;
            //                }
            //                else
            //                {
            //                    teamPicks[i].GWPlayer.stats.gw_points += teamPicks[i].GWPlayer.explain[j].stats[k].points;
            //                }
            //                //}
            //                //else
            //                //{

            //                //}
            //            }
            //        }
            //    }
            //    else if (teamPicks[i].GWGames.LastOrDefault().started ?? true && teamPicks[i].is_captain)
            //    {
            //        //if captain didnt play assign double points to vice
            //        if (teamPicks[i].is_captain)
            //        {
            //            var vc = teamPicks.Find(x => x.is_vice_captain);
            //            if (vc != null)
            //            {
            //                if (vc.GWGames.LastOrDefault().started ?? true)
            //                {
            //                    if (vc.GWGames.Any(x => x.minutes != 0) || vc.GWPlayer.stats.minutes > 0)
            //                    {
            //                        teamPicks[i].is_captain = false;
            //                        vc.is_captain = true;
            //                        vc.is_vice_captain = false;
            //                        vc.GWPlayer.stats.gw_points = vc.GWPlayer.stats.gw_points * teamPicks[i].multiplier;
            //                    }
            //                }
            //                else
            //                {
            //                    teamPicks[i].is_captain = false;
            //                    vc.is_captain = true;
            //                    vc.is_vice_captain = false;
            //                    vc.GWPlayer.stats.gw_points = vc.GWPlayer.stats.gw_points * teamPicks[i].multiplier;
            //                }
            //            }
            //        }
            //    }

            //}
            return teamPicks;
        }

        private AutomaticSub MakeOutfieldAutoSub(List<Pick> picks, Pick starter, Pick sub, int eventId, int teamId)
        {
            AutomaticSub autoSub = new AutomaticSub();

            autoSub.element_out = starter.element;
            autoSub.element_in = sub.element;
            autoSub.@event = eventId;
            autoSub.entry = teamId;

            var starterPosition = starter.position;
            var subPosition = sub.position;
            sub.multiplier = 1;
            starter.multiplier = 0;
            starter.position = subPosition;
            picks = AssignStarterPosition(picks.Where(x => x.multiplier > 0).OrderBy(x => x.position).ToList(), starter, sub);

            return autoSub;
        }

        private List<Pick> AssignStarterPosition(List<Pick> picks, Pick starter, Pick sub)
        {
            var starterElementType = starter.player.element_type;
            var subElementType = sub.player.element_type;

            var defs = picks.FindAll(x => x.player.element_type == 2);
            var mids = picks.FindAll(x => x.player.element_type == 3);
            var fwds = picks.FindAll(x => x.player.element_type == 4);

            if (starterElementType == subElementType)
            {
                var playersInPosition = picks.FindAll(x => x.player.element_type == starterElementType && x.multiplier > 0);
                var playersInBehindPosition = picks.FindAll(x => x.player.element_type == (starterElementType - 1) && x.multiplier > 0);

                for (var i = 0; i < playersInPosition.Count; i++)
                {
                    playersInPosition[i].position = playersInBehindPosition.LastOrDefault().position + (i + 1);
                }
            }
            else
            {
                //midfielder coming off & forward coming on
                if (starterElementType == 3 && subElementType == 4)
                {
                    for (var i = 0; i < mids.Count; i++)
                    {
                        mids[i].position = defs.LastOrDefault().position + (i + 1);
                    }

                    for (var i = 0; i < fwds.Count; i++)
                    {
                        fwds[i].position = mids.LastOrDefault().position + (i + 1);
                    }
                }
                //midfielder coming off & defender coming on
                else if (starterElementType == 3 && subElementType == 2)
                {
                    for (var i = 0; i < defs.Count; i++)
                    {
                        defs[i].position = (i + 2);
                    }

                    for (var i = 0; i < mids.Count; i++)
                    {
                        mids[i].position = defs.LastOrDefault().position + (i + 1);
                    }
                }
                //fwd coming off & defender coming on
                else if (starterElementType == 4 && subElementType == 2)
                {
                    for (var i = 0; i < defs.Count; i++)
                    {
                        defs[i].position = (i + 2);
                    }

                    for (var i = 0; i < fwds.Count; i++)
                    {
                        fwds[i].position = mids.LastOrDefault().position + (i + 1);
                    }
                }
                //fwd coming off & mid coming on
                else if (starterElementType == 4 && subElementType == 3)
                {
                    for (var i = 0; i < mids.Count; i++)
                    {
                        mids[i].position = defs.LastOrDefault().position + (i + 1);
                    }

                    for (var i = 0; i < fwds.Count; i++)
                    {
                        fwds[i].position = mids.LastOrDefault().position + (i + 1);
                    }
                }
                //def coming off & mid coming on
                else if (starterElementType == 2 && subElementType == 3)
                {
                    for (var i = 0; i < defs.Count; i++)
                    {
                        defs[i].position = (i + 2);
                    }

                    for (var i = 0; i < mids.Count; i++)
                    {
                        mids[i].position = defs.LastOrDefault().position + (i + 1);
                    }
                }
                //def coming off & fwd coming on
                else if (starterElementType == 2 && subElementType == 4)
                {
                    for (var i = 0; i < defs.Count; i++)
                    {
                        defs[i].position = (i + 2);
                    }

                    for (var i = 0; i < fwds.Count; i++)
                    {
                        fwds[i].position = mids.LastOrDefault().position + (i + 1);
                    }
                }
            }

            return picks.OrderBy(x => x.position).ToList();
        }

        public async Task<List<Pick>> GetPicks(List<Pick> picks, int teamId)
        {
            return picks;
        }
    }
}
