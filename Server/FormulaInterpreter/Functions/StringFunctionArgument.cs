namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    public class StringFunctionArgument : IFunctionArgumnet<string>
    {
        private string val;

        public string Value
        {
            get { return val; }
        }

        public VALUES_FLAG_DB? Flag { get; private set; }

        public VALUES_FLAG_DB? flag
        {
            set { Flag = value; }
        }


        public dynamic ArgumentValue
        {
            get { return val; }
            set { val = value.ToString(); }
        }

        public FunctionArgumentArgsTypeEnum ArgumentType
        {
            get { return FunctionArgumentArgsTypeEnum.String; }
        }
    }
}