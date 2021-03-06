﻿using FPL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FPL.Contracts
{
    public interface IHttpClient
    {
        string GetBaseUrl();

        void SetHeader(string key, string value);

        Task<HttpResponseMessage> GetAsync(string resource);

        Task<HttpResponseMessage> GetAsync(string resource, IDictionary<string, string> parameters);

        Task<HttpResponseMessage> PostAsync(string resource, string body);

        Task<HttpResponseMessage> PostLoginAsync(HttpClientHandler handler, LoginAttempt body);

        Task<HttpResponseMessage> PostAuthAsync(HttpClientHandler handler, string resource, string body);

        Task<HttpResponseMessage> PutAsync(string resource, string body);

        Task<HttpResponseMessage> DeleteAsync(string resource);
        Task<HttpResponseMessage> GetAuthAsync(HttpClientHandler handler, string resource);
    }
}
