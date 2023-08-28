using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace VPet.Plugin.ModMaker.Converters;

public class MaxConverter : IMultiValueConverter
{
    public object Convert(
        object[] values,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture
    )
    {
        return values.Max(i => (double)i);
    }

    public object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        System.Globalization.CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}
