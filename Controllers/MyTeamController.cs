using FPL.Attributes;
using FPL.Models;
using FPL.Models.FPL;
using FPL.ViewModels.FPL;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Team = FPL.Models.Team;
using FPLTeam = FPL.Models.FPL.Team;
using System.Globalization;
using FPL.Contracts;

namespace FPL.Controllers
{
    [FPLCookie]
    public class MyTeamController : BaseController
    {
        private static int teamId;

        public MyTeamController(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "My Team";

            var viewModel = new MyTeamViewModel();

            HttpClientHandler handler = new HttpClientHandler();

            int currentGwId = await GetCurrentGameWeekId();

            if (Request.Cookies["teamId"] == null) teamId = await GetTeamId();
            else teamId = Convert.ToInt32(Request.Cookies["teamId"]);
            
            var response = await _httpClient.GetAuthAsync(CreateHandler(handler), $"my-team/{teamId}");

            if (!response.IsSuccessStatusCode)
            {
                return RedirectToAction("Error", "Home");
            }

            //look for 404 error here

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var myTeamJSON = JObject.Parse(content);

            MyTeam myTeam = JsonConvert.DeserializeObject<MyTeam>(myTeamJSON.ToString());

            //var teamPicksJSON = AllChildren(myTeamJSON)
            //    .First(c => c.Type == JTokenType.Array && c.Path.Contains("picks"))
            //    .Children<JObject>();

            //JObject transfersObject = (JObject)myTeamJSON["transfers"];
            //TransferInfo transferInfo = transfersObject.ToObject<TransferInfo>();

            List<Pick> teamPicks = new List<Pick>();
            List<Pick> teamPicksLastWeek = new List<Pick>();
            List<Transfer> transfers = new List<Transfer>();
            List<PlayerPosition> positions = new List<PlayerPosition>();

            //foreach (JObject result in teamPicksJSON)
            //{
            //    Pick p = result.ToObject<Pick>();
            //    teamPicks.Add(p);
            //}

            if (currentGwId > 1)
            {
                teamPicksLastWeek = await GetLastWeeksTeam(teamPicksLastWeek, teamId, currentGwId);

                foreach (var p in myTeam.picks)
                {
                    if (teamPicksLastWeek.FindAll(x => x.element == p.element).Count == 0)
                    {
                        p.IsNewTransfer = true;
                    }
                }
            }

            int gameweekId = currentGwId;
            var eventStatus = await GetEventStatus();
            var lastEvent = eventStatus.status.LastOrDefault();
            var isEventFinished = false;

            if (lastEvent != null)
            {
                if (lastEvent.bonus_added && eventStatus.leagues != "Updating")
                {
                    isEventFinished = true;
                }

            }

            transfers = await GetTeamTransfers(teamId);
            positions = await GetPlayerPositionInfo();
            myTeam.picks = await AddPlayerSummaryDataToTeam(myTeam.picks, gameweekId);
            myTeam.picks = await AddPlayerGameweekDataToTeam(myTeam.picks, gameweekId);
            myTeam.picks = await CalculateTotalPointsContributed(myTeam.picks, transfers, gameweekId);
            myTeam.picks = myTeam.picks.OrderBy(x => x.position).ToList();
            FPLTeam teamDetails = await GetTeamInfo(teamId);
            var entryHistory = await GetEntryHistory(teamId, currentGwId);
            var completeEntryHistory = new CompleteEntryHistory();
            completeEntryHistory = await GetCompleteEntryHistory(completeEntryHistory, teamId);
            entryHistory = await AddExtraDatatoEntryHistory(entryHistory, completeEntryHistory);
            entryHistory.BuyingPower = CalculateTeamBuyingPower(myTeam.picks, entryHistory);

            viewModel.MyTeam = myTeam;
            viewModel.CurrentGwId = gameweekId;
            //viewModel.Picks = teamPicks;
            viewModel.Team = teamDetails;
            viewModel.Positions = positions;
            viewModel.TotalPoints = teamDetails.summary_overall_points ?? 0;
            //viewModel.TransferInfo = transferInfo;
            viewModel.IsEventFinished = isEventFinished;
            viewModel.EntryHistory = entryHistory;
            viewModel.CompleteEntryHistory = completeEntryHistory;
            viewModel.EventStatus = eventStatus;

            return View(viewModel);
        }

