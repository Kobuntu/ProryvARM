using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal;

namespace Proryv.Servers.Calculation.FormulaInterpreter
{
    public interface IDBInterfaceAdapter
    {

        string GetObjectName(string Oper_ID,
            Proryv.AskueARM2.Server.DBAccess.Internal.Formulas.F_OPERATOR.F_OPERAND_TYPE Oper_type, byte? Channel);

        string GetFormulaName(string f_id, enumFormulasTable formulasTable);


        SqlConnection GetSqlConnection();

    }
}
