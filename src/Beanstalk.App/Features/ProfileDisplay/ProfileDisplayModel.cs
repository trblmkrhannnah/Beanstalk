using System.Collections.Generic;

namespace Beanstalk.App.Features.ProfileDisplay;

public sealed class ProfileDisplayModel
{
    public string? Title { get; set; }
    public string? Bio { get; set; }

    public string? ImageUrl { get; set; }

    public ProfileDisplayThemeModel Theme { get; set; } = new();

    public List<ProfileDisplayLinkModel> Links { get; set; } = [];
}