using System;

namespace Beanstalk.Database.Entities;

public class ProfileLink
{
    public Guid Id { get; set; }

    public Guid ProfileId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsVisible { get; set; } = true;

    public UserProfile? Profile { get; set; }
}