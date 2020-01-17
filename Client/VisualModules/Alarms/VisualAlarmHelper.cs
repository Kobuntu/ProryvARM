using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Windows.DataPresenter.DataSources;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common;

namespace Proryv.ElectroARM.Alarms.Alarm
{
    public static class VisualAlarmHelper
    {
        #region Public

        public static IFreeHierarchyObject ExtractHierObjectFromDynamicDataItem(DynamicDataItem dataItem)
        {
            string un;
            if (!dataItem.TryGetPropertyValue("ID", out un)) return null;

            byte b;
            if (!dataItem.TryGetPropertyValue("TypeHierarchy", out b)) return null;

            var typeHierarchy = (enumTypeHierarchy) b;

            return HierarchyObjectHelper.ToHierarchyObject(un, (enumTypeHierarchy) typeHierarchy);
        }

        public static IFreeHierarchyObject ExtractParentObjectFromDynamicDataItem(DynamicDataItem dataItem)
        {
            string un;
            if (!dataItem.TryGetPropertyValue("ParentId", out un)) return null;

            byte b;
            if (!dataItem.TryGetPropertyValue("ParentTypeHierarchy", out b)) return null;

            var typeHierarchy = (enumTypeHierarchy)b;

            return HierarchyObjectHelper.ToHierarchyObject(un, (enumTypeHierarchy)typeHierarchy);
        }

        public static string ExtractAlarmConfirmStatusCategoryFromDynamicDataItem(DynamicDataItem dataItem)
        {
            int? id;
            if (!dataItem.TryGetPropertyValue("AlarmConfirmStatusCategory_ID", out id) || !id.HasValue) return null;

            Dict_Alarms_ConfirmStatus dacs;
            if (EnumClientServiceDictionary.DictConfirmStatuses != null && EnumClientServiceDictionary.DictConfirmStatuses.TryGetValue(id.Value, out dacs)
                && dacs != null)
            {
                return dacs.AlarmConfirmStatusCategoryName;
            }

            return null;
        }

        #endregion
    }
}
