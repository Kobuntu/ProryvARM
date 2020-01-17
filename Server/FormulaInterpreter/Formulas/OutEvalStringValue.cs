using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.Formulas;
using Proryv.AskueARM2.Server.DBAccess.Internal.Utils;
using Proryv.Servers.Calculation.DBAccess.Common;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Proryv.AskueARM2.Server.DBAccess.Internal.Formulas.Polskaya;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    internal struct OutEvalStringValue
    {
        private double m_value;
        private VALUES_FLAG_DB flag;

        public double Value
        {
            get { return m_value; }
            set
            {
                m_value = value;
                flag = VALUES_FLAG_DB.None;
                FunctionDevider = false;
            }
        }

        public bool FunctionDevider;

        public void SetValue(VALUES_FLAG_DB _flag, double _value)
        {
            m_value = _value;
            FunctionDevider = false;
            flag = _flag;
        }

        public void GetValueAndCumulateFlag(ref VALUES_FLAG_DB _flag, ref double _value)
        {
            _value = m_value;
            _flag = _flag.CompareAndReturnMostBadStatus(flag);
        }

        public void GetValueAndCumulateFlag(ref VALUES_FLAG_DB _flag, ref FunctionArgumentList _value,
            ref int index)
        {
            if (StringValue != null)
            {
                _flag = _flag.CompareAndReturnMostBadStatus(flag);
                _value[index] = new StringFunctionArgument() { ArgumentValue = StringValue, flag = _flag };

            }
            else
            {
                _flag = _flag.CompareAndReturnMostBadStatus(flag);
                _value[index] = new DoubleFunctionArgumnet() { ArgumentValue = m_value, flag = _flag };

            }
        }


        public TVALUES_DB DBValue
        {
            get
            {

                // return default(TVALUES_DB);
                //TODO: ContainerINtegration
                //return  new TVALUES_DB(flag, m_value);

                return new TVALUES_DB(flag, m_value);
            }
        }

        public void Operate(OutEvalStringValue operand, Operations operation)
        {
            flag = flag.CompareAndReturnMostBadStatus(operand.flag);
            switch (operation)
            {
                case Operations.adding:
                    m_value = (m_value + operand.Value);
                    break;
                case Operations.division:
                    if (operand.Value == 0)
                    {
                        m_value = 0;
                        flag = VALUES_FLAG_DB.DivideByZero;
                    }
                    else
                    {
                        m_value = (m_value / operand.Value);
                    }
                    break;

                case Operations.multiplication:
                    m_value = (m_value * operand.Value);
                    break;

                case Operations.subtraction:
                    m_value = (m_value - operand.Value);
                    break;

                case Operations.func_devider:
                    return; // ничего делать с разделителем не надо

                default:
                    throw new Exception("Выражение некорректно!");
            }
        }

        public object Tag;

        public string StringValue { get; set; }
    }
}
