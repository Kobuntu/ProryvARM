using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.Data
{
    public class TTreeCounter
    {
        public string ElementsName;
        public Func<int> Calculator;
        public SynchronizationContext Context;
    }
}
