using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Server.VisualCompHelpers.Data
{
    public class FormulaRowsRange
    {
        public string SheetName;
        public int Row1;
        public int Row2;
        public int Col;
        public int? Col2;
        public string Before;
        public string After;
        public bool IsMinus;
    }
}
