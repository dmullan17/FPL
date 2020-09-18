using FPL.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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
