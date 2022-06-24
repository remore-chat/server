using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Remore.WinUI.Converters
{
    public class PascalCaseToWords : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Regex.Replace(value.ToString(), "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;;
        }
    }
}
