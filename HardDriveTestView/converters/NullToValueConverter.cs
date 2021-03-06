﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace HardDriveTestView.converters
{
    public class NullToValueConverter : IValueConverter
    {
        public object NullValue { get; set; }
        public object NotNullValue { get; set; }

        public bool Reservsered { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? NullValue : NotNullValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
