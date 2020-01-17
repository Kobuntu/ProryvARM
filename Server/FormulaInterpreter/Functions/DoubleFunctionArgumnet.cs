namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    public class DoubleFunctionArgumnet : IFunctionArgumnet<double>
    {
        private double val;

        public double Value
        {
            get { return val; }
        }

        public VALUES_FLAG_DB? Flag { get; set; }

        public VALUES_FLAG_DB? flag
        {
            set { Flag = value; }
        }

        public dynamic ArgumentValue
        {
            get { return val; }
            set { val = (double)value; }
        }

        public FunctionArgumentArgsTypeEnum ArgumentType
        {
            get { return FunctionArgumentArgsTypeEnum.Double; }
        }
    }
}