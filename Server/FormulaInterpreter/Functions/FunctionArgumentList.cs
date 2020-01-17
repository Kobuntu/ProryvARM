using System;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    public class FunctionArgumentList : FunctionArgumentCollection<IFunctionArgumnet>
    {
        public DateTime StartDateTime;

        public DateTime EndDateTime;

        public int HalfHourIndex;

        public enumTimeDiscreteType DiscreteType;


        public FunctionArgumentList() : base()
        {
        }

        public FunctionArgumentList(int capacity) : base(capacity)
        {
        }
    }
}