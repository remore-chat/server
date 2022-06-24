using System;
using System.Threading.Tasks;

using Remore.WinUI.Contracts.Services;
using Remore.WinUI.Core.Helpers;

using Windows.Storage;

namespace Remore.WinUI.Services
{
    public class LocalSettingsServicePackaged : ILocalSettingsService
    {
        public event EventHandler<object> SettingsUpdated;

        public async Task<T> ReadSettingAsync<T>(string key)
        {
            object obj = null;

            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }

            return default;
        }

        public async Task SaveSettingAsync<T>(string key, T value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = await Json.StringifyAsync(value);
            SettingsUpdated?.Invoke(this, null);
        }
    }
}
