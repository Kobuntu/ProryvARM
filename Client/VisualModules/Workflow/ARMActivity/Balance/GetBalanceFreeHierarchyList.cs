using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.ARM.Common;
using Proryv.Workflow.Activity.ARM.PropertyEditors.HierarchyObjectSelector;
using Proryv.Workflow.Activity.Settings;

namespace Proryv.Workflow.Activity.ARM.Balance
{
    public class GetBalanceFreeHierarchyList : BaseArmActivity<bool>
    {
        public GetBalanceFreeHierarchyList()
        {
            this.DisplayName = "Получить универсальные балансы";

            var type = typeof(GetBalanceFreeHierarchyList);
            var builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(type, "TimeZoneId", new EditorAttribute(typeof(TimeZoneIdPropEditor), typeof(TimeZoneIdPropEditor)));
            builder.AddCustomAttributes(type, "ObjectIds", new EditorAttribute(typeof(HierarchyObjectIdEditor), typeof(HierarchyObjectIdEditor)));

            //builder.AddCustomAttributes(type, "StartDateTime", new DesignerAttribute(type));
            //builder.AddCustomAttributes(typeof(GetBalanceFreeHierarchyList), "StartDateTime", new EditorAttribute(typeof(DialogSelectDateTime), typeof(DialogSelectDateTime)));

            MetadataStore.AddAttributeTable(builder.CreateTable());

            UnitDigit = EnumUnitDigit.Kilo;
        }

        #region Входящие параметры

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификаторы балансов")]
        [RequiredArgument]
        public InArgument<List<string>> BalanceFreeHierarchyUNs { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Конечная дата")]
        public InArgument<DateTime> EndDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Период дискретизации")]
        [TypeConverter(typeof(EnumDiscreteTypeConverter))]
        public enumTimeDiscreteType DiscreteType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Часовой пояс")]
        [DefaultValue("Russian Standard Time")]
        public string TimeZoneId { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Единицы измерения")]
        [TypeConverter(typeof(EnumUnitDigitTypeConverter))]
        [DefaultValue(EnumUnitDigit.Kilo)]
        public EnumUnitDigit UnitDigit { get; set; }

        #endregion

        #region Выходящие параметры

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Балансы")]
        public OutArgument<List<BalanceFreeHierarchyCalculatedResult>> Balances { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибки")]
        public OutArgument<string> Errors { get; set; }

        #endregion

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (UnitDigit == EnumUnitDigit.Null)
                metadata.AddValidationError("Выберите единицы измерения!");

            base.CacheMetadata(metadata);
        }

        protected override bool Execute(CodeActivityContext context)
        {
            try
            {
                var res = ARM_Service.BL_GetFreeHierarchyBalanceResult(BalanceFreeHierarchyUNs.Get(context),
                    StartDateTime.Get(context),
                    EndDateTime.Get(context),
                    TimeZoneId, TExportExcelAdapterType.toXLSx, false,
                    DiscreteType, UnitDigit, false, UnitDigit, false, false, 0, 0, false, false, false);

                if (res != null && res.CalculatedValues!=null)
                {
                    Balances.Set(context, res.CalculatedValues.Values.ToList());

                    if (res.Errors != null && res.Errors.Length > 0)
                    {
                        Errors.Set(context, res.Errors.ToString());
                    }
                }
            }

            catch (Exception ex)
            {
                Errors.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            return string.IsNullOrEmpty(Errors.Get(context));
        }
    }
}
