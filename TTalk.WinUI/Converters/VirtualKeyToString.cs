using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.WinUI.Converters
{
    public class VirtualKeyToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var @string = value.ToString();
            return @string.Replace("VK_", "").ToLowerInvariant().Capitalize();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
