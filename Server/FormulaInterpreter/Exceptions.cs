using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.Servers.Calculation.FormulaInterpreter
{
    class FormulaParseException : Exception
    {
        public FormulaParseException(string message) : base(message)
        {
            
        }
    }
}
