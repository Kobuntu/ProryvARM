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
using System.Text.RegularExpressions;
using Proryv.Workflow.Activity.ARM.FTP;
using Proryv.Workflow.Activity.Settings;

namespace Proryv.Workflow.Activity.ARM
{
    public class FtpListDirectoryDetails : FtpBase
    {
        public FtpListDirectoryDetails()
        {
            this.DisplayName = "FTP - Получить перечень файлов";
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
        
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список файлов")]
        public OutArgument<IEnumerable<FtpDirectoryItem>> FileList { get; set; }


        /// <summary>
        /// получение списка файлов
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override bool Execute(CodeActivityContext context)
        {
            string ftpURL = null;
            string username = null;
            string password = null;
            string folder;
            string port = null;
            
            ftpURL = context.GetValue(FTPUrl);
            username = context.GetValue(Username);
            password = context.GetValue(Password);
            folder = context.GetValue(Folder);
            port = context.GetValue(Port);

            try
            {
                if (!(ftpURL.StartsWith("ftp"))) { ftpURL = "ftp://" + ftpURL; }
                if (!(port.EndsWith("/"))) { port += "/"; }
                if (!(port.StartsWith(":"))) { port = ":" + port; }
                if (!string.IsNullOrEmpty(folder))
                    if (!(folder.EndsWith("/"))) { folder += "/"; }
                string fullURL = ftpURL + port + folder ;
                if (!(fullURL.EndsWith("/"))) { fullURL += "/"; }
                
                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(fullURL);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.Credentials = new NetworkCredential(username, password);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;
                
                List<FtpDirectoryItem> returnValue = new List<FtpDirectoryItem>();
                string[] list = null;

                using (FtpWebResponse response = (FtpWebResponse) request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        list = reader.ReadToEnd().Split(new string[] {"\r\n"}, StringSplitOptions.None);//RemoveEmptyEntries);
                    }
                }

                foreach (string line in list)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;


                    #region не работает 1
                    ////http://stackoverflow.com/questions/25246426/extracting-file-names-from-webrequestmethods-ftp-listdirectorydetails
                    //string regex =
                    //                @"^" +                          //# Start of line
                    //                @"(?<dir>[\-ld])" +             //# File size          
                    //                @"(?<permission>[\-rwx]{9})" +  //# Whitespace          \n
                    //                @"\s+" +                        //# Whitespace          \n
                    //                @"(?<filecode>\d+)" +
                    //                @"\s+" +                        //# Whitespace          \n
                    //                @"(?<owner>\w+)" +
                    //                @"\s+" +                        //# Whitespace          \n
                    //                @"(?<group>\w+)" +
                    //                @"\s+" +                        //# Whitespace          \n
                    //                @"(?<size>\d+)" +
                    //                @"\s+" +                        //# Whitespace          \n
                    //                @"(?<month>\w{3})" +            //# Month (3 letters)   \n
                    //                @"\s+" +                        //# Whitespace          \n
                    //                @"(?<day>\d{1,2})" +            //# Day (1 or 2 digits) \n
                    //                @"\s+" +                        //# Whitespace          \n
                    //                @"(?<timeyear>[\d:]{4,5})" +    //# Time or year        \n
                    //                @"\s+" +                        //# Whitespace          \n
                    //                @"(?<filename>(.*))" +          //# Filename            \n
                    //                @"$";

                    //var split = new Regex(regex).Match(line);
                    //string dir = split.Groups["dir"].ToString();
                    //string filename = split.Groups["filename"].ToString();
                    //bool isDirectory = !string.IsNullOrWhiteSpace(dir) && dir.Equals("d", StringComparison.OrdinalIgnoreCase);
                    #endregion

                    #region вариант 2

                    //Regex FtpListDirectoryDetailsRegex = new Regex(@".*(?<month>(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec))\s*(?<day>[0-9]*)\s*(?<yearTime>([0-9]|:)*)\s*(?<fileName>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    //string ftpResponse = "-r--r--r-- 1 ftp ftp              0 Nov 19 11:08 aaa.txt";
                    //Match match = FtpListDirectoryDetailsRegex.Match(ftpResponse);
                    //string month = match.Groups["month"].Value;
                    //string day = match.Groups["day"].Value;
                    //string yearTime = match.Groups["yearTime"].Value;
                    //string fileName = match.Groups["fileName"].Value;


                    ////Using ListDirectoryDetails should yield a line starting with "-" or "d" 
                    ////I have not seen a scenario when either characters are not present. "d" == directory, "-" == file 

                    ////http://www.seesharpdot.net/?p=242

                    #endregion


                    // Windows FTP Server Response Format
                    // DateCreated    IsDirectory    Name
                    string data = line;

                    // Parse date
                    string date = data.Substring(0, 17);
                    DateTime dateTime = DateTime.Parse(date, System.Globalization.CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat);
                    //DateTime.Parse(date);
                    data = data.Remove(0, 24);

                    // Parse <DIR>
                    string dir = data.Substring(0, 5);
                    bool isDirectory = dir.Equals("<dir>", StringComparison.InvariantCultureIgnoreCase);

                    if (isDirectory)
                        continue;

                    data = data.Remove(0, 5);
                    data = data.Remove(0, 10);

                    // Parse name
                    string filename = data;

                    // Create directory info
                    FtpDirectoryItem item = new FtpDirectoryItem
                    {
                        BaseUri = new Uri(fullURL),
                        DateCreated = dateTime,
                        IsDirectory = isDirectory,
                        StringName = filename
                    };

                    //Debug.WriteLine(item.AbsolutePath);
                    //без подпапок
                    //item.Items = item.IsDirectory ? Execute(item.AbsolutePath, username, password) : null;
                    
                    returnValue.Add(item);
                }
                 

                FileList.Set(context, returnValue);
            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
            }
             


            return string.IsNullOrEmpty(Error.Get(context));
        }
      


    }
}
