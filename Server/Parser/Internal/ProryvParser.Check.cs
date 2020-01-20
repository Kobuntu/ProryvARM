using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace Proryv.Servers.Calculation.Parser.Internal
{
    public partial class ProryvParser
    {
        #region Exception provider

        private static string[] errorsList = {
            "Синтаксическая ошибка",        //0
            "Интегральная константа слишком велика",    //1        значение целочисленной константы слишком велико
            "Выражение пусто",             //2
            "Деление на ноль",              //3
            "Неожиданный конец выражения",  //4
            "Имя '{0}' не существует в текущем контексте",   //5
            "Ошибка синтаксиса - остались необработанные лексемы",  //6
            "( пропущено",         //7
            ") пропущено",         //8
            "Поле, метод или свойство не найдено: '{0}'", //9
            "Оператор '{0}' не может быть применен к операндам типа '{1}' и тип '{2}'",    //10
            "Функция не найдена: '{0}'",                     //11
            "Нет перегрузки метода '{0}' не принимает '{1}' аргументов",   //12
            "Функция '{0}' имеет недопустимый аргумент '{1}': не возможно конвертировать из '{2}' в '{3}'",   //13
            "Функция '{0}' пока не реализована",            //14
            "Метод '{0}' имеет недопустимый аргумент '{1}': не возможно конвертировать из '{2}' в '{3}'",  //15
            "'{0}' не содержит определения для '{1}'",   //16
            "There is no matching overloaded method for '{0}({1})'"};   //17

        private enum ParserErrorCode
        {
            SyntaxError = 0,
            IntegralConstantIsTooLarge = 1,
            ExpressionIsEmpty = 2,
            DivisionByZero = 3,
            UnexpectedEndOfExpression = 4,
            NameDoesNotExistInCurrentContext = 5,
            UnprocessedLexemesRemain = 6,
            LeftParenthesisExpected = 7,
            RightParenthesisExpected = 8,
            FieldMethodOrPropertyNotFound = 9,
            OperatorCannotBeAppliedToOperands = 10,
            FunctionNotFound = 11,
            NoOverloadForMethodTakesNArguments = 12,
            FunctionHasInvalidArgument = 13,
            FunctionNotYetImplemented = 14,
            MethodHasInvalidArgument = 15,
            ItemDoesNotContainDefinition = 16,
            NoMatchingOverloadedMethod = 17
        }


        // Отображение сообщения о синтаксической ошибке
        private void ThrowError(ParserErrorCode code)
        {
            ThrowError(code, null, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        private void ThrowError(ParserErrorCode code, string message1)
        {
            ThrowError(code, null, message1, string.Empty, string.Empty, string.Empty);
        }

        private void ThrowError(ParserErrorCode code, string message1, string message2)
        {
            ThrowError(code, null, message1, message2, string.Empty, string.Empty);
        }

        private void ThrowError(ParserErrorCode code, string message1, string message2, string message3)
        {
            ThrowError(code, null, message1, message2, message3, string.Empty);
        }

        private void ThrowError(ParserErrorCode code, string message1, string message2, string message3, string message4)
        {
            ThrowError(code, null, message1, message2, message3, message4);
        }


        private void ThrowError(ParserErrorCode code, StiToken token)
        {
            ThrowError(code, token, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        private void ThrowError(ParserErrorCode code, StiToken token, string message1)
        {
            ThrowError(code, token, message1, string.Empty, string.Empty, string.Empty);
        }

        private void ThrowError(ParserErrorCode code, StiToken token, string message1, string message2)
        {
            ThrowError(code, token, message1, message2, string.Empty, string.Empty);
        }

        private void ThrowError(ParserErrorCode code, StiToken token, string message1, string message2, string message3)
        {
            ThrowError(code, token, message1, message2, message3, string.Empty);
        }

        private void ThrowError(ParserErrorCode code, StiToken token, string message1, string message2, string message3, string message4)
        {
            string errorMessage = "Неизвестная ошибка";
            int errorCode = (int)code;
            if (errorCode < errorsList.Length)
            {
                errorMessage = string.Format(errorsList[errorCode], message1, message2, message3, message4);
            }
            var fullMessage = "Ошибка парсера: " + errorMessage;
            var ex = new StiParserException(fullMessage) {BaseMessage = errorMessage};
            if (token == null) throw ex;
            ex.Position = expressionPosition + token.Position;
            ex.Length = token.Length;
            throw ex;
        }

        public class StiParserException : Exception
        {
            public string BaseMessage = null;
            public int Position = -1;
            public int Length = -1;

            public StiParserException(string message)
                : base(message)
            {
            }

            public StiParserException()
                : base()
            {
            }
        }

        #endregion

        #region CheckTypes
        private void CheckTypes(List<StiAsmCommand> asmList)
        {
            if (asmList == null || asmList.Count == 0) return;
            Stack<StiCheckTypeData> stack = new Stack<StiCheckTypeData>();
            List<StiCheckTypeData> argsList = null;
            Type[] types = null;
            StiCheckTypeData par1;
            StiCheckTypeData par2;

            foreach (StiAsmCommand asmCommand in asmList)
            {
                Type type = typeof(object);

                switch (asmCommand.Type)
                {
                    case StiAsmCommandType.PushValue:
                        stack.Push(new StiCheckTypeData(asmCommand.Parameter1.GetType(), asmCommand.Position, asmCommand.Length));
                        break;

                    //case StiAsmCommandType.PushVariable:
                    //    #region push variable type
                    //    string varName = (string)asmCommand.Parameter1;
                    //    StiVariable var = report.Dictionary.Variables[varName];
                    //    if (var != null)
                    //    {
                    //        type = var.Type;
                    //    }
                    //    else
                    //    {
                    //        if (report.Variables != null && report.Variables.ContainsKey(varName))
                    //        {
                    //            object varValue = report.Variables[varName];
                    //            if (varValue != null) type = varValue.GetType();
                    //        }
                    //    }
                    //    stack.Push(new StiCheckTypeData(type, asmCommand.Position, asmCommand.Length));
                    //    #endregion
                    //    break;

                    //case StiAsmCommandType.PushSystemVariable:
                    //    object systemVariableValue = get_systemVariable(asmCommand.Parameter1);
                    //    if (systemVariableValue != null) type = systemVariableValue.GetType();
                    //    stack.Push(new StiCheckTypeData(type, asmCommand.Position, asmCommand.Length));
                    //    break;

                    case StiAsmCommandType.PushComponent:
                        stack.Push(new StiCheckTypeData((asmCommand.Parameter1 == null ? typeof(object) : asmCommand.Parameter1.GetType()), asmCommand.Position, asmCommand.Length));
                        break;

                    case StiAsmCommandType.CopyToVariable:
                        stack.Peek();
                        break;

                    case StiAsmCommandType.PushFunction:
                        #region Push function value
                        argsList = new List<StiCheckTypeData>();
                        for (int index = 0; index < (int)asmCommand.Parameter2; index++)
                        {
                            argsList.Add(stack.Pop());
                        }
                        argsList.Reverse();
                        types = new Type[argsList.Count];
                        for (int index = 0; index < argsList.Count; index++)
                        {
                            types[index] = argsList[index].TypeCode;
                        }
                        StiParserMethodInfo methodInfo = GetParserMethodInfo((ProryvFunctionType)asmCommand.Parameter1, types);
                        type = methodInfo != null ? methodInfo.ReturnType : typeof(object);
                        stack.Push(new StiCheckTypeData(type, asmCommand.Position, asmCommand.Length));
                        #endregion
                        break;

                    case StiAsmCommandType.PushMethod:
                        #region Push method value
                        argsList = new List<StiCheckTypeData>();
                        for (int index = 0; index < (int)asmCommand.Parameter2; index++)
                        {
                            argsList.Add(stack.Pop() as StiCheckTypeData);
                        }
                        argsList.Reverse();
                        types = new Type[argsList.Count];
                        for (int index = 0; index < argsList.Count; index++)
                        {
                            types[index] = argsList[index].TypeCode;
                        }
                        type = GetMethodResultType((StiMethodType)asmCommand.Parameter1, types);
                        stack.Push(new StiCheckTypeData(type, asmCommand.Position, asmCommand.Length));
                        #endregion
                        break;

                    case StiAsmCommandType.PushProperty:
                        type = GetPropertyType((StiPropertyType)asmCommand.Parameter1, stack.Pop().TypeCode);
                        stack.Push(new StiCheckTypeData(type, asmCommand.Position, asmCommand.Length));
                        break;

                    case StiAsmCommandType.PushArrayElement:
                        #region Push array value
                        argsList = new List<StiCheckTypeData>();
                        for (int index = 0; index < (int)asmCommand.Parameter1; index++)
                        {
                            argsList.Add(stack.Pop());
                        }
                        argsList.Reverse();
                        types = new Type[argsList.Count];
                        for (int index = 0; index < argsList.Count; index++)
                        {
                            types[index] = argsList[index].TypeCode;
                        }
                        type = GetArrayElementType(types);
                        stack.Push(new StiCheckTypeData(type, asmCommand.Position, asmCommand.Length));
                        #endregion
                        break;

                    case StiAsmCommandType.Add:
                    case StiAsmCommandType.Sub:
                    case StiAsmCommandType.Mult:
                    case StiAsmCommandType.Div:
                    case StiAsmCommandType.Mod:
                    case StiAsmCommandType.Shl:
                    case StiAsmCommandType.Shr:
                    case StiAsmCommandType.And:
                    case StiAsmCommandType.Or:
                    case StiAsmCommandType.Xor:
                    case StiAsmCommandType.And2:
                    case StiAsmCommandType.Or2:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        types = new Type[2] { par1.TypeCode, par2.TypeCode };
                        StiParserMethodInfo methodInfo2 = GetParserMethodInfo((ProryvFunctionType)asmCommand.Type, types);
                        type = methodInfo2 != null ? methodInfo2.ReturnType : typeof(object);
                        stack.Push(new StiCheckTypeData(type, asmCommand.Position, asmCommand.Length));
                        break;

                    case StiAsmCommandType.Neg:
                    case StiAsmCommandType.Not: 
                        par1 = stack.Pop();
                        types = new Type[1] { par1.TypeCode };
                        StiParserMethodInfo methodInfo3 = GetParserMethodInfo((ProryvFunctionType)asmCommand.Type, types);
                        type = methodInfo3 != null ? methodInfo3.ReturnType : typeof(object);
                        stack.Push(new StiCheckTypeData(type, asmCommand.Position, asmCommand.Length));
                        break;

                    //case StiAsmCommandType.Power:

                    case StiAsmCommandType.CompareLeft:
                    case StiAsmCommandType.CompareLeftEqual:
                    case StiAsmCommandType.CompareRight:
                    case StiAsmCommandType.CompareRightEqual:
                    case StiAsmCommandType.CompareEqual:
                    case StiAsmCommandType.CompareNotEqual:
                        par2 = stack.Pop();
                        par1 = stack.Pop();
                        //
                        // need to check type equality; todo
                        //
                        type = typeof(bool);
                        stack.Push(new StiCheckTypeData(type, asmCommand.Position, asmCommand.Length));
                        break;

                    case StiAsmCommandType.Cast:
                        par1 = stack.Pop();
                        type = Type.GetType("System." + asmCommand.Parameter1);
                        stack.Push(new StiCheckTypeData(type, asmCommand.Position, asmCommand.Length));
                        break;

                }
            }
        }


        private Type GetMethodResultType(StiMethodType type, Type[] args)
        {
            Type baseType = args[0];
            int count = args.Length - 1;
            string methodName = type.ToString();
            MethodInfo[] methods = baseType.GetMethods();
            bool flag0 = false;
            bool flag1 = false;
            foreach (var mi in methods)
            {
                if (mi.Name != methodName) continue;
                flag0 = true;
                ParameterInfo[] pi = mi.GetParameters();
                if (pi.Length != count) continue;
                flag1 = true;
                bool flag2 = true;
                for (int index = 0; index < count; index++)
                {
                    if (IsImplicitlyCastableTo(args[index + 1], pi[index].ParameterType)) continue;
                    flag2 = false;
                    break;
                }
                if (flag2) return mi.ReturnType;
            }

            if (!flag0)
            {
                ThrowError(ParserErrorCode.FieldMethodOrPropertyNotFound, Enum.GetName(typeof(StiMethodType), type));
            }

            if (!flag1)
            {
                ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, Enum.GetName(typeof(StiMethodType), type), count.ToString());
            }

            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < count; index++)
            {
                sb.Append(args[index + 1].Namespace == "System" ? args[index + 1].Name : args[index + 1].ToString());
                if (index < count - 1) sb.Append(",");
            }

            ThrowError(ParserErrorCode.NoMatchingOverloadedMethod, Enum.GetName(typeof(StiMethodType), type), sb.ToString());

            return typeof(object);
        }

        private Type GetPropertyType(StiPropertyType type, Type baseType)
        {
            string propertyName = type.ToString();
            PropertyInfo[] properties = baseType.GetProperties();
            foreach (var pi in properties)
            {
                if (pi.Name == propertyName) return pi.PropertyType;
            }

            ThrowError(ParserErrorCode.FieldMethodOrPropertyNotFound, Enum.GetName(typeof(StiPropertyType), type));

            return typeof(object);
        }

        private Type GetArrayElementType(Type[] argsList)
        {
            Type baseType = argsList[0];

            if (argsList.Length < 2) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "get_ArrayElement", (argsList.Length - 1).ToString());

            if (baseType == typeof(string))
            {
                if (argsList.Length != 2) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "string.get_Item", (argsList.Length - 1).ToString());
                if (!IsImplicitlyCastableTo(argsList[1].GetType(), typeof(int))) ThrowError(ParserErrorCode.NoMatchingOverloadedMethod, "string.get_Item", argsList[1].GetType().ToString()); ;
                return typeof(char);
            }

            PropertyInfo pi = baseType.GetProperty("Item");
            if (pi != null)
            {
                ParameterInfo[] pis = pi.GetGetMethod().GetParameters();
                if (pis.Length > 0)
                {
                    if (argsList.Length < 2) ThrowError(ParserErrorCode.NoOverloadForMethodTakesNArguments, "object.get_Item", (argsList.Length - 1).ToString());
                    // !!! need check parameters types
                    return pi.PropertyType;
                }
            }
            else if (baseType.IsAssignableFrom(typeof(Array)))
            {
                // !!! need test
                // !!! need check parameters types
                return baseType.UnderlyingSystemType;
            }
            else if (baseType.IsAssignableFrom(typeof(IList)))
            {
                // !!! need test
                // !!! need check parameters types
                return baseType.UnderlyingSystemType;
            }

            return typeof(object);
        }



        private class StiCheckTypeData
        {
            public Type TypeCode;
            public int Position = -1;
            public int Length = -1;

            public StiCheckTypeData(Type typeCode, int position, int length)
            {
                this.TypeCode = typeCode;
                this.Position = position;
                this.Length = length;
            }

            public override string ToString()
            {
                return string.Format("{0}", TypeCode);
            }
        }
         

        #endregion


        #region IsImplicitlyCastableTo
        private static readonly Dictionary<KeyValuePair<Type, Type>, bool> ImplicitCastCache = new Dictionary<KeyValuePair<Type, Type>, bool>();

        public static bool IsImplicitlyCastableTo(Type from, Type to)
        {
            if (from == null || to == null) return false;

            // not strictly necessary, but speeds things up and avoids polluting the cache
            if (to.IsAssignableFrom(from))
            {
                return true;
            }

            var key = new KeyValuePair<Type, Type>(from, to);
            bool cachedValue;
            //if (ImplicitCastCache.TryGetCachedValue(key, out cachedValue))
            if (ImplicitCastCache.TryGetValue(key, out cachedValue))
            {
                return cachedValue;
            }

            bool result = false;
            try
            {
                GetMethod(() => AttemptImplicitCast<object, object>())
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(from, to)
                    .Invoke(null, new object[0]);
                result = true;
            }
            //catch (TargetInvocationException ex)
            //{
            //    result = !((ex.InnerException is RuntimeBinderException) && ex.InnerException.Message.Contains("System.Collections.Generic.List<"));
            //}
            catch
            {
                //for our purposes it is sufficient simply to catch any error
            }

            ImplicitCastCache[key] = result;
            return result;
        }

        private static void AttemptImplicitCast<TFrom, TTo>()
        {
            // based on the IL produced by:
            // dynamic list = new List<TTo>();
            // list.Add(default(TFrom));
            // We can't use the above code because it will mimic a cast in a generic method
            // which doesn't have the same semantics as a cast in a non-generic method

            var list = new List<TTo>(capacity: 1);
            var binder = Microsoft.CSharp.RuntimeBinder.Binder.InvokeMember(
                flags: CSharpBinderFlags.ResultDiscarded,
                name: "Add",
                typeArguments: null,
                context: typeof(ProryvParser),
                argumentInfo: new[] 
                { 
                    CSharpArgumentInfo.Create(flags: CSharpArgumentInfoFlags.None, name: null), 
                    CSharpArgumentInfo.Create(
                        flags: CSharpArgumentInfoFlags.UseCompileTimeType, 
                        name: null
                    ),
                }
            );
            var callSite = CallSite<Action<CallSite, object, TFrom>>.Create(binder);
            callSite.Target.Invoke(callSite, list, default(TFrom));
        }

        /// <summary>
        /// Gets a method info of a void method.
        /// Example: GetMethod(() => Console.WriteLine("")); will return the
        /// MethodInfo of WriteLine that receives a single argument.
        /// </summary>
        private static MethodInfo GetMethod(Expression<Action> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            var body = expression.Body;
            if (body.NodeType != ExpressionType.Call)
                throw new ArgumentException("expression.Body must be a Call expression.", "expression");

            MethodCallExpression callExpression = (MethodCallExpression)body;
            return callExpression.Method;
        }
        #endregion


        #region GetTypeName
        private string GetTypeName(object value)
        {
            if (value == null)
            {
                return "null";
            }
            else
            {
                return value.GetType().ToString();
            }
        }
        #endregion

    }
}
