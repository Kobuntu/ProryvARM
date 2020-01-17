using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Internal.Utils;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation;
using Proryv.AskueARM2.Server.DBAccess.Public.ConsumptionSchedule;
using Proryv.AskueARM2.Server.WCF;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils.Data;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;
using Proryv.Servers.Calculation.DBAccess.Common;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Utils
{
    public partial class ParallelBDHelper
    {
        public TDRParams Param;
        public enumTypeInformation IsPower;

        public StringBuilder Errors = new StringBuilder();

        /// <summary>
        /// Результат
        /// </summary>
        public readonly object ArchivesValue30OrHour;

        #region Конструкторы

        public ParallelBDHelper()
        {
        }


        /// <summary>
        /// Для поканального опроса
        /// </summary>
        /// <param name="ti_ChanelType_List">список ТИ с каналами</param>
        /// <param name="param">Параметры запроса</param>
        /// <param name="isPower">Энергия или мощность</param>
        public ParallelBDHelper(List<TI_ChanelType> ti_ChanelType_List, TDRParams param, enumTypeInformation isPower)
        {
            Param = param;
            IsPower = isPower;
            param.IsConsumptionScheduleAnalyse = param.IsConsumptionScheduleAnalyse && param.ConsumptionScheduleDict != null && param.ConsumptionScheduleDict.Count > 0;
            var archivesValue30OrHour = new ConcurrentStack<TArchivesValue>();

            try
            {
                //Parallel.ForEach(Partitioner.Create(0, ti_ChanelType_List.Count, Settings.MaxTableParams)
                //    , new ParallelOptions
                //    {
                //        MaxDegreeOfParallelism = 4,
                //        CancellationToken = param.CancellationToken ?? CancellationToken.None
                //    }
                //    , range => GetArchivesListWithCA(Param, archivesValue30OrHour, ti_ChanelType_List.GetRange(range.Item1, range.Item2 - range.Item1)));

                GetArchivesListWithCA(Param, archivesValue30OrHour, ti_ChanelType_List);

                //Округление работает только по получасовкам
                if (param.RecalculatorResiudes != null)
                {
                    //Перерасчет остатков для ТИ у которых они небыли найдены, сохранение посуточных остатков в базе
                    param.RecalculatorResiudes.RecalculateResiudes(param, GetArchivesListWithCA);
                }
            }
            catch (OperationCanceledException)
            {
                Errors.Append("Операция была отменена\r\n");
            }

            ArchivesValue30OrHour = archivesValue30OrHour;
        }

        /// <summary>
        /// Для достоверности
        /// </summary>
        /// <param name="TI_List"></param>
        /// <param name="param"></param>
        public ParallelBDHelper(List<TI_ChanelType> TI_List, TDRParams param, bool useInactiveChannel)
        {
            Param = param;
            IsPower = enumTypeInformation.Energy;

            var archivesValues = new ConcurrentStack<TArchivesValue>();
            var archivesValue30OrHour = new ConcurrentStack<TArchivesValidate>();

            var ct = param.CancellationToken ?? CancellationToken.None;

            try
            {
                Parallel.ForEach(Partitioner.Create(0, TI_List.Count, Settings.MaxTableParams)
                    , new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 4,
                        CancellationToken = ct
                    }
                    , range => GetArchivesListWithCA(param, archivesValues, TI_List.GetRange(range.Item1, range.Item2 - range.Item1), useInactiveChannel));

                archivesValues
                    .AsParallel()
                    .WithCancellation(ct)
                    .ForAll((Action<TArchivesValue>)((archivesValue) =>
                    {
                        //Основные данные
                        var validateValue = new TValidate
                        {
                            TI_Ch_ID = archivesValue.TI_Ch_ID as TI_ChanelType,
                            PS_ID = archivesValue.PS_ID,
                            F_FLAG_List = archivesValue.Val_List.ToValidate(),
                            Total_Flag = archivesValue.TotalFlag.AccomulateTotalStatistic(),
                            FlagByOtherDataSource = archivesValue.FlagByOtherDataSource,
                        };
                        //Достоверности по остальным источникам

                        //данные по КА
                        TValidate contrValidateValue = null;
                        if (archivesValue.ContrAgents != null)
                        {
                            contrValidateValue = new TValidate
                            {
                                TI_Ch_ID = new TI_ChanelType
                                {
                                    TI_ID = archivesValue.ContrAgents.ContrTI_ID.GetValueOrDefault(),
                                    ChannelType = archivesValue.TI_Ch_ID.ChannelType,
                                    IsCA = true,
                                },
                                F_FLAG_List = archivesValue.ContrAgents.Val_List.ToValidate(),
                                Total_Flag = archivesValue.ContrAgents.TotalFlag,
                            };
                        }

                        //Данные по ОВ
                        List<TOV_Validate> ovValidateList = new List<TOV_Validate>();
                        if (archivesValue.OV_Values_List != null && archivesValue.OV_Values_List.Count > 0)
                        {
                            foreach (var tovArchiveValue in archivesValue.OV_Values_List)
                            {
                                var ovValidateValue = new TOV_Validate()
                                {
                                    PS_ID = validateValue.PS_ID,
                                    TI_Ch_ID =
                                        new TI_ChanelType()
                                        {
                                            TI_ID = tovArchiveValue.TI_ID,
                                            ChannelType = archivesValue.TI_Ch_ID.ChannelType,
                                            IsCA = archivesValue.TI_Ch_ID.IsCA
                                        },
                                    DTStart = tovArchiveValue.DTServerStart,
                                    DTEnd = tovArchiveValue.DTServerEnd,
                                };

                                ovValidateValue.F_FLAG_List = MyComparer.ToValidate(tovArchiveValue.Val_List);
                                ovValidateValue.Total_Flag = tovArchiveValue.TotalFlag;
                                ovValidateList.Add(ovValidateValue);
                            }
                        }

                        var validateByTIValue = new TArchivesValidate()
                        {
                            TIType = archivesValue.TIType,
                            IsSmallTI = archivesValue.IsSmallTI,
                            TI_Validate = validateValue,
                            ContrTI_Validate = contrValidateValue,
                            OV_Validate = ovValidateList,
                            
                        };

                        archivesValue30OrHour.Push(validateByTIValue);
                    }));

            }
            catch (OperationCanceledException)
            {

            }

            ArchivesValue30OrHour = archivesValue30OrHour;

        }

        /// <summary>
        /// Для опроса по иерархии
        /// </summary>
        /// <param name="TI_List"></param>
        /// <param name="param"></param>
        /// <param name="ID_list"></param>
        public ParallelBDHelper(List<IHierarchyChannelID> TI_List, TDRParams param, SortedSet<ID_Hierarchy> ID_list)
        {
            Param = param;
            IsPower = enumTypeInformation.Energy;

            var archivesValues = new ConcurrentStack<TArchivesValue>();
            var archivesValue = new ConcurrentDictionary<ID_Hierarchy, Dictionary<byte, List<TVALUES_DB>>>(new ID_Hierarchy_EqualityComparer());

            try
            {
                var ct = param.CancellationToken ?? CancellationToken.None;

                Parallel.ForEach(Partitioner.Create(0, TI_List.Count, Settings.MaxTableParams)
                    , new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 4,
                        CancellationToken = ct
                    }
                    , range =>
                    {
                        var tiArray = TI_List.GetRange(range.Item1, range.Item2 - range.Item1)
                            .Select(ti => new TI_ChanelType()
                            {
                                TI_ID = int.Parse(ti.ID),
                                ChannelType = ti.Channel,
                                IsCA = ti.TypeHierarchy == enumTypeHierarchy.Info_ContrTI,
                            })
                            .ToList();

                        GetArchivesListWithCA(param, archivesValues, tiArray); //Запрашиваем архивные значения
                    });

                foreach (var archivesValueGroupBySide in archivesValues.GroupBy(g => g.TI_Ch_ID.IsCA))
                {
                    var typeHierarchy = archivesValueGroupBySide.Key ? enumTypeHierarchy.Info_ContrTI : enumTypeHierarchy.Info_TI;
                    archivesValueGroupBySide.GroupBy(g => g.TI_Ch_ID.TI_ID)
                        .AsParallel()
                        .WithCancellation(ct)
                        .ForAll((archivesGroupById) =>
                        {
                            var id = new ID_Hierarchy(typeHierarchy, archivesGroupById.Key.ToString(CultureInfo.InvariantCulture));
                            var archivesDict = new Dictionary<byte, List<TVALUES_DB>>();

                            foreach (var valueByChannel in archivesGroupById)
                            {
                                archivesDict[valueByChannel.TI_Ch_ID.ChannelType] = valueByChannel.Val_List;
                            }

                            archivesValue.TryAdd(id, archivesDict);
                        });
                }
            }
            catch (OperationCanceledException)
            {

            }

            ArchivesValue30OrHour = archivesValue;
        }


        #endregion

        #region Работаем с базой данных

        /// <summary>
        /// Добавляем список архивных значение в общую структуру архивных значение (эта процедура работает и с КА)
        /// </summary>
        /// <param name="TI_Array">Строка для SQL процедуры. Перечисляем через ; структуры ТИ+тип канала между ТИ и типом канала разделитель ,</param>
        /// <param name="dTStart">Время начала выборки</param>
        /// <param name="dTEnd">Время конца выборки</param>
        /// <param name="isCoeffEnabled">Наличие коэффициента трансформации</param>
        /// <param name="TypeArchTable">Тип таблицы для запроса</param>
        /// <param name="tiList">Список (ТИ+канал ТИ)</param>
        /// <param name="IsCAReverse">Брать значения для КА (каналы) как обычно, инвертированно (false), или напрямую (true)</param>
        /// <param name="archivesValue30OrHour">Архивы с получасовками, которые наполняем</param>
        /// <returns></returns>
        private void GetArchivesListWithCA(TDRParams param, ConcurrentStack<TArchivesValue> archivesValue30OrHour, List<TI_ChanelType> tiArray, bool useInactiveChannel = false)
        {
            #region Формируем данные по ТИ

            try
            {
                var tiForFormulasComparer = new TiChanelNoTimeZoneNoDataSourceNoTpEqualityComparer();
                var formulaToTiDict = new Dictionary<TI_ChanelType, List<FormulaToTiCalculateInfo>>(tiForFormulasComparer);
                var formulasForTiIds = new List<TFormulaParam>();

                //Округление работает только по получасовкам и часовкам
                var isNeedResiduesTable = param.RoundData 
                    && param.IsNotRecalculateResiudes
                    && (param.DiscreteType == enumTimeDiscreteType.DBHalfHours 
                    ||  param.DiscreteType == enumTimeDiscreteType.DBHours);

#if DEBUG
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
#endif

                using (var cnctns = new System.Data.SqlClient.SqlConnection(Settings.DbConnectionString))
                using (var cmd = cnctns.CreateCommand())
                {
                    cmd.CommandText = "usp2_ArchComm_ReadArrayWithCA";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandTimeout = 600;
                    cmd.Parameters.AddWithValue("@DateStart", param.DtServerStart);
                    cmd.Parameters.AddWithValue("@DateEnd", param.DtServerEnd);
                    cmd.Parameters.AddWithValue("@isCoeffEnabled", param.IsCoeffEnabled);
                    cmd.Parameters.AddWithValue("@IsCAEnabled", param.isCAEnabled);
                    cmd.Parameters.AddWithValue("@IsOVEnabled", !param.OvMode.CheckFlag(enumOVMode.IsOVDisabled));
                    cmd.Parameters.AddWithValue("@isOVIntervalEnabled",
                        !param.OvMode.CheckFlag(enumOVMode.IsOVIntervalWithoutTI_Disabled) &&
                        !param.OvMode.CheckFlag(enumOVMode.IsOVIntervalWithTI_Disabled));

                    cmd.Parameters.AddWithValue("@IsReadCalculatedValues", param.IsReadCalculatedValues);
                    cmd.Parameters.AddWithValue("@IsValidateOtherDataSource", param.IsValidateOtherDataSource);

                    cmd.Parameters.AddWithValue("@IsNeedWsTable", param.IsNeedWsTable);

                    cmd.Parameters.AddWithValue("@IsNeedResiduesTable", isNeedResiduesTable);
                    cmd.Parameters.AddWithValue("@HalfHoursShiftClientFromServer",
                        param.HalfHoursShiftClientFromServer);
                    cmd.Parameters.AddWithValue("@UseInactiveChannel", useInactiveChannel);
                    cmd.Parameters.AddWithValue("@UseLossesCoefficient", param.UseLossesCoefficient);

                    cmd.Parameters.AddWithValue("@UseActUndercount", param.UseActUndercount); 

                    var userDefinedTypeTable = SQLTableTypeAdapter<TI_ChanelType>.ArchiveValuesToDataTable(tiArray);
                    //Таблица с ТИ и каналами
                    cmd.Parameters.AddWithValue("@TIArray", userDefinedTypeTable);


                    //Список идентификаторов актов недоучета, которые исключаем из чтения (для модуля ручного ввода по акту недоучета)
                    if (param.ExcludedActUndercountUns != null && param.ExcludedActUndercountUns.Count > 0)
                    {
                        cmd.Parameters.AddWithValue("@excludedActUndercountUns",
                            string.Join(",", param.ExcludedActUndercountUns));
                    }

                    cmd.Parameters.AddWithValue("@isReturnPreviousDispatchDateTime",
                        param.IsReturnPreviousDispatchDateTime);

                    cnctns.Open();

                    //Читаем данные
                    using (var dr = cmd.ExecuteReader())
                    {
                        //Словарь приоритетов выставленный вручную по ТИ, затем по ТП, затем по номеру месяца
                        Dictionary<int, Dictionary<int, Dictionary<Guid, Dictionary<int, DBDataSourceToTiTp>>>> dataSourceTiTpByTiByMonthNumber = null;
                        Dictionary<int, Dictionary<Guid, List<IPeriodCoeff>>> coeffTransformationDict = null;
                        Dictionary<int, List<IPeriodStatus>> transformationDisabledDict = null;
                        var dataSourceIdToTypeDict = new Dictionary<int, EnumDataSourceType>();
                        var priorityList = new List<DBPriorityList>();
                        Dictionary<int, List<DBResiduesTable>> resiudesDict = null;
                        Dictionary<int, List<TLossesCoefficient>> lossesCoefficientsDict = null;

                        #region Привязка ТИ к формулам

                        PeriodFactory manualEnteredHalfHourIndexes = null;

                        if (param.IsReadCalculatedValues)
                        {
                            if (dr.HasRows)
                            {
                                var tiIndx = dr.GetOrdinal("TI_ID");
                                var channelIndx = dr.GetOrdinal("ChannelType");
                                var formulaIdIndx = dr.GetOrdinal("Formula_UN");
                                var startIndx = dr.GetOrdinal("StartDateTime");
                                var endIndex = dr.GetOrdinal("FinishDateTime");

                                manualEnteredHalfHourIndexes = new PeriodFactory();
                                var formulaToTiInfos = new List<FormulaToTiCalculateInfo>();

                                while (dr.Read())
                                {
                                    var formulaUn = dr[formulaIdIndx] as string;
                                    var dtStart = dr.GetDateTime(startIndx);
                                    var dtEnd = dr[endIndex] as DateTime?;

                                    formulaToTiInfos.Add(new FormulaToTiCalculateInfo
                                    {
                                        TiIdChannel = new TI_ChanelType
                                        {
                                            TI_ID = dr.GetInt32(tiIndx),
                                            ChannelType = dr.GetByte(channelIndx),
                                        },
                                        Formula_UN = formulaUn,
                                        StartDateTime = dtStart,
                                        FinishDateTime = dtEnd,
                                    });

                                    formulasForTiIds.Add(new TFormulaParam
                                    {
                                        FormulaID = formulaUn,
                                        FormulasTable = enumFormulasTable.Info_Formula_Description,
                                        IsFormulaHasCorrectDescription = true,
                                        StartDateTime = dtStart,
                                        FinishDateTime = dtEnd,
                                        ManualEnteredHalfHourIndexes = manualEnteredHalfHourIndexes,
                                    });
                                }

                                formulaToTiDict = formulaToTiInfos
                                    .GroupBy(g => g.TiIdChannel, tiForFormulasComparer)
                                    .ToDictionary(k => k.Key, v => v.ToList(), tiForFormulasComparer);
                            }

                            dr.NextResult();
                        }

                        #endregion

                        #region Таблица с коэфф. потерь для ТИ

                        if (param.UseLossesCoefficient)
                        {
                            if (dr.HasRows)
                            {
                                var tiIndx = dr.GetOrdinal("TI_ID");
                                var startDtIndx = dr.GetOrdinal("StartDateTime");
                                var endDtIndx = dr.GetOrdinal("FinishDateTime");
                                var lossesCoefficientIndx = dr.GetOrdinal("LossesCoefficient");

                                lossesCoefficientsDict = new Dictionary<int, List<TLossesCoefficient>>();
                                List<TLossesCoefficient> lossesCoefficients = null;
                                var prevTiId = -1;

                                while (dr.Read())
                                {
                                    var tiId = dr.GetInt32(tiIndx);
                                    if (tiId != prevTiId)
                                    {
                                        lossesCoefficients = new List<TLossesCoefficient>();
                                        lossesCoefficientsDict[tiId] = lossesCoefficients;

                                        prevTiId = tiId;
                                    }

                                    lossesCoefficients.Add(new TLossesCoefficient
                                    {
                                        TI_ID = tiId,
                                        StartDateTime = dr.GetDateTime(startDtIndx),
                                        FinishDateTime = dr[endDtIndx] as DateTime?,
                                        LossesCoefficient = dr.GetDouble(lossesCoefficientIndx),
                                    });
                                }
                            }

                            dr.NextResult();
                        }

                        #endregion

                        #region Выборка округлений 80020

                        if (isNeedResiduesTable)
                        {
                            if (dr.HasRows)
                            {
                                var tiIndx = dr.GetOrdinal("TI_ID");
                                var eventDateIndx = dr.GetOrdinal("EventDate");
                                var chInd = dr.GetOrdinal("ChannelType");
                                var dsIndx = dr.GetOrdinal("DataSource_ID");
                                var vIndx = dr.GetOrdinal("VAL");
                                var ldIndx = dr.GetOrdinal("LatestDispatchDateTime");

                                var residuesTable = new List<DBResiduesTable>();
                                while (dr.Read())
                                {
                                    var v = dr[vIndx] as double?;
                                    if (!v.HasValue) continue;

                                    residuesTable.Add(new DBResiduesTable
                                    {
                                        TI_ID = dr.GetInt32(tiIndx),
                                        EventDate = dr.GetDateTime(eventDateIndx),
                                        ChannelType = dr.GetByte(chInd),
                                        DataSourceType =
                                            Internal.Extention.CreateToEnumDataSourceType((byte) dr.GetInt32(dsIndx)),
                                        Val = v.Value,
                                        LatestDispatchDateTime = dr.GetDateTime(ldIndx),
                                    });
                                }

                                resiudesDict = residuesTable
                                    .GroupBy(r => r.TI_ID)
                                    .ToDictionary(k => k.Key, v => v.ToList());
                            }

                            dr.NextResult();
                        }


                        #endregion

                        #region Выборка связи ТИ-ТП-приоритетный источник

                        if (dr.HasRows)
                        {
                            int tiIndx = dr.GetOrdinal("TI_ID");
                            int tpIndx = dr.GetOrdinal("TP_ID");
                            int sourceIdIndx = dr.GetOrdinal("DataSource_ID");
                            int numberIndx = dr.GetOrdinal("MonthNumber");
                            int closedPeriodIndex = dr.GetOrdinal("ClosedPeriod_ID");

                            var dataSourceTiTpByTiList = new List<DBDataSourceToTiTp>();
                            while (dr.Read())
                            {
                                dataSourceTiTpByTiList.Add(new DBDataSourceToTiTp()
                                {
                                    DataSource_ID = dr.GetInt32(sourceIdIndx),
                                    TI_ID = dr.GetInt32(tiIndx),
                                    TP_ID = dr.GetInt32(tpIndx),
                                    MonthNumber = dr.GetInt32(numberIndx),
                                    ClosedPeriod_ID = dr[closedPeriodIndex] as Guid?,
                                });
                            }

                            dataSourceTiTpByTiByMonthNumber = dataSourceTiTpByTiList
                                .GroupBy(d => d.TI_ID) //Группировка по ТИ
                                .ToDictionary(d => d.Key
                                    , v => v.GroupBy(g => g.TP_ID) //По ТП
                                        .ToDictionary(k => k.Key, vv => vv
                                            .GroupBy(gg => gg.ClosedPeriod_ID.GetValueOrDefault()) //По закрытию периода
                                            .ToDictionary(k => k.Key, vvv => vvv
                                                .GroupBy(gg => gg.MonthNumber) //По периоду
                                                .ToDictionary(kk => kk.Key, vvvv => vvvv.FirstOrDefault()))));
                        }

                        #endregion

                        dr.NextResult();

                        #region Соотношения идентификаторов и типов источников

                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                EnumDataSourceType sourceType =
                                    Internal.Extention.CreateToEnumDataSourceType(dr.GetByte(1));
                                if (Enum.IsDefined(typeof(EnumDataSourceType), sourceType))
                                {
                                    dataSourceIdToTypeDict.Add(dr.GetInt32(0), sourceType);
                                }
                            }
                        }

                        #endregion

                        dr.NextResult();

                        #region Словарь приоритетов

                        if (dr.HasRows)
                        {
                            int numberIndx = dr.GetOrdinal("MonthNumber");
                            int sourceIdIndx = dr.GetOrdinal("DataSource_ID");
                            int priorityIndx = dr.GetOrdinal("Priority");
                            int closedPeriodIndex = dr.GetOrdinal("ClosedPeriod_ID");

                            while (dr.Read())
                            {
                                priorityList.Add(new DBPriorityList()
                                {
                                    DataSource_ID = dr.GetInt32(sourceIdIndx),
                                    MonthNumber = dr.GetInt32(numberIndx),
                                    Priority = dr.GetInt32(priorityIndx),
                                    ClosedPeriod_ID = dr[closedPeriodIndex] as Guid?,
                                });
                            }
                        }

                        #endregion

                        #region Чтение коэфф. трансформации

                        if (param.IsCoeffEnabled)
                        {
                            dr.NextResult();
                            coeffTransformationDict = ReadCoeffTransformation(dr);

                            //Периоды когда коэфф. заблокирован
                            dr.NextResult();
                            transformationDisabledDict = ReadTransformationDisabled(dr);
                        }

                        #endregion

                        dr.NextResult();

                        dr.Read();

                        //Читаем количество ПУ
                        var puNumbers = (int) dr["puNumbers"];
                        dr.NextResult();

                        //Здесь читаем таблицу отмены зимнего времени 26.10.2014 г.
                        List<ArchBit_30_Values_WS> wsList = null;
                        if (param.IsNeedWsTable)
                        {
                            wsList = ReadWsTable(dr);
                            dr.NextResult();
                        }

                        #region Перебираем все ТИ в списке

                        //Пишем данные в промежуточную структуру
                        for (int i = 0; i < puNumbers; i++)
                        {
                            if (param.CancellationToken.HasValue &&
                                param.CancellationToken.Value.IsCancellationRequested)
                            {
                                cmd.Cancel();
                                return;
                            }

                            dr.Read();
                            //Guid? closedPeriod_ID = dr[4] as Guid?;
                            //Идентификатор ПУ
                            if (dr.HasRows)
                            {
                                List<Tuple<int, int?>> notWorkedRange;
                                var av = ReadTiParams(dr, out notWorkedRange, param.ClientTimeZoneId);
                                var id = av.TI_Ch_ID;

                                //Собираем индексы получасовок введенных вручную
                                av.ManualEnteredHalfHourIndexes = manualEnteredHalfHourIndexes;
                                
                                #region Коэфф. трансформации

                                List<IPeriodCoeff> coeffTransformationList = null;
                                Dictionary<Guid, List<IPeriodCoeff>> coeffByClosedPeriod;
                                if (coeffTransformationDict != null &&
                                    coeffTransformationDict.TryGetValue(av.TI_Ch_ID.TI_ID, out coeffByClosedPeriod) && coeffByClosedPeriod != null)
                                {
                                    coeffByClosedPeriod.TryGetValue(av.TI_Ch_ID.ClosedPeriod_ID.GetValueOrDefault(),
                                        out coeffTransformationList);
                                }

                                if (coeffTransformationList == null)
                                {
                                    coeffTransformationList = new List<IPeriodCoeff>
                                    {
                                        new TPeriodCoeff
                                        {
                                            PeriodValue = 1,
                                            StartDateTime = param.DtServerStart,
                                            FinishDateTime = param.DtServerEnd,
                                        }
                                    };
                                }

                                av.CoeffTransformations = coeffTransformationList;

                                #endregion

                                #region Периоды отключения коэфф. трансформации

                                List<IPeriodStatus> сoeffTransformationDisabledPeriods = null;
                                if (transformationDisabledDict != null &&
                                    transformationDisabledDict.TryGetValue(av.TI_Ch_ID.TI_ID,
                                        out сoeffTransformationDisabledPeriods))
                                {
                                    av.CoeffTransformationDisabledPeriods = сoeffTransformationDisabledPeriods;
                                }

                                #endregion

                                #region Читаем получасовки, накапливаем в запрошенный период дискретизации

                                if (useInactiveChannel || !av.IsChannelAbsent)
                                {
                                    dr.NextResult();
                                    Dictionary<int, DBDataSourceToTiTp> dataSourceTiTpByMonthNumber = null;
                                    if (id.TP_ID.HasValue && dataSourceTiTpByTiByMonthNumber != null)
                                    {
                                        Dictionary<int, Dictionary<Guid, Dictionary<int, DBDataSourceToTiTp>>> dataSourceTiTpByTp;
                                        if (
                                            dataSourceTiTpByTiByMonthNumber.TryGetValue(id.TI_ID,
                                                out dataSourceTiTpByTp) &&
                                            dataSourceTiTpByTp != null)
                                        {
                                            Dictionary<Guid, Dictionary<int, DBDataSourceToTiTp>>
                                                dataSourceTiTpByClosedPeriod;
                                            if (
                                                dataSourceTiTpByTp.TryGetValue(id.TP_ID.Value,
                                                    out dataSourceTiTpByClosedPeriod) &&
                                                dataSourceTiTpByClosedPeriod != null)
                                            {
                                                dataSourceTiTpByClosedPeriod.TryGetValue(
                                                    id.ClosedPeriod_ID.GetValueOrDefault(),
                                                    out dataSourceTiTpByMonthNumber);
                                            }
                                        }
                                    }

                                    Dictionary<int, List<DBPriorityList>> totalPriorityByMonthNumber = null;
                                    if (priorityList.Count > 0)
                                    {
                                        totalPriorityByMonthNumber = priorityList
                                            .Where(
                                                p =>
                                                    p.ClosedPeriod_ID.GetValueOrDefault() ==
                                                    id.ClosedPeriod_ID.GetValueOrDefault())
                                            .GroupBy(g => g.MonthNumber)
                                            .ToDictionary(k => k.Key,
                                                v => v.OrderByDescending(p => p.Priority).ToList());
                                    }

                                    List<TLossesCoefficient> lossesCoefficients = null;

                                    if (lossesCoefficientsDict != null)
                                    {
                                        lossesCoefficientsDict.TryGetValue(av.TI_Ch_ID.TI_ID,
                                            out lossesCoefficients);
                                    }

                                    GetArchivesValues(av, dr, param, Errors, dataSourceTiTpByMonthNumber,
                                        totalPriorityByMonthNumber, dataSourceIdToTypeDict, notWorkedRange, wsList,
                                        lossesCoefficients);

                                    #region расчет остатоков от деления 

                                    if (param.RecalculatorResiudes != null)
                                    {
                                        DBResiduesTable prevDayResiude = null;
                                        List<DBResiduesTable> tiResidues = null; //список остатков за предыдущие сутки по данной ТИ
                                        if (resiudesDict != null && resiudesDict.TryGetValue(id.TI_ID, out tiResidues) && tiResidues != null)
                                        {
                                            prevDayResiude = tiResidues.FirstOrDefault(r => r.ChannelType == av.TI_Ch_ID.ChannelType && r.DataSourceType == av.DataSourceType);
                                        }

                                        param.RecalculatorResiudes.CalculateResiudes(av, prevDayResiude,
                                            useLossesCoefficient: param.UseLossesCoefficient,
                                            isCalculateAnyway: param.IsNotRecalculateResiudes, isNotRecalculateResiudes: param.IsNotRecalculateResiudes);
                                    }

                                    #endregion

                                    archivesValue30OrHour.Push(av);
                                }
                                else if (param.IsReadAbsentChannel)
                                {
                                    av.Val_List = new List<TVALUES_DB>();
                                    var mask = VALUES_FLAG_DB.IsNull; //флагов не хватает помечаем этим флагом
                                    if (av.IsReadCalculatedValues)
                                    {
                                        mask |= VALUES_FLAG_DB.РучнойВвод;
                                    }

                                    var exceptedHalfhoursCalculator = new ExceptedHalfhoursCalculator(param.DtServerStart, param.ClientTimeZoneId,
                                        null, dataSourceIdToTypeDict, null,
                                        false, notWorkedRange, param.IntervalTimeList, param.UnitDigit);

                                    var valuesFlagDb = av.TotalFlag;
                                    exceptedHalfhoursCalculator.FillVoidHalfHours(av.Val_List, param.DiscreteType, VALUES_FLAG_DB.None, ref valuesFlagDb);

                                    if (av.IsValidateOtherDataSource)
                                    {
                                        mask |= VALUES_FLAG_DB.DataNotFull;
                                        av.FlagByOtherDataSource = new List<TValidateFlagDataSource>();

                                        foreach (var dataSourceType in DBStatic.AllDataSource)
                                        {
                                            av.FlagByOtherDataSource.Add(new TValidateFlagDataSource(dataSourceType)
                                            {
                                                Flag = mask,
                                            });
                                        }
                                    }

                                    archivesValue30OrHour.Push(av);
                                }

                                #endregion

                                dr.NextResult();
                            }
                        }

                        #endregion
                    }
                }

