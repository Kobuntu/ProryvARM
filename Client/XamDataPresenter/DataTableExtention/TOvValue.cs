using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter.DataTableExtention
{
    public class TOvValue : IFValue, IConvertible
    {
        /// <summary>
        /// Собственное значение ОВ
        /// </summary>
        public double F_VALUE { get; set; }

        /// <summary>
        /// Флаг
        /// </summary>
        public VALUES_FLAG_DB F_FLAG { get; set; }

        //Неразнесенное значение
        public double UnreplacedValue { get; set; }

        #region IConvertible

        public TypeCode GetTypeCode()
        {
            return TypeCode.Double;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return F_VALUE != 0.0;
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            return F_VALUE;
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return (decimal)F_VALUE;
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            return F_VALUE.ToString(provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return F_VALUE;
        }

        #endregion

        public override bool Equals(object x)
        {
            var xv = x as IFValue;
            if (xv == null) return false;

            return F_FLAG == xv.F_FLAG &&
                   F_VALUE == xv.F_VALUE;
        }

        public override int GetHashCode()
        {
            var flag = (uint)F_FLAG;
            flag = (flag << 26) | (flag >> (32 - 26));

            return (int)((int)F_VALUE ^ flag);
        }
    }
}
