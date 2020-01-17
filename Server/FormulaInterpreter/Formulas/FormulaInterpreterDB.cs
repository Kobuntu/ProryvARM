using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.FormulaInterpreter;
using Proryv.Servers.Calculation.DBAccess.Enums;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;
using Proryv.Servers.Calculation.FormulaInterpreter.Formulas;
using Proryv.Servers.Calculation.DBAccess.Common;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{

    public class FormulaInterpreterDB : IDisposable
    {
        readonly HashSet<string> _recursionCallStack;

        private FORMULAS_EXPRESSIONS _fExpr;

        private enumFormulasTable _formulasTable;
        //private readonly int _numbersHalfHours;
        
        private readonly IDateTimeExtensions _dateTimeExtensions;
        private readonly Dictionary<string, Variable> _variablesDict;
        private readonly IFormulaArchivesPrecalculator _formulaArchivesPrecalculator;
        public readonly InterpretatorParams InterpretatorParams;
        public FormulaArchives Archives;


        //string timeZoneId, bool isReadCalculatedValues,
          //  DateTime dtServerStart, DateTime dtServerEnd, EnumDataSourceType? dataSourceType,

        public FormulaInterpreterDB(FORMULAS_EXPRESSIONS fExpr, IDBInterfaceAdapter nameInterface  
            , InterpretatorParams interpretatorParams
            , IMyListConverters myListConverters, IGetNotWorkedPeriodService IGetNotWorkedPeriodService, IDateTimeExtensions dateTimeExtensions)
        {
            _fExpr = fExpr;
            InterpretatorParams = interpretatorParams;
            //this.SubFormulas = new System.Collections.Hashtable();
            _recursionCallStack = new HashSet<string>();
            _variablesDict = new Dictionary<string, Variable>();
            //_variablesDict.Add("myVar", new Variable("myVar", 0, 1440, new TFormulaConstant() { ArchiveValues = new List<TVALUES_DB>() { new TVALUES_DB(VALUES_FLAG_DB.DataCorrect, 20010) } }, "1666"));
            _nameInterface = nameInterface;
            
            _dateTimeExtensions = dateTimeExtensions;
            
            _formulaArchivesPrecalculator = new FormulaArchivesPrecalculator(interpretatorParams.TimeZoneId
                , interpretatorParams.IsReadCalculatedValues, interpretatorParams.ServerStartDateTime, interpretatorParams.ServerEndDateTime
                , interpretatorParams.DataSourceType, myListConverters, IGetNotWorkedPeriodService, _dateTimeExtensions);
        }

        private readonly IDBInterfaceAdapter _nameInterface;

        public FORMULAS_EXPRESSIONS FORMULAS_EXPR
        {
            get { return _fExpr; }
            set { _fExpr = value; }
        }

        /// <summary>
        /// Делаем расчет по формуле
        /// </summary>
        /// <param name="formulaId">Идентификатор формулы</param>
        /// <param name="inDataTI">Данные по ТИ</param>
        /// <param name="inDataIntegrals">Интегральные данные</param>
        /// <param name="inDataTP">Данные по ТП</param>
        /// <param name="inDataSection">Данные по сечению</param>
        /// <param name="indxStart">Начальный индекс, с которого формула активна</param>
        /// <param name="indxEnd">Конечный индекс</param>
        /// <param name="unitDigitCoeff">Коэфф. дорасчета ед. измерения</param>
        /// <param name="formulaTPType">Тип формулы ТП</param>
        /// <param name="formulasTable">Табличный тип формулы (обычная или ТП)</param>
        /// <param name="tpId">Идентификатор ТП к которой привязана формула</param>
        /// <param name="rangeInSectionForEntirePeriod">Используется ли формула в сечении, где действует ограничение на период действия ТП</param>
        /// <param name="rangeIndexesInSectionList">Индексы с которого действуют ТП в сечении</param>
        /// <param name="hiHiSetpoint">Верхняя уставка, по которой проверяем формулу</param>
        /// <param name="loLoSetpoint">Нижняя уставка, по которой проверяем формулу</param>
        /// <param name="inFormulaConstants">Данные по константам, используемым в формуле</param>
        /// <param name="formulasDisabledPeriods">Индексы получасовок между которыми формула не считается</param>
        /// <returns></returns>
        public List<TVALUES_DB> InterpretFormula(string formulaId
            ,bool isSumm
            , int indxStart, int indxEnd, double unitDigitCoeff, enumClientFormulaTPType formulaTPType,
            enumFormulasTable formulasTable, bool rangeInSectionForEntirePeriod = true,
            IEnumerable<IPeriodIndexesTpInSection> rangeIndexesInSectionList = null, double? hiHiSetpoint = null, double? loLoSetpoint = null,
            IEnumerable<IFormulasDisabledPeriod> formulasDisabledPeriods = null, 
            string measureUnitUn = null, PeriodFactory manualEnteredHalfHourIndexes = null)
        {
            var isCalculateBetweenIndexes = !InterpretatorParams.TechProfilePeriod.HasValue && (indxStart > 0 || indxEnd < (InterpretatorParams.NumbersHalfHours - 1));

            var isExistDisablePeriod = formulasDisabledPeriods != null && formulasDisabledPeriods.ToList().Count > 0;

            _recursionCallStack.Clear();

            double? coeff;
            if (string.IsNullOrEmpty(measureUnitUn))
            {
                if (unitDigitCoeff > 1)
                {
                    coeff = 1 / unitDigitCoeff;
                    if (InterpretatorParams.TypeInformation == enumTypeInformation.Power)
                    {
                        coeff *= 2;
                    }
                }
                else if(InterpretatorParams.TypeInformation == enumTypeInformation.Power)
                {
                    coeff = 2;
                }
                else
                {
                    coeff = null;
                }
            }
            else
            {
                coeff = null;
            }

            _formulasTable = formulasTable;

            var formula = GetFormulaByID(formulaId);

            var polskaParams = new PolskaParams(formula.F_ID, _formulasTable, 
                InterpretatorParams.StartDateTime, InterpretatorParams.EndDateTime, InterpretatorParams.DiscreteType, InterpretatorParams.NumbersHalfHours, formula.UnitDigit, InterpretatorParams.TechProfilePeriod.HasValue);

            var parser = ComposeExpressionPolskaya(formula, polskaParams, indxStart, indxEnd, InterpretatorParams.NumbersHalfHours, isCalculateBetweenIndexes);
            parser.Compile();

            //var result = new List<TVALUES_DB>();

            using (var accamulator = new FormulaAccamulator(InterpretatorParams.IntervalTimeList, isSumm, formula.UnitDigit))
            {
                //Считаем всегда по получасовкам
                for (var halfHourIndex = 0; halfHourIndex < InterpretatorParams.NumbersHalfHours; halfHourIndex++)
                {
                    // рассчет по формуле
                    if (!isCalculateBetweenIndexes || halfHourIndex >= indxStart && halfHourIndex <= indxEnd)
                    {
                        #region Ограничиваем диапазоном в сечении

                        if (!rangeInSectionForEntirePeriod)
                        {
                            if (!rangeIndexesInSectionList.Any(range => range.StartIndex <= halfHourIndex && (range.FinishIndex ?? InterpretatorParams.NumbersHalfHours - 1) >= halfHourIndex))
                            {
                                accamulator.Accamulate(0, VALUES_FLAG_DB.TpNotInSectionRange, enumClientFormulaTPType.TpNotInSectionRange);
                                continue;
                            }
                        }

                        #endregion

                        #region Ограничиваем когда формула заблокирована напряму таблицей Info_Formula_DisabledPeriod

                        if (isExistDisablePeriod)
                        {
                            if (formulasDisabledPeriods.Any(range => halfHourIndex >= range.StartIndx && halfHourIndex <= range.FinishIndx))
                            {
                                accamulator.Accamulate(0, VALUES_FLAG_DB.FormulaNotInRange);
                                //result.Add(new Formula_VALUES_DB(VALUES_FLAG_DB.FormulaNotInRange, 0));
                                continue;
                            }
                        }

                        #endregion

                        #region Ограничиваем периодами, когда данные были введены вручную (считать не нужно)

                        if (manualEnteredHalfHourIndexes != null && manualEnteredHalfHourIndexes.HavePeriod(halfHourIndex))
                        {
                            //result.Add(new Formula_VALUES_DB(VALUES_FLAG_DB.None, 0));
                            accamulator.Accamulate(0, VALUES_FLAG_DB.None);
                            continue;
                        }

                        #endregion

                        var val = parser.EvaluateStringValue(halfHourIndex);

                        #region Обрабатываем уставки

                        if (hiHiSetpoint.HasValue && val.F_VALUE >= hiHiSetpoint) val.F_FLAG |= VALUES_FLAG_DB.HiHiSetpointExcess;
                        if (loLoSetpoint.HasValue && val.F_VALUE <= loLoSetpoint) val.F_FLAG |= VALUES_FLAG_DB.LoLoSetpointExcess;

                        #endregion

                        //Учитываем что это мощность или размерность (кило, мега и т.д.)
                        if (coeff.HasValue)
                        {
                            val.F_VALUE *= coeff.Value;
                        }

                        val.FormulaTPType = formulaTPType;

                        accamulator.Accamulate(val.F_VALUE, val.F_FLAG, formulaTPType);
                        //result.Add(val.F_VALUE, val.F_FLAG);
                    }
                    else
                    {
                        accamulator.Accamulate(0, VALUES_FLAG_DB.FormulaNotInRange, enumClientFormulaTPType.NotInRange);
                        //Ограничения по времени действия формулы
                        //result.Add(new Formula_VALUES_DB(VALUES_FLAG_DB.FormulaNotInRange, 0, enumClientFormulaTPType.NotInRange));
                    }
                }

                return accamulator.Result;
            }

            //return result;
        }


        internal FORMULA GetFormulaByID(string f_id)
        {
            f_id = string.IsNullOrEmpty(f_id) ? "_" : f_id.Trim();

            FORMULA f;
            _fExpr.FORMULAS_LIST.TryGetValue(f_id, out f);

            return f;
        }

        /// <summary>
        /// Проверка формулы
        /// </summary>
        /// <param name="formulaParams">формула</param>
        /// <param name="dataSourceType">Источник данных</param>
        public void CheckFormula(IFormulaParam formulaParams)
        {
            _recursionCallStack.Clear();
            int? tpId = null;
            if (formulaParams.tP_CH_ID != null) tpId = formulaParams.tP_CH_ID.TP_ID;
            var formula = GetFormulaByID(formulaParams.FormulaID);

            var polskaParams = new PolskaParams(formula.F_ID, _formulasTable, formulaParams.StartDateTime, formulaParams.FinishDateTime.GetValueOrDefault(), 
                enumTimeDiscreteType.DBHalfHours, 1, null);

            var parser = ComposeExpressionPolskaya(formula, polskaParams, 0, 1, 1, false);
            parser.EvaluateStringValue(0);
        }

        /// <summary>
        /// Набор расчетных параметров для обсчета формулы
        /// </summary>
        /// <param name="formulaParams"></param>
        /// <param name="resultTi"></param>
        /// <param name="resultIntegral"></param>
        /// <param name="resultTp"></param>
        /// <param name="resultSection"></param>
        public void BuildVariableParams(IFormulaParam formulaParams, string msTimeZoneId = null, bool isArchTech = false)
        {
            if (_recursionCallStack.Contains(formulaParams.FormulaID))
                throw new FormulaParseException("Обнаружена рекурсия формулы\n[" + GetOperNameFromDB(formulaParams.FormulaID, F_OPERATOR.F_OPERAND_TYPE.Formula, null) + "]");

            _recursionCallStack.Add(formulaParams.FormulaID);

            _formulasTable = formulaParams.FormulasTable;

            int? tpId = null;
            if (formulaParams.tP_CH_ID != null)
            {
                tpId = formulaParams.tP_CH_ID.TP_ID;
            }

            if (Archives == null)
            {
                Archives = new FormulaArchives(isArchTech, tpId);
            }

            //Часовой пояс в котором запрашиваем данные
            //var tz = (string.IsNullOrEmpty(msTimeZoneId) ? formulaParams.MsTimeZoneId : msTimeZoneId);

            var f = GetFormulaByID(formulaParams.FormulaID);
            if (f == null)
            {
                //Ф-ла не описана, либо не входит в наш диапазон
                _recursionCallStack.Remove(formulaParams.FormulaID);
                return;
            }

            foreach (var operators in f.F_OPERATORS)
            {
                var operIdInt = 0;

                if (string.IsNullOrEmpty(operators.OPER_ID)) continue;

                if (operators.OPER_TYPE != F_OPERATOR.F_OPERAND_TYPE.Formula && operators.OPER_TYPE != F_OPERATOR.F_OPERAND_TYPE.FormulaConstant)
                {
                    if (int.TryParse(operators.OPER_ID, out operIdInt) == false)
                    {
                        throw new FormulaParseException("Идентификатор ТИ [" + operators.OPER_ID + "] должен быть целочисленным!");
                    }

                    if (!operators.TI_CHANNEL.HasValue)
                    {
                        throw new FormulaParseException("В формуле не указан канал измерения!");
                    }
                }

                switch (operators.OPER_TYPE)
                {
                    case F_OPERATOR.F_OPERAND_TYPE.UANode:
                        Archives.FormulaUaNodeVariableDataTypeList.Add(new TUANodeDataId
                        {
                            UANodeId = operIdInt,
                            DataType = (UANodeDataIdDataTypeEnum)operators.TI_CHANNEL.GetValueOrDefault()
                        });
                        break;

                    case F_OPERATOR.F_OPERAND_TYPE.Section:
                        Archives.SectionSorted.Add(new TSectionChannel { Section_ID = operIdInt, ChannelType = operators.TI_CHANNEL.Value });
                        break;
                    case F_OPERATOR.F_OPERAND_TYPE.TP_channel:
                        HashSet<TP_ChanelType> tpChannelTypes;
                        if (!Archives.TPChanelTypeList.TryGetValue(operIdInt, out tpChannelTypes) || tpChannelTypes == null)
                        {
                            Archives.TPChanelTypeList[operIdInt] = tpChannelTypes = new HashSet<TP_ChanelType>(new TP_ChanelComparer());
                        }

                        tpChannelTypes.Add(new TP_ChanelType
                        {
                            TP_ID = operIdInt,
                            ChannelType = operators.TI_CHANNEL.Value,
                            ClosedPeriod_ID = formulaParams.ClosedPeriod_ID,
                        });
                        break;
                    case F_OPERATOR.F_OPERAND_TYPE.Integral_Channel:
                        HashSet<TI_ChanelType> integralChannelTypes;
                        if (!Archives.IntegralChanelTypeList.TryGetValue(operIdInt, out integralChannelTypes) || integralChannelTypes == null)
                        {
                            Archives.IntegralChanelTypeList[operIdInt] = integralChannelTypes = new HashSet<TI_ChanelType>(new ITI_ChanelComparer());
                        }

                        integralChannelTypes.Add(new TI_ChanelType
                        {
                            TI_ID = operIdInt,
                            ChannelType = operators.TI_CHANNEL.Value,
                            IsCA = operators.OPER_TYPE == F_OPERATOR.F_OPERAND_TYPE.ContrTI_Chanel,
                            TP_ID = tpId,
                            DataSourceType = InterpretatorParams.DataSourceType,
                            ClosedPeriod_ID = formulaParams.ClosedPeriod_ID,
                            //MsTimeZone = tz,
                        });
                        break;
                    case F_OPERATOR.F_OPERAND_TYPE.ContrTI_Chanel:
                    case F_OPERATOR.F_OPERAND_TYPE.TI_channel:
                        HashSet<TI_ChanelType> tiChannelTypes;
                        if (!Archives.TIChanelTypeList.TryGetValue(operIdInt, out tiChannelTypes) || tiChannelTypes == null)
                        {
                            Archives.TIChanelTypeList[operIdInt] = tiChannelTypes = new HashSet<TI_ChanelType>(new ITI_ChanelComparer());
                        }

                        tiChannelTypes.Add(new TI_ChanelType
                        {
                            TI_ID = operIdInt,
                            ChannelType = operators.TI_CHANNEL.Value,
                            IsCA = operators.OPER_TYPE == F_OPERATOR.F_OPERAND_TYPE.ContrTI_Chanel,
                            TP_ID = tpId,
                            DataSourceType = InterpretatorParams.DataSourceType,
                            ClosedPeriod_ID = formulaParams.ClosedPeriod_ID,
                            //MsTimeZone = tz,
                        });
                        break;
                    case F_OPERATOR.F_OPERAND_TYPE.FormulaConstant:
                        Archives.FormulaConstantIds.Add(operators.OPER_ID);
                        break;
                    case F_OPERATOR.F_OPERAND_TYPE.Formula:

                        IFormulaParam fId = new FormulaParam
                        {
                            FormulaID = operators.OPER_ID,
                            FormulasTable = _formulasTable,
                            tP_CH_ID = formulaParams.tP_CH_ID,
                            MsTimeZoneId = formulaParams.MsTimeZoneId
                        };

                        //Собираем переменные из вложенной формулы
                        BuildVariableParams(fId, msTimeZoneId);

                        break;
                }
            }

            _recursionCallStack.Remove(formulaParams.FormulaID);
        }

        private Polskaya ComposeExpressionPolskaya(FORMULA f, PolskaParams polskaParams
            , int indxStart, int indxEnd, int numbersHalfHours, bool isCalculateBetweenIndexes = true)
        {
            var parser = new Polskaya(polskaParams, _nameInterface, _formulaArchivesPrecalculator, _variablesDict);

            if (_recursionCallStack.Contains(f.F_ID))
                throw new FormulaParseException("Обнаружена рекурсия формулы\n[" + GetOperNameFromDB(f.F_ID, F_OPERATOR.F_OPERAND_TYPE.Formula, null) + "]");
            _recursionCallStack.Add(f.F_ID);

            foreach (F_OPERATOR operators in f.F_OPERATORS)
            {
                switch (operators.OPER_TYPE)
                {
                    case F_OPERATOR.F_OPERAND_TYPE.Section:
                    case F_OPERATOR.F_OPERAND_TYPE.TP_channel:
                    case F_OPERATOR.F_OPERAND_TYPE.ContrTI_Chanel:
                    case F_OPERATOR.F_OPERAND_TYPE.Integral_Channel:
                    case F_OPERATOR.F_OPERAND_TYPE.TI_channel:
                    case F_OPERATOR.F_OPERAND_TYPE.FormulaConstant:
                    case F_OPERATOR.F_OPERAND_TYPE.UANode:

                        if (!parser.ContainsVariable(operators.Name))
                        {
                            Variable var;
                            if (Archives == null)
                            {
                                // режим поиска параметров и проверки правильности
                                var = new Variable(operators.Name, indxStart, indxEnd, isCalculateBetweenIndexes: isCalculateBetweenIndexes);
                            }
                            else 
                            {
                                IGetAchives data = Archives.GetArchiveByOperandType(operators);
                                if (data == null && !Archives.IsArchTech)
                                {
                                    if (operators.OPER_TYPE == F_OPERATOR.F_OPERAND_TYPE.TP_channel)
                                    {
                                        throw new FormulaParseException("Расчет формулы невозможен. Не найдено значение для ТП \"" +
                                                                GetOperNameFromDB(operators.OPER_ID, operators.OPER_TYPE, operators.TI_CHANNEL) + "\"\nНе описан канал или не найдена для него формула.");
                                    }

                                    throw new FormulaParseException("Расчет формулы невозможен. Отсутствуют значения для \"" +
                                                            GetOperNameFromDB(operators.OPER_ID, operators.OPER_TYPE, operators.TI_CHANNEL) + "\"\n из формулы <" + GetOperNameFromDB(f.F_ID, F_OPERATOR.F_OPERAND_TYPE.Formula, null) + ">");

                                }

                                var = new Variable(operators.Name, indxStart, indxEnd, data, operators.TI_CHANNEL, isCalculateBetweenIndexes);
                            }

                            parser.CreateVariable(var);
                        }

                        parser.Expression.Append(operators.PRE_OPERANDS ?? "").Append(operators.Name).Append(operators.AFTER_OPERANDS ?? "");
                        break;
                    case F_OPERATOR.F_OPERAND_TYPE.Formula:
                        parser.Expression.Append(operators.PRE_OPERANDS ?? "").Append(operators.Name).Append(operators.AFTER_OPERANDS ?? "");
                        if (!parser.ContainsVariable(operators.Name))
                        {
                            var innerFId = GetFormulaByID(operators.OPER_ID);
                            if (innerFId != null)
                            {
                                var p = new PolskaParams(innerFId.F_ID, _formulasTable, polskaParams.StartDateTime, polskaParams.EndDateTime, polskaParams.DiscreteType,
                                    numbersHalfHours, innerFId.UnitDigit);
                                var formulaExpression = ComposeExpressionPolskaya(innerFId, p, indxStart, indxEnd, numbersHalfHours, isCalculateBetweenIndexes);

                                //Время дейстаия внутренней формулы берем из основной
                                var var = new Variable(operators.Name, indxStart, indxEnd, formulaExpression, isCalculateBetweenIndexes);
                                parser.CreateVariable(var);
                            }
                            else
                            {
                                parser.CreateVariable(new Variable(operators.Name, -1, -1));
                            }
                        }
                        break;
                    case F_OPERATOR.F_OPERAND_TYPE.Constanta:
                        parser.Expression.Append(operators.PRE_OPERANDS).Append(operators.AFTER_OPERANDS ?? "");
                        break;
                    case F_OPERATOR.F_OPERAND_TYPE.None:
                        if (string.IsNullOrEmpty(operators.PRE_OPERANDS) && string.IsNullOrEmpty(operators.AFTER_OPERANDS)) throw new FormulaParseException("Формула не описана!");
                        parser.Expression.Append(operators.PRE_OPERANDS ?? "").Append(operators.AFTER_OPERANDS ?? "");
                        break;
                    default:
                        throw new FormulaParseException("Неизвестный тип оператора!");
                }
            }

            _recursionCallStack.Remove(f.F_ID);

            return parser;
        }
        
        private string GetOperNameFromDB(string Oper_ID, F_OPERATOR.F_OPERAND_TYPE Oper_type, byte? Channel)
        {
            if (_nameInterface != null)
            {
                return _nameInterface.GetObjectName(Oper_ID, Oper_type, Channel);
            }

            return Oper_ID;
        }

        public void Dispose()
        {
            Archives = null;
        }
    }
    internal class InternalRecordIndexPointer
    {
        public int Index = 0;
    }

    internal class InternalRecordIndexPointerTech
    {
        public DateTime EventDateTime;
        public int Index = 0;
    }

    internal class FormulaInterpreterVariable
    {
        public double Value;
        public byte ChType;
        public string Ti_id;
        public F_OPERATOR.F_OPERAND_TYPE Oper_type;

        public FormulaInterpreterVariable(string Ti_id, byte ChType, F_OPERATOR.F_OPERAND_TYPE Oper_type)
        {
            this.ChType = ChType;
            this.Ti_id = Ti_id;
            this.Value = 0;
            this.Oper_type = Oper_type;

        }
    }
}
