using System;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    public interface IFunctionArgumnet<T> : IFunctionArgumnet where T : IConvertible
    {
        T Value { get; }
     
    }




    public interface IFunctionArgumnet
    {
        //что бы избежать Box-Unbox
        dynamic ArgumentValue { get; set; }

        FunctionArgumentArgsTypeEnum ArgumentType { get; }

        System.Nullable<VALUES_FLAG_DB> Flag
        {
            get;
        }
    }
}