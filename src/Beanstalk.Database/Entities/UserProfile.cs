using System;
using System.Collections.Generic;

namespace Beanstalk.Database.Entities;

public class UserProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? DisplayName { get; set; }

    public string? Bio { get; set; }

    public string? BackgroundStyle { get; set; }

    public Guid? ThemePaletteId { get; set; }

    public Guid? SelectedImageId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public ApplicationUser? User { get; set; }

    public ThemePalette? ThemePalette { get; set; }

    public Image? SelectedImage { get; set; }

    public List<Image> Images { get; set; } = new();

    public List<ProfileLink> Links { get; set; } = new();
}