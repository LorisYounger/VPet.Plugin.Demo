using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace VPet.Plugin.ModMaker.Converters;

public class MarginConverter : IMultiValueConverter
{
    public object Convert(
        object[] values,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture
    )
    {
        if (values.Length == 0)
        {
            return new Thickness();
        }
        else if (values.Length == 1)
        {
            return new Thickness()
            {
                Left = System.Convert.ToDouble(values[0]),
                Top = default,
                Right = default,
                Bottom = default
            };
        }
        else if (values.Length == 2)
        {
            return new Thickness()
            {
                Left = System.Convert.ToDouble(values[0]),
                Top = System.Convert.ToDouble(values[1]),
                Right = default,
                Bottom = default
            };
        }
        else if (values.Length == 3)
        {
            return new Thickness()
            {
                Left = System.Convert.ToDouble(values[0]),
                Top = System.Convert.ToDouble(values[1]),
                Right = System.Convert.ToDouble(values[2]),
                Bottom = default
            };
        }
        else
        {
            return new Thickness()
            {
                Left = System.Convert.ToDouble(values[0]),
                Top = System.Convert.ToDouble(values[1]),
                Right = System.Convert.ToDouble(values[2]),
                Bottom = System.Convert.ToDouble(values[3])
            };
        }
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
