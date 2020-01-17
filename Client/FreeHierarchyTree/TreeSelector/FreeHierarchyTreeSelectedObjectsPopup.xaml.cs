using Infragistics.Windows.DataPresenter;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.TreeSelector
{
    /// <summary>
    /// Логика взаимодействия для FreeHierarchyTreeSelectedObjectsPopup.xaml
    /// </summary>
    public partial class FreeHierarchyTreeSelectedObjectsPopup : UserControl, IDisposable
    {
        private FreeHierarchyTreeDescriptor _descriptor;

        public FreeHierarchyTreeSelectedObjectsPopup(FreeHierarchyTreeDescriptor descriptor)
        {
            InitializeComponent();

            _descriptor = descriptor;
        }

        public void Dispose()
        {
            _descriptor = null;
        }

        private void FoundedGridOnSelectedItemsChanged(object sender, Infragistics.Windows.DataPresenter.Events.SelectedItemsChangedEventArgs e)
        {
            if (!IsLoaded) return;

            var xamGrid = sender as DataPresenterBase;
            if (xamGrid == null) return;

            KeyValuePair<int, FreeHierarchyTreeItem> selItem;

            if (xamGrid.SelectedItems.Records.Count > 0)
            {

                var record = xamGrid.SelectedItems.Records[0] as DataRecord;
                if (record == null) return;

                selItem = (KeyValuePair<int, FreeHierarchyTreeItem>)record.DataItem;

            }
            else if (xamGrid.SelectedItems.Cells.Count > 0)
            {
                var cell = xamGrid.SelectedItems.Cells[0] as Cell;
                if (cell == null || cell.Record == null) return;

                selItem = (KeyValuePair<int, FreeHierarchyTreeItem>)cell.Record.DataItem;
            }
            else
            {
                return;
            }

            if (selItem.Value == null || selItem.Value.HierObject == null) return;

            _descriptor.ExpandAndSelect(selItem.Value.HierObject, false);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_descriptor == null) return;

            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (_descriptor.SelectedItems == null)
                {
                    dgSelected.DataSource = null;
                }
                else
                {
                    dgSelected.DataSource = _descriptor
                        .SelectedItems;
                }
            }), System.Windows.Threading.DispatcherPriority.Normal);
        }
    }
}
