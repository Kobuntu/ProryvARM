using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Forecasting.Data
{
    /// <summary>
    /// Параметры для обсчета прогноза
    /// </summary>
    [DataContract]
    public class TForecastCalculateParams
    {
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
        /// Период дискретизации
        /// </summary>
        [DataMember]
        public enumTimeDiscreteType DiscreteType;


        /// <summary>
        /// Часовой пояс, периода
        /// </summary>
        [DataMember]
        public string TimeZoneId;


        /// <summary>
        /// Идентификатор 
        /// </summary>
        [DataMember]
        public readonly string ForecastObject_UN;

        /// <summary>
        /// Расчетная модель, которую будем использовать
        /// </summary>

        [DataMember]
        public readonly int? ForecastModelUserSelected;

        /// <summary>
        /// Расчетные модели, доступные для данного объекта, отсортированные по приоритету
        /// </summary>

        [DataMember]
        public readonly HashSet<int> AvailableForecastModels;


        /// <summary>
        /// Подготовленный архив для прогнозирования
        /// </summary>
        //[DataMember]
        //public Dictionary<EnumMeasureUnitCategory, List<TVALUES_DB>> Archives;
        [DataMember]
        public readonly List<ForecastByInputParamArchives> ArchivesByInputParam;

        private List<TVALUES_DB> _values = null;

        private List<TVALUES_DB> _energy;

        /// <summary>
        /// Это энергия
        /// </summary>
        [DataMember]
        public List<TVALUES_DB> Energy
        {
            get
            {
                if (_energy != null) return _energy;

                var fe = ArchivesByInputParam.FirstOrDefault(i => !string.IsNullOrEmpty(i.MeasureQuantityType_UN) && string.Equals(i.MeasureQuantityType_UN, "EnergyUnit"));
                if (fe != null)
                {
                    _energy = fe.Archives;
                }

                return _energy;
            }
        }

        private List<TVALUES_DB> _volumeFlow;

        /// <summary>
        /// Расход воды
        /// </summary>
        [DataMember]
        public List<TVALUES_DB> VolumeFlow
        {
            get
            {
                if (_volumeFlow != null) return _volumeFlow;

                var fe = ArchivesByInputParam.FirstOrDefault(i => !string.IsNullOrEmpty( i.MeasureQuantityType_UN) && string.Equals(i.MeasureQuantityType_UN, "VolumeFlowUnit"));
                if (fe != null)
                {
                    _volumeFlow = fe.Archives;
                }

                return _volumeFlow;
            }
        }


        /// <summary>
        /// Данные которые использовались в расчетах
        /// </summary>
        [DataMember] public Dictionary<DateTime, List<TVALUES_DB>> DataToUsage;

        //public Dictionary<ID_TypeHierarch>
        //TODO доработать периоды действия ТИ 

        public TForecastCalculateParams(string forecastObject_UN, int? forecastModelUserSelected, HashSet<int> avalableForecastModels,
            List<TVALUES_DB> archivesDbs = null)
        {
            ForecastObject_UN = forecastObject_UN;
            ForecastModelUserSelected = forecastModelUserSelected;
            AvailableForecastModels = avalableForecastModels;

            ArchivesByInputParam = new List<ForecastByInputParamArchives>();
            // Archives = new List<TVALUES_DB>();
            if (archivesDbs != null)
            {
                ArchivesByInputParam.Add(new ForecastByInputParamArchives("")
                {
                    MeasureQuantityType_UN ="EnergyUnit",
                });

                Energy.AddRange(archivesDbs);
            }
        }
    }
}
