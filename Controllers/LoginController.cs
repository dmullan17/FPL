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
        public async Task<IActionResult> Index(LoginAttempt loginAttempt)
        {
            var client = new FPLHttpClient();

            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            var loginAttempt2 = new LoginAttempt
            {
                Login = "dannymullan17@gmail.com",
                Password = "eRx%90zVSWQo"
            };

            var response = await client.PostLoginAsync(handler, GetFplLoginUrl(), loginAttempt2);

            response.EnsureSuccessStatusCode();

            var uri = new Uri(GetFplLoginUrl());

            var responseCookies = cookies.GetCookies(uri).Cast<Cookie>();

            foreach (Cookie cookie in responseCookies)
            {
                Response.Cookies.Append(cookie.Name, cookie.Value);
            }

            return Json(new LoginViewModel
            {
                Redirect = "/home"
            });
        }
    }
}
