using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FPL.Http;
using FPL.Models;
using FPL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FPL.Controllers
{
    public class PlayerController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var viewModel = new PlayerViewModel();

            var client = new FPLHttpClient();

            var response = await client.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            //elements = players
            var resultObjects = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("elements"))
                .Children<JObject>();

            List<Player> players = new List<Player>();

            foreach (JObject result in resultObjects)
            {
                Player p = result.ToObject<Player>();

                players.Add(p);

                //foreach (JProperty property in result.Properties())
                //{

                //}
            }

            // Player player = players.FirstOrDefault();

            viewModel.AllPlayers = players;

            //top ranked fpl player
            viewModel.Player = players.OrderBy(item => item.ict_index_rank).First();
            viewModel.TotalPlayerCount = players.Count();

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