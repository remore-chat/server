using System;
using System.Threading.Tasks;

using TTalk.WinUI.Contracts.Services;
using TTalk.WinUI.Core.Helpers;

using Windows.Storage;

namespace TTalk.WinUI.Services
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
        }
    }
}
