using System;
using System.Globalization;
using System.Windows.Data;

namespace KeyboardDesigner.Converters
{
    public class ScaleConverter : IValueConverter, IMultiValueConverter
    {
        // Single value binding (Static Parameter)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d && parameter != null)
            {
                if (double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double factor))
                {
                    return d * factor;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d && parameter != null)
            {
                if (double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double factor) && factor != 0)
                {
                    return d / factor;
                }
            }
            return value;
        }

        // Multi value binding (Dynamic Parameter: Value, ScaleFactor)
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length >= 2 && values[0] is double val && values[1] is double factor)
            {
                return val * factor;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}