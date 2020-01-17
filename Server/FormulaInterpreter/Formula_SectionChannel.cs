using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.Servers.Calculation.DBAccess.Interface;

namespace Proryv.Servers.Calculation.FormulaInterpreter
{
    class Formula_SectionChannel : ISectionChannel
    {
        /// <summary>
        /// Сечение
        /// </summary>
      
        public int Section_ID { get; set; }
        /// <summary>
        /// Канал
        /// </summary>
     
        public byte ChannelType { get; set; }
    }
}
