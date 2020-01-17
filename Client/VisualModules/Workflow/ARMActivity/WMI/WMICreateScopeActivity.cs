using System;
using System.Activities;
using System.ComponentModel;
using System.Management;
using Microsoft.VisualBasic.Devices;

namespace Proryv.Workflow.Activity.ARM.WMI
{
    [Description("�������� Scope ��� ����������� � WMI ���������� ����������")]
    [DisplayName("WMI �������� Scope")]
    public class WMICreateScopeActivity : BaseArmActivity<bool>
    {
        [Description("��������� ����������� � WMI ���������� ����������")]
        [DisplayName("��������� �����������")]
        [RequiredArgument]
        public InOutArgument<ManagementScope> Scope { get; set; }

        [Description("������������ ���� WMI (WMI namespace) ���������� ����������")]
        [DisplayName("������������ ���� WMI (WMI namespace)")]
        [RequiredArgument]
        public InArgument<string> Namespaces { get; set; }

        [Description("��� ���������� ���������� ��� �����������")]
        [DisplayName("��� ���������� ����������")]
        [RequiredArgument]
        public InArgument<string> HostName { get; set; }

        [Description("��� ������������ ���������� ���������� ��� �����������")]
        [DisplayName("��� ������������ ���������� ����������")]
        [RequiredArgument]
        public InArgument<string> Username { get; set; }

        [Description("������ ������������ ���������� ���������� ��� �����������")]
        [DisplayName("������ ������������ ���������� ����������")]
        [RequiredArgument]
        public InArgument<string> Password { get; set; }

        public WMICreateScopeActivity()
        {
            this.DisplayName = "WMI �������� Scope";
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