        private int CalculateTeamBuyingPower(List<Pick> picks, EntryHistory entryHistory)
        {
            var buyingPower = entryHistory.bank;

            foreach (var pick in picks)
            {
                buyingPower += pick.selling_price;
            }

            return buyingPower;
        }

        private async Task<EntryHistory> GetEntryHistory(int teamId, int gameweekId)
        {
            if (gameweekId != 0)
            {
                var response = await _httpClient.GetAsync($"entry/{teamId}/event/{gameweekId}/picks/");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var entryHistoryJSON = AllChildren(JObject.Parse(content))
                    .First(c => c.Type == JTokenType.Object && c.Path.Contains("entry_history"));

                EntryHistory entryHistory = new EntryHistory();

                entryHistory = entryHistoryJSON.ToObject<EntryHistory>();

                return entryHistory;
            }
            else
            {
                return new EntryHistory();
            }

        }

        public async Task<MyTeam> MakeSub(int sub1, int sub1Position, int sub1ElementType, bool isSub1Captain, bool isSub1ViceCaptain, int sub2, int sub2Position, int sub2ElementType)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler = CreateHandler(handler);

            var response = await _httpClient.GetAuthAsync(handler, $"my-team/{teamId}");

            //look for 404 error here

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var myTeamJSON = JObject.Parse(content);

            MyTeam myTeam = JsonConvert.DeserializeObject<MyTeam>(myTeamJSON.ToString());

            //myTeam.picks = await AddPlayerSummaryDataToTeam(myTeam.picks, gameweekId);

            var payload = new SubPayload();

            payload.chip = null;

            foreach (var pick in myTeam.picks)
            {
				var subPick = new SubPick {
					element = pick.element,
                    element_type = pick.player.element_type,
                    multiplier = pick.multiplier,
					is_captain = pick.is_captain,
					is_vice_captain = pick.is_vice_captain
				};

				if (pick.element == sub1)
                {
	                subPick.position = sub2Position;
                    if (isSub1Captain) subPick.is_captain = false;
                    if (isSub1ViceCaptain) subPick.is_vice_captain = false;
                }
                else if (pick.element == sub2)
                {
	                subPick.position = sub1Position;
	                if (isSub1Captain)
	                {
		                subPick.is_captain = true;
		                subPick.multiplier = 2;
	                }
	                if (isSub1ViceCaptain) subPick.is_vice_captain = true;
                }
                else
                {
	                subPick.position = pick.position;
                }

                payload.picks.Add(subPick);
            }

            //need to figure out a way to ensure players get put in correct position, currently subs only work if the pick coming on plays in the same position as the pick coming off

			//var picks = payload.picks.Where(x => x.position < 12).OrderBy(x => x.position).ToList();
			var starter = payload.picks.Find(x => x.element == sub1);
            var sub = payload.picks.Find(x => x.element == sub2);
			payload.picks = AssignStarterPosition(payload.picks.Where(x => x.multiplier > 0).OrderBy(x => x.position).ToList(), starter, sub);

			var json = JsonConvert.SerializeObject(payload);

            handler = ResetHandler(handler);

            response = await _httpClient.PostAuthAsync(handler, $"my-team/{teamId}/", json);

            response.EnsureSuccessStatusCode();

            content = await response.Content.ReadAsStringAsync();

            return myTeam;
        }

