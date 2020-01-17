using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Forecasting
{
    /// <summary>
    /// Тип объекта прогнозирования
    /// </summary>
    [DataContract]
    public class TForecastObjectType
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [DataMember]
        public int ForecastObjectType_ID { get; set; }

        /// <summary>
        /// Название типа
        /// </summary>
        [DataMember]
        public string ForecastObjectTypeName { get; set; }
        /// <summary>
        /// Комментарий
        /// </summary>
        [DataMember]
        public string Comment { get; set; }
        /// <summary>
        /// Пользователь создавший тип
        /// </summary>
        [DataMember]
        public string User_ID { get; set; }
        /// <summary>
        /// Время создания типа
        /// </summary>
        [DataMember]
        public DateTime ApplyDateTime { get; set; }
        /// <summary>
        /// Режимы работы для типа
        /// </summary>
        [DataMember]
        public List<TForecast_ObjectTypeMode> Forecast_ObjectTypeModes { get; set; }
    }

}