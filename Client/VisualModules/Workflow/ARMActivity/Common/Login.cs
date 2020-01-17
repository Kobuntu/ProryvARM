using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;
using System.ComponentModel;
using System.Activities;

namespace Proryv.Workflow.Activity.ARM
{
    public class Login : BaseArmActivity<bool>
    {

        public Login()
        {
            this.DisplayName = "Авторизация";
        }


        [Description("Имя пользователя")]
        [DisplayName("Имя пользователя")]
        [RequiredArgument]
        public InArgument<string> UserName { get; set; }

        [Description("Пароль")]
        [DisplayName("Пароль")]
        [RequiredArgument]
        public InArgument<string> Password { get; set; }

        
        protected override bool Execute(CodeActivityContext context)
        {
            bool Res = false;
            try
            {
                Res = (ARM_Service.Login(UserName.Get(context), Password.Get(context)) != null);
            }
            catch (Exception ex)
            {
                if (!HideException.Get(context))
                    throw ex;
            }
            return Res;
        }

    }

}
