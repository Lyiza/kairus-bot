namespace KairusBot.Options;

public sealed class VkOptions
{
    public long GroupId { get; set; }

    public string ConfirmationCode { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;
}
