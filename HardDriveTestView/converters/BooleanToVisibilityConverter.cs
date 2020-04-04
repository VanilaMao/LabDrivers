using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HardDriveTestView.converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var condition = (bool)value;
            return condition ? Visibility.Visible: Visibility.Collapsed ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
