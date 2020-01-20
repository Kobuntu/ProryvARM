using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.Servers.Calculation.DBAccess.Common.Data;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data
{
    public class BalanceFreeHierarchyToObject
    {
        public string BalanceFreeHierarchy_UN;
        public string BalanceFreeHierarchyObject_UN;
        public int? FreeHierItem_ID;
        public byte? HierLev1_ID;
        public int? HierLev2_ID;
        public int? HierLev3_ID;
        public int? PS_ID;
        public int? TI_ID;

        public ID_TypeHierarchy ToIdTypeHierarchy()
        {
            int id;
            enumTypeHierarchy typeHierarchy;

            if (PS_ID.HasValue)
            {
                id = PS_ID.Value;
                typeHierarchy = enumTypeHierarchy.Dict_PS;
            }
            else if (FreeHierItem_ID.HasValue)
            {
                id = FreeHierItem_ID.Value;
                typeHierarchy = enumTypeHierarchy.Node;
            }
            else if (HierLev3_ID.HasValue)
            {
                id = HierLev3_ID.Value;
                typeHierarchy = enumTypeHierarchy.Dict_HierLev3;
            }
            else if (HierLev2_ID.HasValue)
            {
                id = HierLev2_ID.Value;
                typeHierarchy = enumTypeHierarchy.Dict_HierLev2;
            }
            else if (HierLev1_ID.HasValue)
            {
                id = HierLev1_ID.Value;
                typeHierarchy = enumTypeHierarchy.Dict_HierLev1;
            }
            else if (TI_ID.HasValue)
            {
                id = TI_ID.Value;
                typeHierarchy = enumTypeHierarchy.Info_TI;
            }
            else
            {
                return null;
            }

            return new ID_TypeHierarchy(typeHierarchy, id);
        }
    }
}
