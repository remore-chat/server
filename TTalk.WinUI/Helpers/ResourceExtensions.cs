using Microsoft.Windows.ApplicationModel.Resources;
using TTalk.WinUI.Services;

namespace TTalk.WinUI.Helpers
{
    internal static class ResourceExtensions
    {
        //private static ResourceLoader _resourceLoader = new ResourceLoader();

        public static string GetLocalized(this string resourceKey)
        {
            return App.GetService<LocalizationService>().GetString(resourceKey);
        }
    }
}
