using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Activities;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using Proryv.Workflow.Activity.Settings;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.Activities.Presentation.Metadata;

namespace Proryv.Workflow.Activity.ARM
{

    public class UspdManualReadRequest : BaseArmActivity<bool>
    {
        public UspdManualReadRequest()
        {
            DisplayName = "Ручной опрос УСПД";
            RequestType = enumManualUspdRequestType.Reglaments;
            LivePeriod = new TimeSpan(0, 0, 10, 0);
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(UspdManualReadRequest), "LoginInfo", new EditorAttribute(typeof(PasswordProperyEditor), typeof(PasswordProperyDialog)));
            builder.AddCustomAttributes(typeof(UspdManualReadRequest), "LivePeriod", new EditorAttribute(typeof(MinuteCountPropertyEditor), typeof(MinuteCountPropertyDialog)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (LoginInfo == null)
                metadata.AddValidationError("Не орпределено свойство 'Авторизация'");
            else
            {
                if (string.IsNullOrEmpty(LoginInfo.UserName))
                    metadata.AddValidationError("Не определено имя пользователя в поле 'Авторизация'");
                if (string.IsNullOrEmpty(LoginInfo.Password))
                    metadata.AddValidationError("Не определен пароль в поле 'Авторизация'");
            }
            base.CacheMetadata(metadata);
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор УСПД")]
        [RequiredArgument]
        public InArgument<int> UspdId { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [Description("Имя пользователя и пароль")]
        [DisplayName("Авторизация")]
        public LoginParams LoginInfo { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Коментарий")]
        public InArgument<string> Comment { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Тип опроса")]
        [TypeConverter(typeof(enumManualUspdRequestTypeTypeConverter))]
        public enumManualUspdRequestType RequestType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Время актуальности воздействия")]
        public TimeSpan LivePeriod { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Приоритет запроса")]
        [TypeConverter(typeof(enumManualReadRequestPriorityTypeConverter))]
        public enumManualReadRequestPriority Priority { get; set; }

        [Description("Строка ошибки")]
        [DisplayName("Ошибка")]
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        public OutArgument<string> Error { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            Error.Set(context, null);
            int uspid = UspdId.Get(context);
            List<int> initial = new List<int>();
            initial.Add(uspid);


            if (string.IsNullOrEmpty(LoginInfo.UserName) || string.IsNullOrEmpty(LoginInfo.Password))
            {
                Error.Set(context, "Не определены параметры авторизации");
                return false;
            }


            try
            {

                string userId = null;
                List<UserInfo> uInfos = ARM_Service.EXPL_Get_All_Users();
                foreach (UserInfo u in uInfos)
                    if (u.UserName == LoginInfo.UserName)
                    {
                        userId = u.User_ID;
                        break;
                    }
                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("Пользователь '" + LoginInfo.UserName + "' не найден в системе");
                }

                DateTime actualTime = new DateTime(1, 1, 1, LivePeriod.Days, LivePeriod.Hours, LivePeriod.Minutes);
                short priority = 128;
                if (Priority == enumManualReadRequestPriority.Hight)
                    priority = 0;

                byte requestType = (byte)RequestType;
                var managePu = new Expl_User_Journal_ManagePU_Request_List()
                {
                    CUS_ID = 0,
                    User_ID = userId,
                    ManageRequestLifeDateTime = actualTime,
                    ManageRequestStatus = 0,
                    ManageRequestType = 21,
                    ManageRequestPriority = priority
                };
                var request = new USPDManage_Manual_ReadRequest()
                {
                    CUS_ID = 0,
                    RequestComment = Comment.Get(context),
                    ManualReadType = requestType
                };

                List<KeyValuePair<int, FailReason>> result = ARM_Service.DM_Send_Manual_ReadigUSPD_Request(LoginInfo.UserName, LoginInfo.Password, managePu, request, initial);

                if (result == null)
                {
                    Error.Set(context, "Не удалось отправить команду");
                    return false;

                }
                if (result.Count > 0)
                {
                    Error.Set(context, GlobalEnumsDictionary.ConvertFailReasonToString(result[0].Value));
                    return false;
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
