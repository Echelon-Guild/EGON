using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EGON.DiscordBot.Services.WarcraftLogs
{
    public class WarcraftLogsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WarcraftLogsService> _logger;
        private string? _accessToken;
        private DateTime _tokenExpiry;

        public WarcraftLogsService(
            HttpClient httpClient,
            ILogger<WarcraftLogsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private async Task<string?> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiry > DateTime.UtcNow)
            {
                return _accessToken;
            }

            string clientId = Environment.GetEnvironmentVariable("WARCRAFT_LOGS_CLIENT_ID") ?? throw new EnvironmentNotConfiguredException("WARCRAFT_LOGS_CLIENT_ID");
            string clientSecret = Environment.GetEnvironmentVariable("WARCRAFT_LOGS_CLIENT_SECRET") ?? throw new EnvironmentNotConfiguredException("WARCRAFT_LOGS_CLIENT_SECRET");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.warcraftlogs.com/oauth/token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        });

            try
            {
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<WarcraftLogsTokenResponse>();
                    _accessToken = content?.AccessToken;
                    _tokenExpiry = DateTime.UtcNow.AddSeconds(content?.ExpiresIn ?? 0);
                    return _accessToken;
                }
                else
                {
                    _logger.LogError("Failed to retrieve access token: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the access token.");
            }

            return null;
        }

        public async Task<T?> GetFromApiAsync<T>(string endpoint)
        {
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No valid access token available.");
                return default;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.warcraftlogs.com/api/v2/client{endpoint}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }
                else
                {
                    _logger.LogError("API request failed: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while calling the Warcraft Logs API.");
            }

            return default;
        }
    }
}
