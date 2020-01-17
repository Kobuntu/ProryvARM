using Infragistics.Windows.DataPresenter.DataSources;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.ElectroARM.Alarms.Alarm;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Proryv.AskueARM2.Client.Visual;

namespace Proryv.ElectroARM.Alarms.Converters
{
    [ValueConversion(typeof(DynamicDataItem), typeof(IFreeHierarchyObject))]
    public class AlarmViewToHierItemConverter : IMultiValueConverter
    {
        private ItemSelector _selector;
        public ItemSelector Selector
        {
            get
            {
                if (_selector != null) return _selector;

                _selector = new ItemSelector();
                return _selector;
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null 
                || values.Length < 2) return null;

            var dataItem = values[1] as DynamicDataItem; // vw_Alarms
            if (dataItem == null || !dataItem.IsDataAvailable) return null;

            IFreeHierarchyObject hierarchyObject = null;
            string stringName;

            if (parameter != null)
            {
                switch (parameter.ToString())
                {
                    case "Object":
                        hierarchyObject = VisualAlarmHelper.ExtractHierObjectFromDynamicDataItem(dataItem);
                        if (hierarchyObject == null)
                        {
                            dataItem.TryGetPropertyValue("ObjectName", out stringName);
                            return stringName;
                        }
                        break;
                    case "Parent":
                        hierarchyObject = VisualAlarmHelper.ExtractParentObjectFromDynamicDataItem(dataItem);
                        if (hierarchyObject == null)
                        {
                            dataItem.TryGetPropertyValue("ParentName", out stringName);
                            return stringName;
                        }
                        break;
                    case "AlarmConfirmStatusCategory":
                        return VisualAlarmHelper.ExtractAlarmConfirmStatusCategoryFromDynamicDataItem(dataItem);
                }
            }


            if (hierarchyObject == null) return null;

            var dt = Selector.SelectTemplate(hierarchyObject, null);
            if (dt != null)
            {
                var content = dt.LoadContent() as FrameworkElement;
                if (content != null)
                {
                    content.DataContext = hierarchyObject;
                    return content;
                }
            }

            return hierarchyObject.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