        private List<SubPick> AssignStarterPosition(List<SubPick> picks, SubPick starter, SubPick sub) {
            var starterElementType = starter.element_type;
            var subElementType = sub.element_type;

            var defs = picks.FindAll(x => x.element_type == 2);
            var mids = picks.FindAll(x => x.element_type == 3);
            var fwds = picks.FindAll(x => x.element_type == 4);

            if (starterElementType == subElementType) {
                var playersInPosition = picks.FindAll(x => x.element_type == starterElementType && x.multiplier > 0);
                var playersInBehindPosition = picks.FindAll(x => x.element_type == (starterElementType - 1) && x.multiplier > 0);

                for (var i = 0; i < playersInPosition.Count; i++) {
                    playersInPosition[i].position = playersInBehindPosition.LastOrDefault().position + (i + 1);
                }
            } else {
                //midfielder coming off & forward coming on
                if (starterElementType == 3 && subElementType == 4) {
                    for (var i = 0; i < mids.Count; i++) {
                        mids[i].position = defs.LastOrDefault().position + (i + 1);
                    }

                    for (var i = 0; i < fwds.Count; i++) {
                        fwds[i].position = mids.LastOrDefault().position + (i + 1);
                    }
                }
                //midfielder coming off & defender coming on
                else if (starterElementType == 3 && subElementType == 2) {
                    for (var i = 0; i < defs.Count; i++) {
                        defs[i].position = (i + 2);
                    }

                    for (var i = 0; i < mids.Count; i++) {
                        mids[i].position = defs.LastOrDefault().position + (i + 1);
                    }
                }
                //fwd coming off & defender coming on
                else if (starterElementType == 4 && subElementType == 2) {
                    for (var i = 0; i < defs.Count; i++) {
                        defs[i].position = (i + 2);
                    }

                    for (var i = 0; i < mids.Count; i++) {
                        mids[i].position = defs.LastOrDefault().position + (i + 1);
                    }

                    for (var i = 0; i < fwds.Count; i++) {
                        fwds[i].position = mids.LastOrDefault().position + (i + 1);
                    }
                }
                //fwd coming off & mid coming on
                else if (starterElementType == 4 && subElementType == 3) {
                    for (var i = 0; i < mids.Count; i++) {
                        mids[i].position = defs.LastOrDefault().position + (i + 1);
                    }

                    for (var i = 0; i < fwds.Count; i++) {
                        fwds[i].position = mids.LastOrDefault().position + (i + 1);
                    }
                }
                //def coming off & mid coming on
                else if (starterElementType == 2 && subElementType == 3) {
                    for (var i = 0; i < defs.Count; i++) {
                        defs[i].position = (i + 2);
                    }

                    for (var i = 0; i < mids.Count; i++) {
                        mids[i].position = defs.LastOrDefault().position + (i + 1);
                    }
                }
                //def coming off & fwd coming on
                else if (starterElementType == 2 && subElementType == 4) {
                    for (var i = 0; i < defs.Count; i++) {
                        defs[i].position = (i + 2);
                    }

                    for (var i = 0; i < mids.Count; i++) {
                        mids[i].position = defs.LastOrDefault().position + (i + 1);
                    }

                    for (var i = 0; i < fwds.Count; i++) {
                        fwds[i].position = mids.LastOrDefault().position + (i + 1);
                    }
                }
            }

            return picks.OrderBy(x => x.position).ToList();
        }

