using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;
using System.Net;
using Proryv.Workflow.Activity.Settings;

namespace Proryv.Workflow.Activity.ARM
{
    public abstract class FtpBase : BaseArmActivity<bool>
    {
        public FtpBase()
        {
            this.DisplayName = "Настройки FTP";
        }


        [Description("Строка ошибки")]
        [DisplayName("Ошибка")]
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        public OutArgument<string> Error { get; set; }

       
        [Description("Идентификатор пользователя в системе")]
        [DisplayName("Идентификатор пользователя")]
        [Category("Отчет")]
        public string User_ID { get; set; }

        //--------------------------------------------------------------       

        [RequiredArgument]
        [Category("FTP-сервер")]
        [DisplayName("Адрес")]
        [Description("Адрес FTP-сервер")]
        public InArgument<string> FTPUrl { get; set; }
        [RequiredArgument]
        [Category("FTP-сервер")]
        [DisplayName("Имя пользователя")]
        public InArgument<string> Username { get; set; }
        [RequiredArgument]
        [Category("FTP-сервер")]
        [DisplayName("Пароль")]
        public InArgument<string> Password { get; set; }
        [Category("FTP-сервер")]
        [DisplayName("Папка")]
        public InArgument<string> Folder { get; set; }
        [Category("FTP-сервер")]
        [DisplayName("Порт")]
        public InArgument<string> Port { get; set; }
        
        


    }
}
