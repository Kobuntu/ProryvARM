using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal;

namespace Proryv.Servers.Calculation.FormulaInterpreter
{
    public static class Helpers
    {
        public static int IndexOf(this StringBuilder sb, string value)
        {
            return sb.IndexOf(value, 0, true);
        }

        public static int IndexOf(this StringBuilder sb, string value, int startIndex, bool ignoreCase)
        {
            int index;
            int length = value.Length;
            int maxSearchLength = (sb.Length - length) + 1;

            if (ignoreCase)
            {
                for (int i = startIndex; i < maxSearchLength; ++i)
                {
                    if (Char.ToLower(sb[i]) == Char.ToLower(value[0]))
                    {
                        index = 1;
                        while ((index < length) && (Char.ToLower(sb[i + index]) == Char.ToLower(value[index])))
                            ++index;

                        if (index == length)
                            return i;
                    }
                }

                return -1;
            }

            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (sb[i] == value[0])
                {
                    index = 1;
                    while ((index < length) && (sb[i + index] == value[index]))
                        ++index;

                    if (index == length)
                        return i;
                }
            }

            return -1;
        }

        public static int IndexOfAny(this StringBuilder sb, char[] value, int startIndex)
        {
            for (int i = startIndex; i < sb.Length; ++i)
            {
                if (value.Contains(sb[i])) return i;
            }

            return -1;
        }

        // <summary>
        /// Выявляем максимально недостоверный статус, добавляем остальные неаварийные статусы
        /// </summary>
        /// <param name="F_FLAG">Флаг который меняем</param>
        /// <param name="F_FLAG1">Флаг с которым сравниваем</param>
        public static VALUES_FLAG_DB CompareAndReturnMostBadStatus(this VALUES_FLAG_DB F_FLAG, VALUES_FLAG_DB F_FLAG1)
        {
            //Если отсутствует канал измерения
            //if (F_FLAG.CheckFlag(VALUES_FLAG_DB.IsChannelAbsented) || F_FLAG1.CheckFlag(VALUES_FLAG_DB.IsChannelAbsented)) return VALUES_FLAG_DB.IsChannelAbsented;

            F_FLAG = F_FLAG | (F_FLAG1 & VALUES_FLAG_DB.AllMainStatuses) | (F_FLAG1 & VALUES_FLAG_DB.AllWarningStatuses);

            return F_FLAG;
        }

        public static VALUES_FLAG_DB CompareAndReturnMostBadStatus(this IEnumerable<VALUES_FLAG_DB> F_list)
        {
            VALUES_FLAG_DB flag = VALUES_FLAG_DB.None;
            foreach (var f in F_list)
            {
                flag = flag.CompareAndReturnMostBadStatus(f);
            }

            return flag;
        }

    }
}