        private async Task<List<Pick>> GetLastWeeksTeam(List<Pick> teamPicksLastWeek, int teamId, int currentGwId)
        {
            var response = await _httpClient.GetAsync($"entry/{teamId}/event/{currentGwId}/picks/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var teamPicksLastWeekJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("picks"))
                .Children<JObject>();

            teamPicksLastWeek = new List<Pick>();

            foreach (JObject result in teamPicksLastWeekJSON)
            {
                Pick p = result.ToObject<Pick>();
                teamPicksLastWeek.Add(p);
            }

            return teamPicksLastWeek;
        }

        private async Task<List<Pick>> AddPlayerSummaryDataToTeam(List<Pick> teamPicks, int gameweekId)
        {
            var response = await _httpClient.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var allPlayersJSON = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("elements"))
                .Children<JObject>();

            List<Player> allPlayers = new List<Player>();

            foreach (JObject result in allPlayersJSON)
            {
                Player p = result.ToObject<Player>();
                allPlayers.Add(p);

                foreach (Pick pick in teamPicks)
                {
                    if (p.id == pick.element)
                    {
                        pick.player = p;
                        pick.NetProfitOnTransfer = pick.selling_price - pick.purchase_price;
                    }
                }
            }

            foreach (var player in allPlayers)
            {
                player.FplIndex = (float)player.bps + float.Parse(player.ict_index, CultureInfo.InvariantCulture.NumberFormat);
            }

            var allPlayersByBps = allPlayers.OrderByDescending(m => m.bps).ToList();
            var allGoalkeepersByBps = allPlayers.Where(x => x.element_type == 1).OrderByDescending(m => m.bps).ToList();
            var allDefendersByBps = allPlayers.Where(x => x.element_type == 2).OrderByDescending(m => m.bps).ToList();
            var allMidfieldersByBps = allPlayers.Where(x => x.element_type == 3).OrderByDescending(m => m.bps).ToList();
            var allForwardsByBps = allPlayers.Where(x => x.element_type == 4).OrderByDescending(m => m.bps).ToList();

            var allPlayersByFplIndex = allPlayers.OrderByDescending(m => m.FplIndex).ToList();
            var allGoalkeepersByFplIndex = allPlayers.Where(x => x.element_type == 1).OrderByDescending(m => m.FplIndex).ToList();
            var allDefendersByFplIndex = allPlayers.Where(x => x.element_type == 2).OrderByDescending(m => m.FplIndex).ToList();
            var allMidfieldersByFplIndex = allPlayers.Where(x => x.element_type == 3).OrderByDescending(m => m.FplIndex).ToList();
            var allForwardsByFplIndex = allPlayers.Where(x => x.element_type == 4).OrderByDescending(m => m.FplIndex).ToList();

            foreach (var player in allPlayers)
            {
                player.BpsRank = allPlayersByBps.IndexOf(player) + 1;
                player.FplRank = allPlayersByFplIndex.IndexOf(player) + 1;

                if (player.element_type == 1)
                {
                    player.BpsPositionRank = allGoalkeepersByBps.IndexOf(player) + 1;
                    player.FplPositionRank = allGoalkeepersByFplIndex.IndexOf(player) + 1;
                }
                else if (player.element_type == 2) 
                {
                    player.BpsPositionRank = allDefendersByBps.IndexOf(player) + 1;
                    player.FplPositionRank = allDefendersByFplIndex.IndexOf(player) + 1;
                }
                if (player.element_type == 3)
                {
                    player.BpsPositionRank = allMidfieldersByBps.IndexOf(player) + 1;
                    player.FplPositionRank = allMidfieldersByFplIndex.IndexOf(player) + 1;
                }
                else if (player.element_type == 4)
                {
                    player.BpsPositionRank = allForwardsByBps.IndexOf(player) + 1;
                    player.FplPositionRank = allForwardsByFplIndex.IndexOf(player) + 1;
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
                    if (t.id == pick.player.TeamId)
                    {
                        pick.player.Team = t;
                    }
                }
            }

            var response1 = await _httpClient.GetAsync("fixtures/");

            response1.EnsureSuccessStatusCode();

            var content1 = await response1.Content.ReadAsStringAsync();

            List<Game> games = JsonConvert.DeserializeObject<List<Game>>(content1);

            List<Game> fixtures = games.FindAll(x => x.started == false);
            List<Game> results = games.FindAll(x => x.started == true);

            foreach (Pick pick in teamPicks)
            {
                pick.player.Team.Fixtures = new List<Game>();
                pick.player.Team.Results = new List<Game>();

                foreach (Game fixture in fixtures)
                {
                    if (pick.player.TeamId == fixture.team_h || pick.player.TeamId == fixture.team_a)
                    {
                        pick.player.Team.Fixtures.Add(fixture);
                    }
                }

                foreach (Game playerFixture in pick.player.Team.Fixtures)
                {
                    foreach (Team t in allTeams)
                    {
                        if (playerFixture.team_h == t.id)
                        {
                            playerFixture.team_h_name = t.name;
                            playerFixture.team_h_short_name = t.short_name;
                        }

                        if (playerFixture.team_a == t.id)
                        {
                            playerFixture.team_a_name = t.name;
                            playerFixture.team_a_short_name = t.short_name;
                        }
                    }
                }

                foreach (Game result in results)
                {
                    if (pick.player.TeamId == result.team_h || pick.player.TeamId == result.team_a)
                    {
                        pick.player.Team.Results.Add(result);
                    }

                    Stat totalBps = result.stats[9];

                    List<PlayerStat> homeBps = totalBps.h;
                    List<PlayerStat> awayBps = totalBps.a;
                    List<PlayerStat> allPlayersInGameBps = homeBps.Concat(awayBps).ToList();
                    allPlayersInGameBps = allPlayersInGameBps.OrderByDescending(x => x.value).ToList();

                    for (var i = 0; i < allPlayersInGameBps.Count; i++)
                    {
                        if (pick.element == allPlayersInGameBps[i].element)
                        {
                            //currently only calculates players total bps ranking - need to find away to get average
                            pick.player.AvgBpsRank += allPlayersInGameBps.IndexOf(allPlayersInGameBps[i]) + 1;
                        }
                    }
                }

                foreach (Game playerResult in pick.player.Team.Results)
                {
                    foreach (Team t in allTeams)
                    {
                        if (playerResult.team_h == t.id)
                        {
                            playerResult.team_h_name = t.name;
                            playerResult.team_h_short_name = t.short_name;
                        }

                        if (playerResult.team_a == t.id)
                        {
                            playerResult.team_a_name = t.name;
                            playerResult.team_a_short_name = t.short_name;
                        }
                    }
                }
            }

            foreach (Pick pick in teamPicks)
            {
                if (gameweekId != 0 && pick.player.Team.Results.Count > 0)
                {
                    pick.player.MinsPlayedPercentage = Math.Round((pick.player.minutes / (pick.player.Team.Results.Count * 90m)) * 100m, 1);

                    if (pick.player.MinsPlayedPercentage > 100)
                    {
                        pick.player.MinsPlayedPercentage = 100;
                    }
                }
                else
                {
                    pick.player.MinsPlayedPercentage = 0;
                }
            }

            return teamPicks;
        }

