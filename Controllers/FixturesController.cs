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
    public class FixturesController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            var viewModel = new FixturesViewModel();

            var client = new FPLHttpClient();

            var response = await client.GetAsync("fixtures/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            List<Game> games = JsonConvert.DeserializeObject<List<Game>>(content);

            List<GameWeek> gameWeeks = await GetAllGameWeeks();
            GameWeek currentGameWeek = gameWeeks.FirstOrDefault(a => a.is_current);
            if (currentGameWeek.finished)
            {
                currentGameWeek = gameWeeks.FirstOrDefault(a => a.is_next);
            }

            List<Game> currentGameWeekGames = new List<Game>();     
            List<Game> liveGames = new List<Game>();

            foreach (Game g in games)
            {
                if (g.Event == currentGameWeek.id)
                {
                    currentGameWeekGames.Add(g);
                }

                if (g.started ?? false)
                {
                    if (!g.finished || !g.finished_provisional)
                    {
                        if (!g.finished_provisional){
                            liveGames.Add(g);
                        }
                    }
                }

                if (g.finished)
                {
                    if (g.team_h_score > g.team_a_score)
                    {
                        g.did_team_h_win = true;
                    }
                    else if (g.team_a_score > g.team_h_score)
                    {
                        g.did_team_a_win = true;
                    }
                }
            }

            currentGameWeekGames = await PopulateFixtureListByGameWeekId(currentGameWeekGames, currentGameWeek.id);

            liveGames = await PopulateFixtureListByGameWeekId(liveGames, currentGameWeek.id);

            viewModel.LiveGames = liveGames;
            viewModel.CurrentGameweek = currentGameWeek;
            viewModel.Fixtures = currentGameWeekGames;
            viewModel.CurrentGameweekId = currentGameWeek.id;

            return View(viewModel);
        }

        [HttpGet]
        [Route("fixtures/{id}")]
        public async Task<IActionResult> Index(int id)
        {
            var viewModel = new FixturesViewModel();

            var client = new FPLHttpClient();

            var response = await client.GetAsync("fixtures/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            List<Game> games = JsonConvert.DeserializeObject<List<Game>>(content);
            List<Game> currentGameWeekGames = new List<Game>();
            List<Game> liveGames = new List<Game>();

            
            foreach (Game g in games)
            {
                if (g.Event == id)
                {
                    currentGameWeekGames.Add(g);
                }

                
                if (g.started ?? false)
                {
                    if (!g.finished || !g.finished_provisional)
                    {
                        if (!g.finished_provisional){
                            liveGames.Add(g);
                        }
                    }
                }

                if (g.finished)
                {
                    if (g.team_h_score > g.team_a_score)
                    {
                        g.did_team_h_win = true;
                    }
                    else if (g.team_a_score > g.team_h_score)
                    {
                        g.did_team_a_win = true;
                    }
                }
            }

            liveGames = await PopulateFixtureListByGameWeekId(liveGames, id);
            currentGameWeekGames = await PopulateFixtureListByGameWeekId(currentGameWeekGames, id);

            viewModel.LiveGames = liveGames;
            viewModel.Fixtures = currentGameWeekGames;
            viewModel.CurrentGameweekId = id;

            return View(viewModel);
        }


        //populates a specified team's fixtures and results
        private async Task<List<Game>> PopulateFixtureListByGameWeekId(List<Game> games, int gameWeekId)
        {
            var client = new FPLHttpClient();

            var response = await client.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var allTeams = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("teams"))
                .Children<JObject>();

            List<Team> teams = new List<Team>();

            foreach (JObject result in allTeams)
            {
                Team t = result.ToObject<Team>();

                teams.Add(t);
            }

            //populate games with Home and Away Team deets
            for (var i = 0; i < games.Count; i++)
            {
                var homeTeamId = games[i].team_h;
                var awayTeamId = games[i].team_a;

                games[i].HomeTeam = teams.Find(x => x.id == homeTeamId);
                games[i].AwayTeam = teams.Find(x => x.id == awayTeamId);

            }
            var allPlayers = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("elements"))
                .Children<JObject>();

            List<Player> allPlayers1 = new List<Player>();

            foreach (JObject result in allPlayers)
            {
                Player p = result.ToObject<Player>();
                allPlayers1.Add(p);
            }

            //populate home stats
            for (var h = 0; h < games.Count; h++)
            {
                for (var i = 0; i < games[h].stats.Count; i++)
                {
                    for (var j = 0; j < games[h].stats[i].h.Count; j++)
                    {
                        int playerId = games[h].stats[i].h[j].element;

                        for (var k = 0; k < allPlayers1.Count; k++)
                        {
                            if (allPlayers1[k].id == playerId)
                            {
                                games[h].stats[i].h[j].Player = allPlayers1[k];
                            }
                        }
                    }
                }
            }

            //populate away stats
            for (var h = 0; h < games.Count; h++)
            {
                for (var i = 0; i < games[h].stats.Count; i++)
                {
                    for (var j = 0; j < games[h].stats[i].a.Count; j++)
                    {
                        int playerId = games[h].stats[i].a[j].element;

                        for (var k = 0; k < allPlayers1.Count; k++)
                        {
                            if (allPlayers1[k].id == playerId)
                            {
                                games[h].stats[i].a[j].Player = allPlayers1[k];
                            }
                        }
                    }
                }
            }

            return games;
        }

    }
}