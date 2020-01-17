using System.Linq;
using System.Windows;
using Infragistics.Windows;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DataPresenter;
using Proryv.AskueARM2.Both.VisualCompHelpers.SpecialFilterOperands;

namespace Proryv.AskueARM2.Client.Styles.Style
{
    /// <summary>
    /// Interaction logic for XamRecordFilterStyle.xaml
    /// </summary>
    public partial class XamRecordFilterStyle
    {
        private void EventSetterOnHandler(object sender, RoutedEventArgs e)
        {
            var d = e.OriginalSource as DependencyObject;
            if (d == null) return;

            var rftc = Utilities.GetAncestorFromType(d, typeof(RecordFilterTreeControl), true) as RecordFilterTreeControl;
            if (rftc == null) return;

            var grid = Utilities.GetAncestorFromType(rftc, typeof(DataPresenterBase), true) as DataPresenterBase;
            if (grid == null) return;

            var fieldLayout = grid.FieldLayouts.FirstOrDefault();
            if (fieldLayout == null) return;

            var rf = fieldLayout.RecordFilters[rftc.Field.Name];
            if (rf == null) return;

            //rf.ApplyPendingFilter();

            try
            {
                rf.Conditions.Add(new ComparisonCondition(ComparisonOperator.Equals, FValueSpecialFilterOperand.FlagValid, "Достоверные"));
                //fieldLayout.RecordFilters.Add(rf);
            }
            catch {}

            //lp.Field.
        }
    }
}

