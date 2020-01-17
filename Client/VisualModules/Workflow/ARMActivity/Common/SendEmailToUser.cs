using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.IO;
using System.Activities.Presentation.Metadata;
using Pop3;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;
using System.Net.Mail;

namespace Proryv.Workflow.Activity.ARM
{
    public class SendEmailToUser : BaseArmActivity<bool>
    {
        public SendEmailToUser()
        {
            //this.Port = 25;
            this.DisplayName = "Отправить электронную почту для пользователя";
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(SendEmailToUser), "UserName", new EditorAttribute(typeof(SendEmailToUserPropEditor), typeof(SendEmailToUserPropEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        [RequiredArgument]
        [Description("Имя пользователя")]
        [DisplayName("Пользователь")]
        public string UserName { get; set; }

        [Description("Тема письма")]
        [DisplayName("Тема")]
        public InArgument<string> Subject { get; set; }

        [DefaultValue(null)]
        [DisplayName("Вложениe")]
        public InArgument<MemoryStream> Attach { get; set; }

        [DefaultValue(null)]
        [DisplayName("Имя вложения")]
        public InArgument<string> AttachName { get; set; }
        
        [DisplayName("Тело письма")]
        public InArgument<string> Body { get; set; }

        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }


        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (string.IsNullOrEmpty(UserName))
                metadata.AddValidationError("Значение свойства 'Пользователь' не может быть пустым");
            base.CacheMetadata(metadata);
        }


        protected override bool Execute(CodeActivityContext context)
        {
            string Email = "";
            if (string.IsNullOrEmpty(UserName))
            {
                Error.Set(context, "Значение свойства 'Пользователь' не может быть пустым");
                return false;
            }
            bool FoundUser = false;
            try
            {
              List<UserInfo> UList = ARM_Service.EXPL_Get_All_Users();
              foreach (UserInfo u in UList)
              {
                  if (u.UserName.ToLower(System.Globalization.CultureInfo.InvariantCulture) == UserName.ToLower(System.Globalization.CultureInfo.InvariantCulture))
                  {
                      FoundUser = true;
                      Email = u.Email;
                      break;
                  }

              }

            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            if (!string.IsNullOrEmpty(Error.Get(context)))
                return false;

            if (!FoundUser)
            {
                Error.Set(context, "Не найден пользователь с именем '" + UserName + "'");
                return false;
            }


            if (string.IsNullOrEmpty(Email))
            {
                Error.Set(context, "У пользователя '"+UserName+"' не определен адрес электронной почты");
                return false;
            }            


            Expl_UserNotify_EMailServiceConfiguration Config = null;
            try
            {
                Config = ARM_Service.EXPL_Get_UserNotify_EmailServiceConfigurations().FirstOrDefault();
                if (Config == null)
                {
                    Error.Set(context, "Не найдено ни одной записи в таблице конфигурации почтового сервиса");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            UserNotify_EMailServiceConfigurationItem ConfigurationItem = null;
            try
            {
               ConfigurationItem = UserNotify_EMailServiceConfigurationItem.FromXElement(Config.Data);
            }
            catch (Exception ex)
            {
                Error.Set(context, "Ошибка разбора Xml : " + ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            //if (UseZipArchive)
            //{
            //    MemoryStream compresStream = new MemoryStream();
            //    ComponentAce.Compression.ZipForge.ZipForge zip = new ComponentAce.Compression.ZipForge.ZipForge();
            //    zip.FileName = attachName + ".zip"; ;
            //    AttachContent.Position = 0;
            //    zip.OpenArchive(compresStream, true);
            //    zip.AddFromStream(attachName + GetFileExtByReportFormat(), AttachContent);
            //    zip.CloseArchive();
            //    Attach = new Attachment(AttachContent, zip.FileName);
            //    zip.Dispose();
            //}
            //

            try
            {
                Pop3Helper.ReadAllMails(UserNotifyConfigClass.Pop3_Host, UserNotifyConfigClass.Smtp_User, UserNotifyConfigClass.Smtp_Password);
                var mailMessage = new System.Net.Mail.MailMessage();

                mailMessage.To.Add(Email);
                if (string.IsNullOrEmpty(Subject.Get(context)))
                    mailMessage.Subject = ConfigurationItem.Smtp_Subject;
                else
                    mailMessage.Subject = Subject.Get(context);

                if (mailMessage.Subject != null)
                    mailMessage.Subject = mailMessage.Subject.Replace("@PsName", "")
                    .Replace("@data", "")
                    .Replace("@ReportName", "");

                mailMessage.Body = Body.Get(context);
                mailMessage.From = new System.Net.Mail.MailAddress(ConfigurationItem.Smtp_From);
                var smtp = new System.Net.Mail.SmtpClient();
                smtp.Host = ConfigurationItem.Smtp_Host;
                smtp.Port = ConfigurationItem.Smtp_Port;

                MemoryStream a = Attach.Get(context);
                if (a != null)
                {
                    string an = AttachName.Get(context);
                    if (string.IsNullOrEmpty(an))
                        an = "Вложение";
                    Attachment at = new Attachment(a, an);
                    mailMessage.Attachments.Add(at);
                }
                smtp.Credentials =
                    new System.Net.NetworkCredential(ConfigurationItem.Smtp_User, ConfigurationItem.Smtp_Password);
                //smtp.EnableSsl = true;
                smtp.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Error.Set(context, "Ошибка отправки письма : " + ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }
            
            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}
