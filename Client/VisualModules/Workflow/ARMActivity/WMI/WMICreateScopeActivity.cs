using System;
using System.Activities;
using System.ComponentModel;
using System.Management;
using Microsoft.VisualBasic.Devices;

namespace Proryv.Workflow.Activity.ARM.WMI
{
    [Description("Создание Scope для подключения к WMI удаленного компьютера")]
    [DisplayName("WMI Создание Scope")]
    public class WMICreateScopeActivity : BaseArmActivity<bool>
    {
        [Description("Результат подключения к WMI удаленного компьютера")]
        [DisplayName("Результат подключения")]
        [RequiredArgument]
        public InOutArgument<ManagementScope> Scope { get; set; }

        [Description("Пространство имен WMI (WMI namespace) удаленного компьютера")]
        [DisplayName("Пространство имен WMI (WMI namespace)")]
        [RequiredArgument]
        public InArgument<string> Namespaces { get; set; }

        [Description("Имя удаленного компьютера для подключения")]
        [DisplayName("Имя удаленного компьютера")]
        [RequiredArgument]
        public InArgument<string> HostName { get; set; }

        [Description("Имя пользователя удаленного компьютера для подключения")]
        [DisplayName("Имя пользователя удаленного компьютера")]
        [RequiredArgument]
        public InArgument<string> Username { get; set; }

        [Description("Пароль пользователя удаленного компьютера для подключения")]
        [DisplayName("Пароль пользователя удаленного компьютера")]
        [RequiredArgument]
        public InArgument<string> Password { get; set; }

        public WMICreateScopeActivity()
        {
            this.DisplayName = "WMI Создание Scope";
        }

        protected override bool Execute(CodeActivityContext context)
        {
            var nm = context.GetValue(this.Namespaces);
            var host = context.GetValue(this.HostName);
            var user = context.GetValue(this.Username);
            var pass = context.GetValue(this.Password);
            var hideex = context.GetValue(this.HideException);

            try
            {

                ConnectionOptions wmConnectOption = null;
                Computer myComputer = new Computer();
                if (host != myComputer.Name)
                {
                    wmConnectOption = new ConnectionOptions
                                          {
                                              Impersonation = ImpersonationLevel.Impersonate,
                                              EnablePrivileges = true,
                                              Username = user,
                                              Password = pass
                                          };
                }
                ManagementScope target = wmConnectOption == null
                                             ? new ManagementScope("\\\\" + host + "\\" + nm)
                                             : new ManagementScope("\\\\" + host + "\\" + nm, wmConnectOption);
                context.SetValue(this.Scope, target);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
                if (hideex)
                    return false;
                throw;
            }
        }
    }
}