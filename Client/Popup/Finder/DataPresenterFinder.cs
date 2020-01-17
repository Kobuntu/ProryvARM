using Infragistics.Windows.DataPresenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.ElectroARM.Controls.Controls.Popup.Finder
{
    public static class DataPresenterFinder
    {
        public static void ExpandAndSelectRowDataPresenter(object row, DataPresenterBase dpb)
        {
            var rec = dpb.GetRecordFromDataItem(row, true);
            if (rec == null) return;

            Action<Record> expandParent = null;

            expandParent = dr =>
            {
                if (dr.ParentRecord == null) return;

                dr.ParentRecord.IsExpanded = true;
                expandParent(dr.ParentRecord);
            };

            expandParent(rec);
            dpb.ActiveRecord = rec;
            rec.IsActive = rec.IsSelected = true;
        }
    }
}
