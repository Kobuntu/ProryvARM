using System.Collections;
using System.Windows;
using System.Windows.Controls;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Xceed.Utils.Collections;
using Xceed.Wpf.DataGrid;

namespace Proryv.AskueARM2.Client.Visual
{
    public partial class XceedDataGridStyles
    {
        private void OnAutoFilterSelectClearAllClick(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe == null) return;

            var dg = fe.FindParent<DataGridControl>();
            if (dg == null) return;

            var viewSource = dg.ItemsSource as DataGridCollectionView;
            if (viewSource == null) return;

            int filters = 0;
            Button button = (Button)sender;
            AutoFilterControl autoFilterControl = button.TemplatedParent as AutoFilterControl;
            string columnFieldName = autoFilterControl.AutoFilterColumn.FieldName;
            bool selectAll = ((string)button.Tag) == "1";

            using (viewSource.DeferRefresh())
            {
                ObservableHashList autoFilterValues = viewSource.AutoFilterValues[columnFieldName] as ObservableHashList;
                using (autoFilterValues.DeferINotifyCollectionChanged())
                {
                    autoFilterValues.Clear();
                    if (selectAll)
                    {

                        IList distinctValues = viewSource.DistinctValues[columnFieldName];
                        foreach (object value in distinctValues)
                            autoFilterValues.Add(value);
                        filters += distinctValues.Count;
                    }
                }

                if (selectAll)
                {
                    ListBox listBox = autoFilterControl.DistinctValuesHost as ListBox;
                    listBox.SelectAll();
                }
            }
        }
    }
}
