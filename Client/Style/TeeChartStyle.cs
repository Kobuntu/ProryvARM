using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using System.Windows;
using System.Windows.Controls;
using Steema.TeeChart.WPF;
using Proryv.AskueARM2.Both.VisualCompHelpers.SteemaTeeChart;
using System.Windows.Controls.Primitives;
using Proryv.AskueARM2.Client.Visual.Common;
using System.Windows.Threading;

namespace Proryv.AskueARM2.Client.Visual
{
    public partial class TeeChartStyle
    {
        void verticalSB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var sb = sender as ScrollBar;
            var helper = (sb.TemplatedParent as TChart).Tag as ArchivesValuesListToTeeChart;
            if (helper != null) helper.VerticalChangedEvent(sb.Value);
        }

        void horizontalSB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var sb = sender as ScrollBar;
            var helper = (sb.TemplatedParent as TChart).Tag as ArchivesValuesListToTeeChart;
            if (helper != null) helper.HorizontalChangedEvent(sb.Value);
        }

        void viewMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb.Tag is bool && (bool)cb.Tag == true)
            {
                var helper = (cb.TemplatedParent as TChart).Tag as ArchivesValuesListToTeeChart;
                if (helper != null) helper.ChartButtonEvent((cb.SelectedItem as ComboBoxItem).Content as ChartButton);
            }
            cb.Tag = true;
        }
    }
}
