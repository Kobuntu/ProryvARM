using System;
using Proryv.AskueARM2.Server.DBAccess.Internal.Formulas.Variables;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;
using Proryv.Servers.Calculation.FormulaInterpreter;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    public class Function
    {
        private string m_id;

        public string id
        {
            get { return m_id; }
            set { m_id = value.ToLower(); }
        }

        public string Description;

        ///<summary>
        /// число аргументов функции
        /// int.MaxValue без ограничений
        ///</summary>
        public int FuncArgsCount = 1;

        /// <summary>
        /// Значение по умолчанию. Если FuncDlgt=null
        /// </summary>
        public double Value = 1;

        public Object UserTag;
        public Object UserTag2;

        public DeletateToFunctionVal FuncDlgt;
        public DeletateToFunctionBool FuncBoolDlgt;

        public BooleanFunction BoolFunc;
    }


    /// <summary>
    /// Можно задавать значения определенного типа
    /// </summary>
    public interface ITypedIndexer
    {
        object GetTypedValue(int index);
        void SetTypedValue(int index, object value);
        int GetLenght();
    }

    /// <summary>
    /// Как перечисление определенного типа
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITypedIndexer<T> : ITypedIndexer where T : class
    {
        T this[int index] { get; set; }
    }

    public delegate TVALUES_DB DeletateToVariableVal(IFormulaParser parser, Variable var);

    public delegate IConvertible DeletateToFunctionVal(IFormulaParser parser, Function func,
        FunctionArgumentList args,
        int arg_index,
        int args_count);

    public delegate IConvertible DeletateToFunctionBool(IFormulaParser parser, WorkedPeriodVariable precalcVariable,
        IConvertible arg1, IConvertible arg2, int indx);

    public delegate bool BooleanFunction(WorkedPeriodVariable objectWork, int indx);
}