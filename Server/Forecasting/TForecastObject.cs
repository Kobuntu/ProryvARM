using Proryv.AskueARM2.Server.DBAccess.Internal.DB;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Forecasting
{
    /// <summary>
    /// Детальная информация об объекте прогнозирования
    /// </summary>
    [DataContract]
    public class TForecastObject
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [DataMember] public string ForecastObject_UN;

        /// <summary>
        /// Название
        /// </summary>
        [DataMember] public string ForecastObjectName;

        /// <summary>
        /// Пользователь 
        /// </summary>
        [DataMember] public string User_ID;

        /// <summary>
        /// Дата время создания/последнего изменения
        /// </summary>
        [DataMember] public DateTime ApplyDateTime;

        /// <summary>
        /// Признак удаления
        /// </summary>
        [DataMember] public bool? IsDeleted;

        /// <summary>
        /// Тип данного объекта
        /// </summary>
        [DataMember]
        public int? ForecastObjectType_ID;

        /// <summary>
        /// Дочерние объекты
        /// </summary>
        [DataMember] public List<string> Children;

        /// <summary>
        /// Доступные модели прогнозирования
        /// </summary>
        [DataMember] public List<int> ForecastModels;

        /// <summary>
        /// Физический параметр связанный с расчетом этого объекта прогнозирования
        /// </summary>
        [DataMember] public Forecast_PhysicalValue ForecastPhysicalValue;

        /// <summary>
        /// Количество минут, через которое разрешено планировать от текущего момента времени
        /// Это значение поумолчанию, если не наполнена таблица Forecast_Objects_PlanTimeRules
        /// </summary>
        [DataMember] public int MaxMinutesPlanEventTime;

        /// <summary>
        /// Коичество минут на глубину которого можно вносить факт. значение
        /// Это значение поумолчанию, если не наполнена таблица Forecast_Objects_PlanTimeRules
        /// </summary>
        [DataMember] public int MaxMinutesEventTimeFact;

        /// <summary>
        /// Группа к которой относится объект прогнозирования (таблица Forecast_Groups)
        /// </summary>
        [DataMember] public byte? ForecastGroup_ID;
    }
}
