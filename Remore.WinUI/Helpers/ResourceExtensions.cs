using Microsoft.Windows.ApplicationModel.Resources;
using Remore.WinUI.Services;

namespace Remore.WinUI.Helpers
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
