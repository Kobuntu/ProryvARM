using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.WCF;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Utils
{
    public class RecalculatorResiudes
    {
        /// <summary>
        /// Количество месяцев для перерасчета остатков
        /// </summary>
        public static byte MonthForRecalculateResidues
        {
            get { return Settings.MonthForRecalculateResidues; }
        }

        /// <summary>
        /// Делать перерасчет остатков всегда, за указанное количество месяцев
        /// </summary>
        public static bool IsRecalculateResiduesAlways
        {
            get { return Settings.IsRecalculateResiduesAlways; }
        }

        /// <summary>
        /// Пересчитанные остатки, их надо в самом конце сохранить в базе
        /// </summary>
        public readonly ConcurrentStack<DBResiduesTable> TiForResaveResiudes;

        /// <summary>
        /// Архивы у которых не найдены остатки в предыдущих сутках, для них надо пересчитать остатки
        /// </summary>
        private readonly ConcurrentStack<TArchivesValue> _tiForRecalculateResiudes;

        /// <summary>
        /// Количество получасовок по суткам
        /// </summary>
        private readonly List<int> _resiudes24Hindexes;

        /// <summary>
        /// Начало по времени клиента
        /// </summary>
        private readonly DateTime _dtStartClient;

        /// <summary>
        /// Начало по времени сервера
        /// </summary>
        private readonly DateTime _dtStartServer;

        /// <summary>
        /// Запрошенные ед. измерения
        /// </summary>
        private readonly EnumUnitDigit _unitDigit;

        /// <summary>
        /// Запрошенные ед. измерения
        /// </summary>
        private readonly enumTimeDiscreteType _discreteType;


        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="dtStartClien">Начало по времени клиента</param>
        /// <param name="dtEndClient">Окончание по времени клиента</param>
        /// <param name="clientTimeZoneId">Идентификатор часового пояса клиента</param>
        /// <param name="unitDigit">Запрошенные ед. измерения</param>
        /// <param name="discreteType"></param>
        public RecalculatorResiudes(DateTime dtStartClien, DateTime dtEndClient, string clientTimeZoneId, EnumUnitDigit unitDigit, enumTimeDiscreteType discreteType)
        {
            _tiForRecalculateResiudes = new ConcurrentStack<TArchivesValue>();
            TiForResaveResiudes = new ConcurrentStack<DBResiduesTable>();
            _dtStartClient = dtStartClien;
            _dtStartServer = _dtStartClient.ClientToServer(clientTimeZoneId);
            _unitDigit = unitDigit;
            _discreteType = discreteType;
            _resiudes24Hindexes = MyListConverters.GetIntervalTimeList(dtStartClien, dtEndClient, enumTimeDiscreteType.DB24Hour, clientTimeZoneId);
        }


        /// <summary>
        /// Округление и формирование остатков по суткам для сохранения
        /// </summary>
        /// <param name="av">ТИ</param>
        /// <param name="tiResidues">Остатки от округления с предыдущих суток</param>
        /// <returns></returns>
        public void CalculateResiudes(TArchivesValue av, DBResiduesTable prevDayResiude,
            bool isCalculateAnyway = false, bool useLossesCoefficient = false, bool isNotRecalculateResiudes = false)
        {
            if (av.Val_List == null || av.Val_List.Count == 0) return;

            if (!isCalculateAnyway || (!IsRecalculateResiduesAlways && prevDayResiude == null))
            {
                _tiForRecalculateResiudes.Push(av);
                return;
            }

            var k = 1000.0 / (double)_unitDigit; //Округляем до кВт

            var reminder = 0.0;

            //Остаток от предыдущего периода
            if (prevDayResiude != null) reminder = prevDayResiude.Val / 1000.0;

            var indxHalfHour = 0;
            var indx24H = 0;

            var numbersInDay = _resiudes24Hindexes[indx24H];

            //для часовых интервалов (80040) (в _resiudes24Hindexes - кол-во получасовых интервалов)
            if (_discreteType == enumTimeDiscreteType.DBHours)
            {
                numbersInDay = Convert.ToInt32(Math.Floor(numbersInDay / 2.0));
            }

            //Пересчитываем с первой получасовки
            foreach (var tvaluesDb in av.Val_List)
            {
                var value = tvaluesDb.F_VALUE / k;
                var newValue = value + reminder;
                value = newValue < 0 ? 0 : Math.Round(newValue, MidpointRounding.AwayFromZero);

                tvaluesDb.F_VALUE = value * k;
                reminder = Math.Round(newValue - value, 3, MidpointRounding.AwayFromZero); //Здесь нужно округлять с большей точностью чем 3, иначе накапливается ошибка

                #region Формируем коллекцию для сохранения расчитанных остатков

                //if (!isNotRecalculateResiudes)
                {
                    //переход на следующие сутки
                    if (++indxHalfHour > numbersInDay)
                    {
                        indxHalfHour = 0;
                        numbersInDay = _resiudes24Hindexes.ElementAtOrDefault(++indx24H);
                        //для часовых интервалов (80040)
                        if (_discreteType == enumTimeDiscreteType.DBHours)
                        {
                            numbersInDay = Convert.ToInt32(Math.Floor(numbersInDay / 2.0));
                        }


                        TiForResaveResiudes.Push(new DBResiduesTable
                        {
                            EventDate = _dtStartServer.Date.AddDays(indx24H - 1),
                            ChannelType = av.TI_Ch_ID.ChannelType,
                            TI_ID = av.TI_Ch_ID.TI_ID,
                            DataSourceType = av.DataSourceType,
                            LatestDispatchDateTime = DateTime.Now,
                            Val = reminder * 1000.0,
                            UseLossesCoefficient = useLossesCoefficient,
                        });
                    }
                }

                #endregion
            }
        }


        public void RecalculateResiudes(TDRParams param, Action<TDRParams, ConcurrentStack<TArchivesValue>, List<TI_ChanelType>, bool> getArchivesAndCalculateResiudesFromMonthStartAction)
        {
            if (getArchivesAndCalculateResiudesFromMonthStartAction == null) return;

#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            if (_tiForRecalculateResiudes != null && _tiForRecalculateResiudes.Count > 0)
            {
                //Перерасчет ТИ по которым небыло найдено остатков в предыдущем дне
                var tiArray = _tiForRecalculateResiudes
                    .Select(t => new TI_ChanelType
                    {
                        TI_ID = t.TI_Ch_ID.TI_ID,
                        ChannelType = t.TI_Ch_ID.ChannelType,
                        ClosedPeriod_ID = t.TI_Ch_ID.ClosedPeriod_ID,
                        DataSourceType = t.DataSourceType,
                        IsCA = t.TI_Ch_ID.IsCA,
                        MsTimeZone = t.TI_Ch_ID.MsTimeZone,
                        TP_ID = t.TI_Ch_ID.TP_ID,
                    })
                    .ToList();
                var archivesValue30OrHour = new ConcurrentStack<TArchivesValue>();

                //Дата/время с которого пересчитываем
                var dtStartMonthClient = param.DtServerStart.AddMonths(-MonthForRecalculateResidues);
                dtStartMonthClient = new DateTime(dtStartMonthClient.Year, dtStartMonthClient.Month, 1);

                var newParam = new TDRParams(param.IsCoeffEnabled, param.isCAEnabled, param.OvMode, param.isCAReverse,
                    enumTimeDiscreteType.DBHalfHours, param.UnitDigit,
                    param.IsPower, param.TIs, dtStartMonthClient.ClientToServer(param.ClientTimeZoneId),
                    param.DtServerStart.AddMinutes(-30),
                    MyListConverters.GetIntervalTimeList(dtStartMonthClient,
                        param.DtServerStart.AddMinutes(-30).ServerToClient(param.ClientTimeZoneId),
                        enumTimeDiscreteType.DBHalfHours, param.ClientTimeZoneId), param.ClientTimeZoneId,
                    param.IsValidateOtherDataSource, param.IsReadCalculatedValues, param.IsReadAbsentChannel,
                    param.RoundData)
                { IsNotRecalculateResiudes = true };

                //Запрос данных с начала месяца
                getArchivesAndCalculateResiudesFromMonthStartAction(newParam, archivesValue30OrHour, tiArray, false);

#if DEBUG
                sw.Stop();
                Console.WriteLine("Запрос архивов для перерасчета - > ПУ: {0} шт, время: {1} млс", tiArray.Count, sw.ElapsedMilliseconds);
                sw.Restart();
#endif

                //if (newParam.RecalculatorResiudes != null && newParam.RecalculatorResiudes.TiForResaveResiudes.Count > 0)
                //{
                //TiForResaveResiudes.PushRange(newParam.RecalculatorResiudes.TiForResaveResiudes.ToArray());
                //}

                var prev24HEventDate = param.DtServerStart.AddMinutes(-30);

                Dictionary<int, List<DBResiduesTable>> resiudesDict = null;
                if (newParam.RecalculatorResiudes != null &&
                    newParam.RecalculatorResiudes.TiForResaveResiudes != null)
                {
                    resiudesDict = newParam.RecalculatorResiudes.TiForResaveResiudes
                        .GroupBy(r => r.TI_ID)
                        .ToDictionary(k => k.Key, v => v.ToList());
                }

                //Дорасчитываем остатки по ТИ у которых их нет в предыдущих сутках
                foreach (var av in _tiForRecalculateResiudes)
                {
                    List<DBResiduesTable> resiudesTi;
                    DBResiduesTable prevDayResiude = null;
                    if (resiudesDict != null && resiudesDict.TryGetValue(av.TI_Ch_ID.TI_ID, out resiudesTi) && resiudesTi != null)
                    {
                        prevDayResiude = resiudesTi
                            .FirstOrDefault(r => r.ChannelType == av.TI_Ch_ID.ChannelType &&
                                                 r.EventDate < prev24HEventDate && //Коллеция идет от Stack поэтому дата, время идет по убывающей
                                                 r.DataSourceType == av.DataSourceType);
                    }

                    CalculateResiudes(av, prevDayResiude, true, param.UseLossesCoefficient, isNotRecalculateResiudes: param.IsNotRecalculateResiudes);
                }

#if DEBUG
                sw.Stop();
                Console.WriteLine("Расчет остатков - > {0} млс", sw.ElapsedMilliseconds);
                sw.Restart();
#endif
            }

            //Запускаем сохранение остатков и выходим
            if (TiForResaveResiudes.Count > 0) Task.Factory.StartNew(() => SaveResiudes(TiForResaveResiudes, param.HalfHoursShiftClientFromServer, param.IsReadCalculatedValues));

#if DEBUG
            sw.Stop();
            Console.WriteLine("Сохранение остатков - > {0} млс", sw.ElapsedMilliseconds);
#endif
        }

        private void SaveResiudes(IEnumerable<DBResiduesTable> tiForResaveResiudes, int halfHoursShiftFromUtc, bool isReadCalculatedValues)
        {
            try
            {
                //Параметры процедуры
                var otherParams = new List<Tuple<string, object>>
                {
                    new Tuple<string, object>("@HalfHoursShiftFromUTC", halfHoursShiftFromUtc),
                    new Tuple<string, object>("@IsReadCalculatedValues", isReadCalculatedValues),
                };

                var sqlAdapter = new SQLTableTypeAdapter<DBResiduesTable>(tiForResaveResiudes);
                sqlAdapter.WriteTableToSQL("usp2_WriteExpDocResidues", "@ResiduesTable", otherParams);
            }
            catch
            {
            }
        }
    }

    /// <summary>
    /// Вспомогательный класс для чтения и сохранения остатков в БД
    /// </summary>
    public class DBResiduesTable
    {
        public int TI_ID { get; set; }
        public DateTime EventDate { get; set; }
        public byte ChannelType { get; set; }
        public EnumDataSourceType DataSourceType;
        public int DataSource_ID
        {
            get { return (int)DataSourceType; }
        }
        public double Val { get; set; }
        public DateTime LatestDispatchDateTime { get; set; }

        public bool UseLossesCoefficient { get; set; }
    }
}
