using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using System.Diagnostics;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;
using System.Net;
using Proryv.Workflow.Activity.ARM.FTP;
using Proryv.Workflow.Activity.Settings;

namespace Proryv.Workflow.Activity.ARM
{
    public class FtpUploadFile : FtpBase
    {
        public FtpUploadFile()
        {
            this.DisplayName = "FTP - выгрузка файла на FTP";
        }
        
        [Description("Строка ошибки")]
        [DisplayName("Ошибка")]
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        public OutArgument<string> Error { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Document")]
        [RequiredArgument]
        public InArgument<MemoryStream> Document { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Имя файла")]
        [RequiredArgument]
        public InArgument<string> FileName { get; set; }
       

        protected override bool Execute(CodeActivityContext context)
        {
            
            string ftpURL = null;
            string username = null;
            string password = null;
            string folder;
            string fileName = null;
            string port = null;
            

            ftpURL = context.GetValue(FTPUrl);
            username = context.GetValue(Username);
            password = context.GetValue(Password);
            folder = context.GetValue(Folder);
            fileName = context.GetValue(FileName);
            port = context.GetValue(Port);
            
            MemoryStream document = Document.Get(context);
           
            FtpWebResponse uploadResponse = null;
            try
            {

                if (!(ftpURL.StartsWith("ftp"))) { ftpURL = "ftp://" + ftpURL; }
                if (!(port.EndsWith("/"))) { port += "/"; }
                if (!(port.StartsWith(":"))) { port = ":"+port; }
                if (!string.IsNullOrEmpty(folder))
                if (!(folder.EndsWith("/"))) { folder += "/"; }
                string fullURL = ftpURL + port + folder + fileName;


                FtpWebRequest uploadRequest;
                ICredentials credentials = new NetworkCredential(username, password);
                uploadRequest = (FtpWebRequest)WebRequest.Create(fullURL);
                uploadRequest.Method = WebRequestMethods.Ftp.UploadFile;
                uploadRequest.Credentials = credentials;
                uploadRequest.Proxy = null;
                uploadRequest.UsePassive = true;
                uploadRequest.UseBinary = true;
                //uploadRequest.KeepAlive = false;


                byte[] buffer = document.GetBuffer();
                document.Close();
                using (Stream reqStream = uploadRequest.GetRequestStream())
                {
                    reqStream.Write(buffer, 0, buffer.Length);
                }
                uploadResponse = (FtpWebResponse)uploadRequest.GetResponse();

            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
            }
            finally
            {
                if (uploadResponse != null)
                {
                    uploadResponse.Close();
                }
                if (document != null)
                {
                    document.Close();
                    document.Dispose();
                }
            }

            return string.IsNullOrEmpty(Error.Get(context));
        }
    }
}
