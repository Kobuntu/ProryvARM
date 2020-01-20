using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace Proryv.Servers.Calculation.Parser.Internal
{
    public partial class ProryvParser
    {
        #region Structures
        public class StiParserData
        {
            public object Data = null;
            public List<StiAsmCommand> AsmList = null;
            public List<StiAsmCommand> AsmList2 = null;
            public List<StiAsmCommand> ConditionAsmList = null;
            public ProryvParser Parser = null;

            public StiParserData(object data, List<StiAsmCommand> asmList, ProryvParser parser)
            {
                this.Data = data;
                this.AsmList = asmList;
                this.Parser = parser;
                this.ConditionAsmList = null;
            }

            public StiParserData(object data, List<StiAsmCommand> asmList, ProryvParser parser, List<StiAsmCommand> conditionAsmList)
            {
                this.Data = data;
                this.AsmList = asmList;
                this.Parser = parser;
                this.ConditionAsmList = conditionAsmList;
            }
        }

        public class StiToken
        {
            public StiTokenType Type = StiTokenType.Empty;
            public string Value;
            public object ValueObject;
            public int Position = -1;
            public int Length = -1;

            public StiToken(StiTokenType type, int position, int length)
            {
                this.Type = type;
                this.Position = position;
                this.Length = length;
            }
            public StiToken(StiTokenType type, int position)
            {
                this.Type = type;
                this.Position = position;
            }
            public StiToken(StiTokenType type)
            {
                this.Type = type;
            }
            public StiToken()
            {
                this.Type = StiTokenType.Empty;
            }

            public override string ToString()
            {
                return string.Format("TokenType={0}{1}", Type.ToString(), Value != null ? string.Format(", value=\"{0}\"", Value) : "");
            }
        }

        public class StiAsmCommand
        {
            public StiAsmCommandType Type;
            public object Parameter1;
            public object Parameter2;
            public int Position = -1;
            public int Length = -1;

            public StiAsmCommand(StiAsmCommandType type)
                : this(type, null, null)
            {
            }

            public StiAsmCommand(StiAsmCommandType type, object parameter)
                : this(type, parameter, null)
            {
            }

            public StiAsmCommand(StiAsmCommandType type, object parameter1, object parameter2)
            {
                this.Type = type;
                this.Parameter1 = parameter1;
                this.Parameter2 = parameter2;
            }

            public override string ToString()
            {
                return string.Format("{0}({1},{2})", Type.ToString(),
                    Parameter1 != null ? Parameter1.ToString() : "null",
                    Parameter2 != null ? Parameter2.ToString() : "null");
            }
        }

        public class StiParserMethodInfo
        {
            public ProryvFunctionType Name;
            public int Number;
            public Type[] Arguments;
            public Type ReturnType;

            public StiParserMethodInfo(ProryvFunctionType name, int number, Type[] arguments)
            {
                this.Name = name;
                this.Number = number;
                this.Arguments = arguments;
                this.ReturnType = typeof(string);
            }

            public StiParserMethodInfo(ProryvFunctionType name, int number, Type[] arguments, Type returnType)
            {
                this.Name = name;
                this.Number = number;
                this.Arguments = arguments;
                this.ReturnType = returnType;
            }

        }
        #endregion

        #region Fields

        private string inputExpression = string.Empty;
        private object sender = null;

        private int position = 0;
        private List<StiToken> tokensList = null;
        private StiToken currentToken = null;
        private int tokenPos = 0;
        private List<StiAsmCommand> asmList = null;
        private Hashtable hashAliases = null;

        public int expressionPosition = 0;

        private bool isVB
        {
            get
            {
                return false;
            }
        }
        #endregion
        
        #region ExecuteAsm
        public object ExecuteAsm(object objectAsmList)
        {
            List<StiAsmCommand> asmList = objectAsmList as List<StiAsmCommand>;
            if (asmList == null || asmList.Count == 0) return null;
            Stack stack = new Stack();
            ArrayList argsList = null;
            object par1 = 0;
            object par2 = 0;
            foreach (StiAsmCommand asmCommand in asmList)
            {
                switch (asmCommand.Type)
                {
                    case StiAsmCommandType.PushValue:
                        stack.Push(asmCommand.Parameter1);
                        break;
                    case StiAsmCommandType.PushVariable:
                        //stack.Push(getVariableValue((string)asmCommand.Parameter1));
                        stack.Push((string)asmCommand.Parameter1);
                        break;
                    case StiAsmCommandType.PushSystemVariable:
                        stack.Push(get_systemVariable(asmCommand.Parameter1));
                        break;
                    case StiAsmCommandType.PushComponent:
                        stack.Push(asmCommand.Parameter1);
                        break;

                    //case StiAsmCommandType.CopyToVariable:
                    //    //report.Dictionary.Variables[(string)asmCommand.Parameter1].ValueObject = stack.Peek();
                        //report[(string)asmCommand.Parameter1] = stack.Peek();
                        //break;

                    case StiAsmCommandType.PushFunction:
                        #region Push function value
                        argsList = new ArrayList();
                        for (int index = 0; index < (int)asmCommand.Parameter2; index++)
                        {
                            argsList.Add(stack.Pop());
                        }
                        argsList.Reverse();
                        //result = null;
                        stack.Push(call_func(asmCommand.Parameter1, argsList));
                        #endregion
                        break;

                    case StiAsmCommandType.PushMethod:
                        #region Push method value
                        argsList = new ArrayList();
                        for (int index = 0; index < (int)asmCommand.Parameter2; index++)
                        {
                            argsList.Add(stack.Pop());
                        }
                        argsList.Reverse();
                        stack.Push(call_method(asmCommand.Parameter1, argsList));
                        #endregion
                        break;

                    case StiAsmCommandType.PushProperty:
                        argsList = new ArrayList {stack.Pop()};
                        stack.Push(call_property(asmCommand.Parameter1, argsList));
                        break;

                   case StiAsmCommandType.ProryvSpreadsheetProperty:
                        var info = asmCommand.Parameter1 as PropertyInfo;
                        if (info != null && _proryvSpreadsheetObject != null)
                        {
                            stack.Push(info.GetValue(_proryvSpreadsheetObject, null));
                        }
                        else stack.Push(null);
                        break;

                   case StiAsmCommandType.ProryvFreeHierarchyBalanceSignature:
                        info = asmCommand.Parameter1 as PropertyInfo;
                        if (info != null)
                        {
                            var s = stack.Peek();
                            if (s != null) stack.Push(info.GetValue(s, null));
                            else stack.Push(null);
                        }
                        else stack.Push(null);
                        break;
    
                    case StiAsmCommandType.PushArrayElement:
                        #region Push array value
                        argsList = new ArrayList();
                        for (int index = 0; index < (int)asmCommand.Parameter1; index++)
                        {
                            argsList.Add(stack.Pop());
                        }
                        argsList.Reverse();
                        stack.Push(call_arrayElement(argsList));
                        #endregion
                        break;

                    case StiAsmCommandType.Add:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_Add(par1, par2));
                        break;
                    case StiAsmCommandType.Sub:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_Sub(par1, par2));
                        break;

                    case StiAsmCommandType.Mult:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_Mult(par1, par2));
                        break;
                    case StiAsmCommandType.Div:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_Div(par1, par2));
                        break;
                    case StiAsmCommandType.Mod:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_Mod(par1, par2));
                        break;

                    case StiAsmCommandType.Power:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_Pow(par1, par2));
                        break;

                    case StiAsmCommandType.Neg:
                        par1 = stack.Pop();
                        stack.Push(op_Neg(par1));
                        break;

                    case StiAsmCommandType.Cast:
                        par1 = stack.Pop();
                        par2 = asmCommand.Parameter1;
                        stack.Push(op_Cast(par1, par2));
                        break;

                    case StiAsmCommandType.Not:
                        par1 = stack.Pop();
                        stack.Push(op_Not(par1));
                        break;

                    case StiAsmCommandType.CompareLeft:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_CompareLeft(par1, par2));
                        break;
                    case StiAsmCommandType.CompareLeftEqual:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_CompareLeftEqual(par1, par2));
                        break;
                    case StiAsmCommandType.CompareRight:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_CompareRight(par1, par2));
                        break;
                    case StiAsmCommandType.CompareRightEqual:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_CompareRightEqual(par1, par2));
                        break;

                    case StiAsmCommandType.CompareEqual:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_CompareEqual(par1, par2));
                        break;
                    case StiAsmCommandType.CompareNotEqual:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_CompareNotEqual(par1, par2));
                        break;

                    case StiAsmCommandType.Shl:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_Shl(par1, par2));
                        break;
                    case StiAsmCommandType.Shr:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_Shr(par1, par2));
                        break;

                    case StiAsmCommandType.And:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_And(par1, par2));
                        break;
                    case StiAsmCommandType.Or:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_Or(par1, par2));
                        break;
                    case StiAsmCommandType.Xor:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_Xor(par1, par2));
                        break;

                    case StiAsmCommandType.And2:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_And2(par1, par2));
                        break;
                    case StiAsmCommandType.Or2:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        stack.Push(op_Or2(par1, par2));
                        break;

                }
            }
            return stack.Pop();
        }

        #endregion

        #region Calls

        private int get_category(object par)
        {
            if (par == null) return -1;
            Type type = par.GetType();
            int category = 0;
            if (type == typeof(string) || type == typeof(char)) category = 1;
            else if (type == typeof(decimal)) category = 2;
            else if (type == typeof(double) || type == typeof(float)) category = 3;
            else if (type == typeof(ulong)) category = 4;
            else if (type == typeof(long)) category = 5;
            else if (type == typeof(uint) || type == typeof(ushort) || type == typeof(byte)) category = 6;
            else if (type == typeof(int) || type == typeof(short) || type == typeof(sbyte)) category = 7;
            else if (type == typeof(DateTime)) category = 8;
            else if (type == typeof(bool)) category = 9;
            return category;
        }

        //private StiParserDataType get_category2(object par)
        //{
        //    //if (par == null) return StiParserDataType.None;
        //    if (par == null) return StiParserDataType.Object;
        //    Type type = par.GetType();
        //    if (type == typeof(double)) return StiParserDataType.zDouble;
        //    if (type == typeof(decimal)) return StiParserDataType.zDecimal;
        //    if (type == typeof(Int32)) return StiParserDataType.Int32;
        //    if (type == typeof(string)) return StiParserDataType.String;
        //    if (type == typeof(bool)) return StiParserDataType.Bool;
        //    if (type == typeof(float)) return StiParserDataType.zFloat;
        //    if (type == typeof(UInt32)) return StiParserDataType.UInt32;
        //    if (type == typeof(byte)) return StiParserDataType.Byte;
        //    if (type == typeof(sbyte)) return StiParserDataType.SByte;
        //    if (type == typeof(Int16)) return StiParserDataType.Int16;
        //    if (type == typeof(UInt16)) return StiParserDataType.UInt16;
        //    if (type == typeof(Int64)) return StiParserDataType.Int64;
        //    if (type == typeof(UInt64)) return StiParserDataType.UInt64;
        //    if (type == typeof(char)) return StiParserDataType.Char;
        //    if (type == typeof(DateTime)) return StiParserDataType.DateTime;
        //    if (type == typeof(TimeSpan)) return StiParserDataType.TimeSpan;
        //    if (type == typeof(System.Drawing.Image)) return StiParserDataType.Image;
        //    if (type == typeof(object)) return StiParserDataType.Object;
        //    return StiParserDataType.None;
        //}

        #region CheckParserMethodInfo
        private int CheckParserMethodInfo(ProryvFunctionType type, ArrayList args)
        {
            int count = args.Count;
            Type[] types = new Type[count];
            for (int index = 0; index < count; index++)
            {
                if (args[index] == null)
                {
                    types[index] = typeof(object);
                }
                else
                {
                    types[index] = args[index].GetType();
                }
            }

            StiParserMethodInfo methodInfo = GetParserMethodInfo(type, types);

            if (methodInfo != null) return methodInfo.Number;

            return 0;
        }

        public StiParserMethodInfo GetParserMethodInfo(ProryvFunctionType type, Type[] args)
        {
            object obj = MethodsHash[type];
            if (obj == null) return null;
            int count = args.Length;

            List<StiParserMethodInfo> methods = (List<StiParserMethodInfo>)obj;
            bool flag1 = false;
            foreach (StiParserMethodInfo methodInfo in methods)
            {
                if (methodInfo.Arguments.Length != count) continue;
                flag1 = true;
                bool flag2 = true;
                for (int index = 0; index < count; index++)
                {
                    if (IsImplicitlyCastableTo(args[index], methodInfo.Arguments[index])) continue;
                    flag2 = false;
                    break;
                }
                if (flag2) return methodInfo;
            }

            if (!flag1)
            {
                ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, Enum.GetName(typeof(ProryvFunctionType), type), count.ToString());
            }

            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < count; index++)
            {
                sb.Append(args[index].Namespace == "System" ? args[index].Name : args[index].ToString());
                if (index < count - 1) sb.Append(",");
            }

            ThrowError(ParserErrorCode.NoMatchingOverloadedMethod, Enum.GetName(typeof(ProryvFunctionType), type), sb.ToString());
            return null;
        }
        #endregion
 
        //----------------------------------------
        // Получение элемента массива
        //----------------------------------------
        private object call_arrayElement(ArrayList argsList)
        {
            object baseValue = argsList[0];

            if (argsList.Count < 2) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "get_ArrayElement", (argsList.Count - 1).ToString());

            if (baseValue is String)
            {
                if (argsList.Count != 2) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "string.get_Item", (argsList.Count - 1).ToString());
                int index = Convert.ToInt32(argsList[1]);
                return (baseValue as string)[index];
            }

            if (baseValue == null) return null;

            var pi = baseValue.GetType().GetProperty("Item");
            if (pi != null)
            {
                if (pi.GetGetMethod().GetParameters().Length > 0)
                {
                    if (argsList.Count < 2) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "object.get_Item", (argsList.Count - 1).ToString());
                    object[] args = new object[argsList.Count - 1];
                    for (int index = 0; index < argsList.Count - 1; index++)
                    {
                        args[index] = argsList[index + 1];
                    }
                    return pi.GetValue(baseValue, args);
                }
            }
            else if (baseValue is Array)
            {
                int[] args = new int[argsList.Count - 1];
                for (int index = 0; index < argsList.Count - 1; index++)
                {
                    args[index] = Convert.ToInt32(argsList[index + 1]);
                }
                return (baseValue as Array).GetValue(args);
            }
            else if (baseValue is IList)
            {
                return (baseValue as IList)[Convert.ToInt32(argsList[1])];     //only firts dimension, temporarily
            }
            return null;
        }

        //----------------------------------------
        // Получение системной переменной
        //----------------------------------------
        private object get_systemVariable(object name)
        {
            switch ((ProryvSystemVariableType)name)
            {
                case ProryvSystemVariableType.Sender: return sender;

                case ProryvSystemVariableType.DateTimeNow: return DateTime.Now;
                case ProryvSystemVariableType.DateTimeToday: return DateTime.Today;
            }
            return null;
        }

        #endregion

        #region ParseTextValue
        public List<StiAsmCommand> ParseToAsm(string inputExpression)
        {
            this.inputExpression = inputExpression;
            MakeTokensList();
            asmList = new List<StiAsmCommand>();
            eval_exp();
            return asmList;
        }

        private static bool CheckForStoreToPrint(object objAsmList)
        {
            bool result = false;
            List<StiAsmCommand> asmList = objAsmList as List<StiAsmCommand>;
            if (asmList != null)
            {
                foreach (StiAsmCommand command in asmList)
                {
                    if (command.Type == StiAsmCommandType.PushSystemVariable)
                    {
                        ProryvSystemVariableType type = (ProryvSystemVariableType)command.Parameter1;
                        if (type == ProryvSystemVariableType.PageNumber ||
                            type == ProryvSystemVariableType.PageNumberThrough ||
                            type == ProryvSystemVariableType.TotalPageCount ||
                            type == ProryvSystemVariableType.TotalPageCountThrough ||
                            type == ProryvSystemVariableType.PageNofM ||
                            type == ProryvSystemVariableType.PageNofMThrough ||
                            type == ProryvSystemVariableType.IsFirstPage ||
                            type == ProryvSystemVariableType.IsFirstPageThrough ||
                            type == ProryvSystemVariableType.IsLastPage ||
                            type == ProryvSystemVariableType.IsLastPageThrough)
                        {
                            result = true;
                            break;
                        }
                    }
                    if (command.Type == StiAsmCommandType.PushFunction)
                    {
                        ProryvFunctionType type = (ProryvFunctionType)command.Parameter1;
                        if (type >= ProryvFunctionType.pCount && type <= ProryvFunctionType.pLast ||
                            type >= ProryvFunctionType.prCount && type <= ProryvFunctionType.prLast ||
                            type >= ProryvFunctionType.piCount && type <= ProryvFunctionType.piLast ||
                            type >= ProryvFunctionType.priCount && type <= ProryvFunctionType.priLast ||
                            type == ProryvFunctionType.GetAnchorPageNumber || type == ProryvFunctionType.GetAnchorPageNumberThrough)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }
        #endregion

    }
}
