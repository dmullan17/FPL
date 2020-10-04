using FPL.Attributes;
using FPL.Http;
using FPL.Models;
using FPL.Models.FPL;
using FPL.Models.GWPlayerStats;
using FPL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Team = FPL.Models.FPL.Team;

namespace FPL.Controllers
{
    [FPLCookie]
    public class FPLController : BaseController
    {
        private static int TeamId = 2675560;

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
            int gwpoints = GetGameWeekPoints(teamPicks);
            Team teamDetails = await GetTeamInfo();

            viewModel.picks = teamPicks;
            viewModel.Team = teamDetails;
            viewModel.GWPoints = gwpoints;
            viewModel.TotalPoints = (teamDetails.summary_overall_points - teamDetails.summary_event_points) + gwpoints;

            return View(viewModel);
        }

        private int GetGameWeekPoints(List<Pick> teamPicks)
        {
            int gwpoints = 0;

            foreach (Pick pick in teamPicks)
            {
                while (pick.position < 12)
                {
                    if (pick.is_captain)
                    {
                        gwpoints += pick.GWPlayer.stats.gw_points * 2;
                    }
                    else
                    {
                        gwpoints += pick.GWPlayer.stats.gw_points;
                    }

                    break;
                }
            }

            return gwpoints;
        }

        private async Task<Team> GetTeamInfo()
        {
            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            Uri target = new Uri("https://fantasy.premierleague.com/api/");

            foreach (string s in Request.Cookies.Keys)
            {
                string cookieValue = Request.Cookies[s];
                cookies.Add(new Cookie(s, cookieValue) { Domain = target.Host });
            }

            var client = new FPLHttpClient();

            var response = await client.GetAuthAsync(handler, $"entry/{TeamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Team teamDetails = JsonConvert.DeserializeObject<Team>(content);

            return teamDetails;

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

            int currentGW = await GetCurrentGameWeek();

            //get player stats specific to the gameweek
            var response = await client.GetAsync("event/" + currentGW + "/live/");

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

            //totalling a players total gw stats
            for (var i = 0; i < teamPicks.Count; i++)
            {
                for (var j = 0; j < teamPicks[i].GWPlayer.explain.Count; j++)
                {
                    for (var k = 0; k < teamPicks[i].GWPlayer.explain[j].stats.Count; k++)
                    {
                        if (teamPicks[i].GWPlayer.explain[j].stats[k].points != 0)
                        {
                            teamPicks[i].GWPlayer.stats.gw_points += teamPicks[i].GWPlayer.explain[j].stats[k].points;
                        }
                    }
                }
            }

            return teamPicks;
        }
    }
}
