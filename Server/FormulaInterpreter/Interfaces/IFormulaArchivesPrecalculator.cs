using System.Collections.Generic;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;

namespace Proryv.Servers.Calculation.FormulaInterpreter
{
    public interface IFormulaArchivesPrecalculator
    {
        TVALUES_DB GetArchive(IHierarchyChannelID id, int halfHourIndx, enumTimeDiscreteType discreteType,
            List<DBAccess.Interface.Data.TVALUES_DB> requestedArchives);

        double ×÷è(IHierarchyChannelID id, int halfHourIndx, enumTimeDiscreteType discreteType);
    }
}