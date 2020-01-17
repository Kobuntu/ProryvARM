using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;
using Proryv.AskueARM2.Server.DBAccess.Internal.Formulas.Variables;
using Proryv.Servers.Calculation.DBAccess.Common.Ext;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.FormulaInterpreter;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;
using Proryv.Servers.Calculation.FormulaInterpreter.Formulas;
using Proryv.AskueARM2.Server.DBAccess.Internal.Enums;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    public class Polskaya : IGetAchives, IFormulaParser
    {
        public object UserTag;
        public readonly StringBuilder Expression;

        private const int MemoryAllockStep_bytes = 1000;
        private const int InitVariablesCount = 100;

        private const char FunctionArgsDeviderChar = ';'; //Разделитель аргументов

        //private const char FunctionArgsDeviderCharAlternative = ','; // Второй разделитель аргументов
        private const char FunctionArgString = (char)39; //Разделитель текстового аргумента внутри функции func('a');

        private const char FunctionArgStringAlternative = '"'
            ; //Второй разделитель текстового аргумента внутри функции func("b");

        internal enum Operations : byte
        {
            unknown = 0,
            adding = 1,
            subtraction = 2,
            multiplication = 3,
            division = 4,
            openbracket = 5,
            closebracket = 6,
            func_devider = 7,
            func_precalc = 8
        };

        // приоритеты операций. !!!Порядок соответствует перечислению Operations!!!
        private readonly byte[] OperationsPriority = { 0, 2, 2, 3, 3, 1, 3, 1, 1 };

        private readonly char[] OperationsSymbols = { '+', '-', '*', '/', '(', ')', FunctionArgsDeviderChar }
            ; //,//FunctionArgString,FunctionArgStringAlternative};

        private readonly List<PolskayaVariable> _operationStack;
        private int OperationStackCount = 0;

        private readonly List<PolskayaVariable> OutString
            ; // InitVariablesCount переменных + InitVariablesCount операций

        private readonly OutEvalStringValue[] OutEvalString = new OutEvalStringValue[2 * InitVariablesCount];
        private FunctionArgumentList FunctionArgs = new FunctionArgumentList();

        private Operations CurrentOperation = Operations.unknown;
        private PolskayaVariable CurrentOperationOutString;

        private readonly Dictionary<string, Variable> _variablesDict;

        //private readonly List<Variable> _precalculatedVariables = new List<Variable>();
        private readonly SortedList<string, Function> FunctionList = new SortedList<string, Function>();

        /// <summary>
        /// Словарь для предрасчетных ф-ий
        /// </summary>
        private readonly Dictionary<string, PrecalculatedFunctionVariable> _precalculatedFunctionDict;

        /// <summary>
        /// Словарь для предрасчетных логических ф-ий 
        /// </summary>
        private readonly Dictionary<string, Function> _precalculatedFunctionBooleanDict;

        private int OperationOutStackIndex;

        private bool Compiled = false;

        private readonly IDBInterfaceAdapter NameInterface;
        private readonly System.Globalization.CultureInfo _provider = new System.Globalization.CultureInfo("en-US");
        private const NumberStyles _style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;

        private readonly PolskaParams _polskaParams;

        public Polskaya(PolskaParams polskaParams,
            IDBInterfaceAdapter nameInterface, 
            IFormulaArchivesPrecalculator formulaArchivesPrecalculator,
            Dictionary<string, Variable> variablesDict)
        {
            _polskaParams = polskaParams;

            NameInterface = nameInterface;
            
            _operationStack = new List<PolskayaVariable>(MemoryAllockStep_bytes);
            OutString = new List<PolskayaVariable>();
            Expression = new StringBuilder();
            _variablesDict = variablesDict;

            #region Ф-ии

            CreateFunction(new MinFunction());
            CreateFunction(new MaxFunction());
            CreateFunction(new SinFunction());
            CreateFunction(new CosFunction());
            CreateFunction(new PowFunction());
            CreateFunction(new ExpFunction());
            CreateFunction(new LogFunction());
            CreateFunction(new IfFunction());
            CreateFunction(new MacroIfFunction());
            CreateFunction(new RoundFunction());
            CreateFunction(new ValueStatusCodeContains());
            CreateFunction(new ValueStatusCodeContainsAllAlarmStatuses());

            #endregion

            // Добавляем возможность использовать пользовательский функции
            CreateFunction(new GetHalfHourIndex());
            CreateFunction(new UserDefinedSQLFunction(nameInterface));
            CreateFunction(new UserDefinedCSharpFunction());
            CreateFunction(new GetHalfHourDateTime());

            CreateFunction(new AbsFunction());

            #region Добавление ф-ий которые требуют предварительной подгрузки данных

            _precalculatedFunctionDict = new Dictionary<string, PrecalculatedFunctionVariable>();
            foreach (var precalculatedFormulasFunction in Enum.GetNames(typeof(EnumFormulasFunction))
                .Select(s => (EnumFormulasFunction)Enum.Parse(typeof(EnumFormulasFunction), s)).Select(
                    formulasFunction =>
                        formulasFunction.CreatePrecalculatedFormulasFunctionDescription(formulaArchivesPrecalculator)))
            {
                _precalculatedFunctionDict[precalculatedFormulasFunction.Id] = precalculatedFormulasFunction;
            }

            _precalculatedFunctionBooleanDict = new Dictionary<string, Function>();
            var f = new IfEnabledFunction();
            _precalculatedFunctionBooleanDict[f.id] = f;



            #endregion
        }

        public bool ContainsVariable(string var_name)
        {
            return _variablesDict.ContainsKey(var_name);
        }

        public void CreateVariable(Variable var)
        {
            if (!_variablesDict.ContainsKey(var.Id))
            {
                _variablesDict.Add(var.Id, var);
            }
        }

        public bool ContainsFunction(string func_name)
        {
            return FunctionList.ContainsKey(func_name.ToLower());
        }

        public void CreateFunction(Function func)
        {
            if (ContainsFunction(func.id) == false)
                FunctionList.Add(func.id, func);
        }

        public void Compile()
        {
            if (Expression.Length == 0)
                throw new Exception("Формула пуста!");
            int oper_index = 0;
            int last_oper_index = 0;

            OperationStackCount = 0;
            string v_id;


            // Ломаются строки 
            //Expression.Replace(" ", "");

            RemoveWhiteSpacesOuterQuotation(Expression);


            // обработка логических функций if

            CompileMacrosFunctions();


            bool terminate = true;
            do
            {
                terminate = true;

                while (Extention.IndexOf(Expression, "++") > 0)
                {
                    Expression.Replace("++", "+");
                    terminate = false;
                }

                while (Extention.IndexOf(Expression, "+-") > 0)
                {
                    Expression.Replace("+-", "-");
                    terminate = false;
                }

                while (Extention.IndexOf(Expression, "-+") > 0)
                {
                    Expression.Replace("-+", "-");
                    terminate = false;
                }

                while (Extention.IndexOf(Expression, "--") > 0)
                {
                    Expression.Replace("--", "+");
                    terminate = false;
                }
            } while (terminate == false);

            Expression.Replace("(-", "(0-");
            Expression.Replace("(+", "(0+");

            Expression.Replace(FunctionArgsDeviderChar + "+", FunctionArgsDeviderChar + "0+");
            Expression.Replace(FunctionArgsDeviderChar + "-", FunctionArgsDeviderChar + "0-");

            if (Expression[0] == '-' ||
                Expression[0] == '+')
                Expression.Insert(0, "0");

            while ((oper_index = Extention.IndexOfAny(Expression, OperationsSymbols, oper_index)) >= 0)
            {
                // обрабатываем переменную
                v_id = Expression.ToString(last_oper_index, (oper_index - last_oper_index));
                string stringVal = v_id;

                if (!string.IsNullOrEmpty(v_id))
                {

                    //Если началась строка то не учитываем внутри нее значения
                    if ((v_id[0] == FunctionArgString || v_id[0] == FunctionArgStringAlternative) &&
                        v_id[v_id.Length - 1] != FunctionArgString &&
                        v_id[v_id.Length - 1] != FunctionArgStringAlternative)
                    {
                        findEndStringIndex:
                        oper_index = Extention.IndexOfAny(Expression, new char[] { FunctionArgString, FunctionArgStringAlternative },
                                         oper_index) + 1;
                        //Проверяем что это не /" /'
                        if (Expression[oper_index] == '/')
                            goto findEndStringIndex;
                        v_id = Expression.ToString(last_oper_index, (oper_index - last_oper_index));
                        stringVal = v_id;
                        if (string.IsNullOrEmpty(v_id))
                        {

                            SintaxAnalizer(Expression[oper_index]);

                            InsertToStack(Expression[oper_index++], EnumPolskayaType.Operation, oper_index);
                            last_oper_index = oper_index;
                            continue;
                        }
                    }


                    Variable v;
                    if (_variablesDict.TryGetValue(v_id, out v))
                    {
                        OutString.Add(new PolskayaVariable(EnumPolskayaType.Variable, v));
                    }
                    else
                    {
                        v_id = v_id.ToLower();
                        // возможно, это функция
                        if (FunctionList.ContainsKey(v_id))
                        {
                            // проверим, если открывающаяся скобка после функции
                            if (Expression[oper_index] != '(')
                                throw new Exception("За функцией " + v_id + " не следуют открывающиеся скобки!");

                            Function f;
                            FunctionList.TryGetValue(v_id, out f);
                            InsertToStack(f, EnumPolskayaType.Function); // номер функции в коллекции
                            //InsertToStack(FunctionList.IndexOfKey(v_id) + FuncNumDelta);// номер функции в коллекции
                            CurrentOperation = Operations.func_devider;
                            OutString.Add(new PolskayaVariable(EnumPolskayaType.FunctionDescription,
                                CurrentOperation)); // признак того, что далее следуют аргуметы функции
                        }
                        else if (_precalculatedFunctionDict.ContainsKey(v_id))
                        {
                            // проверим, если открывающаяся скобка после функции
                            if (Expression[oper_index] != '(')
                                throw new Exception("За функцией " + v_id + " не следуют открывающиеся скобки!");

                            PrecalculatedFunctionVariable f;
                            _precalculatedFunctionDict.TryGetValue(v_id, out f);
                            InsertToStack(f, EnumPolskayaType.PrecalcFunctionVariable); // номер функции в коллекции

                            //InsertToStack(FunctionList.IndexOfKey(v_id) + FuncNumDelta);// номер функции в коллекции
                            CurrentOperation = Operations.func_precalc;
                            OutString.Add(new PolskayaVariable(EnumPolskayaType.FunctionDescription,
                                CurrentOperation)); // признак того, что далее следует аргумет функции
                        }
                        else
                        {
                            // возможно, это константа
                            double constant_val = 0;

                            bool isStringVal = false;

                            if (v_id.IndexOf(',') >= 0)
                                _provider.NumberFormat.NumberDecimalSeparator = ",";
                            else if (v_id.IndexOf('.') >= 0)
                                _provider.NumberFormat.NumberDecimalSeparator = ".";


                            //Возможно это строковый параметр аргумента 
                            // Проверяем что в начале и в конце ковычки
                            if ((v_id[0] == FunctionArgString && v_id[v_id.Length - 1] == FunctionArgString) ||
                                 (v_id[0] == FunctionArgStringAlternative && v_id[v_id.Length - 1] == FunctionArgStringAlternative))
                            {

                                isStringVal = !string.IsNullOrEmpty(stringVal);
                                if (isStringVal)
                                {
                                    OutString.Add(new PolskayaVariable(EnumPolskayaType.String, stringVal.Trim(FunctionArgString, FunctionArgStringAlternative)));

                                }
                            }


                            if (!isStringVal)
                            {
                                if (!double.TryParse(v_id, _style, _provider, out constant_val))
                                {
                                    string fName = NameInterface.GetFormulaName(_polskaParams.FormulaUn, _polskaParams.FormulasTable);
                                    string name;
                                    if (!variableToName(v_id, out name))
                                    {
                                        throw new Exception(fName + ": Переменная [" + name +
                                                            "] не создана (или число неверного формата)!");
                                    }
                                    else
                                    {
                                        throw new Exception(fName + ": Возможно после \"" + name +
                                                            "\" пропущен оператор!");
                                    }
                                }

                                OutString.Add(new PolskayaVariable(EnumPolskayaType.Double, constant_val));
                            }
                        }
                    }
                }
                else
                    SintaxAnalizer(Expression[oper_index]);

                InsertToStack(Expression[oper_index++], EnumPolskayaType.Operation, oper_index);
                last_oper_index = oper_index;
            }
            // последней остается переменная
            v_id = Expression.ToString(last_oper_index, Expression.Length - last_oper_index);

            if (v_id != string.Empty)
            {
                Variable v;
                if (_variablesDict.TryGetValue(v_id, out v))
                {
                    OutString.Add(new PolskayaVariable(EnumPolskayaType.Variable, v));
                }
                else
                {
                    // если переменная не найдена, возможно, это константа
                    double constant_val = 0;

                    if (v_id.IndexOf(',') >= 0)
                        _provider.NumberFormat.NumberDecimalSeparator = ",";
                    else if (v_id.IndexOf('.') >= 0)
                        _provider.NumberFormat.NumberDecimalSeparator = ".";

                    if (!double.TryParse(v_id, _style, _provider, out constant_val))
                    {
                        string fName = NameInterface.GetFormulaName(_polskaParams.FormulaUn, _polskaParams.FormulasTable);

                        string name;
                        if (!variableToName(v_id, out name))
                        {
                            throw new Exception(fName + ": Переменная [" + name +
                                                "] не создана (или число неверного формата)!");
                        }
                        else
                        {
                            throw new Exception(fName + ": Возможно после \"" + name + "\" пропущен оператор!");
                        }
                    }

                    OutString.Add(new PolskayaVariable(EnumPolskayaType.Double, constant_val));
                }
            }

            Final();
            Compiled = true;
        }

        /// <summary>
        /// Убираем пробелы за исключением строк '' "" 
        /// </summary>
        /// <param name="expression"></param>
        private void RemoveWhiteSpacesOuterQuotation(StringBuilder expression)
        {

            //            Expression.Replace(" ", "");
            //            return;

            int openQuotation = 0;
            openQuotation = Extention.IndexOfAny(Expression, new char[] { FunctionArgString, FunctionArgStringAlternative }, 0);

            if (openQuotation <= 0)
            {
                //Нет строк внутри
                Expression.Replace(" ", "");
                return;
            }
            Expression.Replace(" ", "", 0, openQuotation);

            var closeQuotation = 0;

            while (openQuotation > 0 && (closeQuotation =
                       Extention.IndexOfAny(Expression, new char[] { FunctionArgString, FunctionArgStringAlternative },
                           openQuotation)) > 0)
            {
                openQuotation = Extention.IndexOfAny(Expression, new char[] { FunctionArgString, FunctionArgStringAlternative },
                    closeQuotation);
                if (openQuotation > 0)
                {
                    Expression.Replace(" ", "", closeQuotation, openQuotation - closeQuotation);
                    openQuotation = Extention.IndexOfAny(Expression, new char[] { FunctionArgString, FunctionArgStringAlternative },
                        closeQuotation + 1);
                }
                else
                {
                    Expression.Replace(" ", "", closeQuotation, Expression.Length - closeQuotation);
                }
            }
        }

        private bool variableToName(string v_id, out string name)
        {
            name = v_id;
            if (string.IsNullOrEmpty(v_id))
            {
                return false;
            }

            int indx1 = v_id.IndexOf("_"), indx2, indx3;
            if (indx1 < 0) return false; //Это оператор
            indx2 = v_id.IndexOf("_", indx1 + 1);
            if (indx2 < 0) return false;
            indx3 = v_id.IndexOf("_", indx2 + 1);
            if (indx3 < 0) return false;

            byte? channel = null;

            byte ch;
            if (byte.TryParse(v_id.Substring(indx1 + 1, indx2 - indx1 - 1), out ch))
            {
                channel = ch;
            }

            int t;

            if (!int.TryParse(v_id.Substring(indx2 + 1, indx3 - indx2 - 1), out t))
            {
                return false;
            }

            F_OPERATOR.F_OPERAND_TYPE type = (F_OPERATOR.F_OPERAND_TYPE)t;

            name = NameInterface.GetObjectName(v_id.Substring(0, indx1 <= 22 ? indx1 : 22), type, channel);
            return true;
        }

        private void CompileMacrosFunctions()
        {
            // идея в том, что 
            // заменяя конструкции if-else нулями,
            // получить выражение не содержащее фигурных скобок,else, знаков логики
            var i = Extention.IndexOf(Expression, "если(", 0, true);
            if (i >= 0)
            {
                //Здесь ничего толком не поверяется т.к. возможны многочисленные вложения, поэтому закоментировано
                // сначала макросы
                //var checkMacroReg =
                //    new Regex(@"если\((?!.*?если).+?(?:>|<|!=|==|>=|<=).+?\)\{.+?\}иначе\{.+?\}");

                //string check_expr = Expression.ToString();
                //while (checkMacroReg.IsMatch(check_expr))
                //{
                //    check_expr = checkMacroReg.Replace(check_expr, "0");
                //}

                //if (check_expr.IndexOf("если") >= 0 ||
                //    check_expr.IndexOf('{') >= 0 ||
                //    check_expr.IndexOf('}') >= 0 ||
                //    check_expr.IndexOf("иначе") >= 0 ||
                //    check_expr.IndexOfAny(MacroLogicSymbols) >= 0)
                //    throw new Exception("Выражение некорректно (содержит недопустимые символы)");


                // формируем расчетную строку
                Expression.Replace(">=", ";3;");
                Expression.Replace("<=", ";4;");
                Expression.Replace("==", ";5;");
                Expression.Replace("!=", ";6;");
                Expression.Replace(">", ";1;");
                Expression.Replace("<", ";2;");

                Expression.Replace("){", ";");
                Expression.Replace("}иначе{", ";");
                Expression.Replace("}{", ";");
                Expression.Replace("}", ")");
            }
        }

        private void SintaxAnalizer(char Operation)
        {
            switch (Operation)
            {
                case '+':
                case '-':
                    if (CurrentOperation != Operations.closebracket)
                    {
                        string fName = NameInterface.GetFormulaName(_polskaParams.FormulaUn, _polskaParams.FormulasTable);
                        throw new Exception(string.Format("{2}: Следование {0} за {1} невозможно!",
                            "'+' или '-'", OperationsToString(CurrentOperation), fName));
                    }
                    break;

                case '/':
                case '*':
                    if (CurrentOperation != Operations.closebracket)
                    {
                        string fName = NameInterface.GetFormulaName(_polskaParams.FormulaUn, _polskaParams.FormulasTable);
                        throw new Exception(string.Format("{2}: Следование {0} за {1} невозможно!",
                            "'*' или '/'", OperationsToString(CurrentOperation), fName));
                    }
                    break;

                case '(':
                    if (CurrentOperation == Operations.closebracket)
                    {
                        string fName = NameInterface.GetFormulaName(_polskaParams.FormulaUn, _polskaParams.FormulasTable);
                        throw new Exception(string.Format("{2}: Следование '{0}' за '{1}' невозможно!",
                            '(', ')', fName));
                    }
                    break;

                case ')':
                    if (CurrentOperation != Operations.closebracket)
                    {
                        string fName = NameInterface.GetFormulaName(_polskaParams.FormulaUn, _polskaParams.FormulasTable);
                        var o = _operationStack.ElementAtOrDefault(OperationStackCount - 2);
                        if (o != null && OperationStackCount >= 2 &&
                            (o.PolskayaType == EnumPolskayaType.Function ||
                             o.PolskayaType == EnumPolskayaType.PrecalcFunctionVariable))
                            break; // это закрываются скобки функции без аргументов

                        throw new Exception(string.Format("{2}: Следование '{0}' за {1} невозможно!",
                            ')', OperationsToString(CurrentOperation), fName));
                    }
                    break;

                case FunctionArgsDeviderChar:
                    if (CurrentOperation != Operations.closebracket)
                    {
                        string fName = NameInterface.GetFormulaName(_polskaParams.FormulaUn, _polskaParams.FormulasTable);
                        throw new Exception(string.Format("{2}: Следование '{0}' за {1} невозможно!",
                            FunctionArgsDeviderChar, OperationsToString(CurrentOperation), fName));
                    }
                    break;
            }
        }

        private string OperationsToString(Operations oper)
        {
            switch (oper)
            {
                case Operations.adding:
                    return "'+'";
                case Operations.closebracket:
                    return "')'";
                case Operations.division:
                    return "'/'";
                case Operations.func_devider:
                    return "'" + FunctionArgsDeviderChar + "'";
                case Operations.multiplication:
                    return "'*'";
                case Operations.openbracket:
                    return "'('";
                case Operations.subtraction:
                    return "'-'";
                default:
                    return "'<неопределенный>'";
            }
        }

        public TVALUES_DB EvaluateStringValue(int halfHourIndex)
        {
            if (!Compiled) Compile();

            var count = 0;

            bool isPrecalcOpen = false;
            Variable precalcVariable = null;
            WorkedPeriodVariable wpPrecalcVariable = null;
            foreach (var evalStringValue in OutString)
            {
                switch (evalStringValue.PolskayaType)
                {
                    case EnumPolskayaType.FunctionDescription:
                        CurrentOperation = (Operations)evalStringValue.Variable;
                        OutEvalString[count++].FunctionDevider = true;
                        if (CurrentOperation == Operations.func_precalc)
                        {
                            isPrecalcOpen = true;
                        }
                        break;
                    case EnumPolskayaType.Function:
                        MakeFunction(ref count, evalStringValue.Variable as Function, _polskaParams.StartDateTime, _polskaParams.EndDateTime, halfHourIndex, _polskaParams.DiscreteType);
                        break;
                    case EnumPolskayaType.PrecalcFunctionBoolean:
                        MakePrecalculateBooleanFunction(ref count, evalStringValue.Variable as Function, wpPrecalcVariable, halfHourIndex);
                        break;
                    case EnumPolskayaType.PrecalcFunctionVariable:
                        MakePrecalculateFunctionVariable(ref count, evalStringValue.Variable as PrecalculatedFunctionVariable, precalcVariable, halfHourIndex);
                        break;
                    case EnumPolskayaType.Operation:
                        CurrentOperation = (Operations)evalStringValue.Variable;
                        MakeOperation(ref count);
                        break;
                    case EnumPolskayaType.Double:
                        OutEvalString[count++].Value = (double)evalStringValue.Variable;
                        break;
                    case EnumPolskayaType.String:
                        var stringval = evalStringValue.Variable as string;
                        if (stringval != null)
                        {
                            OutEvalString[count++].StringValue = (string)evalStringValue.Variable;
                        }
                        break;
                    case EnumPolskayaType.Variable:
                        //Переменная или архив
                        var val = evalStringValue.Variable as Variable;
                        if (val != null)
                        {
                            TVALUES_DB dbValue = null;

                            if (isPrecalcOpen)
                            {
                                isPrecalcOpen = false;
                                precalcVariable = val;
                            }
                            else
                            {
                                dbValue = val.GetHalfHourValueByIndex(halfHourIndex);
                            }

                            if (dbValue != null)
                            {
                                if (_polskaParams.UnitDigit.HasValue)
                                {
                                    //Сначала делим 
                                    OutEvalString[count].SetValue(dbValue.F_FLAG, dbValue.F_VALUE / (double) _polskaParams.UnitDigit.Value);
                                }
                                else
                                {
                                    OutEvalString[count].SetValue(dbValue.F_FLAG, dbValue.F_VALUE);
                                }
                            }
                            else 
                            {
                                var flag = _polskaParams.IsArchTech ? VALUES_FLAG_DB.DataNotFull : VALUES_FLAG_DB.IsFormulaNotCorrectEvaluated;
                                OutEvalString[count].SetValue(flag, 0);
                            }
                        }

                        count++;
                        break;
                    case EnumPolskayaType.WorkedPeriodVariable:
                        var wpVal = evalStringValue.Variable as WorkedPeriodVariable;
                        if (wpVal != null)
                        {
                            if (isPrecalcOpen)
                            {
                                isPrecalcOpen = false;
                                wpPrecalcVariable = wpVal;
                            }
                            else
                            {
                                wpVal.FillVariable(halfHourIndex);
                            }
                        }

                        count++;
                        break;
                }
            }

            if (count != 1 || OutEvalString[0].FunctionDevider)
                throw new Exception("Выражение некорректно!");
            return OutEvalString[0].DBValue;
        }

        private void MakeOperation(ref int eval_index)
        {
            if (eval_index < 2)
                throw new Exception("Выражение некорректно! Возможно есть операция без последующего аргумента");

            if (OutEvalString[eval_index - 2].FunctionDevider ||
                OutEvalString[eval_index - 1].FunctionDevider)
                throw new Exception("Выражение некорректно! Операция предшествует аргументу");

            OutEvalString[eval_index - 2].Operate(OutEvalString[eval_index - 1], CurrentOperation);

            eval_index--;
        }

        private void MakeFunction(ref int eval_index, Function func, DateTime startDateTime, DateTime endDateTime, int HalfHourIndex, enumTimeDiscreteType DiscreteType)
        {
            if (func == null) throw new Exception("Ошибка обработки функции ");

            int func_args_start_index = eval_index - 1;
            while (func_args_start_index >= 0 && OutEvalString[func_args_start_index].FunctionDevider == false)
                func_args_start_index--;

            if (func_args_start_index < 0)
                throw new Exception("Ошибка обработки функции " + func.id);

            int args_count = eval_index - func_args_start_index - 1;
            if (func.FuncArgsCount != int.MaxValue && args_count != func.FuncArgsCount)
                throw new Exception(string.Format(" У функции {0} объявлено аргументов {1}, передано в формулу {2}.",
                    func.id, func.FuncArgsCount, args_count));
            else if (func.FuncArgsCount == int.MaxValue && args_count == 0)
                throw new Exception("Функция " + func.id +
                                    " объявлена без ограничения числа аргументов, передано в формулу 0.");

            try
            {
                if (args_count > FunctionArgs.Count)
                    FunctionArgs = new FunctionArgumentList(args_count) { StartDateTime = startDateTime, EndDateTime = endDateTime, HalfHourIndex = HalfHourIndex };
                FunctionArgs.HalfHourIndex = HalfHourIndex;
                FunctionArgs.DiscreteType = DiscreteType;
                VALUES_FLAG_DB flag = VALUES_FLAG_DB.None;


                for (int i = 0; i < args_count; i++)
                {
                    OutEvalString[i + func_args_start_index + 1]
                        .GetValueAndCumulateFlag(ref flag, ref FunctionArgs, ref i);
                }

                OutEvalString[func_args_start_index].SetValue(flag,
                    ((func.FuncDlgt == null)
                        ? func.Value
                        : func.FuncDlgt(this, func,
                            FunctionArgs, 0, args_count).ToDouble(null)));
            }
            catch (Exception exc)
            {
                if (exc.InnerException != null)
                    throw new Exception("Ошибка обработки функции " + func.id + ":" + exc.InnerException.ToString(),
                        exc);
                else
                {
                    throw new Exception("Ошибка обработки функции " + func.id + ":" + exc.ToString(),
                        exc);
                }
            }

            eval_index -= args_count;
        }

        private void MakePrecalculateBooleanFunction(ref int eval_index, Function func,
            WorkedPeriodVariable precalcVariable, int halfHourIndx)
        {
            if (func == null) throw new Exception("Ошибка обработки функции ");

            int func_args_start_index = eval_index - 1;
            while (func_args_start_index >= 0 && OutEvalString[func_args_start_index].FunctionDevider == false)
                func_args_start_index--;

            if (func_args_start_index < 0)
                throw new Exception("Ошибка обработки функции " + func.id);

            int args_count = eval_index - func_args_start_index - 1;
            if (func.FuncArgsCount != int.MaxValue && args_count != func.FuncArgsCount)
                throw new Exception(string.Format(" У функции {0} объявлено аргументов {1}, передано в формулу {2}.",
                    func.id, func.FuncArgsCount, args_count));
            else if (func.FuncArgsCount == int.MaxValue && args_count == 0)
                throw new Exception("Функция " + func.id +
                                    " объявлена без ограничения числа аргументов, передано в формулу 0.");

            try
            {
                IConvertible arg1, arg2;

                if (args_count > FunctionArgs.Count)
                    FunctionArgs = new FunctionArgumentList(3);

                VALUES_FLAG_DB flag = VALUES_FLAG_DB.None;

                for (int i = 0; i < args_count; i++)
                {
                    OutEvalString[i + func_args_start_index + 1]
                        .GetValueAndCumulateFlag(ref flag, ref FunctionArgs, ref i);
                }

                arg1 = FunctionArgs[0].ArgumentValue;
                arg2 = FunctionArgs[1].ArgumentValue;

                OutEvalString[func_args_start_index].SetValue(flag, (double)
                ((func.FuncDlgt == null)
                    ? func.Value
                    : func.FuncBoolDlgt(this, precalcVariable, arg1, arg2, halfHourIndx)));
            }
            catch (Exception exc)
            {
                throw new Exception("Ошибка обработки функции " + func.id, exc);
            }

            eval_index -= args_count;
        }

        private void MakePrecalculateFunctionVariable(ref int eval_index, PrecalculatedFunctionVariable func,
            Variable precalcVariable, int halfHourIndx)
        {
            if (func == null) throw new Exception("Ошибка обработки функции ");

            if (precalcVariable == null)
                throw new Exception(string.Format(" Не найдены аргументы у ф-ии {0}.", func.Id));

            int funcArgsStartIndex = eval_index - 1;
            while (funcArgsStartIndex >= 0 && OutEvalString[funcArgsStartIndex].FunctionDevider == false)
                funcArgsStartIndex--;

            try
            {
                var v = func.Calculate(precalcVariable, halfHourIndx);
                OutEvalString[funcArgsStartIndex].SetValue(v.F_FLAG, v.F_VALUE); //Здесь считать
            }
            catch (Exception exc)
            {
                throw new Exception("Ошибка обработки функции " + func.Id, exc);
            }

            eval_index -= 1;
        }

        // добавление в стек операций
        private void InsertToStack(object Operation, EnumPolskayaType polskayaType, int indx = -1)
        {
            object oper;
            if (Operation is char)
            {
                switch ((char)Operation)
                {
                    case '+':
                        CurrentOperation = Operations.adding;
                        break;

                    case '-':
                        CurrentOperation = Operations.subtraction;
                        break;

                    case '/':
                        CurrentOperation = Operations.division;
                        break;

                    case '*':
                        CurrentOperation = Operations.multiplication;
                        break;

                    case '(':
                        //if (CurrentOperation == Operations.func_devider)
                        //    break;// анализировать ничего не надо
                        CurrentOperation = Operations.openbracket;
                        break;

                    case ')':
                        CurrentOperation = Operations.closebracket;
                        // вытолкнуть в выходную строку все символы до открывающейся скобки
                        //var c = _operationStack.LastOrDefault(o => o.PolsayaType == EnumPolskayaType.Operation && (Operations)o.Variable == Operations.openbracket);
                        var openBrIndex = 0;
                        if (OperationStackCount == 0)
                        {
                            throw new Exception("Выражение некорректно!");
                        }

                        openBrIndex = _operationStack.FindLastIndex(OperationStackCount - 1, OperationStackCount,
                            o => o.PolskayaType == EnumPolskayaType.Operation &&
                                 (Operations)o.Variable == Operations.openbracket);
                        if (openBrIndex < 0) // || (open_br_index == OperationStackCount - 1))
                            throw new Exception("Выражение некорректно! Обнаружены неоткрытые скобки");

                        openBrIndex++;
                        while (OperationStackCount > openBrIndex)
                        {
                            CurrentOperationOutString = _operationStack[OperationStackCount - 1];
                            MakeOutString();
                        }

                        if (openBrIndex > 1)
                        {
                            var op = _operationStack.ElementAtOrDefault(openBrIndex - 2);
                            if (op != null && (op.PolskayaType == EnumPolskayaType.Function ||
                                               op.PolskayaType == EnumPolskayaType.PrecalcFunctionVariable))
                            {
                                CurrentOperationOutString = op;
                                MakeOutString();
                            }
                        }
                        OperationStackCount--; // сама откр скобка уничтожается
                        return;

                    case FunctionArgsDeviderChar:
                        CurrentOperation = Operations.func_devider;
                        // вытолкнуть в выходную строку все символы до открывающейся скобки
                        openBrIndex = _operationStack.FindLastIndex(OperationStackCount - 1, OperationStackCount,
                            o => o.PolskayaType == EnumPolskayaType.Operation &&
                                 (Operations)o.Variable == Operations.openbracket);
                        if (openBrIndex < 0) // || (open_br_index == OperationStackCount - 1))
                            throw new Exception("Выражение некорректно! Разделитель '" + FunctionArgsDeviderChar +
                                                "' не имеет функции!");

                        openBrIndex++;
                        while (OperationStackCount > openBrIndex)
                        {
                            CurrentOperationOutString = _operationStack[OperationStackCount - 1];
                            MakeOutString();
                        }
                        return;
                }
                OperationOutStackIndex = OperationStackCount - 1;
                if (CurrentOperation != Operations.openbracket)
                {
                    // проверяем приоритет
                    while (OperationOutStackIndex >= 0 &&
                           OperationsPriority[(byte)_operationStack[OperationOutStackIndex].Variable] >=
                           OperationsPriority[(int)CurrentOperation])
                    {
                        CurrentOperationOutString = _operationStack[OperationOutStackIndex];
                        MakeOutString();
                        OperationOutStackIndex = OperationStackCount - 1;
                    }
                }
                oper = CurrentOperation;
            }
            else
            {
                // приняли функцию. Пока что никаких проверок
                //CurrentOperation = (Operations) Operation;
                OperationOutStackIndex = OperationStackCount - 1;

                oper = Operation;
            }

            var obj = new PolskayaVariable(polskayaType, oper);

            if (OperationStackCount >= _operationStack.Capacity)
                _operationStack.Add(obj);
            else
                _operationStack.Insert(OperationOutStackIndex + 1, obj);
            OperationStackCount++;
        }

        // расчет выходной строки
        private void MakeOutString()
        {
            if (OutString.Count < 1)
                throw new Exception("Выражение некорректно!");

            OutString.Add(CurrentOperationOutString);
            OperationStackCount--;
        }

        private void Final()
        {
            while (OperationStackCount > 0)
            {
                CurrentOperationOutString = _operationStack[OperationStackCount - 1];
                MakeOutString();
            }

            _operationStack.Clear();
        }

        private class PolskayaVariable
        {
            public readonly EnumPolskayaType PolskayaType;
            public readonly object Variable;

            public PolskayaVariable(EnumPolskayaType polskayaType, object variable)
            {
                PolskayaType = polskayaType;
                Variable = variable;
            }
        }

        private enum EnumPolskayaType : byte
        {
            /// <summary>
            /// операция
            /// </summary>
            Operation = 0,

            /// <summary>
            /// константа
            /// </summary>
            Double = 1,

            /// <summary>
            /// перменная
            /// </summary>
            Variable = 2,

            /// <summary>
            /// ф-ия
            /// </summary>
            Function = 3,

            /// <summary>
            /// ф-ия с предварительных расчетом данных
            /// </summary>
            PrecalcFunctionVariable = 4,

            /// <summary>
            /// аргумент ф-ии
            /// </summary>
            FunctionDescription = 5,
            PrecalcFunctionBoolean = 6,

            /// <summary>
            /// Переменная для определения периода работы
            /// </summary>
            WorkedPeriodVariable = 7,

            /// <summary>
            /// Строковое значение для передачи аргументов в типы
            /// </summary>
            String = 8,
        }

        private List<TVALUES_DB> _values;

        public List<TVALUES_DB> GetValues()
        {
            if (_values != null) return _values;

            _values = new List<TVALUES_DB>();
            for (var j = 0; j < _polskaParams.NumbersValues; j++)
            {
                var val = EvaluateStringValue(j);
                if (_polskaParams.UnitDigit.HasValue && val !=null)
                {
                    val.F_VALUE *= (double)_polskaParams.UnitDigit.Value;
                }

                _values.Add(val);
            }

            return _values;
        }

        public TVALUES_DB TryGetValueByIndex(int i)
        {
            var values = GetValues();
            if (values == null) return null;

            return values[i];
        }

        public TVALUES_DB TryGetNext()
        {
            return null;
        }

        private IHierarchyChannelID _hierarchyId;

        public IHierarchyChannelID HierarchyId
        {
            get
            {
                if (_hierarchyId != null) return _hierarchyId;

                var typeHierarchy = enumTypeHierarchy.Unknown;
                switch (_polskaParams.FormulasTable)
                {
                    case enumFormulasTable.Info_Formula_Description:
                        typeHierarchy = enumTypeHierarchy.Formula;
                        break;
                    case enumFormulasTable.Info_TP2_Contr_Formula_Description:
                        typeHierarchy = enumTypeHierarchy.Formula_TP_CA;
                        break;
                    case enumFormulasTable.Info_TP2_OurSide_Formula_Description:
                        typeHierarchy = enumTypeHierarchy.Formula_TP_OurSide;
                        break;
                }


                // return default(IHierarchyChannelID);
                //TODO: ContainerINtegration
                //_hierarchyId = new ID_Hierarchy_Channel(typeHierarchy, F_ID, 0);
                _hierarchyId = new Formula_ID_Hierarchy_Channel(typeHierarchy, _polskaParams.FormulaUn, 0);
                return _hierarchyId;
            }
            set { _hierarchyId = value; }

        }



    }

    internal enum OutStringValueType
    {
        variable = 1,
        constant = 2,
        operation = 3
    };

    public enum enumErrorVariable
    {
        None = 0,
        DivideIntoZero = 1
    };
}