#if DEBUG
                sw.Stop();
                Console.WriteLine("Запрашиваем данные по ТИ {0} млс", sw.ElapsedMilliseconds);
#endif

                #region Обсчитываем ТИ, которые считаются по формулам

                if (formulasForTiIds.Count > 0)
                {
#if DEBUG
                    sw.Restart();
#endif
                    //!!!Считаем индексы получасовок замещенных вручную, затем пропускаем индексы из формул

                    var result = new FormulasResult(formulasForTiIds,
                        param.DtServerStart.ServerToClient(param.ClientTimeZoneId),
                        param.DtServerEnd.ServerToClient(param.ClientTimeZoneId),
                        param.DiscreteType, null, 0
                        , param.IsCoeffEnabled, false, param.IsPower, false, param.UnitDigit, false,
                        enumOVMode.NormalMode, param.ClientTimeZoneId, false, param.IsReadCalculatedValues,
                        param.RoundData);

                    if (result.Errors != null && result.Errors.Length > 0)
                    {
                        lock (Errors)
                        {
                            Errors.Append(result.Errors);
                        }
                    }

                    foreach (var ftti in formulaToTiDict)
                    {
                        var tiIdChannel = ftti.Key;
                        var formulaToTiInfos = ftti.Value;

                        var tiVals = archivesValue30OrHour.FirstOrDefault(a =>
                            a.TI_Ch_ID.TI_ID == tiIdChannel.TI_ID && a.TI_Ch_ID.ChannelType == tiIdChannel.ChannelType);

                        if (tiVals == null) continue;

                        var isFirst = true;

                        foreach (var formulaToTiInfo in formulaToTiInfos.OrderBy(f => f.StartDateTime))
                        {
                            var fVals = result.Result_Values.FirstOrDefault(fr =>
                                string.Equals(fr.Formula_UN, formulaToTiInfo.Formula_UN));

                            if (fVals == null || fVals.Result_Values == null) continue;

                            var mainFormulaResult = fVals.Result_Values.FirstOrDefault();
                            if (mainFormulaResult==null || mainFormulaResult.Val_List == null) continue;

                            if (isFirst)
                            {
                                tiVals.MeasureUnit_UN = mainFormulaResult.MeasureUnit_UN;
                                isFirst = false;
                            }

                            //Складываем с основным значением
                            for (var i = 0; i < mainFormulaResult.Val_List.Count; i++)
                            {
                                var tVal = tiVals.Val_List.ElementAtOrDefault(i);
                                if (tVal == null) continue;

                                var fVal = mainFormulaResult.Val_List[i];

                                tVal.F_VALUE += fVal.F_VALUE;
                                //Сбрасываем аварийные статусы, берем из формулы
                                tVal.F_FLAG = (tVal.F_FLAG & ~VALUES_FLAG_DB.AllAlarmStatuses).CompareAndReturnMostBadStatus(fVal.F_FLAG & ~VALUES_FLAG_DB.FormulaNotInRange);
                            }

                            //for(var hhVal )
                        }
                    }

#if DEBUG
                    sw.Stop();
                    Console.WriteLine("Дорасчитываем ТИ по формулам {0} млс", sw.ElapsedMilliseconds);
#endif
                }

                #endregion
            }
            catch (Exception ex)
            {
                lock (Errors)
                {
                    Errors.AppendException(ex);
                }
            }

            #endregion
        }


        /// <summary>
        /// Основная процедура чтения архивных значений
        /// </summary>
        public static void GetArchivesValues(TArchivesValue av, System.Data.SqlClient.SqlDataReader dr, TDRParams param, StringBuilder errors,
            Dictionary<int, DBDataSourceToTiTp> dataSourceTiTpByMonthNumber,
            Dictionary<int, List<DBPriorityList>> totalPriorityByMonthNumber
            , Dictionary<int, EnumDataSourceType> dataSourceIdToTypeDict, List<Tuple<int, int?>> notWorkedRange, List<ArchBit_30_Values_WS> wsList
            , List<TLossesCoefficient> lossesCoefficients)
        {
            //Заполняем значениями
            var dateTimeOv = new List<TDateTimeOV>();

            //Точка имеет некомерческий статус
            var tiId = new ID_IsOurSide
            {
                IsOurSide = !av.TI_Ch_ID.IsCA
            };

            #region Обработки логики в случае если точка ОВ

            if (av.IsOV)
            {
                try
                {
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            tiId.ID = dr.IsDBNull(1) ? 0 : dr.GetInt32(1); //Точка которую он замещает
                            if (tiId.ID > 0)
                            {
                                av.IsOVModeEnabled = true; //Реально замещали какую то точку

                                int j;
                                int l;
                                if (dr.IsDBNull(2) || dr.IsDBNull(3))
                                {
                                    j = -1;
                                    l = -1;
                                }
                                else
                                {
                                    var dtStart = dr.GetDateTime(2).RoundToHalfHour(true);
                                    var dtFinish = dr.GetDateTime(3).AddMinutes(1).RoundToHalfHour(false);

                                    if (dtStart < param.DtServerStart)
                                    {
                                        j = 0;
                                    }
                                    else
                                    {
                                        j = Convert.ToInt32(
                                            (dtStart.ServerToUtc() - param.DtServerStart.ServerToUtc()).TotalMinutes /
                                            30);
                                    }

                                    l = Convert.ToInt32((dtFinish.ServerToUtc() - param.DtServerStart.ServerToUtc())
                                                        .TotalMinutes / 30);
                                }

                                dateTimeOv.Add(new TDateTimeOV
                                {
                                    TI_ID = tiId.ID,
                                    StartIndx = j,
                                    EndIndx = l,
                                    ValuesByIndexes = new Dictionary<int, Servers.Calculation.DBAccess.Interface.Data.TVALUES_DB>()
                                });
                            }
                        }
                    }
                }
                catch
                {

                }

                dr.NextResult();
            }

            #endregion

            ConsumptionScheduleTypesByTI consumptionSchedule = null;

            //Смотрим нужен ли анализ потребления по графику, анализ нужно делать только при соблюдении всех условий
            var isConsumptionScheduleAnalyse = param.IsConsumptionScheduleAnalyse
                                                && param.ConsumptionScheduleDict != null
                                                && param.ConsumptionScheduleDict.TryGetValue(av.TI_Ch_ID.TI_ID, out consumptionSchedule)
                                                && consumptionSchedule != null
                                                && !consumptionSchedule.IsAnalysComplete
                                                && consumptionSchedule.ConsumptionScheduleValues != null
                                                && consumptionSchedule.ConsumptionScheduleValues.Count > 0;

            //Индексы когда данная ТИ участвует в обсчете ТП
            List<PeriodIndexesTpInSection> rangeTiIndexesInSectionList = null;
            if (param.RangeTiIndexesInSectionList!=null)
            {
                param.RangeTiIndexesInSectionList.TryGetValue(av.TI_Ch_ID as TI_ChanelType, out rangeTiIndexesInSectionList);
                if (rangeTiIndexesInSectionList!=null && av.TI_Ch_ID.TP_ID > 0)
                {
                    rangeTiIndexesInSectionList.RemoveAll(r => r.TpId != av.TI_Ch_ID.TP_ID);
                }
            }

            //Наполняем 30 минутки
            av.FlagByOtherDataSource = new List<TValidateFlagDataSource>();
            
            var previousDayDispatchDateTimeHistory = new Dictionary<DateTime, TPreviousDayDispatchDateTimeHistory>();

            if (av.IsOV || !param.OvMode.CheckFlag(enumOVMode.OnlyOv))
            {
                ArchiveValuesFactory.GetDatareaderValues(av, dr, param, dateTimeOv, isConsumptionScheduleAnalyse,
                    consumptionSchedule,
                    av.TI_Ch_ID.DataSourceType, errors, dataSourceTiTpByMonthNumber, totalPriorityByMonthNumber,
                    dataSourceIdToTypeDict,
                    notWorkedRange,
                    wsList == null
                        ? null
                        : wsList.Where(ws => ws.TI_ID == av.TI_Ch_ID.TI_ID
                                             && ws.ChannelType == av.TI_Ch_ID.ChannelType),
                    rangeTiIndexesInSectionList,
                    lossesCoefficients, previousDayDispatchDateTimeHistory, param.CalculateMinAndMax);
            }
            else 
            {
                //Пропускаяем значения самой ТИ, только значения ОВ
                if (param.IsReadCalculatedValues && !av.IsCA && param.UseActUndercount)
                {
                    dr.NextResult();
                    dr.NextResult();
                }

                //dr.NextResult();
                //dr.NextResult();

                av.Val_List = new List<TVALUES_DB>();

                foreach (var numbersHalfHoursInOurPeriod in param.IntervalTimeList)
                {
                    for (var hhIndex = 0; hhIndex <= numbersHalfHoursInOurPeriod; hhIndex++)
                    {
                        av.Val_List.Add(new Servers.Calculation.DBAccess.Interface.Data.TVALUES_DB(VALUES_FLAG_DB.None, 0));
                    }
                }
            }

            av.PreviousDayDispatchDateTimeHistory = previousDayDispatchDateTimeHistory;

            if (dateTimeOv.Count > 0)
            {
                av.TiByOvReplaced = dateTimeOv
                    .GroupBy(g => g.TI_ID)
                    .ToDictionary(k => k.Key, v => v.FirstOrDefault());

                dateTimeOv.Clear();
            }

            #region Данные по контр. агенту

            if (param.isCAEnabled && !av.TI_Ch_ID.IsCA)
            {
                dr.NextResult();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        if (!dr.IsDBNull(1))
                        {
                            av.ContrAgents = new TContrAgent
                            {
                                ConterTPCalcCoef = dr.GetDouble(2),
                                ContrTI_ID = dr.IsDBNull(1) ? (int?) null : dr.GetInt32(1),
                                Val_List = new List<TVALUES_DB>()
                            };
                        }
                    }
                }
                dr.NextResult();

                //Берем значения контрагента
                var ca = av.ContrAgents as TContrAgent;

                if (ca != null)
                {
                    if (av.PowerValue != null)
                    {
                        ca.PowerValue = new TDetailPowerValue();
                    }

                    ca.IsReadCalculatedValues = av.IsReadCalculatedValues;
                    ca.IsCoeffEnabled = av.IsCoeffEnabled;
                    ca.IsClosedPeriod = av.IsClosedPeriod;
                    ca.TIType = av.TIType;

                    ca.FlagByOtherDataSource = new List<TValidateFlagDataSource>();

                    ArchiveValuesFactory.GetDatareaderValues(ca, dr, param, dateTimeOv, false, null, null, errors, 
                        dataSourceTiTpByMonthNumber, totalPriorityByMonthNumber, dataSourceIdToTypeDict);
                }
            }

            #endregion

            #region Данные по обходным выключателям

            if (!param.OvMode.CheckFlag(enumOVMode.IsOVDisabled))
            {

                dr.NextResult();
                if (dr.HasRows)
                {
                    var isCoeffTransformationDisabledIndex = dr.GetOrdinal("IsCoeffTransformationDisabled");
                    var idIndex = dr.GetOrdinal("OV_ID");
                    var startDateTimeIndex = dr.GetOrdinal("StartDateTime");
                    var finishDateTimeIndex = dr.GetOrdinal("FinishDateTime");
                    var tiTypeIndex = dr.GetOrdinal("TIType");
                    var tpCoefIndex = dr.GetOrdinal("TPCoef");
                    var isCaIndex = dr.GetOrdinal("IsCA");
                    var serialIndex = dr.GetOrdinal("MeterSerialNumber");
                    var nameIndex = dr.GetOrdinal("TIName");


                    av.OV_Values_List = new List<TOV_Values>();
                    while (dr.Read())
                    {
                        if (dr.IsDBNull(1)) continue;

                        var dtStart = dr.GetDateTime(startDateTimeIndex);
                        var dtEnd = dr.GetDateTime(finishDateTimeIndex);

                        av.OV_Values_List.Add(new TOV_Values
                        {
                            TI_ID = dr.GetInt32(idIndex),
                            DTServerStart = dtStart.RoundToHalfHour(true),
                            DTServerEnd = dtEnd.RoundToHalfHour(true),

                            NoRoundedStart = dtStart,
                            NoRoundedEnd = dtEnd,

                            IsOurSide = !dr.GetBoolean(isCaIndex),
                            TP_Coef = DBValues.DBDouble(dr, tpCoefIndex),
                            Val_List = new List<TVALUES_DB>(),
                            IsCoeffTransformationDisabled = dr.GetBoolean(isCoeffTransformationDisabledIndex),
                            TIType = (enumTIType) dr.GetByte(tiTypeIndex),
                            MeterSerialNumber = dr.GetString(serialIndex),
                            Name = dr.GetString(nameIndex),
                            UnitDigit = param.UnitDigit,
                        });
                    }
                }


                //Берем значения  по обходным выключателям
                if (av.OV_Values_List != null)
                {
                    av.OVRangeDict = new Dictionary<int, List<TIntPair>>();
                    for (var i = 0; i < av.OV_Values_List.Count; i++)
                    {
                        var ov = av.OV_Values_List[i] as TOV_Values;

                        //Определяем некоммерческий ли статус у точки (т.е. это КА и он замещает сам себя)

                        bool isNotCommercStatus = (av.TI_Ch_ID.IsCA && av.TI_Ch_ID.TI_ID == ov.TI_ID);

                        av.IsNotCommercStatus = isNotCommercStatus;
                        DateTime ovStart = ov.DTServerStart,
                            ovEnd = ov.DTServerEnd;
                        var intervalTimeList = MyListConverters.GetIntervalTimeList(ovStart.ServerToClient(param.ClientTimeZoneId), ovEnd.ServerToClient(param.ClientTimeZoneId),
                            param.DiscreteType, param.ClientTimeZoneId);

                        //if (param.DiscreteType == enumTimeDiscreteType.DBHours && ovEnd.Minute == 0 && intervalTimeList.Count > 0) intervalTimeList[intervalTimeList.Count - 1] = 0;

                        dr.NextResult();

                        List<IPeriodCoeff> coeffTransformations = null;
                        List<IPeriodStatus> сoeffTransformationDisabledPeriods = null;

                        //Чтение коэфф. трансформации
                        if (param.IsCoeffEnabled)
                        {
                            var coeffTransformationDict = ReadCoeffTransformation(dr);
                            Dictionary<Guid, List<IPeriodCoeff>> coeffByClosedPeriod;
                            if (coeffTransformationDict.TryGetValue(ov.TI_ID, out coeffByClosedPeriod) && coeffByClosedPeriod != null)
                            {
                                coeffByClosedPeriod.TryGetValue(av.TI_Ch_ID.ClosedPeriod_ID.GetValueOrDefault(), out coeffTransformations);
                            }
                            
                            dr.NextResult();

                            var transformationDisabledDict = ReadTransformationDisabled(dr);
                            if (transformationDisabledDict != null)
                            {
                                transformationDisabledDict.TryGetValue(ov.TI_ID, out сoeffTransformationDisabledPeriods);
                            }

                            dr.NextResult();
                        }

                        ov.FlagByOtherDataSource = new List<TValidateFlagDataSource>();
                        ov.IsReadCalculatedValues = av.IsReadCalculatedValues;
                        ov.IsCoeffEnabled = av.IsCoeffEnabled;
                        ov.IsClosedPeriod = av.IsClosedPeriod;
                        ov.TIType = av.TIType;
                        ov.CoeffTransformations = coeffTransformations;
                        ov.CoeffTransformationDisabledPeriods = сoeffTransformationDisabledPeriods;
                        ov.IsCA = av.IsCA;
                        ov.PowerValue = av.PowerValue;

                        if (param.DiscreteType == enumTimeDiscreteType.DBHours && ovEnd.Minute == 0)
                        {
                            ovEnd = ovEnd.AddMinutes(30);
                        }

                        ArchiveValuesFactory.GetDatareaderValues(ov, dr, param, dateTimeOv, false,
                            null, av.TI_Ch_ID.DataSourceType, errors, dataSourceTiTpByMonthNumber, totalPriorityByMonthNumber, 
                            dataSourceIdToTypeDict, wsByTiChArchives: wsList == null
                                ? null
                                : wsList.Where(ws => ws.TI_ID == ov.TI_ID
                                                     && ws.ChannelType == av.TI_Ch_ID.ChannelType),
                            ovStart: ovStart, ovEnd: ovEnd, ovIntervalTimes: intervalTimeList);
                        //Если суммируем с основными значениями только если ТИ не входит в общий список точек
                        tiId.ID = ov.TI_ID;
                        
                        if (!param.OvMode.CheckFlag(enumOVMode.IsOVSummDisabled) //Если не заблокировано суммирование
                            && (!param.OvMode.CheckFlag(enumOVMode.IsOVSummDisabled_OnlyIf_TIListContainOV)
                                || (param.OvMode.CheckFlag(enumOVMode.IsOVSummDisabled_OnlyIf_TIListContainOV) //Суммируем с ОВ если ОВ не общем списке точек
                                    && !param.TIs.Contains(tiId, new ID_IsOurSide_EqualityComparer())))
                            )
                        {
                            AccomulateOV(param, av, ov, isNotCommercStatus, i, av.OVRangeDict);
                        }
                        else if (isNotCommercStatus && (param.OvMode.CheckFlag(enumOVMode.IsOVDisabled) || param.OvMode.CheckFlag(enumOVMode.IsOVSummDisabled_OnlyIf_TIListContainOV)))
                        {
                            AccomulateOV(param, av, ov, isNotCommercStatus, i, av.OVRangeDict);
                        }
                    }
                }
            }

            #endregion
        }

        private static void AccomulateOV(TDRParams param, TArchivesValue av, TOV_Values ov, bool isNotCommercStatus, 
            int indxOV, IDictionary<int, List<TIntPair>> ovRangeDict)
        {
            int j;

            if (param.DiscreteType == enumTimeDiscreteType.DBInterval)
            {
                //Если сразу за весь интервал
                if (av.Val_List.Count > 0 && ov.Val_List.Count > 0)
                {
                    var ovVal = ov.Val_List[0];
                    var aVal = av.Val_List[0];

                    if (isNotCommercStatus)
                    {

                        aVal.F_VALUE = av.Val_List[0].F_VALUE - ovVal.F_VALUE;
                        if (aVal.F_VALUE < 0) aVal.F_VALUE = 0;
                    }
                    else
                    {
                        aVal.F_VALUE = aVal.F_VALUE + ovVal.F_VALUE;
                        aVal.F_FLAG = ((aVal.F_FLAG & (~VALUES_FLAG_DB.DataNotFull)).CompareAndReturnMostBadStatus(ovVal.F_FLAG) | VALUES_FLAG_DB.IsOVReplaced);
                    }
                }

                List<TIntPair> pairList;
                if (!ovRangeDict.TryGetValue(0, out pairList) || pairList == null)
                {
                    pairList = new List<TIntPair>();
                }

                pairList.Add(new TIntPair(indxOV, 0));

                ovRangeDict[0] = pairList;
            }
            else
            {
                //Если часовки, получасовки, сутки
                //j = Convert.ToInt32(Math.Floor((ov.DTStart.CorrectToServer(param.timeZone, param.isSummerOrWinter) - param.dTStart).TotalMinutes / ((Convert.ToInt32(param.discreteType) + 1) * 30)));
                j = MyListConverters.GetNumbersValuesInPeriod(param.DiscreteType, param.DtServerStart.ServerToClient(param.ClientTimeZoneId),
                    ov.DTServerStart.ServerToClient(param.ClientTimeZoneId), param.ClientTimeZoneId) - 1;

                //if (j < 0) j = 0;
                for (int i = 0; i < ov.Val_List.Count; i++)
                {
                    var vals = ov.Val_List[i];

                    var aVal = av.Val_List.ElementAtOrDefault(j);
                    if (aVal == null) break;

                    aVal.F_FLAG = ((aVal.F_FLAG.CompareAndReturnMostBadStatus(vals.F_FLAG) & (~VALUES_FLAG_DB.DataNotFull)) | VALUES_FLAG_DB.IsOVReplaced) & (~VALUES_FLAG_DB.DataNotFull);
                    aVal.F_VALUE = isNotCommercStatus ? 0 : aVal.F_VALUE + vals.F_VALUE;

                    List<TIntPair> pairList;
                    if (!ovRangeDict.TryGetValue(j, out pairList) || pairList == null)
                    {
                        pairList = new List<TIntPair>();
                    }

                    pairList.Add(new TIntPair(indxOV, i));

                    ovRangeDict[j] = pairList;

                    j++;
                }
            }
        }



        private static Dictionary<int, Dictionary<Guid, List<IPeriodCoeff>>> ReadCoeffTransformation(SqlDataReader dr)
        {
            var dbRequest = new List<Tuple<int, Guid?, IPeriodCoeff>>();

            if (dr.HasRows)
            {
                var coeffIndx = dr.GetOrdinal("Coeff");
                var startIndx = dr.GetOrdinal("StartDateTime");
                var endIndx = dr.GetOrdinal("FinishDateTime");
                var tiIndx = dr.GetOrdinal("ti_id");
                var closedPeriodIndex = dr.GetOrdinal("ClosedPeriod_ID");

                //Из базы возвращается все отсортировано по ТИ
                while (dr.Read())
                {
                    dbRequest.Add(new Tuple<int, Guid?, IPeriodCoeff>(dr.GetInt32(tiIndx), dr[closedPeriodIndex] as Guid?, new TPeriodCoeff
                    {
                        PeriodValue = dr[coeffIndx] as double?,
                        StartDateTime = DBValues.DBDateTime(dr, startIndx),
                        FinishDateTime = DBValues.DBDateTime(dr, endIndx),
                    }));
                }
            }

            return dbRequest
                .GroupBy(g => g.Item1)
                .ToDictionary(k => k.Key, v => v.GroupBy(g => g.Item2.GetValueOrDefault())
                    .ToDictionary(k => k.Key, vv => vv.Select(vvv => vvv.Item3).ToList()));
        }

        private static List<ArchBit_30_Values_WS> ReadWsTable(SqlDataReader dr)
        {
            var result = new List<ArchBit_30_Values_WS>();
            if (!dr.HasRows) return result;

            int tiIndx = dr.GetOrdinal("TI_ID");
            int eventDateIndx = dr.GetOrdinal("EventDate");
            int channelTypeIndx = dr.GetOrdinal("ChannelType");
            int dataSourceIdIndx = dr.GetOrdinal("DataSourceType");
            int val01Indx = dr.GetOrdinal("VAL_01");
            int val02Indx = dr.GetOrdinal("VAL_02");
            int val03Indx = dr.GetOrdinal("VAL_03");
            int val04Indx = dr.GetOrdinal("VAL_04");
            int cal01Indx = dr.GetOrdinal("CAL_01");
            int cal02Indx = dr.GetOrdinal("CAL_02");
            int cal03Indx = dr.GetOrdinal("CAL_03");
            int cal04Indx = dr.GetOrdinal("CAL_04");
            int validStatusIndx = dr.GetOrdinal("ValidStatus");
            int dispatchDateTimeIndx = dr.GetOrdinal("DispatchDateTime");
            int statusIndx = dr.GetOrdinal("Status");


            //Из базы возвращается все отсортировано по ТИ
            while (dr.Read())
            {
                result.Add(new ArchBit_30_Values_WS((int) dr[tiIndx], (DateTime) dr[eventDateIndx], (byte) dr[channelTypeIndx], Internal.Extention.ToEnumDataSourceType((byte) dr[dataSourceIdIndx]),
                    dr[val01Indx] as double?, dr[val02Indx] as double?, dr[val03Indx] as double?, dr[val04Indx] as double?,
                    dr[cal01Indx] as double?, dr[cal02Indx] as double?, dr[cal03Indx] as double?, dr[cal04Indx] as double?,
                    (long) dr[validStatusIndx], (DateTime) dr[dispatchDateTimeIndx], dr[statusIndx] as int?));
            }

            return result;
        }

        private static TArchivesValue ReadTiParams(SqlDataReader dr, out List<Tuple<int, int?>> notWorkedRange, string msTimeZone)
        {
            //проверка на корректность типа
            EnumDataSourceType? dataSourceType = null;
            var dataSource = dr["DataSourceType"] as byte?;
            if (dataSource.HasValue)
            {
                dataSourceType = Internal.Extention.CreateToEnumDataSourceType(dataSource.Value);
                if (!Enum.IsDefined(typeof(EnumDataSourceType), dataSourceType.Value))
                {
                    dataSourceType = null;
                }
            }

            var tiType = enumTIType.Unknown;
            var tt = dr["TIType"] as byte?;
            if (tt.HasValue)
            {
                tiType = Internal.Extention.ToEnumTIType.Invoke(tt.Value);
                //Параметры точки
                if (!Enum.IsDefined(typeof(enumTIType), tiType))
                {
                    tiType = enumTIType.Unknown;
                }
            }

            var av = new TArchivesValue
            {
                TI_Ch_ID = new TI_ChanelType
                {
                    TI_ID = (int) dr["TI_ID"],
                    ChannelType = (byte) dr["ChannelType"],
                    IsCA = (bool) dr["IsCA"],
                    TP_ID = dr["TP_ID"] as int?,
                    ClosedPeriod_ID = dr["ClosedPeriod_ID"] as Guid?,
                    MsTimeZone = msTimeZone,
                    DataSourceType = dataSourceType,
                    //IANA_TimeZoneName = dr["IANA_TimeZoneName"] as string,
                },
                IsOV = (bool) dr["IsOV"],
                IsChannelAbsent = (bool)dr["IsAbsentChannel"],
                TIType = tiType,
                IsSmallTI = dr["IsSmallTI"] as bool?,
                PS_ID = (int) dr["PS_ID"],
                TP_Coef = (double) dr["TPCoef"],
                IsValidateOtherDataSource = (bool) dr["IsValidateOtherDataSource"],
                IsReadCalculatedValues = (bool) dr["IsReadCalculatedValues"],
                IsCoeffEnabled = (bool) dr["isCoeffEnabled"], //&& !(bool) dr["isCoeffTransformationDisabled"]
                IsCoeffTransformationDisabled = (bool)dr["isCoeffTransformationDisabled"],
                IsClosedPeriod = !DBNull.Value.Equals(dr["ClosedPeriod_ID"]),
                MeasureUnit_UN = dr["MeasureUnit_UN"] as string,
                PowerValue = new TDetailPowerValue(),
            };
            
            var val = dr["NotWorkedPeriod"] as string;
            notWorkedRange = new List<Tuple<int, int?>>();
            var rangeArray = val.Split(DBStatic.DelimerDt, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();
            if (rangeArray.Count > 0)
            {
                notWorkedRange.AddRange(DBStatic.MakeRangeDateTimeToIndexes(rangeArray));
            }

            return av;
        }

        /// <summary>
        /// Словарь, когда коэфф. трансформации был заблокирован
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private static Dictionary<int, List<IPeriodStatus>> ReadTransformationDisabled(SqlDataReader dr)
        {
            var dbRequest = new List<Tuple<int, IPeriodStatus>>();

            if (dr.HasRows)
            {
                var coeffIndx = dr.GetOrdinal("IsCoeffTransformationDisabled");
                var startIndx = dr.GetOrdinal("StartDateTime");
                var endIndx = dr.GetOrdinal("FinishDateTime");
                var tiIndx = dr.GetOrdinal("ti_id");

                //Из базы возвращается все отсортировано по ТИ
                while (dr.Read())
                {
                    dbRequest.Add(new Tuple<int, IPeriodStatus>(dr.GetInt32(tiIndx), new TPeriodStatus
                    {
                        PeriodValue = dr[coeffIndx] as bool?,
                        StartDateTime = DBValues.DBDateTime(dr, startIndx),
                        FinishDateTime = DBValues.DBDateTime(dr, endIndx),
                    }));
                }
            }

            return dbRequest
                .GroupBy(g => g.Item1)
                .ToDictionary(k => k.Key, vv => vv.Select(vvv => vvv.Item2).ToList());
        }
    }
}
#endregion