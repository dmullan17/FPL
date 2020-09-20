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

            List<Stat> stats = games.FirstOrDefault(item => item.finished).stats;

            // for (var i = 0; i < stats.Count; i++)
            // {
            //     for (var j = 0; j < stats[i].a.Count; j++)
            //     {
            //         var teamElement = stats[i].a[j].element;
            //     }
            // }  

            Game firstFinishedGame = games.FirstOrDefault(item => item.finished);

            firstFinishedGame = await AddTeamNames(firstFinishedGame);
            // firstFinishedGame = await AddPlayerNamesToStats(firstFinishedGame);

            viewModel.AllGames = games;
            viewModel.Game = firstFinishedGame;
            viewModel.GameStats = firstFinishedGame.stats;
            viewModel.TotalGameCount = games.Count();

            return View(viewModel);
        }

        private async Task<Game> AddTeamNames(Game game)
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

            return game;
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