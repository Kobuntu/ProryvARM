using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.TClasses
{
    /// <summary>
    /// Параметры вложенной формулы
    /// </summary>
    public class TFormulaSelectResult
    {
        public byte ForAutoUse;
        public int InnerLevel;
        public string Formula_UN;
        public int StringNumber;
        public string OperBefore;
        public string UsedFormula_UN;
        public int? TI_ID;
        public byte? ChannelType;
        public int? TP_ID;
        public int? Section_ID;
        public int? ContrTI_ID;
        public string OperAfter;
        public string FormulaName;
        public byte FormulaType_ID;
        public bool IsIntegral;
        public DateTime StartDateTime;
        public DateTime? FinishDateTime;
        public Guid? ClosedPeriod_ID;
        public string MainFormula_UN;
        public string FormulaConstant_UN;
        public string Formula_TP_OurSide_UN;
        public long? UANode_ID;
        /// <summary>
        /// Размерность формулы, в которой она составлена (кило, мега, отсутствует)
        /// </summary>
        public EnumUnitDigit? UnitDigit;

        public override bool Equals(object obj)
        {
            var other = obj as TFormulaSelectResult;
            if (other == null) return false;

            if (_hashCode.HasValue && other._hashCode.HasValue)
            {
                return _hashCode == other._hashCode;
            }

            return string.Equals(Formula_UN, other.Formula_UN) && StringNumber == other.StringNumber
                && EqualityComparer<Guid?>.Default.Equals(ClosedPeriod_ID, other.ClosedPeriod_ID);
        }

        private long? _hashCode;
        public override int GetHashCode()
        {
            if (_hashCode.HasValue) return (int)_hashCode.Value;

            if (!string.IsNullOrEmpty(Formula_UN))
            {
                _hashCode = Formula_UN.GetHashCode();
            }

            if (ClosedPeriod_ID.HasValue)
            {
                _hashCode = _hashCode ^ (ClosedPeriod_ID.Value.GetHashCode() * 397);
            }

            //TODO возможно нужно будет обрабатывать StartDateTime

            if (StringNumber > 0)
            {
                _hashCode = _hashCode ^ ((StringNumber << 6) | (StringNumber >> (32 - 6)));
                return (int)_hashCode;
            }

            if (ChannelType.HasValue)
            {
                _hashCode = _hashCode ^ (((uint)ChannelType.Value << 26) | ((uint)ChannelType.Value >> (32 - 26)));
            }

            if (TI_ID.HasValue)
            {
                _hashCode = _hashCode ^ TI_ID.Value;
            }
            else if (TP_ID.HasValue)
            {
                _hashCode = _hashCode ^ TP_ID.Value;
            }
            else if (Section_ID.HasValue)
            {
                _hashCode = _hashCode ^ Section_ID.Value;
            }
            else if (ContrTI_ID.HasValue)
            {
                _hashCode = _hashCode ^ ContrTI_ID.Value;
            }
            else if (UANode_ID.HasValue)
            {
                _hashCode = _hashCode ^ UANode_ID.Value;
            }
            else if (!string.IsNullOrEmpty(FormulaConstant_UN))
            {
                _hashCode = _hashCode ^ FormulaConstant_UN.GetHashCode();
            }
            else if (!string.IsNullOrEmpty(Formula_TP_OurSide_UN))
            {
                _hashCode = _hashCode ^ Formula_TP_OurSide_UN.GetHashCode();
            }

            return (int)_hashCode;
        }
    }

    public class TFormulaSelectResultComparer : IEqualityComparer<TFormulaSelectResult>
    {
        public bool Equals(TFormulaSelectResult x, TFormulaSelectResult y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(TFormulaSelectResult obj)
        {
            return obj.GetHashCode();
        }
    }
}
