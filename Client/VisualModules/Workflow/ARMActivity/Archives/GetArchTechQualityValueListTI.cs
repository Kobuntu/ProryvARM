using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.Workflow.Activity.Settings;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Reflection;

namespace Proryv.Workflow.Activity.ARM
{

    //[Designer(typeof(GetArchTechQualityLastValuesDesigner))]
    public class GetArchTechQualityValueListTI : BaseArmActivity<bool>
    {
        public GetArchTechQualityValueListTI()
        {
            this.DisplayName = "Получить мгновенные значения для списка ТИ";

            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(GetArchTechQualityValueListTI), "TechParamTypes", new EditorAttribute(typeof(TechParamTypesEditor), typeof(DialogPropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Список идентификаторов ТИ")]
        [RequiredArgument]
        public InArgument<List<int>> TIList { get; set; }

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
        public OutArgument<Dictionary<int, List<TArchTechQualityValue>>> Values { get; set; }
        
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {

            if (TIList.Get(context) == null)
            {
                Error.Set(context, "Не определен список идентификаторов ТИ");
                return false;
            }

            List<int> InList = TIList.Get(context);
            if (InList.Count == 0)
            {
                Error.Set(context, "Список идентификаторов ТИ не должен быть пустым");
                return false;
            }

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
                var dt = ServerFromLook.Get(context);
                var tis = InList.ToDictionary(k=>k, v=> dt);

                var res = ARM_Service.DS_GetArchTech_Quality_LastValues(tis, ServerToLook.Get(context), TechParamTypes, false);
                if (res != null)
                {
                    if (res.Errors != null)
                        Error.Set(context, res.Errors);

                    Values.Set(context, res.LastValues);
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
