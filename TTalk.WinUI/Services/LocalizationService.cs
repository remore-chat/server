using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTalk.WinUI.Contracts.Services;
using TTalk.WinUI.ViewModels;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;

namespace TTalk.WinUI.Services
{
    public class LocalizationService
    {
        private ILocalSettingsService _settingsService;
        private Dictionary<CultureInfo, ResourceMap> _cultureToResourceMap;
        private ResourceMap _currentResourceMap;
        private CultureInfo _currentCulture;

        public CultureInfo CurrentLanguage => _currentCulture;
        public IReadOnlyList<CultureInfo> Languages { get; set; }
        public LocalizationService(ILocalSettingsService settingsService)
        {
            _settingsService = settingsService;
            _cultureToResourceMap = new();
            Languages = new List<CultureInfo>()
            {
                new("en-US"),
                new("ru-RU"),
                new("uk-UA"),
                new("de-DE"),
                new("cs-CZ"),
            };
        }

        public async Task Initialize()
        {
            foreach (var language in Languages)
            {
                var map = ResourceManager.Current.MainResourceMap.GetSubtree(language.Name.ToLowerInvariant());
                _cultureToResourceMap.Add(language, map);
            }

            var savedLanguageCode = await _settingsService.ReadSettingAsync<string>(SettingsViewModel.LanguageSettingsKey) ?? "en-us";
            // Validate language present in Languages list
            if (!Languages.Any(x => x.Name.ToLowerInvariant() == savedLanguageCode.ToLowerInvariant()))
            {
                savedLanguageCode = "en-us";
                await _settingsService.SaveSettingAsync<string>(SettingsViewModel.LanguageSettingsKey, "en-us");
            }

            _currentCulture = Languages.FirstOrDefault(x => x.Name.ToLowerInvariant() == savedLanguageCode);
            _currentResourceMap = _cultureToResourceMap[_currentCulture];
        }

        public string GetString(string key)
        {
            try
            {
                return _currentResourceMap.GetValue(key.Replace(".", "/")).ValueAsString;
            }
            catch (Exception ex)
            {
                return $"{_currentCulture.Name.ToLowerInvariant()}:{key}";
            }
            
        }

        public CultureInfo GetCurrentCulture()
        {
            return _currentCulture;
        }

        public bool UpdateLanguage(CultureInfo cultureInfo)
        {
            if (cultureInfo.Name.ToLowerInvariant() == _currentCulture.Name.ToLowerInvariant())
            {
                return false;
            }
            _currentCulture = cultureInfo;
            _currentResourceMap = _cultureToResourceMap[cultureInfo];
            Task.Run(async () =>
            {
                await _settingsService.SaveSettingAsync(SettingsViewModel.LanguageSettingsKey, _currentCulture.Name.ToLowerInvariant());
            });
            return true;
        }
    }
}
