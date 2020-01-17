using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    /// <summary>
    /// Формула описания
    /// </summary>
    public class FORMULA
    {
        /// <summary>
        /// Описание формулы
        /// </summary>
        public string F_INFO;
        /// <summary>
        /// Класс формулы
        /// </summary>
        public int F_CLASS;
        /// <summary>
        /// Тип формулы
        /// </summary>
        public int F_TYPE;
        /// <summary>
        /// Идентификатор формулы
        /// </summary>
        public readonly string F_ID;
        /// <summary>
        /// Размерность результа фoрмулы (доли, % или размерные вел. КВТЧ)
        /// </summary>
        public string F_RESULT_TYPE;
        /// <summary>
        /// Режим формулы (из базы или иначе). ВВедено по просьбе Саши  Розинова
        /// </summary>
        public int F_MODE = 0;
        /// <summary>
        /// Верхняя уставка
        /// </summary>
        public float HIGHT_LIMIT = 0;
        /// <summary>
        /// Нижняя уставка
        /// </summary>
        public float LOWER_LIMIT = 0;
        /// <summary>
        /// Операторы формулы
        /// </summary>
        public List<F_OPERATOR> F_OPERATORS = new List<F_OPERATOR>();
        /// <summary>
        /// Размерность в которой составленая формула (Вт, кВт, МВт)
        /// </summary>
        public readonly EnumUnitDigit? UnitDigit;
        public FORMULA(string fId, EnumUnitDigit? unitDigit = null)
        {
            F_ID = fId;
            UnitDigit = unitDigit;
        }
    }
}
