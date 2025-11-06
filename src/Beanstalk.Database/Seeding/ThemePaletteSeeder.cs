using System;
using System.Collections.Generic;
using System.Linq;
using Beanstalk.Database.Data;
using Beanstalk.Database.Entities;

namespace Beanstalk.Database.Seeding;

public sealed class ThemePaletteSeeder
{
    public ThemePalette OceanBreeze { get; } = CreatePalette("Ocean Breeze", "#667eea", "#764ba2", "#ffffff", "#f0f0f0", "#ffffff", "#667eea", true);

    public ThemePalette SunsetGlow { get; } = CreatePalette("Sunset Glow", "#ff6b6b", "#feca57", "#ffffff", "#ffffff", "#ffffff", "#ff6b6b");

    public ThemePalette ForestDeep { get; } = CreatePalette("Forest Deep", "#0f4c3a", "#1e8467", "#ffffff", "#e0e0e0", "#ffffff", "#0f4c3a");

    public ThemePalette MidnightSky { get; } = CreatePalette("Midnight Sky", "#1a1a2e", "#16213e", "#eee", "#ddd", "#0f3460", "#e94560");

    public ThemePalette CherryBlossom { get; } = CreatePalette("Cherry Blossom", "#ff9a9e", "#fecfef", "#5a1f47", "#7a3f67", "#5a1f47", "#ffffff");

    public ThemePalette ArcticIce { get; } = CreatePalette("Arctic Ice", "#a8edea", "#fed6e3", "#2c3e50", "#34495e", "#2c3e50", "#ffffff");

    public ThemePalette DesertSand { get; } = CreatePalette("Desert Sand", "#d4a574", "#f4e4d7", "#3e2723", "#5d4037", "#6d4c41", "#ffffff");

    public ThemePalette NeonNights { get; } = CreatePalette("Neon Nights", "#7f00ff", "#e100ff", "#ffffff", "#f0f0f0", "#00ff88", "#000000");

    public ThemePalette AutumnHarvest { get; } = CreatePalette("Autumn Harvest", "#c94b4b", "#f4a261", "#ffffff", "#fefae0", "#2a9d8f", "#ffffff");

    public ThemePalette Monochrome { get; } = CreatePalette("Monochrome", "#2c2c2c", "#1a1a1a", "#ffffff", "#cccccc", "#ffffff", "#000000");

    public IEnumerable<ThemePalette> AllPalettes => typeof(ThemePaletteSeeder).GetProperties().Where(p => p.PropertyType == typeof(ThemePalette)).Select(p => (ThemePalette)p.GetValue(this)!);

    public void SeedIfRequired(ApplicationDbContext context)
    {
        if (context.ThemePalettes.Any())
            return;

        var palettes = new ThemePaletteSeeder().AllPalettes.ToList();

        context.ThemePalettes.AddRange(palettes);
        context.SaveChanges();
    }

    private static ThemePalette CreatePalette(string name, string backgroundGradient1, string backgroundGradient2, string titleColor, string contentColor, string containerBackground, string containerForeground, bool isDefault = false)
    {
        return new ThemePalette
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsDefault = isDefault,
            BackgroundGradient1 = backgroundGradient1,
            BackgroundGradient2 = backgroundGradient2,
            TitleColor = titleColor,
            ContentColor = contentColor,
            ContainerBackground = containerBackground,
            ContainerForeground = containerForeground
        };
    }
}