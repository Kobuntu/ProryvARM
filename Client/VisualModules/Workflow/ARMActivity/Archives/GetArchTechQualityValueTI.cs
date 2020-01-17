using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.Activities;
using Proryv.Workflow.Activity.Settings;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.Activities.Presentation.PropertyEditing;

namespace Proryv.Workflow.Activity.ARM
{
    public class GetArchTechQualityValueTI : BaseArmActivity<bool>
    {

        public GetArchTechQualityValueTI()
        {
            this.DisplayName = "Получить мгновенные значения для ТИ";

            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(GetArchTechQualityValueTI), "TechParamTypes", new EditorAttribute(typeof(TechParamTypesEditor), typeof(DialogPropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор ТИ")]
        [RequiredArgument]
        public InArgument<int> TiId { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime?> ServerFromLook { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Конечная дата")]
        public InArgument<DateTime?> ServerToLook { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Список  параметров мгновенных значений")]
        public List<enumArchTechParamType> TechParamTypes { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Тип запрашиваемых значений")]
        [TypeConverter(typeof(EnumEnrgyQualityRequestTypeConverter))]
        public enumEnrgyQualityRequestType RequestType { get; set; }
        
                
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [Description("Список значений для каждой ТИ")]
        [DisplayName("Список значений")]
        public OutArgument<List<TArchTechQualityValue>> Values { get; set; }
        
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {

            int ti_id = TiId.Get(context);
            Dictionary<int, DateTime?> List_ti_id = new Dictionary<int, DateTime?>();
            List_ti_id[ti_id] = ServerFromLook.Get(context);

            if (TechParamTypes == null || TechParamTypes.Count == 0)
            {
                Error.Set(context, "Список параметров мгновенных значений не должен быть пустым");
                return false;
            }

            if (RequestType == enumEnrgyQualityRequestType.Archive)
                if (ServerFromLook.Get(context) == null || ServerToLook.Get(context) == null)
                {
                    Error.Set(context, "Для архивных значений начальная и конечная дата должна быть определена");
                    return false;
                }

            try
            {
                var res = ARM_Service.DS_GetArchTech_Quality_LastValues(List_ti_id, ServerToLook.Get(context), TechParamTypes, false);
                if (res != null)
                {
                    if (res.Errors != null)
                        Error.Set(context, res.Errors);

                    List<TArchTechQualityValue> result;
                    if (res.LastValues != null && res.LastValues.TryGetValue(ti_id, out result))
                    {
                        Values.Set(context, result);
                    }
                }
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}
