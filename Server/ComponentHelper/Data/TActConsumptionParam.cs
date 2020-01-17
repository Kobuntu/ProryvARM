using Proryv.AskueARM2.Server.DBAccess.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;

namespace Proryv.AskueARM2.Server.VisualCompHelpers.Data
{
    public class TActConsumptionParam
    {
        public enumTypeOfMeasuring TypeOfMeasuring;
        public string SectionName;
        public string TpName;
        public string PersonName;
        public string Voltage;
        public string ObjectConsumerName;
        public int DoublePrecision;
        public TExportExcelAdapterType ExportType;
        public List<TValue> TiArchives;
        public Dictionary<int, string> TiOurSideDict;
        public Dictionary<int, string> TiContrSideDict;
    }
}
