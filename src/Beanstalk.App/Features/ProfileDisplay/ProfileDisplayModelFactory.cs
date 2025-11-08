using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beanstalk.Database.Data;
using Microsoft.EntityFrameworkCore;

namespace Beanstalk.App.Features.ProfileDisplay;

public sealed class ProfileDisplayModelFactory
{
    private readonly ApplicationDbContext _context;

    public ProfileDisplayModelFactory(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProfileDisplayModel?> TryGetProfile(string username, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName!.ToLower() == username.ToLower(), cancellationToken: cancellationToken);

        if (user is not { IsEnabled: true })
            return null;

        var contextProfile = await _context.UserProfiles
            .Include(p => p.Images)
            .Include(p => p.ThemePalette)
            .FirstOrDefaultAsync(p => p.UserId == user.Id, cancellationToken);

        if (contextProfile == null)
            return null;

        var links = await _context.ProfileLinks
            .Where(l => l.ProfileId == contextProfile.Id && l.IsVisible)
            .OrderBy(l => l.SortOrder).Select(l => new ProfileDisplayLinkModel
            {
                Title = l.Title,
                Url = l.Url
            })
            .ToListAsync(cancellationToken);

        var contextPalette = contextProfile.ThemePalette ?? await _context.ThemePalettes.FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);

        return new ProfileDisplayModel
        {
            Title = contextProfile.DisplayName,
            Bio = contextProfile.Bio,
            Theme = contextPalette != null
                ? new ProfileDisplayThemeModel
                {
                    BackgroundGradient1 = contextPalette.BackgroundGradient1,
                    BackgroundGradient2 = contextPalette.BackgroundGradient2,
                    TitleForeground = contextPalette.TitleColor,
                    ContentForeground = contextPalette.ContentColor,
                    ContainerBackground = contextPalette.ContainerBackground,
                    ContainerForeground = contextPalette.ContainerForeground
                }
                : new ProfileDisplayThemeModel(),
            ImageUrl = contextProfile.SelectedImageId.HasValue ? $"/api/images/{contextProfile.SelectedImageId}" : null,
            Links = links
        };
    }
}