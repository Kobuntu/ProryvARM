using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal.Formulas;

namespace Proryv.Servers.Calculation.FormulaInterpreter
{
    public static class EnumUtils
    {
        public static PrecalculatedFunctionVariable CreatePrecalculatedFormulasFunctionDescription(this Enum e, IFormulaArchivesPrecalculator archivesPrecalculator)
        {
            var description = GetFormulasFunctionDescription(e);

            if (description == null) return null;

            return
                (PrecalculatedFunctionVariable)(Activator.CreateInstance(typeof(PrecalculatedFunctionVariable), description.Id, archivesPrecalculator, description.TypeFormulasFunction, description.DiscreteType));
        }

        public static FormulasFunctionDescription GetFormulasFunctionDescription(this Enum e)
        {
            FormulasFunctionDescription description;
            if (!EnumAtFormulasFunctionDescriptions.TryGetValue(e, out description))
            {
                var s = e.ToString();
                var fi = e.GetType().GetField(s);
                description = fi.GetCustomAttributes(typeof(FormulasFunctionDescription), false).FirstOrDefault() as FormulasFunctionDescription;

                EnumAtFormulasFunctionDescriptions.TryAdd(e, description);
            }

            return description;
        }

        private static readonly ConcurrentDictionary<Enum, FormulasFunctionDescription> EnumAtFormulasFunctionDescriptions = new ConcurrentDictionary<Enum, FormulasFunctionDescription>();



    }
}
