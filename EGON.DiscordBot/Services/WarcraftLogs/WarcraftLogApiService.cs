using EGON.DiscordBot.Models.WarcraftLogs;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EGON.DiscordBot.Services.WarcraftLogs
{
    public class WarcraftLogsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WarcraftLogsApiService> _logger;
        private string? _accessToken;
        private DateTime _tokenExpiry;

        public WarcraftLogsApiService(
            HttpClient httpClient,
            ILogger<WarcraftLogsApiService> logger)
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

        private async Task<TResponse?> PostToApiAsync<TRequest, TResponse>(TRequest? payload, string? endpoint = null) 
        {
            var accessToken = await GetAccessTokenAsync();

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No valid access token available.");
                return default;
            }

            string uri = "https://www.warcraftlogs.com/api/v2/client";

            if (!string.IsNullOrWhiteSpace(endpoint))
                uri += endpoint;

            HttpRequestMessage request;

            if (payload is not null)
            {
                request = new HttpRequestMessage(HttpMethod.Post, uri)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },
                    Content = JsonContent.Create(payload)
                };
            }
            else
            {
                request = new HttpRequestMessage(HttpMethod.Post, uri)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) }
                };
            }

            try
            {
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>();
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

        public async IAsyncEnumerable<Actor?> GetPlayerAttendanceAsync(string raidId)
        {
            var requestPayload = new
            {
                query = "query($code: String!) { reportData { report(code: $code) { masterData { actors { id name type subType server } } } } }",
                variables = new { code = raidId }
            };

            var jsonResponse = await PostToApiAsync<object, JsonDocument>(requestPayload);
            if (jsonResponse == null) yield break;

            var root = jsonResponse.RootElement;
            var actorsElement = root
                .GetProperty("data")
                .GetProperty("reportData")
                .GetProperty("report")
                .GetProperty("masterData")
                .GetProperty("actors");

            foreach (var actorElement in actorsElement.EnumerateArray())
            {
                string type = actorElement.GetProperty("type").GetString() ?? string.Empty;

                if (type != "Player")
                    continue;

                yield return new Actor
                {
                    Id = actorElement.GetProperty("id").GetInt32(),
                    Name = actorElement.GetProperty("name").GetString() ?? string.Empty,
                    Type = type,
                    SubType = actorElement.TryGetProperty("subType", out var subType) ? subType.GetString() : null,
                    Server = actorElement.TryGetProperty("server", out var server) ? server.GetString() : null
                };
            }
        }
    }
}
