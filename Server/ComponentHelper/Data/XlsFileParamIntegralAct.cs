using FlexCel.Core;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Server.VisualCompHelpers.Data
{
    public class XlsFileParamIntegralAct
    {
        public int DoublePrecisionProfile;
        public DateTime DTStart;
        public DateTime DTEnd;
        public string TimeZoneId;
        public bool IsInterval;
        public EnumUnitDigit UnitDigit;

        public Dictionary<byte, string> ChannelNames;
    }
}
