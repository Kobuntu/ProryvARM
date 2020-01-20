using System;

namespace Proryv.Servers.Calculation.Parser.Internal
{
    public partial class ProryvParser
    {
        #region Evaluate the value of the operation.

        #region Add
        private object op_Add(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 <= 1 || category2 <= 1)
            {
                return Convert.ToString(par1) + Convert.ToString(par2);
            }
            else if (category1 == 2 || category2 == 2)
            {
                return Convert.ToDecimal(par1) + Convert.ToDecimal(par2);
            }
            else if (category1 == 3 || category2 == 3)
            {
                return Convert.ToDouble(par1) + Convert.ToDouble(par2);
            }
            else if (category1 == 4 || category2 == 4)
            {
                if (category1 == 5) //long
                    return Convert.ToInt64(par1) + Convert.ToInt64(par2);
                else
                    return Convert.ToUInt64(par1) + Convert.ToUInt64(par2);
            }
            else if (category1 == 5 || category2 == 5)
            {
                return Convert.ToInt64(par1) + Convert.ToInt64(par2);
            }
            else if (category1 == 6 || category2 == 6)
            {
                if (category1 == 7) //int
                    return Convert.ToInt32(par1) + Convert.ToInt32(par2);
                else
                    return Convert.ToUInt32(par1) + Convert.ToUInt32(par2);
            }
            else if (category1 == 7 || category2 == 7)
            {
                return Convert.ToInt32(par1) + Convert.ToInt32(par2);
            }
            else if (category1 == 8 || category2 == 8)
            {
                return new DateTime(Convert.ToDateTime(par1).Ticks + Convert.ToDateTime(par2).Ticks);
            }
            else
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "+", GetTypeName(par1), GetTypeName(par2));
            }
            return null;
        }
        #endregion

        #region Sub
        private object op_Sub(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 <= 1 || category2 <= 1)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "-", GetTypeName(par1), GetTypeName(par2));
            }
            else if (category1 == 2 || category2 == 2)
            {
                return Convert.ToDecimal(par1) - Convert.ToDecimal(par2);
            }
            else if (category1 == 3 || category2 == 3)
            {
                return Convert.ToDouble(par1) - Convert.ToDouble(par2);
            }
            else if (category1 == 4 || category2 == 4 || category1 == 5 || category2 == 5)
            {
                return Convert.ToInt64(par1) - Convert.ToInt64(par2);
            }
            else if (category1 == 6 || category2 == 6 || category1 == 7 || category2 == 7)
            {
                return Convert.ToInt32(par1) - Convert.ToInt32(par2);
            }
            else if (category1 == 8 || category2 == 8)
            {
                return new DateTime(Convert.ToDateTime(par1).Ticks - Convert.ToDateTime(par2).Ticks);
            }
            else
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "-", GetTypeName(par1), GetTypeName(par2));
            }
            return null;
        }
        #endregion

        #region Mult
        private object op_Mult(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 <= 1 || category2 <= 1)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "*", GetTypeName(par1), GetTypeName(par2));
            }
            else if (category1 == 2 || category2 == 2)
            {
                return Convert.ToDecimal(par1) * Convert.ToDecimal(par2);
            }
            else if (category1 == 3 || category2 == 3)
            {
                return Convert.ToDouble(par1) * Convert.ToDouble(par2);
            }
            else if (category1 == 4 || category2 == 4 || category1 == 5 || category2 == 5)
            {
                return Convert.ToInt64(par1) * Convert.ToInt64(par2);
            }
            else if (category1 == 6 || category2 == 6 || category1 == 7 || category2 == 7)
            {
                return Convert.ToInt32(par1) * Convert.ToInt32(par2);
            }
            else
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "*", GetTypeName(par1), GetTypeName(par2));
            }
            return null;
        }
        #endregion

        #region Div
        private object op_Div(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 <= 1 || category2 <= 1)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "/", GetTypeName(par1), GetTypeName(par2));
            }
            else if (category1 == 2 || category2 == 2)
            {
                return Convert.ToDecimal(par1) / Convert.ToDecimal(par2);
            }
            else if (category1 == 3 || category2 == 3)
            {
                return Convert.ToDouble(par1) / Convert.ToDouble(par2);
            }
            else if (category1 == 4 || category2 == 4 || category1 == 5 || category2 == 5)
            {
                return Convert.ToInt64(par1) / Convert.ToInt64(par2);
            }
            else if (category1 == 6 || category2 == 6 || category1 == 7 || category2 == 7)
            {
                return Convert.ToInt32(par1) / Convert.ToInt32(par2);
            }
            else
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "/", GetTypeName(par1), GetTypeName(par2));
            }
            return null;
        }
        #endregion

        #region Mod
        private object op_Mod(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 <= 1 || category2 <= 1)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "%", GetTypeName(par1), GetTypeName(par2));
            }
            else if (category1 == 2 || category2 == 2)
            {
                return Convert.ToDecimal(par1) % Convert.ToDecimal(par2);
            }
            else if (category1 == 3 || category2 == 3)
            {
                return Convert.ToDouble(par1) % Convert.ToDouble(par2);
            }
            else if (category1 == 4 || category2 == 4 || category1 == 5 || category2 == 5)
            {
                return Convert.ToInt64(par1) % Convert.ToInt64(par2);
            }
            else if (category1 == 6 || category2 == 6 || category1 == 7 || category2 == 7)
            {
                return Convert.ToInt32(par1) % Convert.ToInt32(par2);
            }
            else
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "%", GetTypeName(par1), GetTypeName(par2));
            }
            return null;
        }
        #endregion

        #region Pow
        private object op_Pow(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 >= 2 && category2 >= 2 && category1 <= 7 && category2 <= 7)
            {
                return Math.Pow(Convert.ToDouble(par1), Convert.ToDouble(par2));
            }
            else
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "^", GetTypeName(par1), GetTypeName(par2));
            }
            return null;
        }
        #endregion

        #region Neg
        private object op_Neg(object par1)
        {
            int category = get_category(par1);
            if (category <= 1 || category >= 8)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "Negative", GetTypeName(par1));
            }
            return op_Mult(par1, (int)-1);
        }
        #endregion

        #region Not
        private object op_Not(object par1)
        {
            int category = get_category(par1);
            if (category != 9)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "Not", GetTypeName(par1));
            }
            return !Convert.ToBoolean(par1);
        }
        #endregion

        #region Cast
        private object op_Cast(object par1, object par2)
        {
            TypeCode typecode = (TypeCode)par2;
            switch (typecode)
            {
                case TypeCode.Boolean:
                    return Convert.ToBoolean(par1);
                case TypeCode.Byte:
                    return Convert.ToByte(par1);
                case TypeCode.Char:
                    return Convert.ToChar(par1);
                case TypeCode.DateTime:
                    return Convert.ToDateTime(par1);
                case TypeCode.Decimal:
                    return Convert.ToDecimal(par1);
                case TypeCode.Double:
                    return Convert.ToDouble(par1);
                case TypeCode.Int16:
                    return Convert.ToInt16(par1);
                case TypeCode.Int32:
                    return Convert.ToInt32(par1);
                case TypeCode.Int64:
                    return Convert.ToInt64(par1);
                case TypeCode.SByte:
                    return Convert.ToSByte(par1);
                case TypeCode.Single:
                    return Convert.ToSingle(par1);
                case TypeCode.String:
                    return Convert.ToString(par1);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(par1);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(par1);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(par1);
            }
            return par1;
        }
        #endregion

        #region CompareLeft
        private object op_CompareLeft(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 == 0 || category2 == 0)
            {
                IComparable compar1 = par1 as IComparable;
                IComparable compar2 = par2 as IComparable;
                if (compar1 != null && compar2 != null)
                    return compar1.CompareTo(compar2) < 0;
                else
                    ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "<", GetTypeName(par1), GetTypeName(par2));
            }
            if (category1 == 1 || category2 == 1)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "<", GetTypeName(par1), GetTypeName(par2));
            }
            else if (category1 == 2 || category2 == 2 || category1 == 3 || category2 == 3)
            {
                return Convert.ToDecimal(par1) < Convert.ToDecimal(par2);
            }
            else if (category1 == 4 || category2 == 4 || category1 == 6 || category2 == 6)
            {
                return Convert.ToUInt64(par1) < Convert.ToUInt64(par2);
            }
            else if (category1 == 5 || category2 == 5 || category1 == 7 || category2 == 7)
            {
                return Convert.ToInt64(par1) < Convert.ToInt64(par2);
            }
            else if (category1 == 8 || category2 == 8)
            {
                return Convert.ToDateTime(par1) < Convert.ToDateTime(par2);
            }
            else
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "<", GetTypeName(par1), GetTypeName(par2));
            }
            return null;
        }
        #endregion

        #region CompareLeftEqual
        private object op_CompareLeftEqual(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 == 0 || category2 == 0)
            {
                IComparable compar1 = par1 as IComparable;
                IComparable compar2 = par2 as IComparable;
                if (compar1 != null && compar2 != null)
                    return compar1.CompareTo(compar2) <= 0;
                else
                    ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "<=", GetTypeName(par1), GetTypeName(par2));
            }
            if (category1 == 1 || category2 == 1)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "<=", GetTypeName(par1), GetTypeName(par2));
            }
            else if (category1 == 2 || category2 == 2 || category1 == 3 || category2 == 3)
            {
                return Convert.ToDecimal(par1) <= Convert.ToDecimal(par2);
            }
            else if (category1 == 4 || category2 == 4 || category1 == 6 || category2 == 6)
            {
                return Convert.ToUInt64(par1) <= Convert.ToUInt64(par2);
            }
            else if (category1 == 5 || category2 == 5 || category1 == 7 || category2 == 7)
            {
                return Convert.ToInt64(par1) <= Convert.ToInt64(par2);
            }
            else if (category1 == 8 || category2 == 8)
            {
                return Convert.ToDateTime(par1) <= Convert.ToDateTime(par2);
            }
            else
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "<=", GetTypeName(par1), GetTypeName(par2));
            }
            return null;
        }
        #endregion

        #region CompareRight
        private object op_CompareRight(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 == 0 || category2 == 0)
            {
                IComparable compar1 = par1 as IComparable;
                IComparable compar2 = par2 as IComparable;
                if (compar1 != null && compar2 != null)
                    return compar1.CompareTo(compar2) > 0;
                else
                    ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, ">", GetTypeName(par1), GetTypeName(par2));
            }
            if (category1 == 1 || category2 == 1)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, ">", GetTypeName(par1), GetTypeName(par2));
            }
            else if (category1 == 2 || category2 == 2 || category1 == 3 || category2 == 3)
            {
                return Convert.ToDecimal(par1) > Convert.ToDecimal(par2);
            }
            else if (category1 == 4 || category2 == 4 || category1 == 6 || category2 == 6)
            {
                return Convert.ToUInt64(par1) > Convert.ToUInt64(par2);
            }
            else if (category1 == 5 || category2 == 5 || category1 == 7 || category2 == 7)
            {
                return Convert.ToInt64(par1) > Convert.ToInt64(par2);
            }
            else if (category1 == 8 || category2 == 8)
            {
                return Convert.ToDateTime(par1) > Convert.ToDateTime(par2);
            }
            else
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, ">", GetTypeName(par1), GetTypeName(par2));
            }
            return null;
        }
        #endregion

        #region CompareRightEqual
        private object op_CompareRightEqual(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 == 0 || category2 == 0)
            {
                IComparable compar1 = par1 as IComparable;
                IComparable compar2 = par2 as IComparable;
                if (compar1 != null && compar2 != null)
                    return compar1.CompareTo(compar2) >= 0;
                else
                    ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, ">=", GetTypeName(par1), GetTypeName(par2));
            }
            if (category1 == 1 || category2 == 1)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, ">=", GetTypeName(par1), GetTypeName(par2));
            }
            else if (category1 == 2 || category2 == 2 || category1 == 3 || category2 == 3)
            {
                return Convert.ToDecimal(par1) >= Convert.ToDecimal(par2);
            }
            else if (category1 == 4 || category2 == 4 || category1 == 6 || category2 == 6)
            {
                return Convert.ToUInt64(par1) >= Convert.ToUInt64(par2);
            }
            else if (category1 == 5 || category2 == 5 || category1 == 7 || category2 == 7)
            {
                return Convert.ToInt64(par1) >= Convert.ToInt64(par2);
            }
            else if (category1 == 8 || category2 == 8)
            {
                return Convert.ToDateTime(par1) >= Convert.ToDateTime(par2);
            }
            else
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, ">=", GetTypeName(par1), GetTypeName(par2));
            }
            return null;
        }
        #endregion

        #region CompareEqual
        private object op_CompareEqual(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 == -1 || category2 == -1)
            {
                return category1 == category2;
            }
            if (category1 == 0 || category2 == 0)
            {
                IComparable compar1 = par1 as IComparable;
                IComparable compar2 = par2 as IComparable;
                if (compar1 != null && compar2 != null)
                    return compar1.CompareTo(compar2) == 0;
                else
                    return par1.Equals(par2);
            }
            if (category1 == 1 || category2 == 1)
            {
                return Convert.ToString(par1) == Convert.ToString(par2);
            }
            else if (category1 == 2 || category2 == 2 || category1 == 3 || category2 == 3)
            {
                return Convert.ToDecimal(par1) == Convert.ToDecimal(par2);
            }
            else if (category1 == 4 || category2 == 4 || category1 == 6 || category2 == 6)
            {
                return Convert.ToUInt64(par1) == Convert.ToUInt64(par2);
            }
            else if (category1 == 5 || category2 == 5 || category1 == 7 || category2 == 7)
            {
                return Convert.ToInt64(par1) == Convert.ToInt64(par2);
            }
            else if (category1 == 8 || category2 == 8)
            {
                return Convert.ToDateTime(par1) == Convert.ToDateTime(par2);
            }
            return Convert.ToBoolean(par1) == Convert.ToBoolean(par2);
        }
        #endregion

        #region CompareNotEqual
        private object op_CompareNotEqual(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 == 0 || category2 == 0)
            {
                IComparable compar1 = par1 as IComparable;
                IComparable compar2 = par2 as IComparable;
                if (compar1 != null && compar2 != null)
                    return compar1.CompareTo(compar2) != 0;
                else
                    return !par1.Equals(par2);
            }
            if (category1 == 1 || category2 == 1)
            {
                return Convert.ToString(par1) != Convert.ToString(par2);
            }
            else if (category1 == 2 || category2 == 2 || category1 == 3 || category2 == 3)
            {
                return Convert.ToDecimal(par1) != Convert.ToDecimal(par2);
            }
            else if (category1 == 4 || category2 == 4 || category1 == 6 || category2 == 6)
            {
                return Convert.ToUInt64(par1) != Convert.ToUInt64(par2);
            }
            else if (category1 == 5 || category2 == 5 || category1 == 7 || category2 == 7)
            {
                return Convert.ToInt64(par1) != Convert.ToInt64(par2);
            }
            else if (category1 == 8 || category2 == 8)
            {
                return Convert.ToDateTime(par1) != Convert.ToDateTime(par2);
            }
            return Convert.ToBoolean(par1) != Convert.ToBoolean(par2);
        }
        #endregion

        #region Shl
        private object op_Shl(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 < 4 || category1 >= 8 || category2 != 7)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "<<", GetTypeName(par1), GetTypeName(par2));
            }
            else if (category1 == 4 || category2 == 4)
            {
                return Convert.ToUInt64(par1) << Convert.ToInt32(par2);
            }
            else if (category1 == 5 || category2 == 5)
            {
                return Convert.ToInt64(par1) << Convert.ToInt32(par2);
            }
            else if (category1 == 6 || category2 == 6)
            {
                return Convert.ToUInt32(par1) << Convert.ToInt32(par2);
            }
            else if (category1 == 7 || category2 == 7)
            {
                return Convert.ToInt32(par1) << Convert.ToInt32(par2);
            }
            return null;
        }
        #endregion

        #region Shr
        private object op_Shr(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 < 4 || category1 >= 8 || category2 != 7)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, ">>", GetTypeName(par1), GetTypeName(par2));
            }
            else if (category1 == 4 || category2 == 4)
            {
                return Convert.ToUInt64(par1) >> Convert.ToInt32(par2);
            }
            else if (category1 == 5 || category2 == 5)
            {
                return Convert.ToInt64(par1) >> Convert.ToInt32(par2);
            }
            else if (category1 == 6 || category2 == 6)
            {
                return Convert.ToUInt32(par1) >> Convert.ToInt32(par2);
            }
            else if (category1 == 7 || category2 == 7)
            {
                return Convert.ToInt32(par1) >> Convert.ToInt32(par2);
            }
            return null;
        }
        #endregion

        #region And
        private object op_And(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 < 4 || category1 == 8 || category2 < 4 || category2 == 8)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "&", GetTypeName(par1), GetTypeName(par2));
            }
            else if (category1 == 4 || category2 == 4)
            {
                return Convert.ToUInt64(par1) & Convert.ToUInt64(par2);
            }
            else if (category1 == 5 || category2 == 5)
            {
                return Convert.ToInt64(par1) & Convert.ToInt64(par2);
            }
            else if (category1 == 6 || category2 == 6)
            {
                return Convert.ToUInt32(par1) & Convert.ToUInt32(par2);
            }
            else if (category1 == 7 || category2 == 7)
            {
                return Convert.ToInt32(par1) & Convert.ToInt32(par2);
            }
            else if (category1 == 9 || category2 == 9)
            {
                return Convert.ToBoolean(par1) & Convert.ToBoolean(par2);
            }
            return null;
        }
        #endregion

        #region Or
        private object op_Or(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 < 4 || category1 == 8 || category2 < 4 || category2 == 8)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "|", GetTypeName(par1), GetTypeName(par2));
            }
            else if (category1 == 4 || category2 == 4)
            {
                return Convert.ToUInt64(par1) | Convert.ToUInt64(par2);
            }
            else if (category1 == 5 || category2 == 5)
            {
                return Convert.ToInt64(par1) | Convert.ToInt64(par2);
            }
            else if (category1 == 6 || category2 == 6)
            {
                return Convert.ToUInt32(par1) | Convert.ToUInt32(par2);
            }
            else if (category1 == 7 || category2 == 7)
            {
                return Convert.ToInt32(par1) | Convert.ToInt32(par2);
            }
            else if (category1 == 9 || category2 == 9)
            {
                return Convert.ToBoolean(par1) | Convert.ToBoolean(par2);
            }
            return null;
        }
        #endregion

        #region Xor
        private object op_Xor(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 < 4 || category1 == 8 || category2 < 4 || category2 == 8)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "^", GetTypeName(par1), GetTypeName(par2));
            }
            else if (category1 == 4 || category2 == 4)
            {
                return Convert.ToUInt64(par1) ^ Convert.ToUInt64(par2);
            }
            else if (category1 == 5 || category2 == 5)
            {
                return Convert.ToInt64(par1) ^ Convert.ToInt64(par2);
            }
            else if (category1 == 6 || category2 == 6)
            {
                return Convert.ToUInt32(par1) ^ Convert.ToUInt32(par2);
            }
            else if (category1 == 7 || category2 == 7)
            {
                return Convert.ToInt32(par1) ^ Convert.ToInt32(par2);
            }
            else if (category1 == 9 || category2 == 9)
            {
                return Convert.ToBoolean(par1) ^ Convert.ToBoolean(par2);
            }
            return null;
        }
        #endregion

        #region And2
        private object op_And2(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 != 9 || category2 != 9)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "&&", GetTypeName(par1), GetTypeName(par2));
            }
            else
            {
                return Convert.ToBoolean(par1) && Convert.ToBoolean(par2);
            }
            return null;
        }
        #endregion

        #region Or2
        private object op_Or2(object par1, object par2)
        {
            int category1 = get_category(par1);
            int category2 = get_category(par2);

            if (category1 != 9 || category2 != 9)
            {
                ThrowError(ParserErrorCode.OperatorCannotBeAppliedToOperands, "||", GetTypeName(par1), GetTypeName(par2));
            }
            else
            {
                return Convert.ToBoolean(par1) || Convert.ToBoolean(par2);
            }
            return null;
        }
        #endregion

        #endregion    
    }
}
