using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace EnvelopeManager
{
    public class DecimalToColorConverter : IValueConverter
    {
        public static readonly Color POSITIVE_COLOR = Colors.LightGreen;
        public static readonly Color NEGATIVE_COLOR = Colors.PaleVioletRed;
        public static readonly Color DEFAULT_COLOR = Colors.White;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal num)
            {
                if (num > 0.00m) { return new SolidColorBrush(POSITIVE_COLOR); }
                else if (num < 0.00m) { return new SolidColorBrush(NEGATIVE_COLOR); }
            }
            return new SolidColorBrush(DEFAULT_COLOR);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new InvalidOperationException("Cannot convert a color back into a decimal."); }
    }
}
