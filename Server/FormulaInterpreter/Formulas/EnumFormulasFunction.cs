using System;
using System.Collections.Generic;


namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    public enum EnumFormulasFunction
    {
        [FormulasFunctionDescription("None", "<Отсутствует>", EnumTypeFormulasFunction.None, enumTimeDiscreteType.DBInterval)] None = 0,
        [FormulasFunctionDescription("SumForPeriod", "Сумма за запрошенный период", EnumTypeFormulasFunction.Archive, enumTimeDiscreteType.DBInterval)] SumForPeriod = 1,
        [FormulasFunctionDescription("SumForDay", "Сумма за текущие сутки", EnumTypeFormulasFunction.Archive,enumTimeDiscreteType.DB24Hour)] SumForDay = 2,
        [FormulasFunctionDescription("SumForMonth", "Сумма за текущий месяц", EnumTypeFormulasFunction.Archive,enumTimeDiscreteType.DBMonth)] SumForMonth = 3,
        [FormulasFunctionDescription("SumForHour", "Сумма за текущий час", EnumTypeFormulasFunction.Archive,enumTimeDiscreteType.DBHours)] SumForHour = 4,
        [FormulasFunctionDescription("ЧчиЗаМесяц", "Число рабочих часов за расчетный месяц", EnumTypeFormulasFunction.Ччи,enumTimeDiscreteType.DBMonth)] ЧчиЗаМесяц = 5,
        [FormulasFunctionDescription("ЧчиЗаСутки", "Число рабочих часов за текущие сутки", EnumTypeFormulasFunction.Ччи,enumTimeDiscreteType.DB24Hour)] ЧчиЗаСутки = 6,
        [FormulasFunctionDescription("ЧчиЗаПериод", "Число рабочих часов за запрошенный период", EnumTypeFormulasFunction.Ччи,enumTimeDiscreteType.DBInterval)] ЧчиЗаПериод = 7,
        [FormulasFunctionDescription("ЕслиВключено", "Работает ли выбранный объект в расчетной получасовке", EnumTypeFormulasFunction.ИндикаторРаботы, enumTimeDiscreteType.DBHalfHours)]
        ЕслиВключено = 8,
    
        [FormulasFunctionDescription("ПользовательскаяФункцияSQL", "Выполнить SQL скрипт и вернуть результат \n Пример: ПользовательскаяФункцияSQL(\"select {0}\";1) \n Где первый аргумент функции - SQL запрос, далее аргументы {0},{1}...{n} передаваемые в SQL выражении ", EnumTypeFormulasFunction.Archive, enumTimeDiscreteType.DBHalfHours)]
        ПользовательскаяФункцияSql = 9,
        [FormulasFunctionDescription("ПользовательскаяФункцияКод", "Выполнить C# код и вернуть результат", EnumTypeFormulasFunction.Archive, enumTimeDiscreteType.DBHalfHours)]
        ПользовательскаяФункцияКод = 10,
        [FormulasFunctionDescription("ИндексПолучасовогоЗначения", "Получить индекс вычисляемого значения(30-минутного интервала)", EnumTypeFormulasFunction.Archive, enumTimeDiscreteType.DBHalfHours)]
        ПользовательскаяФункция = 11,
        [FormulasFunctionDescription("ВремяПолучасовогоЗначения", @"Получить время для вычисляемого значения(30-минутного интервала)
      Пример:ВремяЗначения('u') - вернет кол-во секунд по Unix
        Поддерживаемые форматы:
   'u' - Время в формате Unix(кол-во секунд от 1.1.1970).
   'd' - День месяца, в диапазоне от 1 до 31. 
   'HH' - Час в 24-часовом формате от 00 до 23. 
   'mm' - Минуты, в диапазоне от 00 до 59. 
   's' - Секунды, в диапазоне от 0 до 59. 
   'MM' - Месяц, в диапазоне от 01 до 12. 
   'yyyy' - Год в виде четырехзначного числа."
 ,EnumTypeFormulasFunction.Archive, enumTimeDiscreteType.DB24Hour)]
        ВремяЗначения = 12,
        [FormulasFunctionDescription("СтатусЗначенияСодержитКод", @"Функция возвращает 1 если в значений содержится(0-отсутсвует) соответсвующий статус:
      Пример:СтатусЗначенияСодержит(Точка измерения 231;1) - результат : 
            0- если статус 'Недостаточно данных' отсутсвует; 
            1- если статус 'Недостаточно данных' присутсвует;
        Поддерживаемые коды статусов:
        
        1-Недостаточно данных
        2-Недостоверные
        3-Помечены как отсутствующие
        16-РучнойВвод 
        32-ЗамещеныКонтрагентом,
        64-Автосбор,
        123-СчитываниеМРСК,
        67108864-Данные ГП/ЭСК,
        134217728-Данные потребителя,
        268435456-Данные МРСК,
        137438953472-Замещены обходным выключателем,
        2251799813685248 - Признаны достоверными
        4503599627370496 - Признаны недостоверными


      

"
            
            
            
            
            
            , EnumTypeFormulasFunction.Archive, enumTimeDiscreteType.DBHalfHours)]
        СтатусЗначенияСодержитКод = 13,
        [FormulasFunctionDescription("СтатусЗначенияНедостоверные", @"Функция возвращает 1 если в значений содержится(0-отсутсвует) соответсвующий статус:
      Пример:СтатусЗначенияНедостоверные(Точка измерения 231) - результат : 
            0- если статус не содержит коды недостоверных значений ; 
            1- если статус  содержит хотя бы один код недостоверных значений;
           "
            
            
            
            
            
            , EnumTypeFormulasFunction.Archive, enumTimeDiscreteType.DBHalfHours)]
        СтатусЗначенияНедостоверные = 14,

    }

    public enum EnumTypeFormulasFunction
    {
        None = 0,
        Archive = 1,
        Ччи = 2,
        ИндикаторРаботы = 3,

    }

    public class FormulasFunctionDescription : Attribute
    {
        /// <summary>
        /// Строковое представление
        /// </summary>
        public readonly string Id;
        /// <summary>
        /// Описание
        /// </summary>
        public readonly string Description;
        /// <summary>
        /// Тип 
        ///  </summary>
        public readonly EnumTypeFormulasFunction TypeFormulasFunction;
        /// <summary>
        /// Период дискретизации
        /// </summary>
        public readonly enumTimeDiscreteType DiscreteType;

        public FormulasFunctionDescription(string id, string description, EnumTypeFormulasFunction typeFormulasFunction, enumTimeDiscreteType discreteType)
        {
            Id = id;
            Description = description;
            TypeFormulasFunction = typeFormulasFunction;
            DiscreteType = discreteType;
        }
    }
}
