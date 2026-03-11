using IdentityService.ExternalProvider;
using IdentityService.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace IdentityService.Services
{
    public class LinkedInService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public LinkedInService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _configuration = configuration;
        }

        public async Task<string> GetTokenAsync(string code)
        {
            _httpClient.BaseAddress = new Uri(_configuration["LinkedInAuth:TokenUrl"]);
            var redirectUri = _configuration["LinkedInAuth:RedirectUri"];
            var clientId = _configuration["LinkedInAuth:ClientId"];
            var clientSecret = _configuration["LinkedInAuth:ClientSecret"];
            var requestUrl = $"oauth/v2/accessToken?grant_type=authorization_code&code={code}&redirect_uri={redirectUri}&client_id={clientId}&client_secret={clientSecret}";
            var response = await _httpClient.GetAsync(requestUrl);
            var token = JsonConvert.DeserializeObject<LinkedInTokenResponse>(await response.Content.ReadAsStringAsync());
            return token.Access_token;
        }
        private async Task<T> GetAsync<T>(string endpoint, string token)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");
            var response = await _httpClient.GetAsync($"{endpoint}?projection=(id,firstName,email-address,lastName,profilePicture(displayImage~:playableStreams))");
            if (!response.IsSuccessStatusCode)
                return default(T);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }

        public async Task<ExternalProviderUserResource> GetUserFromLinkedInAsync(string token)
        {
            var apiClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.linkedin.com/v2/")
            };
            apiClient.DefaultRequestHeaders.Clear();
            apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");
            var url = "me?projection=(id,firstName,lastName,profilePicture(displayImage~:playableStreams))";
            var response = await apiClient.GetAsync(url);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonObj = JsonConvert.DeserializeObject<object>(jsonResponse).ToString();
            jsonObj = jsonObj.Replace("~", "2");

            var profileObj = JsonConvert.DeserializeObject<UserInfo>(jsonObj);

            //
            apiClient.DefaultRequestHeaders.Clear();
            apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");
            url = "emailAddress?q=members&projection=(elements*(handle~))";
            response = await apiClient.GetAsync(url);
            var jsonResponseEmail = await response.Content.ReadAsStringAsync();
            var emailObj = JsonConvert.DeserializeObject<object>(jsonResponseEmail).ToString();
            emailObj = emailObj.Replace("~", "1");
            var profileEmailObj = JsonConvert.DeserializeObject<UserEmail>(emailObj);
            profileObj.email = profileEmailObj.elements[0].handle1.emailAddress;

            var account = new ExternalProviderUserResource()
            {
                Email = profileObj.email,
                FirstName = profileObj.firstName.localized.es_ES, //Cambiar dependendiendo del la localizacion
                LastName = profileObj.lastName.localized.es_ES,
                Picture = profileObj.profilePicture.displayImage2.elements[0].identifiers[0].file
            };

            return account;
        }
    }
}
