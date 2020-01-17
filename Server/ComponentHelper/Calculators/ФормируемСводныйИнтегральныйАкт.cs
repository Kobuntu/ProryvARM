using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Server.VisualCompHelpers
{
    public partial class ExcelReportFreeHierarchyAdapter
    {
        private MemoryStream ФормируемСводныйИнтегральныйАкт(BalanceFreeHierarchyCalculatedResult balanceCalculatedResult)
        {
            var channelNames = new Dictionary<byte, string>
            {
                { 1, "Прием"},
                { 2, "Отдача"}
            };

            var xls = InitIntegralAct(new Data.XlsFileParamIntegralAct
            {
                DoublePrecisionProfile = _doublePrecisionProfile,
                DTStart = _dtStart,
                DTEnd = _dtEnd,
                IsInterval = _isInterval,
                TimeZoneId = _timeZoneId,
                UnitDigit = DBAccess.Internal.EnumUnitDigit.Kilo,
                ChannelNames = channelNames,
            });



            return Export(xls);
        }
    }
}
