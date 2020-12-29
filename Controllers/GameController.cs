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
    public class GameController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var viewModel = new GameViewModel();

            var client = new FPLHttpClient();

            var response = await client.GetAsync("fixtures/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            List<Game> games = JsonConvert.DeserializeObject<List<Game>>(content);

            // List<Stat> stats = games.FirstOrDefault(item => item.finished).stats;

            Game firstFinishedGame = games.FirstOrDefault(item => item.finished);
            firstFinishedGame.stats = await AddPlayerInfoToStats(firstFinishedGame.stats);
            firstFinishedGame = await AddTeamInfo(firstFinishedGame);

            int liverpoolId = 11;
            List<Game> liverpoolFixtures = games.FindAll(item => item.team_h == liverpoolId || item.team_a == liverpoolId);
            liverpoolFixtures = await PopulateFixtureListByTeamId(liverpoolFixtures, liverpoolId);

            viewModel.AllGames = liverpoolFixtures;
            viewModel.Game = firstFinishedGame;
            viewModel.GameStats = firstFinishedGame.stats;
            viewModel.TotalGameCount = games.Count();

            return View(viewModel);
        }

        private async Task<Game> AddTeamInfo(Game game)
        {
            var client = new FPLHttpClient();

            var response = await client.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var resultObjects = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("teams"))
                .Children<JObject>();

            List<Team> teams = new List<Team>();

            foreach (JObject result in resultObjects)
            {
                Team t = result.ToObject<Team>();

                teams.Add(t);
            }

            var homeTeamId = game.team_h;
            var awayTeamId = game.team_a;

            game.team_h_name = teams.Find(x => x.id == homeTeamId).name;
            game.team_a_name = teams.Find(x => x.id == awayTeamId).name;

            game.HomeTeam = teams.Find(x => x.id == homeTeamId);
            game.AwayTeam = teams.Find(x => x.id == awayTeamId);

            return game;
        }

        private async Task<List<Stat>> AddPlayerInfoToStats(List<Stat> stats)
        {
            var client = new FPLHttpClient();

            var response = await client.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var resultObjects = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("elements"))
                .Children<JObject>();

            List<Player> players = new List<Player>();

            foreach (JObject result in resultObjects)
            {
                Player p = result.ToObject<Player>();

                players.Add(p);

            }

            for (var i = 0; i < stats.Count; i++)
            {
                for (var j = 0; j < stats[i].h.Count; j++)
                {
                    int playerId = stats[i].h[j].element;

                    for (var k = 0; k < players.Count; k++)
                    {

                        if (players[k].id == playerId)
                        {
                            stats[i].h[j].Player = players[k];
                        }
                    }
                }
            }

            for (var i = 0; i < stats.Count; i++)
            {
                for (var j = 0; j < stats[i].a.Count; j++)
                {
                    int playerId = stats[i].a[j].element;

                    for (var k = 0; k < players.Count; k++)
                    {

                        if (players[k].id == playerId)
                        {
                            stats[i].a[j].Player = players[k];
                        }
                    }
                }
            }

            return stats;
        }

        //populates a specified team's fixtures and results
        private async Task<List<Game>> PopulateFixtureListByTeamId(List<Game> games, int teamId)
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

            List<Player> liverpoolPlayers = new List<Player>();

            List<Player> nonLiverpoolPlayers = new List<Player>();
            List<int> nonLiverpoolTeamIds = new List<int>();

            foreach (Game g in games)
            {
                if (g.finished)
                {
                    if (g.team_a != teamId)
                    {
                        nonLiverpoolTeamIds.Add(g.AwayTeam.id);
                    }

                    if (g.team_h != teamId)
                    {
                        nonLiverpoolTeamIds.Add(g.HomeTeam.id);
                    }
                }
            }

            foreach (JObject result in allPlayers)
            {
                Player p = result.ToObject<Player>();

                if (p.TeamId == teamId)
                {
                    liverpoolPlayers.Add(p);
                }

                foreach (int id in nonLiverpoolTeamIds)
                {
                    if (p.TeamId == id)
                    {
                        nonLiverpoolPlayers.Add(p);
                    }
                }
            }

            //populate home stats
            for (var h = 0; h < games.Count; h++)
            {
                for (var i = 0; i < games[h].stats.Count; i++)
                {
                    for (var j = 0; j < games[h].stats[i].h.Count; j++)
                    {
                        int playerId = games[h].stats[i].h[j].element;

                        for (var k = 0; k < liverpoolPlayers.Count; k++)
                        {
                            if (liverpoolPlayers[k].id == playerId)
                            {
                                games[h].stats[i].h[j].Player = liverpoolPlayers[k];
                            }
                        }

                        for (var k = 0; k < nonLiverpoolPlayers.Count; k++)
                        {
                            if (nonLiverpoolPlayers[k].id == playerId)
                            {
                                games[h].stats[i].h[j].Player = nonLiverpoolPlayers[k];
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

                        for (var k = 0; k < liverpoolPlayers.Count; k++)
                        {
                            if (liverpoolPlayers[k].id == playerId)
                            {
                                games[h].stats[i].a[j].Player = liverpoolPlayers[k];
                            }
                        }

                        for (var k = 0; k < nonLiverpoolPlayers.Count; k++)
                        {
                            if (nonLiverpoolPlayers[k].id == playerId)
                            {
                                games[h].stats[i].a[j].Player = nonLiverpoolPlayers[k];
                            }
                        }
                    }
                }
            }

            return games;
        }


        private static IEnumerable<JToken> AllChildren(JToken json)
        {
            foreach (var c in json.Children())
            {
                yield return c;
                foreach (var cc in AllChildren(c))
                {
                    yield return cc;
                }
            }
        }
    }
}