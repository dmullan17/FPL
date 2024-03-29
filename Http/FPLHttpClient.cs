﻿using FPL.Contracts;
using FPL.Models;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FPL.Http
{
    public sealed class FPLHttpClient : IHttpClient
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

        private readonly IDictionary<string, string> _headers;

        public FPLHttpClient()
        {
            _headers = new Dictionary<string, string>();
        }

        public string GetBaseUrl()
        {
            return "https://fantasy.premierleague.com/api/";
        }

        public string GetLoginUrl()
        {
            return "https://users.premierleague.com/accounts/login/";
        }

        public void SetHeader(string key, string value)
        {
            if (_headers.ContainsKey(key))
            {
                _headers.Remove(key);
            }

            _headers.Add(key, value);
        }

        public async Task<HttpResponseMessage> GetAsync(string resource)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(GetBaseUrl()) })
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Add("X-Fern-Token", ConfigurationManager.AppSettings["ApiToken"]);
                //client.DefaultRequestHeaders.Add("User-Agent", ConfigurationManager.AppSettings["UserAgent"]);
                client.Timeout = DefaultTimeout;

                foreach (var kvp in _headers)
                {
                    client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }

                HttpResponseMessage response;

                try
                {
                    response = await client.GetAsync(resource).ConfigureAwait(false);
                }
                catch (HttpRequestException)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                return response;
            }
        }

        public async Task<HttpResponseMessage> GetAuthAsync(HttpClientHandler handler, string resource)
        {
            using (var client = new HttpClient(handler) { BaseAddress = new Uri(GetBaseUrl()) })
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = DefaultTimeout;

                foreach (var kvp in _headers)
                {
                    client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }

                HttpResponseMessage response;

                try
                {
                    response = await client.GetAsync(resource).ConfigureAwait(false);
                }
                catch (HttpRequestException)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                return response;
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string resource, IDictionary<string, string> parameters)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(GetBaseUrl()) })
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Add("X-Fern-Token", ConfigurationManager.AppSettings["ApiToken"]);
                //client.DefaultRequestHeaders.Add("User-Agent", ConfigurationManager.AppSettings["UserAgent"]);
                client.Timeout = DefaultTimeout;

                foreach (var kvp in _headers)
                {
                    client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }

                HttpResponseMessage response;

                try
                {
                    response = await client.GetAsync(ParseParameters(resource, parameters)).ConfigureAwait(false);
                }
                catch (HttpRequestException)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                return response;
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string resource, string body)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(GetBaseUrl()) })
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = DefaultTimeout;

                foreach (var kvp in _headers)
                {
                    client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }

                HttpResponseMessage response;

                try
                {
                    response = await client
                        .PostAsync(resource, new StringContent(body, Encoding.UTF8, "application/json"))
                        .ConfigureAwait(false);
                }
                catch (HttpRequestException)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                return response;
            }
        }

        public async Task<HttpResponseMessage> PostLoginAsync(HttpClientHandler handler, LoginAttempt body)
        {
            using (var client = new HttpClient(handler) { BaseAddress = new Uri(GetLoginUrl()) })
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("authority", "users.premierleague.com");
                client.DefaultRequestHeaders.Add("cache-control", "max-age=0");
                client.DefaultRequestHeaders.Add("upgrade-insecure-requests", "1");
                client.DefaultRequestHeaders.Add("origin", "https://fantasy.premierleague.com");
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36");
                client.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                client.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");
                client.DefaultRequestHeaders.Add("sec-fetch-mode", "navigate");
                client.DefaultRequestHeaders.Add("sec-fetch-user", "?1");
                client.DefaultRequestHeaders.Add("sec-fetch-dest", "document");
                client.DefaultRequestHeaders.Add("referer", "https://fantasy.premierleague.com");
                client.DefaultRequestHeaders.Add("accept-language", "en-US,en;q=0.9,he;q=0.8");
        
                client.Timeout = DefaultTimeout;

                IList<KeyValuePair<string, string>> fplLoginRequest = new List<KeyValuePair<string, string>> {
                    { new KeyValuePair<string, string>("login", body.Login) },
                    { new KeyValuePair<string, string>("password", body.Password) },
                    { new KeyValuePair<string, string>("redirect_uri", "https://fantasy.premierleague.com/") },
                    { new KeyValuePair<string, string>("app", "plfpl-web") },
                };

                foreach (var kvp in _headers)
                {
                    client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }

                HttpResponseMessage response;

                try
                {
                    response = await client
                        .PostAsync(GetLoginUrl(), new FormUrlEncodedContent(fplLoginRequest))
                        .ConfigureAwait(false);

                }
                catch (HttpRequestException)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                return response;
            }
        }

        public async Task<HttpResponseMessage> PostAuthAsync(HttpClientHandler handler, string resource, string body)
        {
            using (var client = new HttpClient(handler) { BaseAddress = new Uri(GetBaseUrl()) })
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = DefaultTimeout;

                foreach (var kvp in _headers)
                {
                    client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }

                HttpResponseMessage response;

                try
                {
                    response = await client
                        .PostAsync(resource, new StringContent(body, Encoding.UTF8, "application/json"))
                        .ConfigureAwait(false);
                }
                catch (HttpRequestException)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                return response;
            }
        }

        public async Task<HttpResponseMessage> PutAsync(string resource, string body)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(GetBaseUrl()) })
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Add("X-Fern-Token", ConfigurationManager.AppSettings["ApiToken"]);
                //client.DefaultRequestHeaders.Add("User-Agent", ConfigurationManager.AppSettings["UserAgent"]);
                client.Timeout = DefaultTimeout;

                foreach (var kvp in _headers)
                {
                    client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }

                HttpResponseMessage response;

                try
                {
                    response = await client
                        .PutAsync(resource, new StringContent(body, Encoding.UTF8, "application/json"))
                        .ConfigureAwait(false);
                }
                catch (HttpRequestException)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                return response;
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(string resource)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(GetBaseUrl()) })
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Add("X-Fern-Token", ConfigurationManager.AppSettings["ApiToken"]);
                //client.DefaultRequestHeaders.Add("User-Agent", ConfigurationManager.AppSettings["UserAgent"]);
                client.Timeout = DefaultTimeout;

                foreach (var kvp in _headers)
                {
                    client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }

                HttpResponseMessage response;

                try
                {
                    response = await client
                        .DeleteAsync(resource)
                        .ConfigureAwait(false);
                }
                catch (HttpRequestException)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                return response;
            }
        }

        private static string ParseParameters(string resource, IDictionary<string, string> parameters)
        {
            var stringBuilder = new StringBuilder(resource);

            if (parameters.Keys.Count > 0)
            {
                stringBuilder.Append("?");
            }

            foreach (var kvp in parameters)
            {
                stringBuilder.Append($"{kvp.Key}={kvp.Value}&");
            }

            var result = stringBuilder.ToString();

            return result.EndsWith("&") ? result.Substring(0, result.Length - 1) : result;
        }

    }
}
