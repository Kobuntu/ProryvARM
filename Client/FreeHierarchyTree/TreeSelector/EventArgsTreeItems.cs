using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.TreeSelector
{
    public class EventArgsTreeItems : EventArgs
    {
        public ICollection<FreeHierarchyTreeItem> Items;
    }
}
