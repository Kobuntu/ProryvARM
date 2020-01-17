using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using Proryv.Workflow.Activity.Settings;
using System.Activities.Presentation.Metadata;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.Visual.Common;
using System.Activities.Presentation.PropertyEditing;

namespace Proryv.Workflow.Activity.ARM
{

    public class ManageDeviceParameters : BaseArmActivity<bool>
    {
        public ManageDeviceParameters()
        {
            this.DisplayName = "Управление параметрами приборов";
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(ManageDeviceParameters), "Parameters", new EditorAttribute(typeof(ManageDeviceParametrsPropEditor), typeof(ManageDeviceParametrsDialog)));
            builder.AddCustomAttributes(typeof(ManageDeviceParameters), "LoginInfo", new EditorAttribute(typeof(PasswordProperyEditor), typeof(PasswordProperyDialog)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        [Description("Строка ошибки")]
        [DisplayName("Ошибка")]
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        public OutArgument<string> Error { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор ТИ")]
        [RequiredArgument]
        public InArgument<int> TiId { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [Description("Имя пользователя и пароль")]
        [DisplayName("Авторизация")]
        public LoginParams LoginInfo { get; set; }

//        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
//        [RequiredArgument]
//        [Description("Пароль пользователя")]
//        [DisplayName("Пароль")]
////        [PasswordPropertyText(true)]
//        public string Password { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Параметры прибора")]
        [RequiredArgument]
        public string Parameters { get; set; }

        //Hidden propertys
        //[Browsable(false)]
        //public CommandInfo CommandParam { get; set; }
        //[Browsable(false)]
        //public string Password { get; set; }
        //[Browsable(false)]
        //public Expl_User_Journal_ManagePU_Request_List request { get; set; }
        //[Browsable(false)]
        //public List<Tuple<int, List<KeyValuePair<long, DeviceMethodDescription>>>> methods { get; set; }


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
               if (string.IsNullOrEmpty(Parameters))
                   metadata.AddValidationError("Не определены 'Параметры прибора'");
            }

            base.CacheMetadata(metadata);
        }


        protected override bool Execute(CodeActivityContext context)
        {
            Error.Set(context, null);

            if (Parameters == null)
            {
                Error.Set(context, "Не определены параметры управления");
                return false;
            }

            if (string.IsNullOrEmpty(LoginInfo.UserName) || string.IsNullOrEmpty(LoginInfo.Password))
            {
                Error.Set(context, "Не определены параметры авторизации");
                return false;            
            }

            int ti_id = TiId.Get(context);
            CommandInfo paramscommand;

            try
            {

                try
                {
                    paramscommand = Parameters.DeserializeFromString<CommandInfo>();
                }
                catch (Exception ex)
                {
                    Error.Set(context, "Ошибка преобразования параметров команды" + ex.Message);
                    return false;
                }

//command.SerializeToString<CommandInfo>()   - в строку
//command.DeserializeFromString<CommandInfo>()   - из строки

                List<KeyValuePair<int, FailReason>> Res = ARM_Service.DM_Set_Device_Methods(LoginInfo.UserName, LoginInfo.Password, paramscommand.Request,
                               new List<Tuple<int, List<KeyValuePair<long, DeviceMethodDescription>>>>()
                                {
                                 new Tuple<int, List<KeyValuePair<long, DeviceMethodDescription>>>(paramscommand.DeviceToClass_ID, paramscommand.Methods)
                                },new List<int>() { ti_id });
                if (Res.Count == 1)
                    Error.Set(context, GlobalEnumsDictionary.ConvertFailReasonToString(Res[0].Value));

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
