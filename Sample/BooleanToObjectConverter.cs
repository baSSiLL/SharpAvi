using System;
using System.Windows.Data;

namespace SharpAvi.Sample
{
    public class BooleanToObjectConverter : IValueConverter
    {
        public object FalseValue { get; set; }
        public object TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Equals(value, true) ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Equals(value, TrueValue) ? true : Equals(value, FalseValue) ? false : (bool?)null;
        }
    }
}