        public async Task<List<Pick>> AddPlayerGameweekDataToTeam(List<Pick> teamPicks, int gameweekId)
        {
            var response1 = await _httpClient.GetAsync("fixtures/?event=" + gameweekId);

            response1.EnsureSuccessStatusCode();

            var content1 = await response1.Content.ReadAsStringAsync();

            List<Game> gwGames = JsonConvert.DeserializeObject<List<Game>>(content1);

            gwGames = await PopulateGameListWithTeams(gwGames);
            List<Game> startedGames = gwGames.FindAll(x => x.started == true);

            foreach (Pick pick in teamPicks)
            {
                foreach (Game g in gwGames)
                {
                    if (pick.player.TeamId == g.team_h)
                    {
                        pick.GWOppositionName = g.AwayTeam.name;
                        pick.GWGames.Add(g);
                        continue;
                    }
                    else if (pick.player.TeamId == g.team_a)
                    {
                        pick.GWOppositionName = g.HomeTeam.name;
                        pick.GWGames.Add(g);
                        continue;
                    }
                }

            }

            return teamPicks;
        }

        private async Task<List<Pick>> CalculateTotalPointsContributed(List<Pick> teamPicks, List<Transfer> teamTransfers, int gameweekId)
        {
            //int currentGwId = await GetCurrentGameWeekId();

            foreach (Pick pick in teamPicks)
            {
                foreach (Transfer transfer in teamTransfers)
                {
                    if (pick.element == transfer.element_in)
                    {
                        pick.HadSinceGW = transfer.@event;
                        var gw = gameweekId - pick.HadSinceGW;

                        if (gw != 0)
                        {
                            pick.GWOnTeam = (gameweekId - pick.HadSinceGW) + 1;
                        }
                        else
                        {
                            pick.GWOnTeam = 1;
                        }
                        break;
                    }
                }

                if (pick.HadSinceGW == 1 && !pick.IsNewTransfer)
                {
                    pick.GWOnTeam = gameweekId;
                }
                else if (pick.IsNewTransfer)
                {
                    pick.GWOnTeam = 0;
                }
            }

            //calculate how many points a player has gained for the team, taking into account how long they have actually been on it 

            foreach (Pick pick in teamPicks)
            {
                var response = await _httpClient.GetAsync($"element-summary/" + pick.element + "/");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var playerHistoryJSON = AllChildren(JObject.Parse(content))
                    .First(c => c.Type == JTokenType.Array && c.Path.Contains("history"))
                    .Children<JObject>();

                var playerFixturesJSON = AllChildren(JObject.Parse(content))
                    .First(c => c.Type == JTokenType.Array && c.Path.Contains("fixtures"))
                    .Children<JObject>();

                List<PlayerHistory> playerHistory = new List<PlayerHistory>();
                List<PlayerFixture> playerFixtures = new List<PlayerFixture>();

                foreach (JObject result in playerHistoryJSON)
                {
                    PlayerHistory ph = result.ToObject<PlayerHistory>();
                    playerHistory.Add(ph);
                }

                foreach (PlayerHistory playerGwHistory in playerHistory)
                {
                    if (pick.HadSinceGW <= playerGwHistory.round)
                    {
                        pick.TotalPointsAccumulatedForTeam += playerGwHistory.total_points;
                        continue;
                    }

                }
                
                if (pick.GWOnTeam != 0)
                {
                    pick.PPGOnTeam = (float)pick.TotalPointsAccumulatedForTeam / (float)pick.GWOnTeam;
                }
                else
                {
                    pick.PPGOnTeam = 0;
                }

                foreach (JObject result in playerFixturesJSON)
                {
                    PlayerFixture pf = result.ToObject<PlayerFixture>();
                    playerFixtures.Add(pf);
                }

                pick.player.Fixtures = playerFixtures;

                //foreach (PlayerFixture playerFixture in playerFixtures)
                //{

                //    if (pick.HadSinceGW <= playerGwHistory.round)
                //    {
                //        pick.TotalPointsAccumulatedForTeam += playerGwHistory.total_points;
                //    }
                //}
            }

            return teamPicks;
        }

