using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FPL.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FPL.ViewModels;
using FPL.Contracts;
using FPL.Http;
using FPL.Models.GWPlayerStats;

namespace FPL.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IHttpClient httpClient)
        {
            //_httpClient.BaseAddress = new Uri("https://fantasy.premierleague.com/api/");
            //_httpClient.Timeout = new TimeSpan(0, 0, 30);
            //_httpClient.DefaultRequestHeaders.Clear();

            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            try
            {
                List<Player> allPlayers = await GetAllPlayers();
                List<Team> allTeams = await GetAllTeams();
                List<Game> allGames = await GetAllGames();
                allGames = await PopulateGameListWithTeams(allGames);
                EventStatus eventStatus = await GetEventStatus();
                var currentGameweek = await GetCurrentGameWeek();
                var gameweekId = currentGameweek.id;

                //var FixturesController = new FixturesController(_httpClient);
                //FixturesController

                if (gameweekId != 0)
                {
                    List<GWPlayer> allGwPlayers = await GetAllGwPlayers(gameweekId);
                    List<Game> gwGames = await GetGwGames(gameweekId);
                    allGwPlayers = await PopulateAllGwPlayers(allGwPlayers, allPlayers, allTeams, allGames);
                    allGwPlayers = AddPlayerGameweekDataToPlayers(gwGames, allGwPlayers);
                    allGwPlayers = AddEstimatedBonusToGwPlayers(allGwPlayers, gwGames);

                    viewModel.Players = allGwPlayers;
                    viewModel.GWGames = gwGames;
                    viewModel.AllGames = allGames;

                }

                //viewModel.Gameweeks = gws;
                viewModel.CurrentGameweek = currentGameweek;
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", "Home");
            }
            
            return View(viewModel);
        }

        public List<GWPlayer> AddPlayerGameweekDataToPlayers(List<Game> gwGames, List<GWPlayer> allGwPlayers)
        {
             List<Game> startedGames = gwGames.FindAll(x => x.started == true);

            foreach (GWPlayer gwPlayer in allGwPlayers)
            {
                if (gwPlayer.stats.EstimatedBonus.Count > 0) gwPlayer.stats.EstimatedBonus.Clear();
                if (gwPlayer.stats.BpsRank.Count > 0) gwPlayer.stats.BpsRank.Clear();

                foreach (Game g in startedGames.Where(x => x.team_h == gwPlayer.player.TeamId || x.team_a == gwPlayer.player.TeamId))
                {
                    Stat totalBps = g.stats[9];
                    List<PlayerStat> homeBps = totalBps.h;
                    List<PlayerStat> awayBps = totalBps.a;
                    List<PlayerStat> allPlayersInGameBps = homeBps.Concat(awayBps).ToList();
                    allPlayersInGameBps = allPlayersInGameBps.OrderByDescending(x => x.value).ToList();
                    List<PlayerStat> topPlayersByBps = allPlayersInGameBps.Take(4).ToList();
                    var playerBps = 0;
                    var player = new PlayerStat();

                    if (gwPlayer.player.TeamId == g.HomeTeam.id)
                    {
                        //playerBps = pick.GWGames.Find(x => x.id == g.id).stats[9].h.Find(y => y.element == pick.element).value;
                        var homeStats = gwGames.Find(x => x.id == g.id).stats[9].h;
                        player = homeStats.FirstOrDefault(x => x.element == gwPlayer.id);
                    }
                    else if (gwPlayer.player.TeamId == g.AwayTeam.id)
                    {
                        var awayStats = gwGames.Find(x => x.id == g.id).stats[9].a;
                        player = awayStats.FirstOrDefault(x => x.element == gwPlayer.id);
                    }

                    //if pick did not play
                    if (player is null)
                    {
                        gwPlayer.stats.EstimatedBonus.Add(0);
                        gwPlayer.stats.BpsRank.Add(0);
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
                        if (gwPlayer.id == allPlayersInGameBps[i].element)
                        {
                            gwPlayer.stats.BpsRank.Add(allPlayersInGameBps.IndexOf(allPlayersInGameBps[i]) + 1);
                            break;
                        }
                    }

                    if (topPlayersByBps.Any(x => x.element == gwPlayer.id))
                    {
                        for (var i = 0; i < topPlayersByBps.Count; i++)
                        {
                            if (IsBpsEqual)
                            {
                                if (topPlayersByBps[0].value == playerBps && topPlayersByBps[1].value == playerBps)
                                {
                                    //pick.GWPlayer.stats.BpsRank.Add(1);
                                    gwPlayer.stats.EstimatedBonus.Add(3);
                                }
                                else if (topPlayersByBps[1].value == playerBps && topPlayersByBps[2].value == playerBps)
                                {
                                    //pick.GWPlayer.stats.BpsRank.Add(2);
                                    gwPlayer.stats.EstimatedBonus.Add(2);
                                }
                                else if (topPlayersByBps[2].value == playerBps && topPlayersByBps[3].value == playerBps)
                                {
                                    //pick.GWPlayer.stats.BpsRank.Add(3);
                                    gwPlayer.stats.EstimatedBonus.Add(1);
                                }
                                break;
                            }
                            else if (topPlayersByBps[i].element == gwPlayer.id && !IsBpsEqual)
                            {
                                if (i == 3)
                                {
                                    gwPlayer.stats.EstimatedBonus.Add(0);
                                }
                                else
                                {
                                    gwPlayer.stats.EstimatedBonus.Add(3 - i);
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        gwPlayer.stats.EstimatedBonus.Add(0);
                    }
                }
            }

            foreach (var gwPlayer in allGwPlayers)
            {
                gwPlayer.stats.gw_points = 0;

                if (gwPlayer.stats.minutes != 0)
                {
                    for (var j = 0; j < gwPlayer.explain.Count; j++)
                    {
                        for (var k = 0; k < gwPlayer.explain[j].stats.Count; k++)
                        {
                                gwPlayer.stats.gw_points += gwPlayer.explain[j].stats[k].points;
                        }
                    }
                }

            }

            return allGwPlayers;

        }

        private List<GWPlayer> AddEstimatedBonusToGwPlayers(List<GWPlayer> gwPlayers, List<Game> gwGames)
        {
            var playersWhosGameHasStarted = new List<GWPlayer>();

            //get picks whos gw games has started
            foreach (GWPlayer gwPlayer in gwPlayers)
            {
                var fixturesAndResults = gwPlayer.player.Team.Fixtures.Concat(gwPlayer.player.Team.Results).ToList();
                var playerGwGames = gwGames.FindAll(x => x.team_h == gwPlayer.team.id || x.team_a == gwPlayer.team.id);

                var games = playerGwGames.FindAll(x => x.started ?? true).ToList();

                if (games.Count > 0)
                {
                    playersWhosGameHasStarted.Add(gwPlayer);
                }
            }

            //add estimated bonus to picks whos games have started if bonus hasn't been applied by FPL yet
            foreach (GWPlayer gwPlayer in playersWhosGameHasStarted)
            {
                var fixturesAndResults = gwPlayer.player.Team.Fixtures.Concat(gwPlayer.player.Team.Results).ToList();
                var playerGwGames = gwGames.FindAll(x => x.team_h == gwPlayer.team.id || x.team_a == gwPlayer.team.id);

                if (!playerGwGames.FindAll(x => x.started ?? true).LastOrDefault().finished)
                {
                    gwPlayer.stats.gw_points += gwPlayer.stats.EstimatedBonus.LastOrDefault();
                }
            }

            return gwPlayers;

        }

        public async Task<List<GWPlayer>> PopulateAllGwPlayers(List<GWPlayer> allGwPlayers, List<Player> allPlayers, List<Team> allTeams, List<Game> allGames)
        {
            foreach (var team in allTeams)
            {

            }

            List<Game> fixtures = allGames.FindAll(x => x.started == false);
            List<Game> results = allGames.FindAll(x => x.finished_provisional == true);

            foreach (Player player in allPlayers)
            {
                player.Team.Fixtures = new List<Game>();
                player.Team.Results = new List<Game>();

                foreach (Game fixture in fixtures)
                {
                    if (player.TeamId == fixture.team_h || player.TeamId == fixture.team_a)
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
                    if (player.TeamId == result.team_h || player.TeamId == result.team_a)
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
                            playerResult.HomeTeam = t;
                        }

                        if (playerResult.team_a == t.id)
                        {
                            playerResult.team_a_name = t.name;
                            playerResult.AwayTeam = t;
                        }
                    }
                }
            }

            var query = from agp in allGwPlayers
                        join ap in allPlayers on agp.id equals ap.id
                        select new GWPlayer
                        {
                            player = ap,
                            id = agp.id,
                            stats = agp.stats,
                            explain = agp.explain
                        };

            var query2 = from agp in query.ToList()
                         join at in allTeams on agp.player.TeamId equals at.id
                         select new GWPlayer
                         {
                             player = agp.player,
                             id = agp.id,
                             stats = agp.stats,
                             explain = agp.explain,
                             team = at
                         };

            var playersWhoPlayed = query2.ToList().FindAll(x => x.stats.minutes > 0);

            return playersWhoPlayed;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
