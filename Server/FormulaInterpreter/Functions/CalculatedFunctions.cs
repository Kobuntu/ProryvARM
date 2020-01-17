using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Proryv.AskueARM2.Server.DBAccess.Internal.Formulas.Variables;
using Proryv.Servers.Calculation.FormulaInterpreter;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    #region Предопределенные функции

    public class MinFunction : Function
    {
        public MinFunction()
        {
            id = "min";
            FuncArgsCount = int.MaxValue;
            FuncDlgt = DoMin;
        }

        private IConvertible DoMin(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {
            var result = args[arg_index + --args_count].ArgumentValue;
            while (args_count-- > 0)
                if (args[arg_index + args_count].ArgumentValue < result)
                    result = args[arg_index + args_count];
            return result;
        }
    }

    public class MaxFunction : Function
    {
        public MaxFunction()
        {
            id = "max";
            FuncArgsCount = int.MaxValue;
            FuncDlgt = DoMax;
        }

        private IConvertible DoMax(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {
            var result = args[arg_index + --args_count].ArgumentValue;
            while (args_count-- > 0)
                if (args[arg_index + args_count].ArgumentValue > result)
                    result = args[arg_index + args_count].ArgumentValue;
            return result;
        }
    }

    public class SinFunction : Function
    {
        public SinFunction()
        {
            id = "sin";
            FuncArgsCount = 1;
            FuncDlgt = DoSin;
        }

        private IConvertible DoSin(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {
            return Math.Sin(args[arg_index].ArgumentValue);
        }
    }

    public class CosFunction : Function
    {
        public CosFunction()
        {
            id = "cos";
            FuncArgsCount = 1;
            FuncDlgt = DoCos;
        }

        private IConvertible DoCos(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {
            return Math.Cos(args[arg_index].ArgumentValue);
        }
    }

    public class PowFunction : Function
    {
        public PowFunction()
        {
            id = "pow";
            FuncArgsCount = 2;
            FuncDlgt = DoPow;
        }

        private IConvertible DoPow(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {
            return Math.Pow(args[arg_index].ArgumentValue, args[++arg_index].ArgumentValue);
        }
    }

    public class ExpFunction : Function
    {
        public ExpFunction()
        {
            id = "exp";
            FuncArgsCount = 1;
            FuncDlgt = DoExp;
        }

        private IConvertible DoExp(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {
            return Math.Exp(args[arg_index].ArgumentValue);
        }
    }

    public class LogFunction : Function
    {
        public LogFunction()
        {
            id = "log";
            FuncArgsCount = 1;
            FuncDlgt = DoLog;
        }

        private IConvertible DoLog(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {
            return Math.Log(args[arg_index].ArgumentValue);
        }
    }

    public class AbsFunction : Function
    {
        public AbsFunction()
        {
            id = "abs";
            FuncArgsCount = 1;
            FuncDlgt = DoAbs;
        }

        private IConvertible DoAbs(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {
            return Math.Abs(args[arg_index].ArgumentValue);
        }
    }

    public class IfFunction : Function
    {
        public IfFunction()
        {
            id = "if";
            FuncArgsCount = 6;
            FuncDlgt = DoIf;
        }

        private IConvertible DoIf(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {
            // 0 аргумент - первое число для сравнения
            // 1 аргумент - второе число для сравнения
            // 2 аргумент - точность сравнения (число знаков после запятой, если -1, то как есть, без округления)
            // 3 аргумент - оператор сравнения (1 это >, 2 это <, 3 это >=, 4 это <=, 5 это ==, 6 это !=)
            // 4 аргумент - результат, если условие выполнено
            // 5 аргумент - результат, если условие ложно


            var points = (int)args[arg_index + 2].ArgumentValue;
            if (points >= 0)
                return CompareWithRound(args.OfType<IFunctionArgumnet<double>>().Select(i => i.Value).ToArray(),
                    arg_index, args_count, points);
            return CompareExact(args[arg_index].ArgumentValue, args[arg_index + 1].ArgumentValue,
                (EOperatorType)args[arg_index + 3].ArgumentValue, args[arg_index + 4].ArgumentValue,
                args[arg_index + 5].ArgumentValue);
        }

        private IConvertible CompareWithRound(double[] args, int arg_index, int args_count, int points)
        {
            var _TRUE = false;
            switch ((EOperatorType)args[arg_index + 3])
            {
                case EOperatorType.Equal:
                    if (Math.Round(args[arg_index], points) ==
                        Math.Round(args[arg_index + 1], points))
                        _TRUE = true;
                    break;

                case EOperatorType.Less:
                    if (Math.Round(args[arg_index], points) <
                        Math.Round(args[arg_index + 1], points))
                        _TRUE = true;
                    break;

                case EOperatorType.LessOrEqual:
                    if (Math.Round(args[arg_index], points) <=
                        Math.Round(args[arg_index + 1], points))
                        _TRUE = true;
                    break;

                case EOperatorType.More:
                    if (Math.Round(args[arg_index], points) >
                        Math.Round(args[arg_index + 1], points))
                        _TRUE = true;
                    break;

                case EOperatorType.MoreOrEqual:
                    if (Math.Round(args[arg_index], points) >=
                        Math.Round(args[arg_index + 1], points))
                        _TRUE = true;
                    break;

                case EOperatorType.NotEqual:
                    if (Math.Round(args[arg_index], points) !=
                        Math.Round(args[arg_index + 1], points))
                        _TRUE = true;
                    break;

                default:
                    throw new Exception("Оператор сравнения не поддерживает оператор " + args[arg_index + 2] +
                                        ". (1 это >, 2 это <, 3 это >=, 4 это <=, 5 это ==, 6 это !=)");
            }
            if (_TRUE) return args[arg_index + 4];
            return args[arg_index + 5];
        }

        protected IConvertible CompareExact(double compare_val1, double compare_val2,
            EOperatorType compare_operator, double result_true, double result_false)
        {
            var _TRUE = false;
            switch (compare_operator)
            {
                case EOperatorType.Equal:
                    if (compare_val1 == compare_val2)
                        _TRUE = true;
                    break;

                case EOperatorType.Less:
                    if (compare_val1 < compare_val2)
                        _TRUE = true;
                    break;

                case EOperatorType.LessOrEqual:
                    if (compare_val1 <= compare_val2)
                        _TRUE = true;
                    break;

                case EOperatorType.More:
                    if (compare_val1 > compare_val2)
                        _TRUE = true;
                    break;

                case EOperatorType.MoreOrEqual:
                    if (compare_val1 >= compare_val2)
                        _TRUE = true;
                    break;

                case EOperatorType.NotEqual:
                    if (compare_val1 != compare_val2)
                        _TRUE = true;
                    break;

                default:
                    throw new Exception("Сравнения не поддерживает оператор " + (int)compare_operator +
                                        ". (1 это >, 2 это <, 3 это >=, 4 это <=, 5 это ==, 6 это !=)");
            }
            if (_TRUE) return result_true;
            return result_false;
        }

        protected enum EOperatorType
        {
            More = 1,
            Less = 2,
            MoreOrEqual = 3,
            LessOrEqual = 4,
            Equal = 5,
            NotEqual = 6
        }
    }

    public class MacroIfFunction : IfFunction
    {
        public MacroIfFunction()
        {
            // функция if для расчета макросов вида if(a>b){...}else{...}
            id = "если";
            FuncArgsCount = 5;
            FuncDlgt = DoMacroIf;
        }

        private IConvertible DoMacroIf(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {
            // 0 аргумент - первое число для сравнения
            // 1 аргумент - оператор сравнения (1 это >, 2 это <, 3 это >=, 4 это <=, 5 это ==, 6 это !=)
            // 2 аргумент - второе число для сравнения
            // 3 аргумент - результат, если условие выполнено
            // 4 аргумент - результат, если условие ложно

            return CompareExact(args[arg_index].ArgumentValue, args[arg_index + 2].ArgumentValue,
                (EOperatorType)args[arg_index + 1].ArgumentValue, args[arg_index + 3].ArgumentValue,
                args[arg_index + 4].ArgumentValue);
        }
    }

    public class IfEnabledFunction : BoolPrecalcFunction
    {
        public IfEnabledFunction()
        {
            id = "еслиработает";
            //base.FuncArgsCount = 3;
            //base.FuncBoolDlgt = this.DoMacroIf;
            BoolFunc = BooleanFunction;
        }

        private bool BooleanFunction(WorkedPeriodVariable objectWork, int indx)
        {
            if (objectWork == null) return false;

            return objectWork.IsIndexInWorkedPeriod;
        }
    }

    public abstract class BoolPrecalcFunction : Function
    {
        public BoolPrecalcFunction()
        {
            FuncBoolDlgt = SelectPrecalculated;
        }

        private IConvertible SelectPrecalculated(IFormulaParser parser, WorkedPeriodVariable precalcVariable,
            IConvertible arg1, IConvertible arg2, int indx)
        {
            // 0 аргумент - ф-ия возвращающая логический аргумент
            // 1 аргумент - результат, если условие выполнено
            // 2 аргумент - результат, если условие ложно

            if (BoolFunc == null) throw new Exception("Отсутствует входная ф-ия");

            return BoolFunc(precalcVariable, indx) ? arg1 : arg2;
        }
    }

    public class RoundFunction : Function
    {
        public RoundFunction()
        {
            id = "round";
            FuncArgsCount = 2;
            FuncDlgt = DoRound;
        }

        private IConvertible DoRound(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {
            return Math.Round(args[arg_index].ArgumentValue, (int)args[++arg_index].ArgumentValue,
                MidpointRounding.AwayFromZero);
        }
    }


    /// <summary>
    /// Получить индекс получасовки
    /// </summary>
    public class GetHalfHourIndex : Function
    {
        public GetHalfHourIndex()
        {
            id = "ИндексПолучасовогоЗначения";
            FuncArgsCount = 0;
            FuncDlgt = DoUDF;
        }

        private Dictionary<Function, int> counter = new Dictionary<Function, int>();

        private IConvertible DoUDF(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {

            return args.HalfHourIndex;
        }
    }


    /// <summary>
    /// Получить время текущей получасовки в формате Unix
    /// </summary>
    public class GetHalfHourDateTime : Function
    {
        public GetHalfHourDateTime()
        {
            id = "ВремяПолучасовогоЗначения";
            FuncArgsCount = 1;
            FuncDlgt = DoUDF;

        }
        private IConvertible DoUDF(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {
            var currdate = args.StartDateTime.AddMinutes(30 * args.HalfHourIndex);
            var firstOrDefault = args.OfType<IFunctionArgumnet>().FirstOrDefault<IFunctionArgumnet>();
            if (firstOrDefault == null)

            {
                throw new Exception("Должен быть передан хотя бы один аргумент");
            }
            if (firstOrDefault != null && firstOrDefault.ArgumentType != FunctionArgumentArgsTypeEnum.String)
            {
                throw new Exception("Первым аргументом должен быть формат выводимого значения");
            }
            //Вернуть время в формате Unix
            if (firstOrDefault.ArgumentValue == "u")
            {
                return (Int32)(currdate.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }
            var res = currdate.ToString(firstOrDefault.ArgumentValue);

            return res;
        }

    }



    /// <summary>
    /// Получить время текущей получасовки в формате Unix
    /// </summary>
    public class ValueStatusCodeContains : Function
    {
        public ValueStatusCodeContains()
        {
            id = "СтатусЗначенияСодержитКод";
            FuncArgsCount = 2;
            FuncDlgt = DoStatus;

        }
        private IConvertible DoStatus(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {

            if (args_count != 2)
            {
                throw new Exception("Функция СтатусЗначенияСодержитКод должна иметь два входящих аргумента");
            }
            var firstOrDefault = args.OfType<IFunctionArgumnet>().FirstOrDefault<IFunctionArgumnet>();
            if (firstOrDefault == null)

            {
                throw new Exception("Должен быть передан хотя бы один аргумент");
            }
            if (firstOrDefault != null && firstOrDefault.Flag == null)
            {
                throw new Exception("Нет статуса у значения");
            }

            var last = args.OfType<IFunctionArgumnet>().LastOrDefault<IFunctionArgumnet>();
            if (last != null)
            {
                var code = (ulong)last.ArgumentValue;


                if ((firstOrDefault.Flag.Value & (VALUES_FLAG_DB)code) != 0)
                {
                    return 1;
                }

                return 0;

         


            }
            throw new Exception("Неверный код статуса для проверки в значении");


            //  return firstOrDefault.Flag.Value;
        }

    }


    /// <summary>
    /// Получить время текущей получасовки в формате Unix
    /// </summary>
    public class ValueStatusCodeContainsAllAlarmStatuses : Function
    {
        public ValueStatusCodeContainsAllAlarmStatuses()
        {
            id = "СтатусЗначенияНедостоверные";
            FuncArgsCount = 1;
            FuncDlgt = DoStatus;

        }
        private IConvertible DoStatus(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {

            if (args_count != 1)
            {
                throw new Exception("Функция СтатусЗначенияНедостоверные должна иметь один входящий аргумент");
            }
            var firstOrDefault = args.OfType<IFunctionArgumnet>().FirstOrDefault<IFunctionArgumnet>();
            if (firstOrDefault == null)

            {
                throw new Exception("Должен быть передан хотя бы один аргумент");
            }
            if (firstOrDefault != null && firstOrDefault.Flag == null)
            {
                throw new Exception("Нет статуса у значения");
            }

            if ((firstOrDefault.Flag.Value & VALUES_FLAG_DB.AllAlarmStatuses) != 0)
            {
                return 1;
            }

            return 0;


        }

    }

    /// <summary>
    /// Пользовательская функция выполнить SQL
    /// </summary>
    public class UserDefinedSQLFunction : Function
    {
        private readonly IDBInterfaceAdapter _dbInterfaceAdapter;

        public UserDefinedSQLFunction(IDBInterfaceAdapter dbInterfaceAdapter)
        {
            _dbInterfaceAdapter = dbInterfaceAdapter;
            id = "ПользовательскаяФункцияSql";
            FuncArgsCount = int.MaxValue;
            FuncDlgt = DoSql;

            //Создадим временную таблицу для результатов
            // _connection.Open();
            //  createTemporyResultTable(_connection, _tempTablePrefix);


        }



        /// <summary>
        /// 
        /// </summary>
        // private string _tempTablePrefix = ("#FormulaResults" + Guid.NewGuid()).Replace("-", "");
        //  SqlConnection _connection = new SqlConnection(Settings.DbConnectionString);

        //private int Execution_Id = 0;

        /// <summary>
        /// Создаем временную таблицу куда будут сохранятся результаты
        /// </summary>
        /// <param name="connection"></param>
        void createTemporyResultTable(SqlConnection connection, string tablePrefix)
        {
            //var commandText = String.Format(@"CREATE TABLE {0} 
            //    (
            //    Execution_Id  BIGINT NOT NULL,
            //    SqlQuery NVARCHAR(MAX) NOT NULL,
            //    QueryResult FLOAT NULL)
            //    ", tablePrefix);


            //using (DbCommand cmd1 = new SqlCommand(commandText, _connection))
            //{
            //    cmd1.ExecuteNonQuery();
            //}



        }


        LazyDataTable temporaryResulTable = new LazyDataTable();

        private IConvertible DoSql(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {




            Debug.WriteLine(args.StartDateTime);
            Debug.WriteLine(args.EndDateTime);
            Debug.WriteLine(args.HalfHourIndex);
            var firstOrDefault = args.OfType<IFunctionArgumnet>().FirstOrDefault<IFunctionArgumnet>();
            if (firstOrDefault == null)

            {
                throw new Exception("Должен быть передан хотя бы один аргумент");
            }
            if (firstOrDefault != null && firstOrDefault.ArgumentType != FunctionArgumentArgsTypeEnum.String)
            {
                throw new Exception("Первым аргументом должен быть SQL скрипт");
            }

            using (var dbCon = _dbInterfaceAdapter.GetSqlConnection())
            {
                var t = args.OfType<IFunctionArgumnet>().Skip(1).Select(h => h.ArgumentValue);

                var SQL = string.Format(firstOrDefault.ArgumentValue.ToString(), t.ToArray());

                DbCommand cmd1 = new SqlCommand();
                dbCon.Open();
                cmd1.Connection = dbCon;
                cmd1.CommandText = SQL;
                var result = cmd1.ExecuteScalar();
                return result as IConvertible;
            }


            //        Execution_Id++;
            //            var firstOrDefault = args.OfType<IFunctionArgumnet>().FirstOrDefault<IFunctionArgumnet>();
            //            if (firstOrDefault == null)

            //            {
            //                throw new Exception("Должен быть передан хотя бы один аргумент");
            //            }
            //            if (firstOrDefault != null && firstOrDefault.ArgumentType != FunctionArgumentArgsTypeEnum.String)
            //            {
            //                throw new Exception("Первым аргументом должен быть SQL скрипт");
            //            }


            //            var t = args.OfType<IFunctionArgumnet>().Skip(1).Select(h => h.ArgumentValue);



            //            var SQLText = string.Format(firstOrDefault.ArgumentValue.ToString(), t.ToArray());
            //            var SQLcommand = string.Format(@"declare @out table
            //(
            //out float
            //)
            //insert into @out
            //exec(@sql)
            //INSERT INTO {0} VALUES(@execution_id,@sql,(select * from @out))", _tempTablePrefix);



            //            using (var cmd = new SqlCommand(SQLcommand))

            //            {
            //                Debug.WriteLine("Execute ");
            //                cmd.Connection = _connection;
            //                cmd.Parameters.Add("@execution_id", Execution_Id);
            //                cmd.Parameters.Add("@sql", SQLText);
            //                cmd.ExecuteNonQuery();
            //                Debug.WriteLine("Execute formula SQL");

            //                return new SqlTemporatyTableResult(_tempTablePrefix, _connection, Execution_Id, ref temporaryResulTable);
            //            }
            //using (DbCommand cmd1 = new SqlCommand(_connection))
            //{



            //    cmd1.Connection.Open();
            //    var result = cmd1.ExecuteScalar();
            //    return result as IConvertible;
            //}





        }


        protected class LazyDataTable : DataTable
        {
            public bool IsReadResult;
        }

        protected class SqlTemporatyTableResult : IConvertible
        {
            private readonly string _resultTableName;
            private readonly SqlConnection _connection;
            private readonly long _executionId;
            private readonly LazyDataTable _resultTab;

            internal SqlTemporatyTableResult(string resultTableName, SqlConnection connection, long executionId, ref LazyDataTable resultTab)
            {
                _resultTableName = resultTableName;
                _connection = connection;
                _executionId = executionId;
                _resultTab = resultTab;
            }
            public TypeCode GetTypeCode()
            {
                throw new NotImplementedException();
            }

            public bool ToBoolean(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public char ToChar(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public sbyte ToSByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public byte ToByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public short ToInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public ushort ToUInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public int ToInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public uint ToUInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public long ToInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public ulong ToUInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public float ToSingle(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public double ToDouble(IFormatProvider provider)
            {
                if (!_resultTab.IsReadResult)
                {
                    //Надо проинициализировать 
                    using (SqlCommand command = new SqlCommand(string.Format("Select * From {0}", _resultTableName), _connection))
                    using (SqlDataReader dr = command.ExecuteReader())
                    {

                        _resultTab.Load(dr);
                        _resultTab.IsReadResult = true;
                    }

                    Debug.WriteLine(_resultTab.DataSet);
                }


                var res = _resultTab.Select(string.Format("Execution_Id = {0}", this._executionId));
                var qr = res[0]["QueryResult"];
                if (qr == null) return 0;
                var result = Convert.ToDouble(qr);
                return result;
            }

            public decimal ToDecimal(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public DateTime ToDateTime(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public string ToString(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public object ToType(Type conversionType, IFormatProvider provider)
            {
                throw new NotImplementedException();
            }
        }

    }





    public class UserDefinedCSharpFunction : Function
    {
        public UserDefinedCSharpFunction()
        {
            id = "ПользовательскаяФункцияКод";
            FuncArgsCount = int.MaxValue;
            FuncDlgt = DoCSharp;
        }

        private IConvertible DoCSharp(IFormulaParser parser, Function func, FunctionArgumentList args, int arg_index,
            int args_count)
        {
            var firstOrDefault = args.OfType<IFunctionArgumnet>().FirstOrDefault<IFunctionArgumnet>();
            if (firstOrDefault == null)

            {
                throw new Exception("Должен быть передан хотя бы один аргумент");
            }
            if (firstOrDefault != null && firstOrDefault.ArgumentType != FunctionArgumentArgsTypeEnum.String)
            {
                throw new Exception("Первым аргументом должен быть С# код");

            }

            throw new NotImplementedException("Работа с C# выражениями на текущий момент отключена");
            //var res = Proryv.Servers.Calculation.Parser.ProryvParsersFactory.ParseTextToString(
            //     "{" + firstOrDefault.ArgumentValue.ToString() + "}", null);
            //return res;
        }
    }

    #endregion
}