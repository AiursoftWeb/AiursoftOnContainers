﻿using Aiursoft.Pylon.Exceptions;
using Aiursoft.Pylon.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Aiursoft.Pylon.Services.Authentication.ToMicrosoftServer
{
    public class MicrosoftService : IAuthProvider
    {
        private readonly HTTPService _http;
        private readonly ServiceLocation _serviceLocation;
        private readonly HttpClient _client;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _scope;

        public MicrosoftService(
            HTTPService http,
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            ServiceLocation serviceLocation,
            ILogger<MicrosoftService> logger)
        {
            _http = http;
            _serviceLocation = serviceLocation;
            _client = clientFactory.CreateClient();
            _clientId = configuration["AzureAd:ClientId"];
            _clientSecret = configuration["AzureAd:ClientSecret"];
            _scope = configuration["AzureAd:Scope"];
            if (string.IsNullOrWhiteSpace(_clientId) || string.IsNullOrWhiteSpace(_clientSecret))
            {
                logger.LogWarning("Did not set correct Microsoft credential! You can only access the service property but can execute OAuth process!");
            }
        }

        public string GetName() => "Microsoft";

        public string GetButtonColor() => "dark";

        public string GetButtonIcon() => "microsoft";

        // Line breaks for legibility only

       //https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize?
       //client_id=6731de76-14a6-49ae-97bc-6eba6914391e
       //&response_type=id_token
       //&redirect_uri=http%3A%2F%2Flocalhost%2Fmyapp%2F
       //&scope=openid
       //&response_mode=fragment
       //&state=12345
       //&nonce=678910
        public string GetBindRedirectLink()
        {
            return new AiurUrl("https://login.microsoftonline.com", "/common/oauth2/v2.0/authorize", new MicrosoftAuthAddressModel
            {
                ClientId = _clientId,
                ResponseType = "id_token",
                //RedirectUri = "https://www.baidu.com",
                RedirectUri = new AiurUrl("http://localhost:41066", $"/third-party/bind-account/{GetName()}", new { }).ToString(),
                Scope = "openid", //_scope
                ResponseMode = "fragment",
                State = "12345",
                Nonce= "678910"
            }).ToString();
        }

        public string GetSignInRedirectLink(AiurUrl state)
        {
            return new AiurUrl("https://login.microsoftonline.com", "/common/oauth2/v2.0/authorize", new MicrosoftAuthAddressModel
            {
                ClientId = _clientId,
                ResponseType = "id_token",
                //RedirectUri = "http://localhost:41066/third-party/sign-in/microsoft",
                RedirectUri = new AiurUrl("http://localhost:41066", $"/third-party/sign-in/{GetName()}", new { }).ToString(),
                Scope = "openid", //_scope
                ResponseMode = "fragment",
                State = state.ToString(),
                Nonce = "678910"
            }).ToString();
        }

        public async Task<IUserDetail> GetUserDetail(string code, bool isBinding = false)
        {
            var token = await GetAccessToken(_clientId, _clientSecret, code);
            return await GetUserInfo(token);
        }

        //https://login.microsoftonline.com/common/oauth2/v2.0/token
        private async Task<string> GetAccessToken(string clientId,string clientSecret, string code)
        {
            var apiAddress = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
            var url = new AiurUrl(apiAddress, new MicrosoftAccessTokenAddressModel
            {
                ClientId = clientId,
                ResponseType = code,
                RedirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient",
                ResponseMode = "query",
                Scope = "https://graph.microsoft.com/User.Read",
                State = "12345"
            });
            var json = await _http.Get(url, false);
            var response = JsonConvert.DeserializeObject<AccessTokenResponse>(json);
            if (string.IsNullOrWhiteSpace(response.AccessToken))
            {
                throw new AiurAPIModelException(ErrorType.Unauthorized, "Invalid Microsoft crenditial");
            }
            return response.AccessToken;
        }

        //https://graph.microsoft.com/v1.0/me
        private async Task<MicrosoftUserDetail> GetUserInfo(string accessToken)
        {
            var apiAddress = "https://graph.microsoft.com/v1.0/me";
            var request = new HttpRequestMessage(HttpMethod.Get, apiAddress);

            request.Headers.Add("Authorization", $"token {accessToken}");
            request.Headers.Add("User-Agent", $"curl/7.65.3");

            var response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<MicrosoftUserDetail>(json);
                if (string.IsNullOrWhiteSpace(user.Name))
                {
                    user.Name = Guid.NewGuid().ToString();
                }
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    user.Email = user.Name + $"@from.{GetName().ToLower()}.com";
                }
                return user;
            }
            else
            {
                throw new WebException(response.ReasonPhrase);
            }
        }
    }
}
