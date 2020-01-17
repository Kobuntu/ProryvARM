using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Proryv.Workflow.Activity.Settings;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;
using Proryv.AskueARM2.Client.ServiceReference.DeclaratorService;

namespace Proryv.Workflow.Activity.ARM
{
    public class GetUserIDByLogin : BaseArmActivity<bool>
    {
        public GetUserIDByLogin()
        {
            this.DisplayName = "Получть идентификатор пользователя";
        }

        [Description("Строка ошибки")]
        [DisplayName("Ошибка")]
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        public OutArgument<string> Error { get; set; }
        

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("User_ID")]
        [RequiredArgument]
        public OutArgument<string> User_ID { get; set; }
        
        
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("UserLogin")]
        [RequiredArgument]
        public InArgument<string> UserLogin { get; set; }
        

        protected override bool Execute(CodeActivityContext context)
        {
            string userLogin = UserLogin.Get(context);

            try
            {
               string userID= DeclaratorService.GetUserIDByLogin(userLogin);
              
                if (String.IsNullOrEmpty(userID))
                    Error.Set(context, "пользователь не найден");

                User_ID.Set(context, userID);
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
            }

            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}
