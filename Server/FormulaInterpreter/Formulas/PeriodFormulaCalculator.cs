using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    //TODO Калькулятор для выбора нужного результата из нескольких формул за период
    public class PeriodFormulaCalculator
    {
        public string Id;

        private readonly List<TVALUES_DB> _val1;
        private readonly List<TVALUES_DB> _val2;

        private double _sumVal1;
        private double _sumVal2;

        public PeriodFormulaCalculator()
        {
            _val1 = new List<TVALUES_DB>();
            _val2 = new List<TVALUES_DB>();
        }

        public void Calculate(TVALUES_DB v1, TVALUES_DB v2)
        {
            _val1.Add(v1);
            _val2.Add(v2);

            if (v1 != null) _sumVal1 += v1.F_VALUE; //todo возможно нужно будет проверять достоверность
            if (v2 != null) _sumVal2 += v2.F_VALUE; 
        }
    }
}
