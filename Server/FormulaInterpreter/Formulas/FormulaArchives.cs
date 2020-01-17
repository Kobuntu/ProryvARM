using Proryv.AskueARM2.Server.DBAccess.Internal.Formulas;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using Proryv.Servers.Calculation.DBAccess.Interface;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.Servers.Calculation.FormulaInterpreter.Formulas
{
    public class FormulaArchives
    {
        /// <summary>
        /// Запрашиваются минутки
        /// </summary>
        public readonly bool IsArchTech;

        public readonly Dictionary<int, HashSet<TI_ChanelType>> TIChanelTypeList;
        public Dictionary<int, List<IArchivesValue>> TIValues;

        public readonly Dictionary<int, HashSet<TI_ChanelType>> IntegralChanelTypeList;
        public Dictionary<int, List<IArchivesValue>> IntegralValues;

        public readonly Dictionary<int, HashSet<TP_ChanelType>> TPChanelTypeList;
        public Dictionary<int, List<IArchivesTPValue>> TPValues;

        public readonly HashSet<TSectionChannel> SectionSorted;
        public List<ISectionTPSummation> SectionValues;

        public readonly HashSet<string> FormulaConstantIds;
        public Dictionary<string, IGetAchives> FormulaConstantDict;

        public readonly List<TUANodeDataId> FormulaUaNodeVariableDataTypeList;
        public Dictionary<long, IFormulaUANode> FormulaUaVariables;

        private readonly int? _tpId;

        /// <summary>
        /// Данные для минуток
        /// </summary>
        public List<ArchTechArchive> ArchivesTech;


        public FormulaArchives(bool isArchTech, int? tpId)
        {
            TIChanelTypeList = new Dictionary<int, HashSet<TI_ChanelType>>();
            IntegralChanelTypeList = new Dictionary<int, HashSet<TI_ChanelType>>();
            TPChanelTypeList = new Dictionary<int, HashSet<TP_ChanelType>>();
            SectionSorted = new HashSet<TSectionChannel>(new SectionChannelEqualityComparer());
            FormulaConstantIds = new HashSet<string>();
            FormulaUaNodeVariableDataTypeList = new List<TUANodeDataId>();

            IsArchTech = isArchTech;
            _tpId = tpId;
        }

        public IGetAchives GetArchiveByOperandType(F_OPERATOR operators)
        {
            IGetAchives data;

            if (operators.OPER_TYPE == F_OPERATOR.F_OPERAND_TYPE.FormulaConstant)
            {
                IGetAchives formulaConstant;
                if (FormulaConstantDict != null && FormulaConstantDict.TryGetValue(operators.OPER_ID, out formulaConstant))
                {
                    data = formulaConstant;
                }
                else
                {
                    data = null;
                }
            }
            else
            {
                if (!operators.TI_CHANNEL.HasValue)
                {
                    throw new FormulaParseException("В формуле не указан канал измерения!");
                }

                var id = operators.OPER_ID.ToInt();
                data = null;
                var isCa = operators.OPER_TYPE == F_OPERATOR.F_OPERAND_TYPE.ContrTI_Chanel;
                switch (operators.OPER_TYPE)
                {
                    case F_OPERATOR.F_OPERAND_TYPE.TI_channel:
                    case F_OPERATOR.F_OPERAND_TYPE.ContrTI_Chanel:
                        if (IsArchTech)
                        {
                            data = ArchivesTech.FirstOrDefault(v => v.ID.ID == id && v.ChannelType == operators.TI_CHANNEL.Value);
                        }
                        else if (TIValues != null)
                        {
                            List<IArchivesValue> avs;
                            if (TIValues.TryGetValue(id, out avs) && avs != null)
                            {
                                data = avs.FirstOrDefault(t => t.IsEqual(isCa, id, operators.TI_CHANNEL.Value, operators.ClosedPeriod_ID, _tpId));
                            }
                        }

                        break;
                    case F_OPERATOR.F_OPERAND_TYPE.Integral_Channel:

                        if (IntegralValues != null)
                        {
                            List<IArchivesValue> ivs;
                            if (IntegralValues.TryGetValue(id, out ivs) && ivs != null)
                            {
                                data = ivs.FirstOrDefault(t => t.IsEqual(isCa, id, operators.TI_CHANNEL.Value, operators.ClosedPeriod_ID, _tpId));
                            }
                        }

                        break;
                    case F_OPERATOR.F_OPERAND_TYPE.TP_channel:
                        if (TPValues != null)
                        {
                            List<IArchivesTPValue> ips;
                            if (TPValues.TryGetValue(id, out ips) && ips != null)
                            {
                                data = ips.FirstOrDefault(t =>
                                    t.TpIdChannel.TP_ID == id && t.TpIdChannel.ChannelType == operators.TI_CHANNEL.Value);
                            }
                        }

                        break;
                    case F_OPERATOR.F_OPERAND_TYPE.Section:
                        if (SectionValues != null && SectionValues.Count > 0)
                        {
                            data = SectionValues.FirstOrDefault(s => s.Section_Id == id && s.ChannelType == operators.TI_CHANNEL.Value);
                        }

                        break;
                    case F_OPERATOR.F_OPERAND_TYPE.UANode:
                        {
                            IFormulaUANode uaNode;
                            if (FormulaUaVariables != null && FormulaUaVariables.TryGetValue(id, out uaNode))
                            {
                                data = uaNode; // && (byte)s.Value.uANodeDataId.DataType == channel т.к. все обработки с одними данными, нет смысла запрашивать одно и тоже
                            }
                        }
                        break;
                }
            }

            return data;
        }
    }
}
