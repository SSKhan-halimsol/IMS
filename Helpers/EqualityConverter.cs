﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace IMS.Helpers
{
    public class EqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value != null && value.Equals(parameter);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            (bool)value ? parameter : Binding.DoNothing;
    }
}