using System;
using System.Threading.Tasks;

namespace TTalk.WinUI.Contracts.Services
{
    public interface ILocalSettingsService
    {
        Task<T> ReadSettingAsync<T>(string key);

        Task SaveSettingAsync<T>(string key, T value);

        event EventHandler<object> SettingsUpdated;
    }
}
