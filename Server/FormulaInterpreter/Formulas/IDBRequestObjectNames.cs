using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    public interface IDBRequestObjectNames
    {
        string GetObjectName(string Oper_ID, Proryv.AskueARM2.Server.DBAccess.Internal.Formulas.F_OPERATOR.F_OPERAND_TYPE Oper_type, byte? Channel);
    }
}
