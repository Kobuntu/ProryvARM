using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Archives;
using Proryv.AskueARM2.Server.DBAccess.Public.UA.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Public.Forecasting.Data;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Forecasting
{
    /// <summary>
    /// Фактическое потребление по связанной с прогнозируемым объектом точкой измерения
    /// </summary>
    [DataContract]
    public class TForecastObjectFactDemand
    {
        #region Public variable

        /// <summary>
        /// Объекты прогнозирования
        /// </summary>
        [DataMember]
        public HashSet<string> ForecastObjectUns;
        /// <summary>
        /// Начальная дата
        /// </summary>
        [DataMember]
        public DateTime DtStart;
        /// <summary>
        /// Конечная дата
        /// </summary>
        [DataMember]
        public DateTime DtEnd;

        /// <summary>
        /// Часовой пояс, периода
        /// </summary>
        [DataMember]
        public string TimeZoneId;
        /// <summary>
        /// Порядок ед. изм.
        /// </summary>
        [DataMember]
        public EnumUnitDigit UnitDigit;

        /// <summary>
        /// Период дискретизации, по которому составлен прогноз
        /// </summary>
        [DataMember]
        public enumTimeDiscreteType ForecastDiscreteType;

        /// <summary>
        /// Ручной ввод
        /// </summary>
        [DataMember] public bool IsReadCalculatedValues;

        /// <summary>
        /// Ошибки 
        /// </summary>
        [DataMember]
        public StringBuilder Errors;

        /// <summary>
        /// Фактическое потребление по объекту прогнозирования
        /// </summary>
        [DataMember]
        public Dictionary<string, List<ForecastByInputParamArchives>> Archives;

        #endregion

        public TForecastObjectFactDemand(IEnumerable<string> forecastObjectUns, DateTime dtStart, DateTime dtEnd, string timeZoneId,
            EnumUnitDigit unitDigit, enumTimeDiscreteType forecastDiscreteType, bool isReadCalculatedValues)
        {
            if (forecastObjectUns == null)
            {
                Errors.Append("Выберите объекты!");
                return; //Критическая ошибка
            }

            ForecastObjectUns = new HashSet<string>(forecastObjectUns);
            DtStart = dtStart;
            DtEnd = dtEnd;
            TimeZoneId = timeZoneId;
            UnitDigit = unitDigit;
            ForecastDiscreteType = forecastDiscreteType;
            Archives = new Dictionary<string, List<ForecastByInputParamArchives>>();
            IsReadCalculatedValues = isReadCalculatedValues;
            Errors = new StringBuilder();

            if (dtEnd < dtStart)
            {
                Errors.Append("Начальная дата должна быть больше конечной!");
                return; //Критическая ошибка
            }

            #region Читаем архивы

            //_numberHalfHoursForForecasting = MyListConverters.GetNumbersValuesInPeriod(ForecastDiscreteType, forecastingStart, forecastingEnd, TimeZoneId);

            var objectParams = ForecastObjectFactory.GetForecastObjectParams(ForecastObjectUns, dtStart, dtEnd, null, timeZoneId, 
              Errors, unitDigit, forecastDiscreteType, isReadCalculatedValues);

            foreach(var objectParam in objectParams)
            {
                Archives[objectParam.ForecastObject_UN] = objectParam.ArchivesByInputParam;
            }

            #endregion
        }
    }
}

