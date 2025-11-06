using System;

namespace Beanstalk.Database.Entities;

public class ThemePalette
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public string BackgroundGradient1 { get; set; } = string.Empty;

    public string BackgroundGradient2 { get; set; } = string.Empty;

    // TODO: Un-americanise these - I was an idiot and just accepted the spell-check. Fix DB model/migration.
    public string TitleColor { get; set; } = string.Empty;

    public string ContentColor { get; set; } = string.Empty;

    public string ContainerBackground { get; set; } = string.Empty;

    public string ContainerForeground { get; set; } = string.Empty;
}