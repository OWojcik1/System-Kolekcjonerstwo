using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemKolekcjonerstwo.Models
{
    public class ValueDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string || value is int)
            {
                return value.ToString();
            }
            else if (value is Tuple<List<string>, string> tuple)
            {
                return tuple.Item2;
            }
            else
            {
                return "Niewspierany typ";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
