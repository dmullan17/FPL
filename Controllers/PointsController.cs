﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FPL.Attributes;
using FPL.Http;
using FPL.Models;
using FPL.Models.FPL;
using FPL.ViewModels;
using FPL.ViewModels.FPL;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Team = FPL.Models.Team;
using FPLTeam = FPL.Models.FPL.Team;
using FPL.Models.GWPlayerStats;

namespace FPL.Controllers
{
    [FPLCookie]
    public class PointsController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentGameweekId = await GetCurrentGameWeekId();

            var viewModel = new GameweekPointsViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            var client = new FPLHttpClient();

            var response = await client.GetAuthAsync(CreateHandler(handler), $"entry/{TeamId}/event/{currentGameweekId}/picks/");

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

            var teamAutoSubsJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("automatic_subs"))
                .Children<JObject>();

            List<AutomaticSub> autoSubs = new List<AutomaticSub>();

            foreach (JObject result in teamAutoSubsJSON)
            {
                AutomaticSub sub = result.ToObject<AutomaticSub>();
                autoSubs.Add(sub);
            }

            GWTeam gwTeam = new GWTeam
            {
                picks = teamPicks,
                automatic_subs = autoSubs
            };

            teamPicks = await AddPlayerSummaryDataToTeam(teamPicks);
            teamPicks = await AddPlayerGameweekDataToTeam(teamPicks, currentGameweekId);
            int gwpoints = GetGameWeekPoints(teamPicks);
            FPLTeam teamDetails = await GetTeamInfo();

            viewModel.GWTeam = gwTeam;
            viewModel.Team = teamDetails;
            viewModel.GWPoints = gwpoints;
            viewModel.TotalPoints = (teamDetails.summary_overall_points - teamDetails.summary_event_points) + gwpoints;
            viewModel.GameweekId = currentGameweekId;

            return View(viewModel);
        }

        [HttpGet]
        [Route("points/{id}")]
        public async Task<IActionResult> Index(int id)
        {
            var viewModel = new GameweekPointsViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            var client = new FPLHttpClient();

            int currentGwId = await GetCurrentGameWeekId();

            if (id > currentGwId)
            {
                return RedirectToAction("Index", new { id = currentGwId });
            }

            var response = await client.GetAuthAsync(CreateHandler(handler), $"entry/{TeamId}/event/{id}/picks/");

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

            var teamAutoSubsJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("automatic_subs"))
                .Children<JObject>();

            List<AutomaticSub> autoSubs = new List<AutomaticSub>();

            foreach (JObject result in teamAutoSubsJSON)
            {
                AutomaticSub sub = result.ToObject<AutomaticSub>();
                autoSubs.Add(sub);
            }

            GWTeam gwTeam = new GWTeam
            {
                picks = teamPicks,
                automatic_subs = autoSubs
            };

            teamPicks = await AddPlayerSummaryDataToTeam(teamPicks);
            teamPicks = await AddPlayerGameweekDataToTeam(teamPicks, id);
            int gwpoints = GetGameWeekPoints(teamPicks);
            FPLTeam teamDetails = await GetTeamInfo();

            viewModel.GWTeam = gwTeam;
            viewModel.Team = teamDetails;
            viewModel.GWPoints = gwpoints;
            viewModel.TotalPoints = teamDetails.summary_overall_points;
            viewModel.GameweekId = id;

            return View(viewModel);

        }

        private int GetGameWeekPoints(List<Pick> teamPicks)
        {
            int gwpoints = 0;

            foreach (Pick pick in teamPicks)
            {
                while (pick.position < 12)
                {
                    gwpoints += pick.GWPlayer.stats.gw_points;
                    break;
                }
            }

            return gwpoints;
        }

        private async Task<FPLTeam> GetTeamInfo()
        {
            HttpClientHandler handler = new HttpClientHandler();

            handler = CreateHandler(handler);

            var client = new FPLHttpClient();

            var response = await client.GetAuthAsync(handler, $"entry/{TeamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            FPLTeam teamDetails = JsonConvert.DeserializeObject<FPLTeam>(content);

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

            var allTeamsJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("teams"))
                .Children<JObject>();

            List<Team> allTeams = new List<Team>();

            foreach (JObject result in allTeamsJSON)
            {
                Team t = result.ToObject<Team>();
                allTeams.Add(t);

                foreach (Pick pick in teamPicks)
                {
                    if (t.id == pick.player.team)
                    {
                        pick.player.Team = t;
                    }
                }
            }

            //var response1 = await client.GetAsync("fixtures/");

            //response1.EnsureSuccessStatusCode();

            //var content1 = await response1.Content.ReadAsStringAsync();

            //List<Game> games = JsonConvert.DeserializeObject<List<Game>>(content1);

            //List<Game> fixtures = games.FindAll(x => x.started == false);
            //List<Game> results = games.FindAll(x => x.finished == true);

            //foreach (Pick pick in teamPicks)
            //{
            //    pick.player.Team.Fixtures = new List<Game>();
            //    pick.player.Team.Results = new List<Game>();

            //    foreach (Game fixture in fixtures)
            //    {
            //        if (pick.player.team == fixture.team_h || pick.player.team == fixture.team_a)
            //        {
            //            pick.player.Team.Fixtures.Add(fixture);
            //        }
            //    }

            //    foreach (Game playerFixture in pick.player.Team.Fixtures)
            //    {
            //        foreach (Team t in allTeams)
            //        {
            //            if (playerFixture.team_h == t.id)
            //            {
            //                playerFixture.team_h_name = t.name;
            //            }

            //            if (playerFixture.team_a == t.id)
            //            {
            //                playerFixture.team_a_name = t.name;
            //            }
            //        }
            //    }

            //    foreach (Game result in results)
            //    {
            //        if (pick.player.team == result.team_h || pick.player.team == result.team_a)
            //        {
            //            pick.player.Team.Results.Add(result);
            //        }
            //    }

            //    foreach (Game playerResult in pick.player.Team.Results)
            //    {
            //        foreach (Team t in allTeams)
            //        {
            //            if (playerResult.team_h == t.id)
            //            {
            //                playerResult.team_h_name = t.name;
            //            }

            //            if (playerResult.team_a == t.id)
            //            {
            //                playerResult.team_a_name = t.name;
            //            }
            //        }
            //    }
            //}

            var response2 = await client.GetAsync($"entry/{TeamId}/transfers/");

            response2.EnsureSuccessStatusCode();

            var content2 = await response2.Content.ReadAsStringAsync();

            List<Transfer> transfers = JsonConvert.DeserializeObject<List<Transfer>>(content2);

            return teamPicks;
        }

        private async Task<List<Pick>> AddPlayerGameweekDataToTeam(List<Pick> teamPicks, int gameweekId)
        {
            var client = new FPLHttpClient();

            //get player stats specific to the gameweek
            var response = await client.GetAsync("event/" + gameweekId + "/live/");

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

            var response1 = await client.GetAsync("fixtures/?event=" + gameweekId);

            response1.EnsureSuccessStatusCode();

            var content1 = await response1.Content.ReadAsStringAsync();

            List<Game> gwGames = JsonConvert.DeserializeObject<List<Game>>(content1);

            gwGames = await PopulateGameListWithTeams(gwGames);

            foreach (Pick pick in teamPicks)
            {
                foreach (Game g in gwGames)
                {
                    if (pick.player.team == g.team_h)
                    {
                        pick.GWOppositionName = g.AwayTeam.short_name;
                        pick.GWGame = g;
                    }
                    else if (pick.player.team == g.team_a)
                    {
                        pick.GWOppositionName = g.HomeTeam.short_name;
                        pick.GWGame = g;
                    }
                }
            }
            //get gameweek dreamteam data
            //var response1 = await client.GetAsync("dream-team/" + gameweekId + "/");

            //response1.EnsureSuccessStatusCode();

            //var content1 = await response1.Content.ReadAsStringAsync();

            //RootGWDreamTeam gwDreamTeam = JsonConvert.DeserializeObject<RootGWDreamTeam>(content1);

            //RootGWDreamTeam gwDreamTeam = JsonConvert.DeserializeObject<RootGWDreamTeam>(content1);

            //totalling a players total gw stats
            for (var i = 0; i < teamPicks.Count; i++)
            {
                for (var j = 0; j < teamPicks[i].GWPlayer.explain.Count; j++)
                {
                    for (var k = 0; k < teamPicks[i].GWPlayer.explain[j].stats.Count; k++)
                    {
                        if (teamPicks[i].GWPlayer.explain[j].stats[k].points != 0)
                        {
                            if (teamPicks[i].is_captain)
                            {
                                teamPicks[i].GWPlayer.stats.gw_points += teamPicks[i].GWPlayer.explain[j].stats[k].points * teamPicks[i].multiplier;
                            }
                            else
                            {
                                teamPicks[i].GWPlayer.stats.gw_points += teamPicks[i].GWPlayer.explain[j].stats[k].points;
                            }
                        }
                    }
                }
            }
            return teamPicks;
        }
    }
}
