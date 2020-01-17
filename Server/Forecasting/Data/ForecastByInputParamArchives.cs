using System.Collections.Generic;
using System.Runtime.Serialization;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Forecasting.Data
{
    [DataContract]
    public class ForecastByInputParamArchives
    {
        /// <summary>
        /// Идентификатор Физического параметра
        /// </summary>
        [DataMember]
        public readonly string ForecastInputParamUn;

        /// <summary>
        /// Архивные значения
        /// </summary>
        [DataMember]
        public readonly List<TVALUES_DB> Archives;

        [DataMember]
        public string MeasureQuantityType_UN;

        public ForecastByInputParamArchives(string forecastInputParamUn)
        {
            ForecastInputParamUn = forecastInputParamUn;
            Archives = new List<TVALUES_DB>();
        }
    }
}
