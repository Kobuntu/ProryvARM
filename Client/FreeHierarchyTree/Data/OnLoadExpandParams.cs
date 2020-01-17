using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.Data
{
    public class OnLoadExpandParams
    {
        public ConcurrentStack<ID_TypeHierarchy> Path;
        public bool IsExpandLast;
        public bool IsSelect;
    }
}
