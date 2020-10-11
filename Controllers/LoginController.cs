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

namespace FPL.Controllers
{
    public class LoginController : BaseController
    {
        public String GetFplLoginUrl()
        {
            return "https://users.premierleague.com/accounts/login/";
        }

        public IActionResult Index()
        {
            var viewModel = new LoginViewModel();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] LoginViewModel model)
        {
            var client = new FPLHttpClient();

            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            var loginAttempt = new LoginAttempt
            {
                Login = model.Email,
                Password = model.Password
            };

            var response = await client.PostLoginAsync(handler, loginAttempt);

            response.EnsureSuccessStatusCode();

            var responseCookies = cookies.GetCookies(new Uri(GetFplLoginUrl())).Cast<Cookie>();

            foreach (Cookie cookie in responseCookies)
            {
                Response.Cookies.Append(cookie.Name, cookie.Value);
            }

            var currentGameweek = await GetCurrentGameWeek();

            if (currentGameweek.finished)
            {
                return Json(new LoginViewModel
                {
                    Redirect = "/myteam"
                });
            }
            else
            {
                return Json(new LoginViewModel
                {
                    Redirect = "/points"
                });
            }
        }
    }
}
