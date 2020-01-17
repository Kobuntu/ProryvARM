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
    public class GetAlarmSettings : BaseArmActivity<bool>
    {
        public GetAlarmSettings()
        {
            this.DisplayName = "Получить настройки тревоги";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Идентификатор аварии")]
        public InArgument<int> AlarmSetting_ID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Настройки")]
        public OutArgument<Alarms_Setting> Settings { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            Alarms_Setting res = null;
            try
            {
                res = ARM_Service.ALARM_Get_AlarmSetting(AlarmSetting_ID.Get(context));
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            Settings.Set(context, res);
            return string.IsNullOrEmpty(Error.Get(context));
        }
    }
}
