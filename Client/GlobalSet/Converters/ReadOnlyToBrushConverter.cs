using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Proryv.ElectroARM.Controls.Controls.GlobalSet.Converters
{

    [ValueConversion(typeof(bool), typeof(Brush))]
    public class ReadOnlyToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || Equals(value, DependencyProperty.UnsetValue)) return string.Empty;

            try
            {
                var b = (bool)value;

                return b ? Brushes.SlateGray : Brushes.Black;
            }
            catch
            {
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
