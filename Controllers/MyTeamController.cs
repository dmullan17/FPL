﻿using FPL.Attributes;
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
using Team = FPL.Models.Team;
using FPLTeam = FPL.Models.FPL.Team;
using System.Globalization;
using FPL.Contracts;

namespace FPL.Controllers
{
    [FPLCookie]
    public class MyTeamController : BaseController
    {
        public MyTeamController(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new MyTeamViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            int currentGwId = await GetCurrentGameWeekId();

            int teamId = await GetTeamId();

            var response = await _httpClient.GetAuthAsync(CreateHandler(handler), $"my-team/{teamId}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var myTeamJSON = JObject.Parse(content);

            var teamPicksJSON = AllChildren(myTeamJSON)
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("picks"))
                .Children<JObject>();

            JObject transfersObject = (JObject)myTeamJSON["transfers"];
            TransferInfo transferInfo = transfersObject.ToObject<TransferInfo>();

            List<Pick> teamPicks = new List<Pick>();
            List<Pick> teamPicksLastWeek = new List<Pick>();
            List<Transfer> transfers = new List<Transfer>();
            List<PlayerPosition> positions = new List<PlayerPosition>();

            foreach (JObject result in teamPicksJSON)
            {
                Pick p = result.ToObject<Pick>();
                teamPicks.Add(p);
            }

            teamPicksLastWeek = await GetLastWeeksTeam(teamPicksLastWeek, teamId, currentGwId);

            foreach (var p in teamPicks)
            {
                if (teamPicksLastWeek.FindAll(x => x.element == p.element).Count == 0)
                {
                    p.IsNewTransfer = true;
                }
            }

            transfers = await GetTeamTransfers();
            positions = await GetPlayerPositionInfo();
            teamPicks = await AddPlayerSummaryDataToTeam(teamPicks);
            teamPicks = await CalculateTotalPointsContributed(teamPicks, transfers);
            teamPicks = teamPicks.OrderBy(x => x.position).ToList();
            FPLTeam teamDetails = await GetTeamInfo();

            viewModel.CurrentGwId = await GetCurrentGameWeekId();
            viewModel.Picks = teamPicks;
            viewModel.Team = teamDetails;
            viewModel.Positions = positions;
            viewModel.TotalPoints = teamDetails.summary_overall_points;
            viewModel.TransferInfo = transferInfo;

            return View(viewModel);
        }

        private async Task<List<Pick>> GetLastWeeksTeam(List<Pick> teamPicksLastWeek, int teamId, int currentGwId)
        {
            var response = await _httpClient.GetAsync($"entry/{teamId}/event/{currentGwId}/picks/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var teamPicksLastWeekJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("picks"))
                .Children<JObject>();

            teamPicksLastWeek = new List<Pick>();

            foreach (JObject result in teamPicksLastWeekJSON)
            {
                Pick p = result.ToObject<Pick>();
                teamPicksLastWeek.Add(p);
            }

            return teamPicksLastWeek;
        }

        private async Task<List<Pick>> AddPlayerSummaryDataToTeam(List<Pick> teamPicks)
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
                allPlayers.Add(p);

                foreach (Pick pick in teamPicks)
                {
                    if (p.id == pick.element)
                    {
                        pick.player = p;
                        pick.NetProfitOnTransfer = pick.selling_price - pick.purchase_price;
                    }
                }
            }

            foreach (var player in allPlayers)
            {
                player.FplIndex = (float)player.bps + float.Parse(player.ict_index, CultureInfo.InvariantCulture.NumberFormat);
            }

            var allPlayersByBps = allPlayers.OrderByDescending(m => m.bps).ToList();
            var allGoalkeepersByBps = allPlayers.Where(x => x.element_type == 1).OrderByDescending(m => m.bps).ToList();
            var allDefendersByBps = allPlayers.Where(x => x.element_type == 2).OrderByDescending(m => m.bps).ToList();
            var allMidfieldersByBps = allPlayers.Where(x => x.element_type == 3).OrderByDescending(m => m.bps).ToList();
            var allForwardsByBps = allPlayers.Where(x => x.element_type == 4).OrderByDescending(m => m.bps).ToList();

            var allPlayersByFplIndex = allPlayers.OrderByDescending(m => m.FplIndex).ToList();
            var allGoalkeepersByFplIndex = allPlayers.Where(x => x.element_type == 1).OrderByDescending(m => m.FplIndex).ToList();
            var allDefendersByFplIndex = allPlayers.Where(x => x.element_type == 2).OrderByDescending(m => m.FplIndex).ToList();
            var allMidfieldersByFplIndex = allPlayers.Where(x => x.element_type == 3).OrderByDescending(m => m.FplIndex).ToList();
            var allForwardsByFplIndex = allPlayers.Where(x => x.element_type == 4).OrderByDescending(m => m.FplIndex).ToList();

