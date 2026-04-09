using System.Text.Json;
using KairusBot.Options;
using Microsoft.Extensions.Options;

namespace KairusBot.Services;

public sealed class VkApiService
{
    private readonly HttpClient _httpClient;
    private readonly VkOptions _options;
    private readonly ILogger<VkApiService> _logger;

    public VkApiService(HttpClient httpClient, IOptions<VkOptions> options, ILogger<VkApiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendMessageAsync(
        long peerId,
        string message,
        string? keyboardJson = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            _logger.LogError("Vk AccessToken is not configured; messages.send skipped");
            return;
        }

        var randomId = Random.Shared.NextInt64();
        var form = new Dictionary<string, string>
        {
            ["peer_id"] = peerId.ToString(),
            ["message"] = message,
            ["random_id"] = randomId.ToString(),
            ["access_token"] = _options.AccessToken,
            ["v"] = "5.199"
        };

        if (!string.IsNullOrWhiteSpace(keyboardJson))
        {
            form["keyboard"] = keyboardJson;
        }

        using var content = new FormUrlEncodedContent(form);

        try
        {
            using var response = await _httpClient.PostAsync(
                "https://api.vk.com/method/messages.send",
                content,
                cancellationToken).ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "VK messages.send HTTP {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body);
                return;
            }

            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var code = error.TryGetProperty("error_code", out var c) ? c.GetInt32() : 0;
                var msg = error.TryGetProperty("error_msg", out var m) ? m.GetString() : null;
                _logger.LogError("VK API error: code={Code} msg={Msg}", code, msg ?? string.Empty);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VK messages.send request failed");
        }
    }

    public async Task SendMessageEventAnswerAsync(
        string eventId,
        long userId,
        long peerId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            _logger.LogError("Vk AccessToken is not configured; messages.sendMessageEventAnswer skipped");
            return;
        }

        if (string.IsNullOrWhiteSpace(eventId))
        {
            _logger.LogWarning("messages.sendMessageEventAnswer skipped: empty eventId");
            return;
        }

        var form = new Dictionary<string, string>
        {
            ["event_id"] = eventId,
            ["user_id"] = userId.ToString(),
            ["peer_id"] = peerId.ToString(),
            ["access_token"] = _options.AccessToken,
            ["v"] = "5.199"
        };

        using var content = new FormUrlEncodedContent(form);

        try
        {
            using var response = await _httpClient.PostAsync(
                "https://api.vk.com/method/messages.sendMessageEventAnswer",
                content,
                cancellationToken).ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "VK messages.sendMessageEventAnswer HTTP {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body);
                return;
            }

            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var code = error.TryGetProperty("error_code", out var c) ? c.GetInt32() : 0;
                var msg = error.TryGetProperty("error_msg", out var m) ? m.GetString() : null;
                _logger.LogError("VK API error: code={Code} msg={Msg}", code, msg ?? string.Empty);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VK messages.sendMessageEventAnswer request failed");
        }
    }
}
