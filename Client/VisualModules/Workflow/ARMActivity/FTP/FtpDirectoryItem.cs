using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.Workflow.Activity.ARM.FTP
{
    public struct FtpDirectoryItem
    {
        public Uri BaseUri;

        public string AbsolutePath
        {
            get
            {
                return string.Format("{0}/{1}/", BaseUri, StringName);
            }
        }

        public DateTime DateCreated;
        public bool IsDirectory;
        public string StringName;
        public List<FtpDirectoryItem> Items;
    }

}
