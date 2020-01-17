using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Proryv.AskueARM2.Client.Visual.Common;

namespace Proryv.ElectroARM.Controls.Controls.GlobalSet.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class GlobalToStringConverter : IValueConverter
    {
        private const string _globaName = "Глобальные";
        private readonly string _localName = "Пользовательские (" + Manager.UserName + ")";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || Equals(value, DependencyProperty.UnsetValue)) return string.Empty;

            try
            {
                var b = (bool) value;

                return b ? _globaName : _localName;
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
