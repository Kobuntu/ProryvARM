using System;
using System.Collections.ObjectModel;
using System.Windows;
using Proryv.AskueARM2.Client.Visual.BalanceEditor;
using Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.Data;
using Proryv.ElectroARM.Controls.Common.DragAndDrop;

namespace Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.UI.Controls
{
    /// <summary>
    /// Interaction logic for BalanceFreeHierarchySubsectionListView.xaml
    /// </summary>
    public partial class BalanceFreeHierarchySubsectionListView : IDisposable
    {
        private readonly ListViewDragDropManager<BalanceFreeHierarchySubsectionRow> _dragMgr;
        public BalanceFreeHierarchySubsectionListView()
        {
            InitializeComponent();
            _dragMgr = new ListViewDragDropManager<BalanceFreeHierarchySubsectionRow>(this);
        }

        private void RaiseParentBalanceChanged()
        {
            var frame = this.FindParent<BalanceEditor_FreeHierarchy_Frame>();
            if (frame != null) frame.IsBalanceChanged = true;
        }

        #region OnListViewDrop

        // Handles the Drop event for both ListViews.
        private void OnListViewDrop(object sender, DragEventArgs e)
        {
            var destination = ItemsSource as ObservableCollection<BalanceFreeHierarchySubsectionRow>;
            if (destination == null)
            {
                _dragMgr.IsCancelDrop = true;
                return;
            }

            var data = e.Data.GetData(typeof(WeakReference)) as WeakReference;
            if (data == null || !data.IsAlive)
                return;

            var itemRow = data.Target as BalanceFreeHierarchySubsectionRow;
            if (itemRow == null)
            {
                _dragMgr.IsCancelDrop = true;
                return;
            }

            FrameworkElement parentControl = this.FindParent<BalanceSubsectionControl>();
            if (parentControl == null)
            {
                parentControl = this.FindParent<BalanceSectionControl>();
                if (parentControl == null)
                {
                    _dragMgr.IsCancelDrop = true;
                    return;
                }
            }

            var newParent = parentControl.DataContext as BaseBalanceFreeHierarchyRow;
            if (newParent == null || newParent == itemRow || newParent.CountParentLevels(0) != (itemRow.CountParentLevels(0)-1))
            {
                _dragMgr.IsCancelDrop = true;
                return;
            }

            if (_dragMgr.IsDragInProgress)
                return;

            if (itemRow.ParentSource != null)
            {
                itemRow.ParentSource.Remove(itemRow);
            }

            itemRow.ChangeParent(newParent);

            RaiseParentBalanceChanged();
        }

        #endregion // OnListViewDrop

        public void Dispose()
        {
            _dragMgr.Dispose();
            this.ClearItems();
        }

        private void BalanceFreeHierarchySubsectionListViewOnPreviewDragEnter(object sender, DragEventArgs e)
        {
            
        }
    }
}
