using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Proryv.Workflow.Activity.Settings;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Microsoft.VisualBasic.Activities;
using System.Activities.Presentation.Metadata;

namespace Proryv.Workflow.Activity.ARM
{
    public abstract class BaseWriteAlarmToDB : BaseAlarmActivity
    {

        public BaseWriteAlarmToDB()
        {
            AlarmSeverity = new InArgument<int>(new VisualBasicValue<int>("1"));

            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(BaseWriteAlarmToDB), "AlarmSeverity", new EditorAttribute(typeof(AlarmSeverityEditor), typeof(AlarmSeverityEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());

        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор пользователя")]
        [RequiredArgument]
        public InArgument<string> UserID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор настройки аварии")]
        [RequiredArgument]
        public InArgument<int> AlarmSettingID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Уровень аварии")]
        [RequiredArgument]
        public InArgument<int> AlarmSeverity { get ; set;}


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Сообщение")]
        [RequiredArgument]
        public InArgument<string> AlarmMessage { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Краткое сообщение")]
        public InArgument<string> ShortAlarmMessage { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Описание сообщения")]
        public InArgument<string> AlarmDescription { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Ключ сообщения")]
        [RequiredArgument]
        public InArgument<string> AlarmUniqueKey { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Время формирования тревоги")]
        public InArgument<DateTime?> AlarmDateTime { get; set; }

        public string CheckStr(string str, int maxChars)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (str.Length > maxChars)
                str = str.Substring(0, maxChars - 1);
            return str;
        }
    }


    public class WriteAlarm_TI : BaseWriteAlarmToDB
    {
        public WriteAlarm_TI()
        {
            this.DisplayName = "Записать аварию для ТИ";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор ТИ")]
        [RequiredArgument]
        public InArgument<int> TI_ID { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            bool res = false;
            string Message = CheckStr(AlarmMessage.Get(context), 1024);
            string ShortMessage = CheckStr(ShortAlarmMessage.Get(context), 128);
            string Description = CheckStr(AlarmDescription.Get(context), 1024);
            string UniqueKey = CheckStr(AlarmUniqueKey.Get(context), 1024);

            try
            {
                if (MonitoringChangesAlarm.isChangesAlarm_TI(TI_ID.Get(context), WorkflowActivity_ID.Get(context), UserID.Get(context),
                                                             AlarmSeverity.Get(context), UniqueKey))
                    res = ARM_Service.ALARM_WriteAlarm_TI(TI_ID.Get(context), WorkflowActivity_ID.Get(context), UserID.Get(context),
                                                           AlarmSettingID.Get(context), AlarmSeverity.Get(context),
                                                           Message,
                                                           ShortMessage,
                                                           Description,
                                                           UniqueKey, AlarmDateTime.Get(context));
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }
            return string.IsNullOrEmpty(Error.Get(context)) && res;
        }
    }

    public class WriteAlarm_BalancePS : BaseWriteAlarmToDB
    {
        public WriteAlarm_BalancePS()
        {
            this.DisplayName = "Записать аварию для баланса ПС";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор баланса ПС")]
        [RequiredArgument]
        public InArgument<string> BalancePS_ID { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            var res = false;
            var message = CheckStr(AlarmMessage.Get(context), 1024);
            var shortMessage = CheckStr(ShortAlarmMessage.Get(context), 128);
            var description = CheckStr(AlarmDescription.Get(context), 1024);
            var uniqueKey = CheckStr(AlarmUniqueKey.Get(context), 1024);
            try
            {
                if (MonitoringChangesAlarm.isChangesAlarm_BalancePS(BalancePS_ID.Get(context), WorkflowActivity_ID.Get(context), UserID.Get(context),
                                                                    AlarmSeverity.Get(context), uniqueKey))
                    res = ARM_Service.ALARM_WriteAlarm_BalancePS(BalancePS_ID.Get(context), WorkflowActivity_ID.Get(context), UserID.Get(context),
                                                           AlarmSettingID.Get(context), AlarmSeverity.Get(context),
                                                           message,
                                                           shortMessage,
                                                           description,
                                                           uniqueKey, AlarmDateTime.Get(context));
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }
            return string.IsNullOrEmpty(Error.Get(context)) && res;
        }
    }

    public class WriteAlarm_BalanceFreeHierarchy : BaseWriteAlarmToDB
    {
        public WriteAlarm_BalanceFreeHierarchy()
        {
            this.DisplayName = "Записать аварию для универсального баланса";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор универсально баланса")]
        [RequiredArgument]
        public InArgument<string> BalanceFreeHierarchy_UN { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            var res = false;
            var message = CheckStr(AlarmMessage.Get(context), 1024);
            var shortMessage = CheckStr(ShortAlarmMessage.Get(context), 128);
            var description = CheckStr(AlarmDescription.Get(context), 1024);
            var uniqueKey = CheckStr(AlarmUniqueKey.Get(context), 1024);
            try
            {
                if (MonitoringChangesAlarm.isChangesAlarm_BalanceFreeHierarchy(BalanceFreeHierarchy_UN.Get(context), WorkflowActivity_ID.Get(context), UserID.Get(context),
                    AlarmSeverity.Get(context), uniqueKey))
                    res = ARM_Service.ALARM_WriteAlarm_BalanceFreeHierarchy(BalanceFreeHierarchy_UN.Get(context), WorkflowActivity_ID.Get(context), UserID.Get(context),
                        AlarmSettingID.Get(context), AlarmSeverity.Get(context),
                        message,
                        shortMessage,
                        description,
                        uniqueKey, AlarmDateTime.Get(context));
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }
            return string.IsNullOrEmpty(Error.Get(context)) && res;
        }
    }

    public class WriteAlarm_PS : BaseWriteAlarmToDB
    {
        public WriteAlarm_PS()
        {
            this.DisplayName = "Записать аварию для ПС";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор ПС")]
        [RequiredArgument]
        public InArgument<int> PS_ID { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            bool res = false;
            string Message = CheckStr(AlarmMessage.Get(context), 1024);
            string ShortMessage = CheckStr(ShortAlarmMessage.Get(context), 128);
            string Description = CheckStr(AlarmDescription.Get(context), 1024);
            string UniqueKey = CheckStr(AlarmUniqueKey.Get(context), 1024);
            try
            {
                if (MonitoringChangesAlarm.isChangesAlarm_PS(PS_ID.Get(context), WorkflowActivity_ID.Get(context), UserID.Get(context),
                                                                    AlarmSeverity.Get(context), UniqueKey))
                    res = ARM_Service.ALARM_WriteAlarm_PS(PS_ID.Get(context), WorkflowActivity_ID.Get(context), UserID.Get(context),
                                                           AlarmSettingID.Get(context), AlarmSeverity.Get(context),
                                                           Message,
                                                           ShortMessage,
                                                           Description,
                                                           UniqueKey, AlarmDateTime.Get(context));
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }
            return string.IsNullOrEmpty(Error.Get(context)) && res;
        }
    }

    public class WriteAlarm_Formuls : BaseWriteAlarmToDB
    {
        public WriteAlarm_Formuls()
        {
            this.DisplayName = "Записать аварию для формулы";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор формулы")]
        [RequiredArgument]
        public InArgument<string> Formul_ID { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            bool res = false;
            string Message = CheckStr(AlarmMessage.Get(context), 1024);
            string ShortMessage = CheckStr(ShortAlarmMessage.Get(context), 128);
            string Description = CheckStr(AlarmDescription.Get(context), 1024);
            string UniqueKey = CheckStr(AlarmUniqueKey.Get(context), 1024);
            try
            {
                if (MonitoringChangesAlarm.isChangesAlarm_Formul(Formul_ID.Get(context), WorkflowActivity_ID.Get(context), UserID.Get(context),
                                                                    AlarmSeverity.Get(context), UniqueKey))
                    res = ARM_Service.ALARM_WriteAlarm_Formuls(Formul_ID.Get(context), WorkflowActivity_ID.Get(context), UserID.Get(context),
                                                           AlarmSettingID.Get(context), AlarmSeverity.Get(context),
                                                           Message,
                                                           ShortMessage,
                                                           Description,
                                                           UniqueKey, AlarmDateTime.Get(context));
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }
            return string.IsNullOrEmpty(Error.Get(context)) && res;
        }
    }

    public class WriteAlarm_Users : BaseWriteAlarmToDB
    {
        public WriteAlarm_Users()
        {
            this.DisplayName = "Записать аварию для пользователя";
        }

        protected override bool Execute(CodeActivityContext context)
        {
            bool res = false;
            string Message = CheckStr(AlarmMessage.Get(context), 1024);
            string ShortMessage = CheckStr(ShortAlarmMessage.Get(context), 128);
            string Description = CheckStr(AlarmDescription.Get(context), 1024);
            string UniqueKey = CheckStr(AlarmUniqueKey.Get(context), 1024);
            try
            {
                if (MonitoringChangesAlarm.isChangesAlarm_User(WorkflowActivity_ID.Get(context), UserID.Get(context),
                                                                    AlarmSeverity.Get(context), UniqueKey))
                    res = ARM_Service.ALARM_WriteAlarm_Users(WorkflowActivity_ID.Get(context), UserID.Get(context),
                                                           AlarmSettingID.Get(context), AlarmSeverity.Get(context),
                                                           Message,
                                                           ShortMessage,
                                                           Description,
                                                           UniqueKey, AlarmDateTime.Get(context));
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }
            return string.IsNullOrEmpty(Error.Get(context)) && res;
        }
    }

    public class WriteAlarm_61968SlaveSystems : BaseWriteAlarmToDB
    {
        public WriteAlarm_61968SlaveSystems()
        {
            this.DisplayName = "Записать аварию для подчиненных систем 61968";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор подчиненной системы")]
        [RequiredArgument]
        public InArgument<int> Slave61968System_ID { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            bool res = false;
            string Message = CheckStr(AlarmMessage.Get(context), 1024);
            string ShortMessage = CheckStr(ShortAlarmMessage.Get(context), 128);
            string Description = CheckStr(AlarmDescription.Get(context), 1024);
            string UniqueKey = CheckStr(AlarmUniqueKey.Get(context), 1024);
            try
            {
                if (MonitoringChangesAlarm.isChangesAlarm_61968Systems(Slave61968System_ID.Get(context), WorkflowActivity_ID.Get(context), UserID.Get(context),
                                                                       AlarmSeverity.Get(context), UniqueKey))
                    res = ARM_Service.ALARM_WriteAlarm_Master61968SlaveSystems(Slave61968System_ID.Get(context), WorkflowActivity_ID.Get(context), UserID.Get(context),
                                                           AlarmSettingID.Get(context), AlarmSeverity.Get(context),
                                                           Message,
                                                           ShortMessage,
                                                           Description,
                                                           UniqueKey, AlarmDateTime.Get(context));
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }
            return string.IsNullOrEmpty(Error.Get(context)) && res;
        }
    }


    class MonitoringChangesAlarm
    {
        static Dictionary<string, object> ListSendingAlarms = new Dictionary<string, object>();
        static TimeSpan TimeLiveHistorySendingAlarms = new TimeSpan(1, 0, 0); // через какой интервал очищать информацию о переданных авариях
        static DateTime InsertFirstElementTime = DateTime.Now;

        static bool FoundInList(string Key)
        {
            bool Found;
            lock (ListSendingAlarms)
            {
                Found = ListSendingAlarms.ContainsKey(Key);
                if (!Found)
                {
                    if (ListSendingAlarms.Count == 0)
                        InsertFirstElementTime = DateTime.Now;
                    ListSendingAlarms.Add(Key, null);
                }

                if (DateTime.Now - InsertFirstElementTime > TimeLiveHistorySendingAlarms)
                {
                    ListSendingAlarms.Clear();
                    return false;
                }
            }
            return Found;
        }

        //public static bool FoundKey(string Key)
        //{
        //    return FoundInList(Key);
        //}

        public static bool isChangesAlarm_TI(int TI_ID, int WorkflowActivity_ID, string User_ID, int AlarmSeverity, string AlarmUniqueKey)
        {
            string UniqueKey = AlarmGenerateUniqueKey.GetKey_TI(TI_ID, WorkflowActivity_ID, User_ID, AlarmSeverity, AlarmUniqueKey);
            return !FoundInList(UniqueKey);
        }

        public static bool isChangesAlarm_BalancePS(string BalancePS_ID, int WorkflowActivity_ID, string User_ID, int AlarmSeverity, string AlarmUniqueKey)
        {
            string UniqueKey = AlarmGenerateUniqueKey.GetKey_BalancePS(BalancePS_ID, WorkflowActivity_ID, User_ID, AlarmSeverity, AlarmUniqueKey);
            return !FoundInList(UniqueKey);
        }

        public static bool isChangesAlarm_BalanceFreeHierarchy(string balanceFreeHierarchyUn, int workflowActivityId, string userID, int alarmSeverity, string alarmUniqueKey)
        {
            string UniqueKey = AlarmGenerateUniqueKey.GetKey_BalancePSFreeHierarchy(balanceFreeHierarchyUn, workflowActivityId, userID, alarmSeverity, alarmUniqueKey);
            return !FoundInList(UniqueKey);
        }

        public static bool isChangesAlarm_PS(int PS_ID, int WorkflowActivity_ID, string User_ID, int AlarmSeverity, string AlarmUniqueKey)
        {
            string UniqueKey = AlarmGenerateUniqueKey.GetKey_PS(PS_ID, WorkflowActivity_ID, User_ID, AlarmSeverity, AlarmUniqueKey);
            return !FoundInList(UniqueKey);
        }

        public static bool isChangesAlarm_Formul(string Formula_UN, int WorkflowActivity_ID, string User_ID, int AlarmSeverity, string AlarmUniqueKey)
        {
            string UniqueKey = AlarmGenerateUniqueKey.GetKey_Formul(Formula_UN, WorkflowActivity_ID, User_ID, AlarmSeverity, AlarmUniqueKey);
            return !FoundInList(UniqueKey);
        }

        public static bool isChangesAlarm_User(int WorkflowActivity_ID, string User_ID, int AlarmSeverity, string AlarmUniqueKey)
        {
            string UniqueKey = AlarmGenerateUniqueKey.GetKey_User(WorkflowActivity_ID, User_ID, AlarmSeverity, AlarmUniqueKey);
            return !FoundInList(UniqueKey);
        }

        public static bool isChangesAlarm_61968Systems(int Slave61968System_ID, int WorkflowActivity_ID, string User_ID, int AlarmSeverity, string AlarmUniqueKey)
        {
            string UniqueKey = AlarmGenerateUniqueKey.GetKey_61968Systems(Slave61968System_ID, WorkflowActivity_ID, User_ID, AlarmSeverity, AlarmUniqueKey);
            return !FoundInList(UniqueKey);
        }

    }

}
