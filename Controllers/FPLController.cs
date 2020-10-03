using FPL.Http;
using FPL.Models;
using FPL.Models.FPL;
using FPL.Models.GWPlayerStats;
using FPL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FPL.Controllers
{
    [Authorize]
    public class FPLController : BaseController
    {
        private static int TeamId = 2675560;
        private static int GameWeekId = 4;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new FPLViewModel();

            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            Uri target = new Uri("https://fantasy.premierleague.com/api/");

            foreach (string s in Request.Cookies.Keys)
            {
                string cookieValue = Request.Cookies[s];
                cookies.Add(new Cookie(s, cookieValue) { Domain = target.Host } );
            }

            var client = new FPLHttpClient();

            var response = await client.GetAuthAsync(handler, $"my-team/{TeamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var teamPicksJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("picks"))
                .Children<JObject>();

            List<Pick> teamPicks = new List<Pick>();

            foreach (JObject result in teamPicksJSON)
            {
                Pick p = result.ToObject<Pick>();
                teamPicks.Add(p);
            }

            teamPicks = await AddPlayerSummaryDataToTeam(teamPicks);
            teamPicks = await AddPlayerGameweekDataToTeam(teamPicks);

            viewModel.picks = teamPicks;

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

                foreach (Pick pick in teamPicks)
                {
                    if (p.id == pick.element)
                    {
                        pick.player = p;
                    }
                }           
            }

            return teamPicks;
        }

        private async Task<List<Pick>> AddPlayerGameweekDataToTeam(List<Pick> teamPicks)
        {
            var client = new FPLHttpClient();

            //get player stats specific to the gameweek
            var response = await client.GetAsync("event/" + GameWeekId + "/live/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var allPlayersJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("elements"))
                .Children<JObject>();

            List<GWPlayer> allGwPlayers = new List<GWPlayer>();

            foreach (JObject result in allPlayersJSON)
            {
                GWPlayer p = result.ToObject<GWPlayer>();
                allGwPlayers.Add(p);
            }

            foreach (GWPlayer gwplayer in allGwPlayers)
            {
                foreach (Pick pick in teamPicks)
                {
                    if (pick.element == gwplayer.id)
                    {
                        pick.GWPlayer = gwplayer;
                    }
                }
            }

            return teamPicks;
        }
    }
}
