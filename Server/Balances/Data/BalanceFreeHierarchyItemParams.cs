using System.Collections.Generic;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data
{
    public class BalanceFreeHierarchyItemParams
    {
        /// <summary>
        /// Порядок в балансе
        /// </summary>
        public int SortNumber;
        /// <summary>
        /// Целочисленный идентификатор 
        /// </summary>
        public int Id;
        /// <summary>
        /// Строковый идентификатор 
        /// </summary>
        public string Un;
        /// <summary>
        /// Тип
        /// </summary>
        public enumTypeHierarchy TypeHierarchy;

        /// <summary>
        /// Канал
        /// </summary>
        public byte? ChannelType;

        /// <summary>
        /// Название для баланса
        /// </summary>
        public string Name;

        /// <summary>
        /// Идентификатор раздела
        /// </summary>
        public string BalanceFreeHierarchySectionUn;

        /// <summary>
        /// Подраздел 1 уровня
        /// </summary>
        public string BalanceFreeHierarchySubsectionUn;
        /// <summary>
        /// Подраздел 2 уровня
        /// </summary>
        public string BalanceFreeHierarchySubsectionUn2;
        /// <summary>
        /// Подраздел 3 уровня
        /// </summary>
        public string BalanceFreeHierarchySubsectionUn3;

        /// <summary>
        /// Знак, с которым объект участвует в балансе (null - в балансе не учавствует)
        /// </summary>
        public double? Coef;

        /// <summary>
        /// Погрешность измерения 
        /// </summary>
        public double MeasuringComplexError;

        /// <summary>
        /// Серийный номер датчика
        /// </summary>
        public string MeterSerialNumber;

        /// <summary>
        /// Напряжение
        /// </summary>
        public double? Voltage;

        /// <summary>
        /// Коэффициент трансформации (для ПУ)
        /// </summary>
        public double CoeffTransformation;

        /// <summary>
        /// Коэффициент потерь
        /// </summary>
        public double CoeffLosses;

        /// <summary>
        /// Получасовки по запрошенному диапазону (nullable)
        /// </summary>
        public List<TVALUES_DB> HalfHours;

        /// <summary>
        /// Интегральное показание на начало
        /// </summary>
        public TINTEGRALVALUES_DB IntegralsOnStart;

        /// <summary>
        /// Интегральное показание на окончание
        /// </summary>
        public TINTEGRALVALUES_DB IntegralsOnEnd;

        /// <summary>
        /// Расход по показаниям за период
        /// </summary>
        public double IntegralsDiffSums;
        
        /// <summary>
        /// Это подгруппа отпуска с шин
        /// </summary>
        public bool IsOtpuskShin;
        /// <summary>
        /// Подгруппа главных трансформаторов
        /// </summary>
        //public bool IsGlavTrans;

        /// <summary>
        /// Используется в расчете баланса/небаланса
        /// </summary>
        public bool IsUseInGeneralBalance;

        /// <summary>
        /// Поступление
        /// </summary>
        public bool IsInput;

        /// <summary>
        /// Отдача
        /// </summary>
        public bool IsOutput;

        /// <summary>
        /// Процентная доля данной ТИ в общем значении по даной подгруппе
        /// </summary>
        public double PartPercent;

        /// <summary>
        /// Небаланс по данной ТИ
        /// </summary>
        public double UnBalanceTi;

        /// <summary>
        /// Достоверность
        /// </summary>
        public VALUES_FLAG_DB F_FLAG;


        /// <summary>
        /// Список значений ОВ
        /// </summary>
        public List<TOV_Values> OV_Values_List;

        /// <summary>
        /// Информация о смене трансформаторов
        /// </summary>
        public List<TTransformators_Information> Transformatos_Information;

        /// <summary>
        /// ИИнформация о смене ПУ 
        /// </summary>
        public List<TMetersTO_Information> MetersTO_Information;

        /// <summary>
        /// Информация по периодам констант
        /// </summary>
        public List<ArchCalc_FormulaConstant> FormulaConstantArchives;

        /// <summary>
        /// Информация по замещению по акту недоучета
        /// </summary>
        public List<ArchCalc_Replace_ActUndercount> ReplaceActUndercountList;

        /// <summary>
        /// Значение константы поумолчанию
        /// </summary>
        public double? DefaultFormulaConstantValue;
    }
}
