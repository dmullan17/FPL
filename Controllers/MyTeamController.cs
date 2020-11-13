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
using Team = FPL.Models.Team;
using FPLTeam = FPL.Models.FPL.Team;

namespace FPL.Controllers
{
    [FPLCookie]
    public class MyTeamController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new MyTeamViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            var client = new FPLHttpClient();

            int currentGwId = await GetCurrentGameWeekId();

            int teamId = await GetTeamId();

            var response = await client.GetAuthAsync(CreateHandler(handler), $"my-team/{teamId}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var myTeamJSON = JObject.Parse(content);

            var teamPicksJSON = AllChildren(myTeamJSON)
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("picks"))
                .Children<JObject>();

            JObject transfersObject = (JObject)myTeamJSON["transfers"];
            TransferInfo transferInfo = transfersObject.ToObject<TransferInfo>();

            List<Pick> teamPicks = new List<Pick>();
            List<Transfer> transfers = new List<Transfer>();
            List<PlayerPosition> positions = new List<PlayerPosition>();

            foreach (JObject result in teamPicksJSON)
            {
                Pick p = result.ToObject<Pick>();
                teamPicks.Add(p);
            }

            transfers = await GetTeamTransfers();
            positions = await GetPlayerPositionInfo();
            teamPicks = await AddPlayerSummaryDataToTeam(teamPicks);
            teamPicks = await CalculateTotalPointsContributed(teamPicks, transfers);
            FPLTeam teamDetails = await GetTeamInfo();

            viewModel.Picks = teamPicks;
            viewModel.Team = teamDetails;
            viewModel.Positions = positions;
            viewModel.TotalPoints = teamDetails.summary_overall_points;
            viewModel.TransferInfo = transferInfo;

            return View(viewModel);
        }

        private async Task<List<Pick>> AddPlayerSummaryDataToTeam(List<Pick> teamPicks)
        {
            var client = new FPLHttpClient();

            var response = await client.GetAsync("bootstrap-static/");

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
                    }
                }
            }

            var allPlayersByBps = allPlayers.OrderByDescending(m => m.bps).ToList();
            var allGoalkeepersByBps = allPlayers.Where(x => x.element_type == 1).OrderByDescending(m => m.bps).ToList();
            var allDefendersByBps = allPlayers.Where(x => x.element_type == 2).OrderByDescending(m => m.bps).ToList();
            var allMidfieldersByBps = allPlayers.Where(x => x.element_type == 3).OrderByDescending(m => m.bps).ToList();
            var allForwardsByBps = allPlayers.Where(x => x.element_type == 4).OrderByDescending(m => m.bps).ToList();

            foreach (var player in allPlayers)
            {
                player.BpsRank = allPlayersByBps.IndexOf(player) + 1;

                if (player.element_type == 1)
                {
                    player.BpsPositionRank = allGoalkeepersByBps.IndexOf(player) + 1;
                }
                else if (player.element_type == 2) 
                {
                    player.BpsPositionRank = allDefendersByBps.IndexOf(player) + 1;
                }
                if (player.element_type == 3)
                {
                    player.BpsPositionRank = allMidfieldersByBps.IndexOf(player) + 1;
                }
                else if (player.element_type == 4)
                {
                    player.BpsPositionRank = allForwardsByBps.IndexOf(player) + 1;
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
                    if (t.id == pick.player.team)
                    {
                        pick.player.Team = t;
                    }
                }
            }

            var response1 = await client.GetAsync("fixtures/");

            response1.EnsureSuccessStatusCode();

            var content1 = await response1.Content.ReadAsStringAsync();

            List<Game> games = JsonConvert.DeserializeObject<List<Game>>(content1);

            List<Game> fixtures = games.FindAll(x => x.started == false);
            List<Game> results = games.FindAll(x => x.finished == true);

            foreach (Pick pick in teamPicks)
            {
                pick.player.Team.Fixtures = new List<Game>();
                pick.player.Team.Results = new List<Game>();

                foreach (Game fixture in fixtures)
                {
                    if (pick.player.team == fixture.team_h || pick.player.team == fixture.team_a)
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
                    if (pick.player.team == result.team_h || pick.player.team == result.team_a)
                    {
                        pick.player.Team.Results.Add(result);
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

            //List<Pick> test = pick.FindAll(x => x.HadSinceGW == false);

            //var response2 = await client.GetAsync($"entry/{TeamId}/transfers/");

            //response2.EnsureSuccessStatusCode();

            //var content2 = await response2.Content.ReadAsStringAsync();

            //List<Transfer> transfers = JsonConvert.DeserializeObject<List<Transfer>>(content2);

            foreach (Pick pick in teamPicks)
            {
                pick.player.MinsPlayedPercentage = Math.Round((pick.player.minutes / (pick.player.Team.Results.Count * 90m)) * 100m, 1);
            }

            return teamPicks;
        }

        private async Task<List<Pick>> CalculateTotalPointsContributed(List<Pick> teamPicks, List<Transfer> teamTransfers)
        {
            var client = new FPLHttpClient();

            foreach (Pick pick in teamPicks)
            {
                foreach (Transfer transfer in teamTransfers)
                {
                    if (pick.element == transfer.element_in)
                    {
                        pick.HadSinceGW = transfer.@event;
                    }
                }
            }

            //calculate how many points a player has gained for the team, taking into account how long they have actually been on it 

            foreach (Pick pick in teamPicks)
            {
                var response = await client.GetAsync($"element-summary/" + pick.element + "/");

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
                    }
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
            var client = new FPLHttpClient();

            int teamId = await GetTeamId();

            var response = await client.GetAsync($"entry/{teamId}/transfers/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            List<Transfer> transfers = JsonConvert.DeserializeObject<List<Transfer>>(content);

            var response1 = await client.GetAsync("bootstrap-static/");

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

            var client = new FPLHttpClient();

            int teamId = await GetTeamId();

            var response = await client.GetAuthAsync(handler, $"entry/{teamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            FPLTeam teamDetails = JsonConvert.DeserializeObject<FPLTeam>(content);

            return teamDetails;

        }
    }
}
