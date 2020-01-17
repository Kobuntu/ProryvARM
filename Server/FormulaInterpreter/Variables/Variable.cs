using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal.Enums;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;
using Proryv.Servers.Calculation.FormulaInterpreter;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    public class Variable
    {
        public readonly string Id;

        public readonly IGetAchives Achives;

        public readonly object UserTag2;

        private readonly bool _isCalculateBetweenIndexes;
        private readonly int _indxStart, _indxEnd;

        public Variable(string id, int indxStart, int indxEnd, 
            IGetAchives achives = null, object userTag2 = null, bool isCalculateBetweenIndexes = true)
        {
            Id = id;
            Achives = achives;
            UserTag2 = userTag2;
            _indxStart = indxStart;
            _indxEnd = indxEnd;
            _isCalculateBetweenIndexes = isCalculateBetweenIndexes;
        }

        public TVALUES_DB GetHalfHourValueByIndex(int indx)
        {
            if (!_isCalculateBetweenIndexes || (indx >= _indxStart && indx <= _indxEnd))
            {
                if (Achives != null)
                {
                    return Achives.TryGetValueByIndex(indx);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new TVALUES_DB(VALUES_FLAG_DB.FormulaNotInRange, 0);
            }
        }

        public TVALUES_DB TryGetNext()
        {
            if (Achives != null)
            {
                return Achives.TryGetNext();
            }
            else
            {
                return new TVALUES_DB(VALUES_FLAG_DB.DataNotFull, 0);
            }
        }
    }
}
