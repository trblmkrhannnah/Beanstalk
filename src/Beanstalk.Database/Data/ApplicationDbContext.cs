using System;
using Beanstalk.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beanstalk.Database.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Image> Images => Set<Image>();
    public DbSet<ProfileLink> ProfileLinks => Set<ProfileLink>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<ThemePalette> ThemePalettes => Set<ThemePalette>();
    public DbSet<InviteLink> InviteLinks => Set<InviteLink>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity => { entity.Property(u => u.IsPublished).HasDefaultValue(true); });

        builder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.CreatedUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.SelectedImage)
                .WithMany()
                .HasForeignKey(p => p.SelectedImageId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.ThemePalette)
                .WithMany()
                .HasForeignKey(p => p.ThemePaletteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Image>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ImageData).IsRequired();
            entity.Property(i => i.ThumbnailData).IsRequired(false);
            entity.Property(i => i.ContentType).IsRequired();
            entity.Property(i => i.CreatedUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(i => i.Profile)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProfileLink>(ProfileLinkConfigure);

        builder.Entity<AppSetting>(entity => { entity.HasKey(a => a.Key); });

        builder.Entity<ThemePalette>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired();
            entity.Property(t => t.BackgroundGradient1).IsRequired();
            entity.Property(t => t.BackgroundGradient2).IsRequired();
            entity.Property(t => t.TitleColor).IsRequired();
            entity.Property(t => t.ContentColor).IsRequired();
            entity.Property(t => t.ContainerBackground).IsRequired();
            entity.Property(t => t.ContainerForeground).IsRequired();
            entity.Property(t => t.IsDefault).HasDefaultValue(false);
        });

        builder.Entity<InviteLink>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Code).IsRequired();
            entity.HasIndex(i => i.Code).IsUnique();
            entity.HasIndex(i => i.IsActive);
            entity.Property(i => i.CreatedUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(i => i.CurrentUses).HasDefaultValue(0);
            entity.Property(i => i.IsActive).HasDefaultValue(true);
            entity.Property(i => i.AutoDeleteWhenExpired).HasDefaultValue(false);

            entity.HasOne(i => i.CreatedBy)
                .WithMany()
                .HasForeignKey(i => i.CreatedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ProfileLinkConfigure(EntityTypeBuilder<ProfileLink> entity)
    {
        entity.HasKey(l => l.Id);
        entity.Property(l => l.Title).IsRequired();
        entity.Property(l => l.Url).IsRequired();
        entity.Property(l => l.IsVisible).HasDefaultValue(true);

        entity.HasOne(l => l.Profile)
            .WithMany(p => p.Links)
            .HasForeignKey(l => l.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(l => new { l.ProfileId, l.SortOrder });
    }
}