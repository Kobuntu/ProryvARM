using System;
using System.Collections.Generic;

namespace Proryv.Servers.Calculation.Parser.Internal
{
    public partial class ProryvParser
    {
        #region Parser

        //----------------------------------------
        // ����� ����� �����������
        //----------------------------------------
        private void eval_exp()
        {
            tokenPos = 0;
            if (tokensList.Count == 0)
            {
                ThrowError(ParserErrorCode.ExpressionIsEmpty);  //������ ���������
                return;
            }
            eval_exp0();
            if (tokenPos <= tokensList.Count)
            {
                ThrowError(ParserErrorCode.UnprocessedLexemesRemain);  // �������� �������������� �������
            }
        }

        private void eval_exp0()
        {
            get_token();
            eval_exp01();
        }


        //----------------------------------------
        // ��������� ������������
        //----------------------------------------
        private void eval_exp01()
        {
            if (currentToken.Type == StiTokenType.Variable)
            {
                StiToken variableToken = currentToken;
                get_token();
                if (currentToken.Type != StiTokenType.Assign)
                {
                    tokenPos--;
                    currentToken = tokensList[tokenPos - 1];
                }
                else
                {
                    get_token();
                    eval_exp1();
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.CopyToVariable, variableToken.Value));
                    return;
                }
            }
            eval_exp1();
        }

        //----------------------------------------
        // Conditional Operator  ? : 
        //----------------------------------------
        private void eval_exp1()
        {
            eval_exp10();
            if (currentToken.Type == StiTokenType.Question)
            {
                get_token();
                eval_exp10();
                if (currentToken.Type != StiTokenType.Colon) ThrowError(ParserErrorCode.SyntaxError, currentToken);  //c������������� ������
                get_token();
                eval_exp10();
                asmList.Add(new StiAsmCommand(StiAsmCommandType.PushFunction, ProryvFunctionType.IIF, 3));
            }
        }


        //----------------------------------------
        // ���������� ���
        //----------------------------------------
        private void eval_exp10()
        {
            eval_exp11();
            while (currentToken.Type == StiTokenType.DoubleOr)
            {
                get_token();
                eval_exp11();
                asmList.Add(new StiAsmCommand(StiAsmCommandType.Or2));
            }
        }

        //----------------------------------------
        // ���������� �
        //----------------------------------------
        private void eval_exp11()
        {
            eval_exp12();
            while (currentToken.Type == StiTokenType.DoubleAnd)
            {
                get_token();
                eval_exp12();
                asmList.Add(new StiAsmCommand(StiAsmCommandType.And2));
            }
        }

        //----------------------------------------
        // �������� ���
        //----------------------------------------
        private void eval_exp12()
        {
            eval_exp14();
            if (currentToken.Type == StiTokenType.Or)
            {
                get_token();
                eval_exp14();
                asmList.Add(new StiAsmCommand(StiAsmCommandType.Or));
            }
        }

        //----------------------------------------
        // �������� ����������� ���
        //----------------------------------------
        private void eval_exp14()
        {
            eval_exp15();
            if (currentToken.Type == StiTokenType.Xor)
            {
                get_token();
                eval_exp15();
                asmList.Add(new StiAsmCommand(StiAsmCommandType.Xor));
            }
        }

        //----------------------------------------
        // �������� �
        //----------------------------------------
        private void eval_exp15()
        {
            eval_exp16();
            if (currentToken.Type == StiTokenType.And)
            {
                get_token();
                eval_exp16();
                asmList.Add(new StiAsmCommand(StiAsmCommandType.And));
            }
        }


        //----------------------------------------
        // Equality (==, !=)
        //----------------------------------------
        private void eval_exp16()
        {
            eval_exp17();
            if (currentToken.Type == StiTokenType.Equal || currentToken.Type == StiTokenType.NotEqual)
            {
                StiAsmCommand command = new StiAsmCommand(StiAsmCommandType.CompareEqual);
                if (currentToken.Type == StiTokenType.NotEqual) command.Type = StiAsmCommandType.CompareNotEqual;
                get_token();
                eval_exp17();
                asmList.Add(command);
            }
        }

        //----------------------------------------
        // Relational and type testing (<, >, <=, >=, is, as)
        //----------------------------------------
        private void eval_exp17()
        {
            eval_exp18();
            if (currentToken.Type == StiTokenType.Left || currentToken.Type == StiTokenType.LeftEqual ||
                currentToken.Type == StiTokenType.Right || currentToken.Type == StiTokenType.RightEqual)
            {
                StiAsmCommand command = null;
                if (currentToken.Type == StiTokenType.Left) command = new StiAsmCommand(StiAsmCommandType.CompareLeft);
                if (currentToken.Type == StiTokenType.LeftEqual) command = new StiAsmCommand(StiAsmCommandType.CompareLeftEqual);
                if (currentToken.Type == StiTokenType.Right) command = new StiAsmCommand(StiAsmCommandType.CompareRight);
                if (currentToken.Type == StiTokenType.RightEqual) command = new StiAsmCommand(StiAsmCommandType.CompareRightEqual);
                get_token();
                eval_exp18();
                asmList.Add(command);
            }
        }


        //----------------------------------------
        // Shift (<<, >>)
        //----------------------------------------
        private void eval_exp18()
        {
            eval_exp2();
            if ((currentToken.Type == StiTokenType.Shl) || (currentToken.Type == StiTokenType.Shr))
            {
                StiAsmCommand command = new StiAsmCommand(StiAsmCommandType.Shl);
                if (currentToken.Type == StiTokenType.Shr) command.Type = StiAsmCommandType.Shr;
                get_token();
                eval_exp2();
                asmList.Add(command);
            }
        }


        //----------------------------------------
        // �������� ��� ��������� ���� ���������
        //----------------------------------------
        private void eval_exp2()
        {
            eval_exp3();
            while ((currentToken.Type == StiTokenType.Plus) || (currentToken.Type == StiTokenType.Minus))
            {
                StiToken operation = currentToken;
                get_token();
                eval_exp3();
                if (operation.Type == StiTokenType.Minus)
                {
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.Sub));
                }
                else if (operation.Type == StiTokenType.Plus)
                {
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.Add));
                }
            }
        }


        //----------------------------------------
        // ��������� ��� ������� ���� ����������
        //----------------------------------------
        private void eval_exp3()
        {
            eval_exp4();
            while (currentToken.Type == StiTokenType.Mult || currentToken.Type == StiTokenType.Div || currentToken.Type == StiTokenType.Percent)
            {
                StiToken operation = currentToken;
                get_token();
                eval_exp4();
                if (operation.Type == StiTokenType.Mult)
                {
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.Mult));
                }
                else if (operation.Type == StiTokenType.Div)
                {
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.Div));
                }
                if (operation.Type == StiTokenType.Percent)
                {
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.Mod));
                }
            }
        }


        //----------------------------------------
        // ���������� � �������
        //----------------------------------------
        private void eval_exp4()
        {
            eval_exp5();
            //if (currentToken.Type == StiTokenType.Xor)
            //{
            //    get_token();
            //    eval_exp4();
            //    asmList.Add(new StiAsmCommand(StiAsmCommandType.Power));
            //}
        }


        //----------------------------------------
        // ���������� �������� + � -
        //----------------------------------------
        private void eval_exp5()
        {
            StiAsmCommand command = null;
            if (currentToken.Type == StiTokenType.Plus || currentToken.Type == StiTokenType.Minus || currentToken.Type == StiTokenType.Not)
            {
                if (currentToken.Type == StiTokenType.Minus) command = new StiAsmCommand(StiAsmCommandType.Neg);
                if (currentToken.Type == StiTokenType.Not) command = new StiAsmCommand(StiAsmCommandType.Not);
                get_token();
            }
            eval_exp6();
            if (command != null)
            {
                asmList.Add(command);
            }
        }


        //----------------------------------------
        // ��������� ��������� � �������
        //----------------------------------------
        private void eval_exp6()
        {
            if (currentToken.Type == StiTokenType.LParenthesis)
            {
                get_token();
                if (currentToken.Type == StiTokenType.Cast)
                {
                    TypeCode typeCode = (TypeCode)TypesList[currentToken.Value];
                    get_token();
                    if (currentToken.Type != StiTokenType.RParenthesis)
                    {
                        ThrowError(ParserErrorCode.RightParenthesisExpected);  //������������������ ������
                    }
                    get_token();
                    eval_exp5();
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.Cast, typeCode));
                }
                else
                {
                    eval_exp1();
                    if (currentToken.Type != StiTokenType.RParenthesis)
                    {
                        ThrowError(ParserErrorCode.RightParenthesisExpected);  //������������������ ������
                    }
                    get_token();
                    if (currentToken.Type == StiTokenType.Dot)
                    {
                        get_token();
                        eval_exp7();
                    }
                    if (currentToken.Type == StiTokenType.LBracket)
                    {
                        eval_exp62();
                    }
                }
            }
            else
            {
                eval_exp62();
            }
        }


        //----------------------------------------
        // ��������� ��������
        //----------------------------------------
        private void eval_exp62()
        {
            if (currentToken.Type == StiTokenType.LBracket)
            {
                int argsCount = 0;
                while (argsCount == 0 || currentToken.Type == StiTokenType.Comma)
                {
                    get_token();
                    eval_exp1();
                    argsCount++;
                }
                if (currentToken.Type != StiTokenType.RBracket)
                {
                    ThrowError(ParserErrorCode.SyntaxError, currentToken);  //������������������ ���������� ������  //!!!
                }
                asmList.Add(new StiAsmCommand(StiAsmCommandType.PushArrayElement, argsCount + 1));
                get_token();
                if (currentToken.Type == StiTokenType.LBracket)
                {
                    eval_exp62();
                }
                if (currentToken.Type == StiTokenType.Dot)
                {
                    get_token();
                    eval_exp7();
                }
            }
            else
            {
                eval_exp7();
            }
        }


        //----------------------------------------
        // ���������� ������� � �������
        //----------------------------------------
        private void eval_exp7()
        {
            atom();
            if (currentToken.Type == StiTokenType.Dot)
            {
                get_token();
                eval_exp7();
            }
            if (currentToken.Type == StiTokenType.LBracket)
            {
                eval_exp62();
            }
        }


        //----------------------------------------
        // ��������� �������� ����� ��� ����������
        //----------------------------------------
        private void atom()
        {
            switch (currentToken.Type)
            {
                case StiTokenType.Variable:
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.PushVariable, currentToken.Value));
                    get_token();
                    return;
                case StiTokenType.SystemVariable:
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.PushSystemVariable, SystemVariablesList[currentToken.Value]));
                    get_token();
                    return;
                case StiTokenType.Function:
                {
                    StiToken function = currentToken;
                    ProryvFunctionType functionType;
                    var objFunc = FunctionsList[function.Value];
                    if (objFunc != null)
                    {
                        functionType = (ProryvFunctionType)objFunc;
                    }
                    else
                    {
                        functionType = (ProryvFunctionType)UserFunctionsList[function.Value];
                    }
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.PushFunction, functionType, get_args_count(functionType)));
                    get_token();
                    return;
                }
                case StiTokenType.ProryvFunction:
                    var args = get_args();
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.PushFunction, ProryvFunctionType.proryvFunction, args.Count));
                    get_token();
                    return;
                case StiTokenType.ProryvSpreadsheetProperties:
                {
                    StiToken function = currentToken;
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.ProryvSpreadsheetProperty, _proryvSpreadsheetProperties[function.Value.ToLowerInvariant()]));
                    get_token();
                    return;
                }
                case StiTokenType.ProryvFreeHierarchyBalanceSignature:
                {
                    StiToken function = currentToken;
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.ProryvFreeHierarchyBalanceSignature, _proryvFreeHierarchyBalanceSignature[function.Value.ToLowerInvariant()]));
                    get_token();
                    return;
                }
                case StiTokenType.Method:
                    StiToken method = currentToken;
                    StiMethodType methodType = (StiMethodType)MethodsList[method.Value];
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.PushMethod, methodType, get_args_count(methodType) + 1));
                    get_token();
                    return;
                case StiTokenType.Property:
                {
                    StiToken function = currentToken;
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.PushProperty, PropertiesList[function.Value]));
                    get_token();
                    return;
                }
                case StiTokenType.DataSourceField:
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.PushDataSourceField, currentToken.Value));
                    get_token();
                    return;
                case StiTokenType.BusinessObjectField:
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.PushBusinessObjectField, currentToken.Value));
                    get_token();
                    return;
                case StiTokenType.Number:
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.PushValue, currentToken.ValueObject));
                    get_token();
                    return;
                case StiTokenType.String:
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.PushValue, currentToken.ValueObject));
                    get_token();
                    return;
                default:
                    if (currentToken.Type == StiTokenType.Empty)
                    {
                        ThrowError(ParserErrorCode.UnexpectedEndOfExpression);  //����������� ����� ���������
                    }
                    ThrowError(ParserErrorCode.SyntaxError, currentToken);  //c������������� ������
                    break;
            }
        }


        //----------------------------------------
        // ��������� ���������� �������
        //----------------------------------------
        private int get_args_count(object name)
        {
            var args = get_args();

            //��������� ����� ��������� ���� �������� � ���� ���������
            int bitsValue = 0;
            if (ParametersList.Contains(name)) bitsValue = (int)ParametersList[name];
            int bitsCounter = 1;
            foreach (var arg in args)
            {
                if ((bitsValue & bitsCounter) > 0)
                {
                    asmList.Add(new StiAsmCommand(StiAsmCommandType.PushValue, arg));
                }
                else
                {
                    asmList.AddRange(arg);
                }
                bitsCounter = bitsCounter << 1;
            }

            return args.Count;
        }

        private List<List<StiAsmCommand>> get_args()
        {
            var args = new List<List<StiAsmCommand>>();

            get_token();
            if (currentToken.Type != StiTokenType.LParenthesis) ThrowError(ParserErrorCode.LeftParenthesisExpected);   //��������� ����������� ������
            get_token();
            if (currentToken.Type == StiTokenType.RParenthesis)
            {
                //������ ������
                return args;
            }
            else
            {
                tokenPos--;
                currentToken = tokensList[tokenPos - 1];
            }

            //��������� ������ ��������
            List<StiAsmCommand> tempAsmList = asmList;
            do
            {
                asmList = new List<StiAsmCommand>();
                eval_exp0();
                args.Add(asmList);
            }
            while (currentToken.Type == StiTokenType.Comma);
            asmList = tempAsmList;

            if (currentToken.Type != StiTokenType.RParenthesis) ThrowError(ParserErrorCode.RightParenthesisExpected);   //��������� ����������� ������
            return args;
        }


        private void get_token()
        {
            if (tokenPos < tokensList.Count)
            {
                currentToken = tokensList[tokenPos];
            }
            else
            {
                currentToken = new StiToken();
            }
            tokenPos++;
        }

        #endregion
    }
}
