
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;
using Proryv.Servers.Calculation.FormulaInterpreter;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    /// <summary>
    /// Ф-мя возвращающая результат предварительного обсчета другой формулы
    /// </summary>
    public class PrecalculatedFunctionVariable
    {
        /// <summary>
        /// Строковое представление
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// Тип 
        ///  </summary>
        public readonly EnumTypeFormulasFunction TypeFormulasFunction;

        /// <summary>
        /// Период дискретизации
        /// </summary>
        public readonly enumTimeDiscreteType DiscreteType;

        internal readonly IFormulaArchivesPrecalculator FormulaArchivesPrecalculator;

        public PrecalculatedFunctionVariable(string id, IFormulaArchivesPrecalculator formulaArchivesPrecalculator, EnumTypeFormulasFunction typeFormulasFunction, enumTimeDiscreteType discreteType)
        {
            FormulaArchivesPrecalculator = formulaArchivesPrecalculator;
            TypeFormulasFunction = typeFormulasFunction;
            DiscreteType = discreteType;
            Id = id.ToLower();
        }

        public TVALUES_DB Calculate(Variable var, int indx)
        {
            //TODO: ContainerINtegration
          //  return default(TVALUES_DB);
            //if (var.Achives == null) return new TVALUES_DB(VALUES_FLAG_DB.IsFormulaNotCorrectEvaluated, 0);


            //if (TypeFormulasFunction == EnumTypeFormulasFunction.Archive)
            //{
            //    return FormulaArchivesPrecalculator.GetArchive(var.Achives.HierarchyId, indx, DiscreteType, var.Achives.GetValues())
            //           ?? new TVALUES_DB(VALUES_FLAG_DB.IsFormulaNotCorrectEvaluated, 0);
            //}

            //var h = FormulaArchivesPrecalculator.Ччи(var.Achives.HierarchyId, indx, DiscreteType);
            //return new TVALUES_DB(VALUES_FLAG_DB.None, h);

            if (var.Achives == null) return new Servers.Calculation.DBAccess.Interface.Data.TVALUES_DB(VALUES_FLAG_DB.IsFormulaNotCorrectEvaluated, 0);


            if (TypeFormulasFunction == EnumTypeFormulasFunction.Archive)
            {
                return FormulaArchivesPrecalculator.GetArchive(var.Achives.HierarchyId, indx, DiscreteType, (List<Servers.Calculation.DBAccess.Interface.Data.TVALUES_DB>)var.Achives.GetValues())
                       ?? new Servers.Calculation.DBAccess.Interface.Data.TVALUES_DB(VALUES_FLAG_DB.IsFormulaNotCorrectEvaluated, 0);
            }

            var h = FormulaArchivesPrecalculator.Ччи(var.Achives.HierarchyId, indx, DiscreteType);
            return new Servers.Calculation.DBAccess.Interface.Data.TVALUES_DB(VALUES_FLAG_DB.None, h);

           }
    }
}
