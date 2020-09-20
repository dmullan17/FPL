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
    public class TeamController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var viewModel = new TeamViewModel();

            var client = new FPLHttpClient();

            var response = await client.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            //events = gameweeks
            var resultObjects = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("teams"))
                .Children<JObject>();

            List<Team> teams = new List<Team>();

            foreach (JObject result in resultObjects)
            {
                Team t = result.ToObject<Team>();

                teams.Add(t);

                //foreach (JProperty property in result.Properties())
                //{

                //}
            }

            Team team = teams.FirstOrDefault();

            viewModel.AllTeams = teams;
            viewModel.Team = team;

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