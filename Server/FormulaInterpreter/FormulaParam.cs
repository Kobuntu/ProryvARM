using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.Servers.Calculation.DBAccess.Interface;

namespace Proryv.Servers.Calculation.FormulaInterpreter
{
    class FormulaParam : IFormulaParam
    {

        public string FormulaID { get; set; }

        public ITP_ChanelType tP_CH_ID { get; set; }



        public enumFormulasTable FormulasTable { get; set; }
        public string FormulaName { get; set; }

        /// <summary>
        /// Корректно ли описана формула
        /// </summary>
        private bool _IsFormulaHasCorrectDescription = true;

        public bool IsFormulaHasCorrectDescription
        {
            get { return _IsFormulaHasCorrectDescription; }
            set { _IsFormulaHasCorrectDescription = value; }
        }

        public bool ForAutoUse { get; set; }

        /// <summary>
        /// Признак простой формулы (состоит только из одной ТИ, без коэфф.)
        /// </summary>
        public bool isUsedSingleTi { get; set; }

        public double Voltage { get; set; }
        public int? SortNumber { get; set; }

        public byte FormulaType_ID { get; set; }

        //Период действия формулы
        public DateTime StartDateTime { get; set; }

        public DateTime? FinishDateTime { get; set; }

        public double? HighLimit { get; set; }
        public double? LowerLimit { get; set; }



        public HashSet<TFormulaSelectResult> UsedFormulas;

        /// <summary>
        /// Часовой пояс для данной ТП
        /// </summary>
        public string MsTimeZoneId { get; set; }

        /// <summary>
        /// Закрытый период в котором формула действует
        /// </summary>
        public Guid? ClosedPeriod_ID { get; set; }

        /// <summary>
        /// Идентификатор ед. изм.
        /// </summary>
        public string MeasureUnit_UN { get; set; }
    }
}
