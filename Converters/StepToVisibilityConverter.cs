using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IMS.Resources
{
    public class StepToVisibilityConverter : IValueConverter
    {
        public int Step { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int currentStep && currentStep == Step)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}