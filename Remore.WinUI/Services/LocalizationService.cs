using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remore.WinUI.Contracts.Services;
using Remore.WinUI.ViewModels;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;

namespace Remore.WinUI.Services
{
    public class LocalizationService
    {
        private ILocalSettingsService _settingsService;
        private Dictionary<CultureInfo, ResourceMap> _cultureToResourceMap;
        private ResourceMap _currentResourceMap;
        private CultureInfo _currentCulture;
        private CultureInfo _fallbackCulture;

        public CultureInfo CurrentLanguage => _currentCulture;
        public IReadOnlyList<CultureInfo> Languages { get; set; }
        public LocalizationService(ILocalSettingsService settingsService)
        {
            _settingsService = settingsService;
            _cultureToResourceMap = new();
            Languages = new List<CultureInfo>()
            {
                new("en-us"),
                new("ru-ru"),
                new("de-de"),
                new("uk-ua"),
                new("cs-cz"),
                new("ro-RO"),
                new("pl-pl"),
                new("nl-nl"),
                new("es-ES"),
                new("be-BY"),
                new("az-AZ"),
                new("ar-SA"),
            };
        }
        public event EventHandler<object> LanguageUpdated;

        public async Task Initialize()
        {
            foreach (var language in Languages)
            {
                if (language.Name.ToLowerInvariant() == "en-us")
                {
                    _fallbackCulture = language;
                }
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
                //Fallback to english
                try
                {
                    return _cultureToResourceMap[_fallbackCulture].GetValue(key.Replace(".", "/")).ValueAsString;
                }
                catch (Exception)
                {
                    return $"{_currentCulture.Name.ToLowerInvariant()}:{key}";
                }
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
            LanguageUpdated?.Invoke(this, null);
            Task.Run(async () =>
            {
                await _settingsService.SaveSettingAsync(SettingsViewModel.LanguageSettingsKey, _currentCulture.Name.ToLowerInvariant());
            });
            return true;
        }
    }
}
