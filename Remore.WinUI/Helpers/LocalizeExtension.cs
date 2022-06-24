using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remore.WinUI.Services;

namespace Remore.WinUI.Helpers
{
    [MarkupExtensionReturnType(ReturnType = typeof(string))]

    public class LocalizeExtension : MarkupExtension
    {
        public LocalizeExtension()
        {

        }


        public LocalizeExtension(string key)
        {
            this.Key = key;
        }

        public string Key { get; set; }

        public string Context { get; set; }



        protected override object ProvideValue(IXamlServiceProvider serviceProvider)
        {
            var keyToUse = Key;
            if (!string.IsNullOrWhiteSpace(Context))
                keyToUse = $"{Context}/{Key}";

            return App.GetService<LocalizationService>().GetString(keyToUse);
        }
    }
}
