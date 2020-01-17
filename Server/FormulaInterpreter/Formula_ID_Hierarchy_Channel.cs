using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.Servers.Calculation.DBAccess.Interface;

namespace Proryv.Servers.Calculation.FormulaInterpreter
{
    class Formula_ID_Hierarchy_Channel : IHierarchyChannelID
    {
       

        public Formula_ID_Hierarchy_Channel(enumTypeHierarchy typeHierarchy, string f_ID, byte channel)
        {
            TypeHierarchy = typeHierarchy;
            this.ID = f_ID;
            this.Channel = channel;
        }

        public enumTypeHierarchy TypeHierarchy { get; set; }
        public string ID { get; set; }
        public byte Channel { get; set; }
        public enumRequestType RequestType { get; set; }
        public enumArchTechParamType? ArchTechParamType { get; set; }

        public override bool Equals(object obj)
        {
            var id = obj as IHierarchyChannelID;
            if (id == null) return false;

            return (TypeHierarchy == id.TypeHierarchy) && (Channel == id.Channel) && ID == id.ID;
        }

        public override int GetHashCode()
        {
            string s = String.Format("{0}{1}{2}", TypeHierarchy, Channel, ID);
            return s.GetHashCode();
        }
    }
}
