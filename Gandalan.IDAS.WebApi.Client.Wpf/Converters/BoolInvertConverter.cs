using System;
using System.Globalization;
using System.Windows.Data;

namespace Gandalan.IDAS.WebApi.Client.Wpf.Controls;

public class BoolInvertConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var booleanValue = (bool)value;
        return !booleanValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var booleanValue = (bool)value;
        return !booleanValue;
    }
}
