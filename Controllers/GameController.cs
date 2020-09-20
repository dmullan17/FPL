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

            // var resultObjects = AllChildren(JObject.Parse(content))
            //     .First(c => c.Type == JTokenType.Array && c.Path.Contains("stats"))
            //     .Children<JObject>();

            // List<Stat> stats = new List<Stat>();

            // foreach (JObject result in resultObjects)
            // {
            //     Stat s = result.ToObject<Stat>();

            //     stats.Add(s);

            //     //foreach (JProperty property in result.Properties())
            //     //{

            //     //}
            // }

            // var test = games[2].stats[0].home[0].value;

            viewModel.AllGames = games;
            viewModel.Game = games.FirstOrDefault(item => item.finished);
            viewModel.GameStats = games.FirstOrDefault(item => item.finished).stats;
            viewModel.TotalGameCount = games.Count();

            return View(viewModel);
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