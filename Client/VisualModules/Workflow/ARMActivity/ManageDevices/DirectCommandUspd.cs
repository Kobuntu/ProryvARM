using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.ComponentModel;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using Proryv.Workflow.Activity.Settings;

namespace Proryv.Workflow.Activity.ARM
{
    public class DirectCommandUspd : BaseArmActivity<bool>
    {
        public DirectCommandUspd()
        {
            DisplayName = "Управление параметрами УСПД";
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(DirectCommandUspd), "Parameters", new EditorAttribute(typeof(CommandUspdParamPropertyEditor), typeof(CommandUspdParametrsDialog)));
            builder.AddCustomAttributes(typeof(DirectCommandUspd), "LoginInfo", new EditorAttribute(typeof(PasswordProperyEditor), typeof(PasswordProperyDialog)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        [Description("Строка ошибки")]
        [DisplayName("Ошибка")]
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        public OutArgument<string> Error { get; set; }

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
        [DisplayName("Параметры прибора")]
        [RequiredArgument]
        public string Parameters { get; set; }


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

            int tiId = UspdId.Get(context);
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

                List<KeyValuePair<int, FailReason>> res = ARM_Service.DM_Set_USPD_Methods(LoginInfo.UserName, LoginInfo.Password, paramscommand.Request,
                               new List<Tuple<int, List<KeyValuePair<long, DeviceMethodDescription>>>>()
                                {
                                 new Tuple<int, List<KeyValuePair<long, DeviceMethodDescription>>>(paramscommand.DeviceToClass_ID, paramscommand.Methods)
                                }, new List<int>() { tiId });                
                if (res.Count == 1)
                    Error.Set(context, GlobalEnumsDictionary.ConvertFailReasonToString(res[0].Value));
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
