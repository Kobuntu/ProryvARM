using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.Workflow.Activity.ARM.Common;
using Proryv.Workflow.Activity.ARM.PropertyEditors.HierarchyObjectSelector;
using Proryv.Workflow.Activity.Settings;

namespace Proryv.Workflow.Activity.ARM.Balance
{
    public class GetBalanceFreeHierarchyByObjectList
        : BaseArmActivity<bool>
    {

        public GetBalanceFreeHierarchyByObjectList()
        {
            this.DisplayName = "Получить универсальные балансы по списку объектов";

            var type = typeof(GetBalanceFreeHierarchyByObjectList);
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
        [Description("Объект или список объектов по которым строится баланс")]
        [DisplayName(@"Объект(ы) для построения баланса")]
        [RequiredArgument]
        public string ObjectIds { get; set; }

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

            if (ObjectIds == string.Empty)
            {
                metadata.AddValidationError("Необходимо выбрать объекты!");
            }

            base.CacheMetadata(metadata);
        }

        protected override bool Execute(CodeActivityContext context)
        {
            if (string.IsNullOrEmpty(ObjectIds))
            {
                Errors.Set(context, "Не определены объекты для которых будет сформирован баланс");
                return false;
            }

            MultiPsSelectedArgs args;
            try
            {
                args = ObjectIds.DeserializeFromString<MultiPsSelectedArgs>();
            }
            catch (Exception ex)
            {
                Errors.Set(context, "Ошибка преобразования параметров " + ex.Message);
                return false;
            }

            try
            {
                var res = ARM_Service.BL_GetFreeHierarchyBalanceByObjects(args.PSList,
                    StartDateTime.Get(context),
                    EndDateTime.Get(context), TimeZoneId,
                    DiscreteType,UnitDigit);

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
