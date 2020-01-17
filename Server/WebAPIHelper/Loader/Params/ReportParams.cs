using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;

namespace Proryv.AskueARM2.Server.WCF.Loader
{
    [DataContract]
    public class ReportParams
    {
        [DataMember] public string UserId;

        [DataMember] public ReportExportFormat ExportFormat;

        [DataMember] public string ReportUn;

        [DataMember] public DateTime DtStart;

        [DataMember] public DateTime DtEnd;

        [DataMember] public string TimeZoneId;

        [DataMember] public enumReportGroup ReportGroup;

        [DataMember] public string Args;
    }
}
