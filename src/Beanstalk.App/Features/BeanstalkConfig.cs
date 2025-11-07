using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Beanstalk.Database.Data;
using Beanstalk.Database.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Beanstalk.App.Features;

public sealed class BeanstalkConfig(ApplicationDbContext context, AuthenticationStateProvider authStateProvider, UserManager<ApplicationUser> userManager)
{
    public ConfigItem<bool> RegistrationsEnabled => new("RegistrationsOpen", true, OnGetValue, OnSetValue);
    public ConfigItem<int> MaxImagesPerUser => new("MaxImagesPerUser", 10, OnGetValue, OnSetValue);
    public ConfigItem<long> MaxImageSizeBytes => new("MaxImageSizeBytes", 10 * 1024 * 1024, OnGetValue, OnSetValue);

    private async Task<TValue> OnGetValue<TValue>(string key, TValue defaultValue, CancellationToken cancellationToken) where TValue : IParsable<TValue>
    {
        var configValue = await context.AppSettings.FindAsync(key, cancellationToken);

        if (configValue is not null && TValue.TryParse(configValue.Value, CultureInfo.InvariantCulture, out var parsedValue))
            return parsedValue;

        return defaultValue;
    }

    private async Task OnSetValue<TValue>(string key, TValue value, CancellationToken cancellationToken) where TValue : IParsable<TValue>
    {
        var state = await authStateProvider.GetAuthenticationStateAsync();
        var user = await userManager.GetUserAsync(state.User);

        if (!state.User.IsInRole("Admin"))
            throw new UnauthorizedAccessException();

        var configValue = await context.AppSettings.FindAsync(key, cancellationToken);

        var stringValue = value.ToString();

        if (configValue == null)
        {
            configValue = new AppSetting { Key = key, Value = stringValue };
            context.AppSettings.Add(configValue);
        }
        else
        {
            configValue.Value = stringValue;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public class ConfigItem<TValue>(string key, TValue defaultValue, Func<string, TValue, CancellationToken, Task<TValue>> onGetValue, Func<string, TValue, CancellationToken, Task> onSetValue) where TValue : IParsable<TValue>
    {
        public Task<TValue> Get(CancellationToken cancellationToken = default)
        {
            return onGetValue(key, defaultValue, cancellationToken);
        }

        public Task Set(TValue value, CancellationToken cancellationToken = default)
        {
            return onSetValue(key, value, cancellationToken);
        }
    }
}