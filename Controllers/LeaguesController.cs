using FPL.Attributes;
using FPL.Http;
using FPL.Models;
using FPL.Models.FPL;
using FPL.ViewModels.FPL;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FPL.Controllers
{
    [FPLCookie]
    public class LeaguesController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new LeaguesViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            var client = new FPLHttpClient();

            int teamId = await GetTeamId();

            var response = await client.GetAuthAsync(CreateHandler(handler), $"entry/{teamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var leaguesJSON = JObject.Parse(content);

            JObject leaguesObject = (JObject)leaguesJSON["leagues"];
            Leagues leagues = leaguesObject.ToObject<Leagues>();

            leagues = await AddPlayerStandingsToLeague(leagues);

            viewModel.ClassicLeagues = leagues.classic;
            viewModel.H2HLeagues = leagues.h2h;
            viewModel.Cup = leagues.cup;

            return View(viewModel);
        }

        public async Task<Leagues> AddPlayerStandingsToLeague(Leagues leagues)
        {
            var currentGameWeekId = await GetCurrentGameWeekId();

            var client = new FPLHttpClient();

            //var privateClassicLeagues = leagues.classic.FindAll(x => x.league_type == "x");

            foreach (var l in leagues.classic)
            {
                var response = await client.GetAsync($"leagues-classic/{l.id}/standings");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var leagueJSON = JObject.Parse(content);

                JObject leagueStandingsObject = (JObject)leagueJSON["standings"];

                var leaguePlayersJSON = AllChildren(leagueStandingsObject)
                    .First(c => c.Type == JTokenType.Array && c.Path.Contains("results"))
                    .Children<JObject>();

                foreach (JObject result in leaguePlayersJSON)
                {
                    Result r = result.ToObject<Result>();
                    l.Standings.results.Add(r);
                }

                //foreach (var player in l.Standings.results)
                //{
                //    response = await client.GetAsync($"entry/{player.entry}/event/{currentGameWeekId}/picks/");

                //    response.EnsureSuccessStatusCode();

                //    content = await response.Content.ReadAsStringAsync();

                //    var playerEntryJSON = JObject.Parse(content);

                //    var playerGwPicksJSON = AllChildren(playerEntryJSON)
                //        .First(c => c.Type == JTokenType.Array && c.Path.Contains("picks"))
                //        .Children<JObject>();

                //    foreach (JObject result in playerGwPicksJSON)
                //    {
                //        Pick p = result.ToObject<Pick>();
                //        player.GwPicks.Add(p);
                //    }
                //}
            }




            return leagues;

        }


        public async Task<List<Pick>> GetPlayersTeam(int teamId)
        {
            var currentGameWeekId = await GetCurrentGameWeekId();

            var client = new FPLHttpClient();

            var response = await client.GetAsync($"entry/{teamId}/event/{currentGameWeekId}/picks/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var playerEntryJSON = JObject.Parse(content);

            var playerGwPicksJSON = AllChildren(playerEntryJSON)
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("picks"))
                .Children<JObject>();

            List<Pick> team = new List<Pick>();

            foreach (JObject result in playerGwPicksJSON)
            {
                Pick p = result.ToObject<Pick>();
                team.Add(p);
            }

            response = await client.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            content = await response.Content.ReadAsStringAsync();

            var allPlayersJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("elements"))
                .Children<JObject>();

            List<Player> allPlayers = new List<Player>();

            foreach (JObject result in allPlayersJSON)
            {
                Player p = result.ToObject<Player>();

                foreach (Pick pick in team)
                {
                    if (p.id == pick.element)
                    {
                        pick.player = p;
                    }
                }
            }

            return team;

        }
    }
}
