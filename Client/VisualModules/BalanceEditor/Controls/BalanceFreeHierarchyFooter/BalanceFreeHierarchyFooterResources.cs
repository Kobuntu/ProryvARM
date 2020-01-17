using Infragistics.Windows;
using Infragistics.Windows.DataPresenter;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.UI.Controls.BalanceFreeHierarchyFooter
{
    public partial class BalanceFreeHierarchyFooterResources
    {
        #region Сортировка 

        private void DragSource_Drop(object sender, Infragistics.DragDrop.DropEventArgs e)
        {
            var emp = dp.DataRecord.DataItem;
            //Collection.Remove(emp);

            var collection = dp.DataPresenter.DataSource as IList;
            if (collection == null) return;

            collection.Remove(emp);

            try
            {
                if (overIndex < 0) overIndex = 0;
                //dp.DataRecord.IsSelected = true;
                collection.Insert(overIndex, emp);
            }
            catch
            {

            }
        }

        DataRecordPresenter dp;
        DataRecordPresenter dragOver;
        //int currentIndex;
        private void DragSource_DragStart(object sender, Infragistics.DragDrop.DragDropStartEventArgs e)
        {
            dp = Utilities.GetAncestorFromType(e.DragSource as DependencyObject, typeof(DataRecordPresenter), false) as DataRecordPresenter;
            //currentIndex = dp.Record.Index;

        }

        private void CardPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

        }
        int overIndex;
        private void DragSource_DragOver(object sender, Infragistics.DragDrop.DragDropMoveEventArgs e)
        {
            dragOver = Utilities.GetAncestorFromType(e.DropTarget as DependencyObject, typeof(DataRecordPresenter), false) as DataRecordPresenter;
            overIndex = dragOver.Record.VisibleIndex;

        }

        #endregion
    }
}
