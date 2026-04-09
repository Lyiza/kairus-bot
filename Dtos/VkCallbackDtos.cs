using System.Text.Json.Serialization;
using System.Text.Json;

namespace KairusBot.Dtos;

public sealed class VkCallbackRequest
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("group_id")]
    public long GroupId { get; set; }

    [JsonPropertyName("object")]
    public VkCallbackObject? Object { get; set; }

    [JsonPropertyName("secret")]
    public string? Secret { get; set; }
}

public sealed class VkCallbackObject
{
    [JsonPropertyName("message")]
    public VkMessage? Message { get; set; }

    [JsonPropertyName("event_id")]
    public string? EventId { get; set; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("peer_id")]
    public long PeerId { get; set; }

    [JsonPropertyName("payload")]
    public JsonElement? Payload { get; set; }
}

public sealed class VkMessage
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("date")]
    public long Date { get; set; }

    [JsonPropertyName("peer_id")]
    public long PeerId { get; set; }

    [JsonPropertyName("from_id")]
    public long FromId { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("payload")]
    public string? Payload { get; set; }
}
