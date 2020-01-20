using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.Servers.Calculation.DBAccess.Common.Data;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data
{
    public class BalancePsParam
    {
        public string BalanceUn;
        public List<TI_ChanelType> Tis;
        public List<TP_ChanelType> Tps;
        public List<string> Formulas;
    }
}
