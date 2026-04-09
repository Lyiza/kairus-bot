using System.Text.Json.Serialization;

namespace KairusBot.Models;

public sealed class ResultInlinePayload
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("route_name")]
    public string RouteName { get; set; } = string.Empty;

    [JsonPropertyName("start_coordinates")]
    public string StartCoordinates { get; set; } = string.Empty;
}