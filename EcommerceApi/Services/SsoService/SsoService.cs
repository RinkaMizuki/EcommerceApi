using EcommerceApi.ExtensionExceptions;
using System.Net;

namespace EcommerceApi.Services.SsoService
{
    public class SsoService : ISsoService
    {
        private readonly IConfiguration _config;
        public SsoService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> SsoDefaultTokenVerify(string token)
        {
            try
            {
                using var httpClient = new HttpClient();
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{_config.GetSection("SsoBaseUri").Value}api/v1/auth/verify-token"),
                    Headers = {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {token}" },
                            { HttpRequestHeader.Accept.ToString(), "application/json" },
                    },
                };
                var response = await httpClient.SendAsync(httpRequestMessage);
                using HttpContent content = response.Content;

                return await content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new HttpStatusException(ex.StatusCode, ex.Message);
            }
        }

        public async Task<string> SsoFacebookTokenVerify(string fbToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{_config.GetSection("SsoBaseUri").Value}api/v1/auth/verify-facebook-token"),
                    Headers = {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {fbToken}" },
                            { HttpRequestHeader.Accept.ToString(), "application/json" },
                    },
                };
                var response = await httpClient.SendAsync(httpRequestMessage);
                using HttpContent content = response.Content;

                return await content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new HttpStatusException(ex.StatusCode, ex.Message);
            }
        }

        public async Task<string> SsoGoogleTokenVerify(string ggToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{_config.GetSection("SsoBaseUri").Value}api/v1/auth/verify-google-token"),
                    Headers = {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {ggToken}" },
                            { HttpRequestHeader.Accept.ToString(), "application/json" },
                    },
                };
                var response = await httpClient.SendAsync(httpRequestMessage);
                using HttpContent content = response.Content;

                return await content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new HttpStatusException(ex.StatusCode, ex.Message);
            }
        }
    }
}
