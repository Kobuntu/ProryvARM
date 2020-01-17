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
using Proryv.Workflow.Activity.ARM.PropertyEditors;

namespace Proryv.Workflow.Activity.ARM.Alarms
{
    public class RegisterAlarmEntityTypes : BaseAlarmActivity
    {
        public RegisterAlarmEntityTypes()
        {
            this.DisplayName = "Зарегистрировать сущности для тревоги";

            AttributeTableBuilder builder = new AttributeTableBuilder();

            builder.AddCustomAttributes(typeof(RegisterAlarmEntityTypes), "AlarmEntitiesList", new EditorAttribute(typeof(AlarmEntitiesMaskEditor), typeof(DialogPropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Список сущностей принимаемых тревогой")]
        public List<enumAlarmType> AlarmEntitiesList { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            if (AlarmEntitiesList == null)
            {
                Error.Set(context, "Список сущностей принимаемых тревогой должен быть определен");
                return false;
            }
            try
            {
                Int64 value=0;
                foreach (var val in AlarmEntitiesList)
                {
                    value = value | (Int64)val;
                }
                string queryString=@"update Workflow_Activity_List set AlarmEntitiesMask="+value.ToString()+" where WorkflowActivity_ID="+WorkflowActivity_ID.Get(context).ToString();
                int Res = ARM_Service.SQL_Execute_Query(queryString, null, 60);

                if (Res <= 0)
                    Error.Set(context, "Тревога не найдена");
                else return true;
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
