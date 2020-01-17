using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Controls;

namespace Proryv.AskueARM2.Client.Styles.Style
{
    public partial class DatePickerResourceDictionary : ResourceDictionary
    {
        public DatePickerResourceDictionary()
       {
          InitializeComponent();
       }

        private void UIElementOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Decimal ||e.Key == Key.Divide || e.Key == Key.Subtract)
            {
                var tb = sender as DateTimeTextBox;
                if (tb != null)
                {
                    var a = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
                    tb.RaiseEvent(
                        new TextCompositionEventArgs(
                            InputManager.Current.PrimaryKeyboardDevice,
                            new TextComposition(InputManager.Current, tb, a)) { RoutedEvent = TextCompositionManager.TextInputEvent });
                    e.Handled = true;
                }
            }
        }
    }
}