            foreach (var player in allPlayers)
            {
                player.BpsRank = allPlayersByBps.IndexOf(player) + 1;
                player.FplRank = allPlayersByFplIndex.IndexOf(player) + 1;

                if (player.element_type == 1)
                {
                    player.BpsPositionRank = allGoalkeepersByBps.IndexOf(player) + 1;
                    player.FplPositionRank = allGoalkeepersByFplIndex.IndexOf(player) + 1;
                }
                else if (player.element_type == 2) 
                {
                    player.BpsPositionRank = allDefendersByBps.IndexOf(player) + 1;
                    player.FplPositionRank = allDefendersByFplIndex.IndexOf(player) + 1;
                }
                if (player.element_type == 3)
                {
                    player.BpsPositionRank = allMidfieldersByBps.IndexOf(player) + 1;
                    player.FplPositionRank = allMidfieldersByFplIndex.IndexOf(player) + 1;
                }
                else if (player.element_type == 4)
                {
                    player.BpsPositionRank = allForwardsByBps.IndexOf(player) + 1;
                    player.FplPositionRank = allForwardsByFplIndex.IndexOf(player) + 1;
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

            foreach (Pick pick in teamPicks)
            {
                pick.player.MinsPlayedPercentage = Math.Round((pick.player.minutes / (pick.player.Team.Results.Count * 90m)) * 100m, 1);

                if (pick.player.MinsPlayedPercentage > 100)
                {
                    pick.player.MinsPlayedPercentage = 100;
                }
            }

            return teamPicks;
        }

        private async Task<List<Pick>> CalculateTotalPointsContributed(List<Pick> teamPicks, List<Transfer> teamTransfers)
        {
            int currentGwId = await GetCurrentGameWeekId();

            foreach (Pick pick in teamPicks)
            {
                foreach (Transfer transfer in teamTransfers)
                {
                    if (pick.element == transfer.element_in)
                    {
                        pick.HadSinceGW = transfer.@event;
                        var gw = currentGwId - pick.HadSinceGW;

                        if (gw != 0)
                        {
                            pick.GWOnTeam = (currentGwId - pick.HadSinceGW) + 1;
                        }
                        else
                        {
                            pick.GWOnTeam = 1;
                        }
                    }
                }

                if (pick.HadSinceGW == 1 && !pick.IsNewTransfer)
                {
                    pick.GWOnTeam = currentGwId;
                }
                else if (pick.IsNewTransfer)
                {
                    pick.GWOnTeam = 0;
                }
            }

            //calculate how many points a player has gained for the team, taking into account how long they have actually been on it 

            foreach (Pick pick in teamPicks)
            {
                var response = await _httpClient.GetAsync($"element-summary/" + pick.element + "/");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var playerHistoryJSON = AllChildren(JObject.Parse(content))
                    .First(c => c.Type == JTokenType.Array && c.Path.Contains("history"))
                    .Children<JObject>();

                var playerFixturesJSON = AllChildren(JObject.Parse(content))
                    .First(c => c.Type == JTokenType.Array && c.Path.Contains("fixtures"))
                    .Children<JObject>();

                List<PlayerHistory> playerHistory = new List<PlayerHistory>();
                List<PlayerFixture> playerFixtures = new List<PlayerFixture>();

                foreach (JObject result in playerHistoryJSON)
                {
                    PlayerHistory ph = result.ToObject<PlayerHistory>();
                    playerHistory.Add(ph);
                }

                foreach (PlayerHistory playerGwHistory in playerHistory)
                {
                    if (pick.HadSinceGW <= playerGwHistory.round)
                    {
                        pick.TotalPointsAccumulatedForTeam += playerGwHistory.total_points;
                        continue;
                    }

                }
                
                if (pick.GWOnTeam != 0)
                {
                    pick.PPGOnTeam = (float)pick.TotalPointsAccumulatedForTeam / (float)pick.GWOnTeam;
                }
                else
                {
                    pick.PPGOnTeam = 0;
                }

                foreach (JObject result in playerFixturesJSON)
                {
                    PlayerFixture pf = result.ToObject<PlayerFixture>();
                    playerFixtures.Add(pf);
                }

                pick.player.Fixtures = playerFixtures;

                //foreach (PlayerFixture playerFixture in playerFixtures)
                //{

                //    if (pick.HadSinceGW <= playerGwHistory.round)
                //    {
                //        pick.TotalPointsAccumulatedForTeam += playerGwHistory.total_points;
                //    }
                //}
            }

            return teamPicks;
        }

        private async Task<List<Transfer>> GetTeamTransfers()
        {
            int teamId = await GetTeamId();

            var response = await _httpClient.GetAsync($"entry/{teamId}/transfers/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            List<Transfer> transfers = JsonConvert.DeserializeObject<List<Transfer>>(content);

            var response1 = await _httpClient.GetAsync("bootstrap-static/");

            response1.EnsureSuccessStatusCode();

            var content1 = await response1.Content.ReadAsStringAsync();

            var allPlayersJSON = AllChildren(JObject.Parse(content1))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("elements"))
                .Children<JObject>();

            List<Player> allPlayers = new List<Player>();

            foreach (JObject result in allPlayersJSON)
            {
                Player p = result.ToObject<Player>();

                foreach (Transfer transfer in transfers)
                {
                    if (transfer.element_in == p.id)
                    {
                        transfer.PlayerIn = p;
                    }

                    if (transfer.element_out == p.id)
                    {
                        transfer.PlayerOut = p;
                    }
                }
            }

            return transfers;
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
    }
}
