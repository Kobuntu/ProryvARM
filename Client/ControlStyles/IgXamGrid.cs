using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.DataPresenter.Events;
using Proryv.AskueARM2.Both.VisualCompHelpers.SpecialFilterOperands;
using Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter.DataTableExtention;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;

namespace Proryv.ElectroARM.Controls.Styles
{
    public partial class IgXamGrid
    {
        private void dp_FieldLayoutInitialized(object sender, FieldLayoutInitializedEventArgs e)
        {
            if (e.FieldLayout == null || e.FieldLayout.Fields == null) return;
            foreach (Field field in e.FieldLayout.Fields)
            {
                Type dataType = field.DataType;

                if (dataType == typeof(DateTime))
                {
                    field.Settings.FilterOperatorDefaultValue = ComparisonOperator.Equals;
                    //Меням настройки фильтра поумолчанию
                    field.Settings.FilterOperatorDropDownItems = ComparisonOperatorFlags.Equals
                                                                 | ComparisonOperatorFlags.LessThan
                                                                 | ComparisonOperatorFlags.LessThanOrEqualsTo
                                                                 | ComparisonOperatorFlags.GreaterThan
                                                                 | ComparisonOperatorFlags.GreaterThanOrEqualsTo;
                }
                else if (dataType == typeof(ObjectIdCollection) || dataType == typeof(double))
                {
                    field.Settings.FilterOperatorDefaultValue = ComparisonOperator.Equals;
                }
                else if (dataType != typeof(bool))
                {
                    //Меням настройки фильтра поумолчанию
                    field.Settings.FilterOperatorDefaultValue = ComparisonOperator.Contains;
                }
            }
        }

        private void FilterDropDownPopulatingOnHandler(object sender, RecordFilterDropDownPopulatingEventArgs e)
        {
            if (!_filterResolvedTypes.Contains(e.Field.DataType) || e.DropDownItems == null) return;

            e.DropDownItems.RemoveAll(f => !f.IsAction);
            e.IncludeUniqueValues = false;

            AddFilterDropDownItems(e.DropDownItems);


            //var fi = e.MenuItems[0].CommandParameter;
            //e.MenuItems.RemoveAll(f=>f.Command == null);

            //var mi = new FieldMenuDataItem
            //{
            //    Header = "Фильтр достоверности",
            //};

            //e.MenuItems.Add(mi);

            //AddFilterMenuItems(mi.Items, fi);
        }

        #region Работа с фильтром

        private readonly List<Type> _filterResolvedTypes = new List<Type>
        {
            typeof(IFValue), typeof(ulong), typeof(VALUES_FLAG_DB), typeof(TVALUES_DB)
        };

        private void AddFilterDropDownItems(IList<FilterDropDownItem> filterList)
        {

            filterList.Add(CreateFilterDropDownItem(FValueSpecialFilterOperand.FlagValid, "flagGood.png"));
            filterList.Add(CreateFilterDropDownItem(FValueSpecialFilterOperand.FlagDataNotFull, "flagGray.png"));
            filterList.Add(CreateFilterDropDownItem(FValueSpecialFilterOperand.FlagNotCorrect, "flagBad.png"));
        }

        private FilterDropDownItem CreateFilterDropDownItem(SpecialFilterOperandBase filterOperandBase, string imageSource,
            ComparisonOperator comparisonOperator = ComparisonOperator.Equals)
        {
            return new FilterDropDownItem(new ComparisonCondition(comparisonOperator, filterOperandBase), filterOperandBase.Name)
            {
                Image = new BitmapImage(new Uri(GlobalEnumsDictionary.PrefixResource + imageSource,
                    UriKind.RelativeOrAbsolute)).GetAsFrozen() as BitmapImage
            };
        }

        #endregion
    }
}
