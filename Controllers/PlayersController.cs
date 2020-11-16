using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FPL.Http;
using FPL.Models;
using FPL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FPL.Controllers
{
    public class PlayersController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            var viewModel = new PlayersViewModel();

            var client = new FPLHttpClient();

            var response = await client.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            //elements = players
            var allPlayersJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("elements"))
                .Children<JObject>();

            List<Player> players = new List<Player>();

            foreach (JObject result in allPlayersJSON)
            {
                Player p = result.ToObject<Player>();
                players.Add(p);
            }

            players = players.Where(x => x.minutes != 0).ToList();

            //create stat for percentage of team goals scored by player

            var allPlayersByBps = players.OrderByDescending(m => m.bps).ToList();
            var allGoalkeepersByBps = players.Where(x => x.element_type == 1).OrderByDescending(m => m.bps).ToList();
            var allDefendersByBps = players.Where(x => x.element_type == 2).OrderByDescending(m => m.bps).ToList();
            var allMidfieldersByBps = players.Where(x => x.element_type == 3).OrderByDescending(m => m.bps).ToList();
            var allForwardsByBps = players.Where(x => x.element_type == 4).OrderByDescending(m => m.bps).ToList();

            foreach (var player in players)
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

                player.CostInterval = new CostInterval
                {
                    Value = player.now_cost / 10
                };

            }

            var allTeamsJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("teams"))
                .Children<JObject>();

            List<Team> allTeams = new List<Team>();

            foreach (JObject result in allTeamsJSON)
            {
                Team t = result.ToObject<Team>();
                allTeams.Add(t);

                foreach (Player player in players)
                {
                    if (t.id == player.team)
                    {
                        player.Team = t;
                    }
                }
            }

            var response1 = await client.GetAsync("fixtures/");

            response1.EnsureSuccessStatusCode();

            var content1 = await response1.Content.ReadAsStringAsync();

            List<Game> games = JsonConvert.DeserializeObject<List<Game>>(content1);

            List<Game> fixtures = games.FindAll(x => x.started == false);
            List<Game> results = games.FindAll(x => x.finished == true);

            foreach (Player player in players)
            {
                player.Team.Fixtures = new List<Game>();
                player.Team.Results = new List<Game>();

                foreach (Game fixture in fixtures)
                {
                    if (player.team == fixture.team_h || player.team == fixture.team_a)
                    {
                        player.Team.Fixtures.Add(fixture);
                    }
                }

                foreach (Game playerFixture in player.Team.Fixtures)
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
                    if (player.team == result.team_h || player.team == result.team_a)
                    {
                        player.Team.Results.Add(result);
                    }
                }

                foreach (Game playerResult in player.Team.Results)
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

            //avg points for player in same value range
            var allPlayersByCost = players.OrderByDescending(m => m.now_cost).ToList();
            var mostExpensivePlayerValueInterval = allPlayersByCost.FirstOrDefault().now_cost / 10;
            var leastExpensivePlayerValueInterval = allPlayersByCost.LastOrDefault().now_cost / 10;
            var playerValueIntervalRange = (mostExpensivePlayerValueInterval - leastExpensivePlayerValueInterval) + 1;

            //var allGoalkeepersByTotalPoints = players.Where(x => x.element_type == 1).OrderByDescending(m => m.total_points).ToList();
            //var allDefendersByTotalPoints = players.Where(x => x.element_type == 2).OrderByDescending(m => m.total_points).ToList();
            //var allMidfieldersByTotalPoints = players.Where(x => x.element_type == 3).OrderByDescending(m => m.total_points).ToList();
            //var allForwardsByTotalPoints = players.Where(x => x.element_type == 4).OrderByDescending(m => m.total_points).ToList();

            for (var i = leastExpensivePlayerValueInterval; i <= mostExpensivePlayerValueInterval; i++)
            {
                List<Player> playersCostInterval = players.FindAll(x => x.CostInterval.Value == i).OrderByDescending(m => m.total_points).ToList();
                var allGoalkeepersByTotalPoints = playersCostInterval.Where(x => x.element_type == 1).OrderByDescending(m => m.total_points).ToList();
                var allDefendersByTotalPoints = playersCostInterval.Where(x => x.element_type == 2).OrderByDescending(m => m.total_points).ToList();
                var allMidfieldersByTotalPoints = playersCostInterval.Where(x => x.element_type == 3).OrderByDescending(m => m.total_points).ToList();
                var allForwardsByTotalPoints = playersCostInterval.Where(x => x.element_type == 4).OrderByDescending(m => m.total_points).ToList();

                foreach (var p in playersCostInterval)
                {
                    p.CostInterval.PointsRanking = playersCostInterval.IndexOf(p) + 1;

                    if (p.element_type == 1)
                    {
                        p.CostInterval.PointsPositionRanking = allGoalkeepersByTotalPoints.IndexOf(p) + 1;
                    }
                    else if (p.element_type == 2)
                    {
                        p.CostInterval.PointsPositionRanking = allDefendersByTotalPoints.IndexOf(p) + 1;
                    }
                    if (p.element_type == 3)
                    {
                        p.CostInterval.PointsPositionRanking = allMidfieldersByTotalPoints.IndexOf(p) + 1;
                    }
                    else if (p.element_type == 4)
                    {
                        p.CostInterval.PointsPositionRanking = allForwardsByTotalPoints.IndexOf(p) + 1;
                    }
                }
            }

            foreach (Player player in players)
            {
                foreach (Game result in player.Team.Results)
                {
                    if (player.team == result.team_h)
                    {
                        List<PlayerStat> bps = result.stats[9].h;

                        foreach (PlayerStat playerStat in bps)
                        {
                            if (playerStat.element == player.id)
                            {
                                player.GamesPlayed++;
                            }
                        }
                    }
                    else if (player.team == result.team_a)
                    {
                        List<PlayerStat> bps = result.stats[9].a;

                        foreach (PlayerStat playerStat in bps)
                        {
                            if (playerStat.element == player.id)
                            {
                                player.GamesPlayed++;
                            }
                        }
                    }
                }

                if (player.GamesPlayed != 0)
                {
                    player.MinsPlayedPercentage = Math.Round((player.minutes / (player.Team.Results.Count * 90m)) * 100m, 1);
                }
                else
                {
                    player.MinsPlayedPercentage = 0;
                }

            }

            List<PlayerPosition> positions = new List<PlayerPosition>();
            positions = await GetPlayerPositionInfo();

            viewModel.Positions = positions;
            viewModel.AllPlayers = players;
            viewModel.Player = players.OrderBy(item => item.ict_index_rank).First();
            viewModel.TotalPlayerCount = players.Count();

            return View(viewModel);
        }

    }
}