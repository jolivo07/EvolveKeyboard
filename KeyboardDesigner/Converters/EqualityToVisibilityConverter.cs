using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KeyboardDesigner.Converters
{
    public class EqualityToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2) return Visibility.Collapsed;

            var val1 = values[0];
            var val2 = values[1];

            if (val1 == null && val2 == null) return Visibility.Visible;
            if (val1 != null && val1.Equals(val2)) return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}