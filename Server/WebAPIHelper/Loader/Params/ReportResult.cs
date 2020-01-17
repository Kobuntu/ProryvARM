using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;

namespace Proryv.AskueARM2.Server.WCF.Loader.Params
{
    [DataContract]
    public class ReportResult
    {
        [DataMember]
        public TReportResult Report;
        [DataMember]
        public MemoryStream CompressedDocument;
    }
}
