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
    public class FtpDeleteFile : FtpBase
    {
        public FtpDeleteFile()
        {
            this.DisplayName = "FTP - удаление файла с FTP";
        }


        [Description("Строка ошибки")]
        [DisplayName("Ошибка")]
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        public OutArgument<string> Error { get; set; }

       
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
            string fullURL = "";
            try
            {
                if (!(ftpURL.StartsWith("ftp"))) { ftpURL = "ftp://" + ftpURL; }
                if (!(port.EndsWith("/"))) { port += "/"; }
                if (!(port.StartsWith(":"))) { port = ":" + port; }
                if (!string.IsNullOrEmpty(folder))
                    if (!(folder.EndsWith("/"))) { folder += "/"; }
                fullURL = ftpURL + port + folder + fileName;


                FtpWebRequest uploadRequest;
                ICredentials credentials = new NetworkCredential(username, password);
                uploadRequest = (FtpWebRequest)WebRequest.Create(fullURL);
                uploadRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                uploadRequest.Credentials = credentials;
                uploadRequest.Proxy = null;
                uploadRequest.UsePassive = true;
                uploadRequest.UseBinary = true;
                //uploadRequest.KeepAlive = false;




                using (var response = (FtpWebResponse)uploadRequest.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        
                    }
                }

                //?
                //string result = String.Empty;
                //FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                //long size = response.ContentLength;
                //Stream datastream = response.GetResponseStream();
                //StreamReader sr = new StreamReader(datastream);
                //result = sr.ReadToEnd();
                //sr.Close();
                //datastream.Close();
                //response.Close();

            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message +" "+ fullURL);
            }
            finally
            {
            }

            return string.IsNullOrEmpty(Error.Get(context));
        }

        

    }
}
