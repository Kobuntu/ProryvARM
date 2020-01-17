using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Forecasting
{
    /// <summary>
    /// Режимы работы для типа
    /// </summary>
    [DataContract]
    public class TForecast_ObjectTypeMode
    {
        /// <summary>
        /// Идентификатор типа режима работы
        /// </summary>
         [DataMember]
        public int ForecastObjectTypeMode_ID { get; set; }

        /// <summary>
        /// Название типа режима работы для типа
        /// </summary>


        [DataMember]
        public string ForecastObjectTypeModeName { get; set; }

        /// <summary>
        /// Мощность для режима работы
        /// </summary>
            [DataMember]
        public double PowerValue { get; set; }
    }
}