        private async Task<List<Transfer>> GetTeamTransfers(int teamId)
        {
            var response = await _httpClient.GetAsync($"entry/{teamId}/transfers/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            List<Transfer> transfers = JsonConvert.DeserializeObject<List<Transfer>>(content);

            var response1 = await _httpClient.GetAsync("bootstrap-static/");

            response1.EnsureSuccessStatusCode();

            var content1 = await response1.Content.ReadAsStringAsync();

            var allPlayersJSON = AllChildren(JObject.Parse(content1))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("elements"))
                .Children<JObject>();

            List<Player> allPlayers = new List<Player>();

            foreach (JObject result in allPlayersJSON)
            {
                Player p = result.ToObject<Player>();

                foreach (Transfer transfer in transfers)
                {
                    if (transfer.element_in == p.id)
                    {
                        transfer.PlayerIn = p;
                    }

                    if (transfer.element_out == p.id)
                    {
                        transfer.PlayerOut = p;
                    }
                }
            }

            return transfers;
        }

        private async Task<FPLTeam> GetTeamInfo(int teamId)
        {
            HttpClientHandler handler = new HttpClientHandler();

            handler = CreateHandler(handler);

            var response = await _httpClient.GetAuthAsync(handler, $"entry/{teamId}/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            FPLTeam teamDetails = JsonConvert.DeserializeObject<FPLTeam>(content);

            return teamDetails;

        }
    }
}
