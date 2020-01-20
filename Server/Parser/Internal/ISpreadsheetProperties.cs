using System;
using System.Collections.Generic;

namespace Proryv.Servers.Calculation.Parser.Internal
{
    /// <summary>
    /// Вспомогательный интерфейс для наполнения всего что между {} в Excel
    /// </summary>
    public interface ISpreadsheetProperties
    {
        #region Параметры обработки отчета

        DateTime НачальнаяДата { get; }
        DateTime КонечнаяДата { get; }
        string Филиал { get; }
        string НазваниеОтчета { get; }
        string ЕдиницыИзмерения { get; }

        List<IFreeHierarchyBalanceSignature> Подписанты { get;}

        #endregion

        #region Объект, который содержит статичные процедуры для реализации ф-ий описанных между {}

        Type ProryvFunctionFactory { get; }

        #endregion
    }
}
