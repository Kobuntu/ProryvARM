using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas.Variables
{
    public class WorkedPeriodVariable
    {
        public readonly string Id;
        public bool IsIndexInWorkedPeriod;

        private List<Tuple<int, int?>> WorkedIndexes;

        public WorkedPeriodVariable()
        {

        }

        public void FillVariable(int indx)
        {
            if (WorkedIndexes == null) return;

            IsIndexInWorkedPeriod =  WorkedIndexes.Any(wi => wi.Item1 >= indx && (!wi.Item2.HasValue || indx <= wi.Item2.Value));
        }
    }
}
