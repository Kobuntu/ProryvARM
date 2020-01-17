using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Proryv.Workflow.Activity.Settings;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.Workflow.Activity.ARM
{
    public class GetObjectName : BaseArmActivity<bool>
    {
        public GetObjectName()
        {
            this.DisplayName = "Получить имя объекта";
            PathDelim = '/';
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Идентификатор объекта")]
        public InArgument<object> ID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Тип объекта")]
        [TypeConverter(typeof(enumObjectTypeForNameTypeConverter))]
        public enumObjectTypeForName ObjectType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Разделитель пути")]
        public char PathDelim { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Имя")]
        public OutArgument<string> ObjName { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Путь")]
        public OutArgument<string> ObjPath { get; set; }


        protected override bool Execute(CodeActivityContext context)
        {
            object obj = ID.Get(context);
            int? IntId = null;
            string StrId = null;
            Guid? GuidId = null;

            if (obj == null)
            {
                Error.Set(context, "Не определен Идентификатор объекта");
                return false;
            }
            try
            {
                if (ObjectType == enumObjectTypeForName.HierLev1 ||
                    ObjectType == enumObjectTypeForName.HierLev2 ||
                    ObjectType == enumObjectTypeForName.HierLev3 ||
                    ObjectType == enumObjectTypeForName.PS ||
                    ObjectType == enumObjectTypeForName.Section ||
                    ObjectType == enumObjectTypeForName.TI)
                {
                    IntId = Convert.ToInt32(obj);
                }
                if (ObjectType == enumObjectTypeForName.Balance_PS || 
                    ObjectType == enumObjectTypeForName.Balance_HierLev0 ||
                    ObjectType == enumObjectTypeForName.Balance_HierLev3 ||
                    ObjectType == enumObjectTypeForName.User ||
                    ObjectType == enumObjectTypeForName.Formula)
                {
                    StrId = Convert.ToString(obj);
                }
            }
            catch
            {
                Error.Set(context, "Ошибка преобразования идентификатора объекта");
                return false;
            }

            TObjectName res = null;
            try
            {
                res = ARM_Service.ALARM_GetObjectName(ObjectType, IntId, StrId, GuidId, PathDelim);
            }
            catch(Exception ex)
            {
                Error.Set(context, ex.Message);
            }

            if (res != null)
            {
                ObjName.Set(context, res.Name);
                ObjPath.Set(context, res.Path);
                Error.Set(context, res.Error);
            }

            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}
