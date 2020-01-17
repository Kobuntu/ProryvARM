using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using Proryv.Workflow.Activity.Settings;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.Activities.Presentation.Metadata;

namespace Proryv.Workflow.Activity.ARM
{
    public class ManageManualReadRequest : BaseArmActivity<bool>
    {
        public ManageManualReadRequest()
        {
            DisplayName = "Ручной опрос ПУ";
            RequestType = enumManualReadRequestType.Integrals;
            LivePeriod = new TimeSpan(0, 0, 10, 0);
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(ManageManualReadRequest), "LoginInfo", new EditorAttribute(typeof(PasswordProperyEditor), typeof(PasswordProperyDialog)));
            builder.AddCustomAttributes(typeof(ManageManualReadRequest), "LivePeriod", new EditorAttribute(typeof(MinuteCountPropertyEditor), typeof(MinuteCountPropertyDialog)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (LoginInfo == null)
                metadata.AddValidationError("Не орпределено свойство 'Авторизация'");
            else
            { 
               if (string.IsNullOrEmpty(LoginInfo.UserName))
                metadata.AddValidationError("Не орпределено имя пользователя в поле 'Авторизация'");
               if (string.IsNullOrEmpty(LoginInfo.Password))
                   metadata.AddValidationError("Не определен пароль в поле 'Авторизация'");
            }
            base.CacheMetadata(metadata);
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор ТИ")]
        [RequiredArgument]
        public InArgument<int> TiId { get; set; }

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
        [DisplayName("Тип запрашиваемых значений")]
        [TypeConverter(typeof(enumManualReadRequestTypeTypeConverter))]
        public enumManualReadRequestType RequestType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Время актуальности воздействия")]
        public TimeSpan LivePeriod { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [Description("Если не указана, то текущее время")]
        [DisplayName("Дата чтения показаний для интегралов")]
        public InArgument<DateTime?> DateReadValues { get; set; }

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
            int ti_id = TiId.Get(context);
            List<int> Initial = new List<int>();
            Initial.Add(ti_id);


            if (string.IsNullOrEmpty(LoginInfo.UserName) || string.IsNullOrEmpty(LoginInfo.Password))
            {
                Error.Set(context, "Не определены параметры авторизации");
                return false;            
            }


            try
            {

                string userID = null;
                List<UserInfo> uInfos = ARM_Service.EXPL_Get_All_Users();
                foreach (UserInfo u in uInfos)
                    if (u.UserName == LoginInfo.UserName)
                    {
                        userID = u.User_ID;
                        break;
                    }
                if (string.IsNullOrEmpty(userID))
                {
                    throw new Exception("Пользователь '" + LoginInfo.UserName + "' не найден в системе"); 
                }

                DateTime actualTime = new DateTime(1, 1, 1, LivePeriod.Days, LivePeriod.Hours, LivePeriod.Minutes);
                short priority = 128;
                if (Priority == enumManualReadRequestPriority.Hight)
                    priority = 0;

                byte requestType = (byte) RequestType;
                var managePu = new Expl_User_Journal_ManagePU_Request_List()
                {
                    CUS_ID = 0,
                    User_ID = userID,
                    ManageRequestLifeDateTime = actualTime,
                    ManageRequestStatus = 0,
                    ManageRequestType = 6,
                    ManageRequestPriority = priority
                };
                var request = new DeviceManage_Manual_ReadRequest()
                {
                    CUS_ID = 0,
                    RequestComment = Comment.Get(context),
                    ManualReadType = requestType
                };
                if (requestType == 1)
                {
                    request.ReadDateTime = DateReadValues.Get(context);
                }
                else request.ReadDateTime = null;
                List<Tuple<int, FailReason>> result = null;

                result = ARM_Service.DM_Send_Manual_ReadigMeter_Request(LoginInfo.UserName, LoginInfo.Password, managePu, request, Initial);

                    if (result == null)
                    {
                        Error.Set(context, "Не удалось отправить команду");
                        return false;            
                    }
                    if (result.Count > 0)
                    {
                        Error.Set(context, GlobalEnumsDictionary.ConvertFailReasonToString(result[0].Item2));
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
