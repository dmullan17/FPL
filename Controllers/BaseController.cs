using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FPL.Http;
using FPL.Models;
using FPL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FPL.Controllers
{
    public class BaseController : Controller
    {
        public static readonly int TeamId = 2675560;

        public string GetBaseUrl()
        {
            return "https://fantasy.premierleague.com/api/";
        }

        public async Task<List<PlayerPosition>> GetPlayerPositionInfo()
        {
            var client = new FPLHttpClient();

            var response = await client.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var resultObjects = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("element_types"))
                .Children<JObject>();

            List<PlayerPosition> playerPositions = new List<PlayerPosition>();

            foreach (JObject result in resultObjects)
            {
                PlayerPosition pp = result.ToObject<PlayerPosition>();
                playerPositions.Add(pp);
            }

            return playerPositions;
        }

        public async Task<GameWeek> GetCurrentGameWeek()
        {
            var client = new FPLHttpClient();

            var response = await client.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            //events = gameweeks
            var resultObjects = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("events"))
                .Children<JObject>();

            List<GameWeek> gws = new List<GameWeek>();

            foreach (JObject result in resultObjects)
            {
                GameWeek gw = result.ToObject<GameWeek>();
                gws.Add(gw);
            }

            GameWeek currentGameweek = gws.FirstOrDefault(a => a.is_current);

            return currentGameweek;
        }

        public async Task<int> GetCurrentGameWeekId()
        {
            var client = new FPLHttpClient();

            var response = await client.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            //events = gameweeks
            var resultObjects = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("events"))
                .Children<JObject>();

            List<GameWeek> gws = new List<GameWeek>();

            foreach (JObject result in resultObjects)
            {
                GameWeek gw = result.ToObject<GameWeek>();
                gws.Add(gw);
            }

            GameWeek currentGameweek = gws.FirstOrDefault(a => a.is_current);

            return currentGameweek.id;
        }

        public async Task<List<GameWeek>> GetAllGameWeeks()
        {
            var viewModel = new GameWeekViewModel();

            var client = new FPLHttpClient();

            var response = await client.GetAsync("bootstrap-static/");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            //events = gameweeks
            var resultObjects = AllChildren(JObject.Parse(content))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("events"))
                .Children<JObject>();

            List<GameWeek> gws = new List<GameWeek>();

            foreach (JObject result in resultObjects)
            {
                GameWeek gw = result.ToObject<GameWeek>();

                gws.Add(gw);

                //foreach (JProperty property in result.Properties())
                //{

                //}
            }

            GameWeek currentGameweek = gws.FirstOrDefault(a => a.is_current);

            // viewModel.Gameweeks = gws;
            // viewModel.CurrentGameweek = currentGameweek;

            return gws;
        }

        public static IEnumerable<JToken> AllChildren(JToken json)
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

        public HttpClientHandler CreateHandler(HttpClientHandler handler)
        {
            CookieContainer cookies = new CookieContainer();
            handler.CookieContainer = cookies;
            Uri target = new Uri(GetBaseUrl());

            foreach (string s in Request.Cookies.Keys)
            {
                string cookieValue = Request.Cookies[s];
                cookies.Add(new Cookie(s, cookieValue) { Domain = target.Host });
            }

            return handler;
        }

    }
}