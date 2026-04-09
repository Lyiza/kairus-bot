namespace KairusBot.Models;

public sealed class UserSearchSession
{
    public SearchStep CurrentStep { get; set; } = SearchStep.None;

    public string? SelectedRegion { get; set; }

    public string? SelectedCity { get; set; }

    public string? SelectedMood { get; set; }

    public string? SelectedRoadType { get; set; }
}

