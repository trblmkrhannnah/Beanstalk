using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Beanstalk.Database.Data;
using Beanstalk.Database.Entities;
using Microsoft.AspNetCore.Identity;

namespace Beanstalk.App.Features;

public sealed class BeanstalkConfig(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
{
    public ConfigItem<int> MaxImagesPerUser => new("MaxImagesPerUser", 10, OnGetValue);
    public ConfigItem<long> MaxImageSizeBytes => new("MaxImageSizeBytes", 10 * 1024 * 1024, OnGetValue);

    private async Task<TValue> OnGetValue<TValue>(string key, TValue defaultValue, CancellationToken cancellationToken) where TValue : IParsable<TValue>
    {
        var configValue = await context.AppSettings.FindAsync("MaxImagesPerUser", cancellationToken);

        if (configValue is not null && TValue.TryParse(configValue.Value, CultureInfo.InvariantCulture, out var parsedValue))
            return parsedValue;

        return defaultValue;
    }

    public class ConfigItem<TValue>(string key, TValue defaultValue, Func<string, TValue, CancellationToken, Task<TValue>> onGetValue) where TValue : IParsable<TValue>
    {
        public Task<TValue> Get(CancellationToken cancellationToken = default)
        {
            return onGetValue(key, defaultValue, cancellationToken);
        }
    }
}