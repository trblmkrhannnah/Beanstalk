using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Beanstalk.Database.Data;
using Beanstalk.Database.Entities;
using Microsoft.AspNetCore.Identity;

namespace Beanstalk.App.Features;

public sealed class BeanstalkConfig
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BeanstalkConfig(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public ConfigItem<int> MaxImagesPerUser => new("MaxImagesPerUser", 10, OnGetValue);
    public ConfigItem<long> MaxImageSizeBytes => new("MaxImageSizeBytes", 10 * 1024 * 1024, OnGetValue);

    private async Task<TValue> OnGetValue<TValue>(string key, TValue defaultValue, CancellationToken cancellationToken) where TValue : IParsable<TValue>
    {
        var configValue = await _context.AppSettings.FindAsync("MaxImagesPerUser", cancellationToken);

        if (configValue is not null && TValue.TryParse(configValue.Value, CultureInfo.InvariantCulture, out var parsedValue))
            return parsedValue;

        return defaultValue;
    }

    public class ConfigItem<TValue> where TValue : IParsable<TValue>
    {
        private readonly TValue _defaultValue;
        private readonly string _key;
        private readonly Func<string, TValue, CancellationToken, Task<TValue>> _onGetValue;

        public ConfigItem(string key, TValue defaultValue, Func<string, TValue, CancellationToken, Task<TValue>> onGetValue)
        {
            _key = key;
            _defaultValue = defaultValue;
            _onGetValue = onGetValue;
        }

        public Task<TValue> Get(CancellationToken cancellationToken = default)
        {
            return _onGetValue(_key, _defaultValue, cancellationToken);
        }
    }
}