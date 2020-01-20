using System;
using System.Collections.Generic;
using System.Text;

namespace Proryv.Servers.Calculation.Parser.Internal
{
    public partial class ProryvParser
    {
        #region Lexer

        #region GetNextLexem
        // Получение очередной лексемы.
        private StiToken GetNextLexem()
        {
            //пропустить пробелы, символы табуляции и другие незначащие символы
            while (position < inputExpression.Length && isWhiteSpace(inputExpression[position])) position++;
            if (position >= inputExpression.Length) return null;

            StiToken token = null;
            char ch = inputExpression[position];
            if (char.IsLetter(ch))
            {
                int pos2 = position + 1;
                while ((pos2 < inputExpression.Length) && (char.IsLetterOrDigit(inputExpression[pos2]) || inputExpression[pos2] == '_')) pos2++;
                token = new StiToken();
                token.Value = inputExpression.Substring(position, pos2 - position);
                token.Type = StiTokenType.Identifier;
                token.Position = position;
                token.Length = pos2 - position;
                position = pos2;

                string alias = token.Value;
                if ((token.Position > 0) && (inputExpression[token.Position - 1] == '.')) alias = "." + alias;
                if (hashAliases!=null && hashAliases.ContainsKey(alias))
                {
                    token.Value = (string)hashAliases[alias];
                }

                return token;
            }
            else if (char.IsDigit(ch))
            {
                token = new StiToken();
                token.Type = StiTokenType.Number;
                token.Position = position;
                token.ValueObject = ScanNumber();
                token.Length = position - token.Position;
                return token;
            }
            else if ((ch == '"') || ((ch == '@') && (position < inputExpression.Length - 1) && (inputExpression[position + 1] == '"')))
            {
                #region "String"
                bool needReplaceBackslash = true;
                if (ch == '@')
                {
                    needReplaceBackslash = false;
                    position++;
                }

                position++;
                int pos2 = position;
                while (pos2 < inputExpression.Length)
                {
                    if (inputExpression[pos2] == '"') break;
                    if (inputExpression[pos2] == '\\') pos2++;
                    pos2++;
                }
                token = new StiToken();
                token.Type = StiTokenType.String;
                //token.ValueObject = inputExpression.Substring(position, pos2 - position).Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\\"", "\"").Replace("\\'", "'").Replace("\\\\", "\\");
                string st = inputExpression.Substring(position, pos2 - position);
                if (needReplaceBackslash)
                {
                    token.ValueObject = ReplaceBackslash(st);
                }
                else
                {
                    token.ValueObject = st;
                }

                token.Position = position - 1;
                position = pos2 + 1;
                token.Length = position - token.Position;
                return token;
                #endregion
            }
            else
            {
                #region check for alias bracket
                if (ch == '[')
                {
                    int pos2 = inputExpression.IndexOf(']', position);
                    if (pos2 != -1)
                    {
                        pos2++;
                        string alias = inputExpression.Substring(position, pos2 - position);
                        if ((position > 0) && (inputExpression[position - 1] == '.'))
                        {
                            alias = "." + alias; 
                        }
                        if (hashAliases!=null && hashAliases.ContainsKey(alias))
                        {
                            token = new StiToken
                            {
                                Value = (string) hashAliases[alias],
                                Type = StiTokenType.Identifier,
                                Position = position,
                                Length = pos2 - position
                            };
                            position = pos2;
                            return token;
                        }
                    }
                }
                #endregion

                #region Delimiters
                int tPos = position;
                position++;
                char ch2 = ' ';
                if (position < inputExpression.Length) ch2 = inputExpression[position];
                switch (ch)
                {
                    case '.': return new StiToken(StiTokenType.Dot, tPos, 1);
                    case '(': return new StiToken(StiTokenType.LParenthesis, tPos, 1);
                    case ')': return new StiToken(StiTokenType.RParenthesis, tPos, 1);
                    case '[': return new StiToken(StiTokenType.LBracket, tPos, 1);
                    case ']': return new StiToken(StiTokenType.RBracket, tPos, 1);
                    case '+': return new StiToken(StiTokenType.Plus, tPos, 1);
                    case '-': return new StiToken(StiTokenType.Minus, tPos, 1);
                    case '*': return new StiToken(StiTokenType.Mult, tPos, 1);
                    case '/': return new StiToken(StiTokenType.Div, tPos, 1);
                    case '%': return new StiToken(StiTokenType.Percent, tPos, 1);
                    case '^': return new StiToken(StiTokenType.Xor, tPos, 1);
                    case ',': return new StiToken(StiTokenType.Comma, tPos, 1);
                    case ':': return new StiToken(StiTokenType.Colon, tPos, 1);
                    case ';': return new StiToken(StiTokenType.SemiColon, tPos, 1);
                    case '?': return new StiToken(StiTokenType.Question, tPos, 1);
                    case '|':
                        if (ch2 == '|')
                        {
                            position++;
                            return new StiToken(StiTokenType.DoubleOr, tPos, 2);
                        }
                        else return new StiToken(StiTokenType.Or, tPos, 1);
                    case '&':
                        if (ch2 == '&')
                        {
                            position++;
                            return new StiToken(StiTokenType.DoubleAnd, tPos, 2);
                        }
                        else return new StiToken(StiTokenType.And, tPos, 1);
                    case '!':
                        if (ch2 == '=' && !isVB)
                        {
                            position++;
                            return new StiToken(StiTokenType.NotEqual, tPos, 2);
                        }
                        else return new StiToken(StiTokenType.Not, tPos, 1);
                    case '=':
                        if (ch2 == '=')
                        {
                            position++;
                            return new StiToken(StiTokenType.Equal, tPos, 2);
                        }
                        else return new StiToken(StiTokenType.Assign, tPos, 1);
                    case '<':
                        if (ch2 == '<')
                        {
                            position++;
                            return new StiToken(StiTokenType.Shl, tPos, 2);
                        }
                        else if (ch2 == '=')
                        {
                            position++;
                            return new StiToken(StiTokenType.LeftEqual, tPos, 2);
                        }
                        else if (ch2 == '>' && isVB)
                        {
                            position++;
                            return new StiToken(StiTokenType.NotEqual, tPos, 2);
                        }
                        else return new StiToken(StiTokenType.Left, tPos, 1);
                    case '>':
                        if (ch2 == '>')
                        {
                            position++;
                            return new StiToken(StiTokenType.Shr, tPos, 2);
                        }
                        else if (ch2 == '=')
                        {
                            position++;
                            return new StiToken(StiTokenType.RightEqual, tPos, 2);
                        }
                        else return new StiToken(StiTokenType.Right, tPos, 1);

                    default:
                        token = new StiToken(StiTokenType.Unknown);
                        token.ValueObject = ch;
                        token.Position = tPos;
                        token.Length = 1;
                        return token;
                }
                #endregion
            }
        }

        private static bool isWhiteSpace(char ch)
        {
            return char.IsWhiteSpace(ch) || ch < 0x20;
        }
        #endregion

        #region BuildAliases

        private static bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name) || !(Char.IsLetter(name[0]) || name[0] == '_'))
                return false;

            for (int pos = 0; pos < name.Length; pos++)
                if (!(Char.IsLetterOrDigit(name[pos]) || (name[pos] == '_'))) return false;

            return true;
        }

        private static string GetCorrectedAlias(string alias)
        {
            if (IsValidName(alias)) return alias;
            return string.Format("[{0}]", alias);
        }
        #endregion

        #region ReplaceBackslash
        private static string ReplaceBackslash(string input)
        {
            StringBuilder output = new StringBuilder();
            for (int index = 0; index < input.Length; index++)
            {
                if ((input[index] == '\\') && (index < input.Length - 1))
                {
                    index++;
                    char ch = input[index];
                    switch (ch)
                    {
                        case '\\':
                            output.Append("\\");
                            break;

                        case '\'':
                            output.Append('\'');
                            break;

                        case '"':
                            output.Append('"');
                            break;

                        case '0':
                            output.Append('\0');
                            break;

                        case 'n':
                            output.Append('\n');
                            break;

                        case 'r':
                            output.Append('\r');
                            break;

                        case 't':
                            output.Append('\t');
                            break;

                        case 'x':
                            StringBuilder sbHex = new StringBuilder();
                            int hexLen = 0;
                            while ((index < input.Length - 1) && (hexLen < 4) && ("0123456789abcdefABCDEF".IndexOf(input[index + 1]) != -1))
                            {
                                sbHex.Append(input[index + 1]);
                                index++;
                                hexLen++;
                            }
                            int resInt = 0;
                            bool resBool = int.TryParse(sbHex.ToString(), System.Globalization.NumberStyles.HexNumber, null, out resInt);
                            output.Append((char)resInt);
                            break;

                        default:
                            output.Append("\\" + ch);
                            break;
                    }
                }
                else
                {
                    output.Append(input[index]);
                }
            }

            return output.ToString();
        }
        #endregion

        #region ScanNumber
        private object ScanNumber()
        {
            TypeCode typecode = TypeCode.Int32;
            int posBegin = position;
            int posBeginAll = position;
            //integer part
            while (position != inputExpression.Length && Char.IsDigit(inputExpression[position]))
            {
                position++;
            }
            if (position != inputExpression.Length && inputExpression[position] == '.' &&
                position + 1 != inputExpression.Length && Char.IsDigit(inputExpression[position + 1]))
            {
                //fractional part
                position++;
                while (position != inputExpression.Length && Char.IsDigit(inputExpression[position]))
                {
                    position++;
                }
                typecode = TypeCode.Double;
            }
            string nm = inputExpression.Substring(posBegin, position - posBegin);
            nm = nm.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            if (position != inputExpression.Length && Char.IsLetter(inputExpression[position]))
            {
                //postfix
                posBegin = position;
                while (position != inputExpression.Length && Char.IsLetter(inputExpression[position]))
                {
                    position++;
                }
                string postfix = inputExpression.Substring(posBegin, position - posBegin).ToLower();
                if (postfix == "f") typecode = TypeCode.Single;
                if (postfix == "d") typecode = TypeCode.Double;
                if (postfix == "m") typecode = TypeCode.Decimal;
                if (postfix == "l") typecode = TypeCode.Int64;
                if (postfix == "u" || postfix == "ul" || postfix == "lu") typecode = TypeCode.UInt64;
            }

            if ((typecode == TypeCode.Int32) && (nm.Length > 9)) typecode = TypeCode.Int64;

            object result = null;
            try
            {
                result = Convert.ChangeType(nm, typecode);
            }
            catch
            {
                if (typecode == TypeCode.Int32 || typecode == TypeCode.Int64 || typecode == TypeCode.UInt32 || typecode == TypeCode.UInt64)
                {
                    ThrowError(ParserErrorCode.IntegralConstantIsTooLarge, new StiToken(StiTokenType.Number, posBeginAll, position - posBeginAll));
                }
            }
            return result;
        }
        #endregion

        #region PostProcessTokensList
        private List<StiToken> PostProcessTokensList(List<StiToken> tokensList)
        {
            List<StiToken> newList = new List<StiToken>();
            tokenPos = 0;
            while (tokenPos < tokensList.Count)
            {
                StiToken token = tokensList[tokenPos];
                tokenPos++;
                if (token.Type == StiTokenType.Identifier)
                {
                    if ((newList.Count > 0) && (newList[newList.Count - 1].Type == StiTokenType.Dot))
                    {
                        if (MethodsList.Contains(token.Value))
                        {
                            token.Type = StiTokenType.Method;
                        }
                        //Proryv
                        else if (Equals(token.Value, "Value") || ProryvPropertiesList.Contains(token.Value) || PropertiesList.Contains(token.Value))
                        {
                            token.Type = StiTokenType.Property;
                        }
                        else
                        {
                            ThrowError(ParserErrorCode.FieldMethodOrPropertyNotFound, token, token.Value);
                        }
                    }
                    else if (TypesList.Contains(token.Value))
                    {
                        token.Type = StiTokenType.Cast;

                        if ((tokenPos + 1 < tokensList.Count) && (tokensList[tokenPos].Type == StiTokenType.Dot))
                        {
                            string tempName = token.Value + "." + tokensList[tokenPos + 1].Value;
                            if (FunctionsList.Contains(tempName))
                            {
                                token.Type = StiTokenType.Function;
                                token.Value = tempName;
                                tokenPos += 2;
                            }
                            //Proryv
                            else if (ProryvFunctionsList.Contains(tempName))
                            {
                                token.Type = StiTokenType.ProryvFunction;
                                token.Value = tempName;
                                tokenPos += 2;
                            }
                            if (SystemVariablesList.Contains(tempName))
                            {
                                token.Type = StiTokenType.SystemVariable;
                                token.Value = tempName;
                                tokenPos += 2;
                            }
                        }
                    }

                    //Proryv
                    else if (ProryvFunctionsList.Contains(token.Value))
                    {
                        token.Type = StiTokenType.ProryvFunction;
                    }
                    else if (_proryvSpreadsheetProperties!=null && _proryvSpreadsheetProperties.ContainsKey(token.Value.ToLowerInvariant()))
                    {
                        token.Type = StiTokenType.ProryvSpreadsheetProperties;
                    }
                    else if (_proryvFreeHierarchyBalanceSignature!=null && _proryvFreeHierarchyBalanceSignature.ContainsKey(token.Value.ToLowerInvariant()))
                    {
                        token.Type = StiTokenType.ProryvFreeHierarchyBalanceSignature;
                    }

                    else if (FunctionsList.Contains(token.Value))
                    {
                        while ((ProryvFunctionType)FunctionsList[token.Value] == ProryvFunctionType.NameSpace)
                        {
                            if (tokenPos + 1 >= tokensList.Count) ThrowError(ParserErrorCode.UnexpectedEndOfExpression);
                            var np = tokensList[tokenPos + 1].Value;
                            token.Value += "." + np;
                            tokenPos += 2;
                            if (!FunctionsList.Contains(token.Value))
                            {
                                if (FunctionsList.Contains(np)) token.Value = np;
                                else ThrowError(ParserErrorCode.FunctionNotFound, token, token.Value);
                            }
                        }
                        token.Type = StiTokenType.Function;
                    }

                    else if (SystemVariablesList.Contains(token.Value) && (token.Value != "value"))
                    {
                        token.Type = StiTokenType.SystemVariable;
                    }

                    //else if (token.Value.ToLowerInvariant() == "true" || token.Value.ToLowerInvariant() == "false")
                    //{
                    //    if (token.Value.ToLowerInvariant() == "true")
                    //        token.ValueObject = true;
                    //    else
                    //        token.ValueObject = false;
                    //    token.Type = StiTokenType.Number;
                    //}
                    //else if (token.Value.ToLowerInvariant() == "null")
                    //{
                    //    token.ValueObject = null;
                    //    token.Type = StiTokenType.Number;
                    //}

                    else if (ConstantsList.Contains(token.Value))
                    {
                        while (ConstantsList[token.Value] == namespaceObj)
                        {
                            if (tokenPos + 1 >= tokensList.Count) ThrowError(ParserErrorCode.UnexpectedEndOfExpression);
                            string oldTokenValue = token.Value;
                            token.Value += "." + tokensList[tokenPos + 1].Value;
                            tokenPos += 2;
                            if (!ConstantsList.Contains(token.Value)) ThrowError(ParserErrorCode.ItemDoesNotContainDefinition, token, oldTokenValue, tokensList[tokenPos + 1].Value);
                        }
                        token.ValueObject = ConstantsList[token.Value];
                        token.Type = StiTokenType.Number;
                    }

                    else if (UserFunctionsList.Contains(token.Value))
                    {
                        token.Type = StiTokenType.Function;
                    }

                    else
                    {
                        ThrowError(ParserErrorCode.NameDoesNotExistInCurrentContext, token, token.Value);
                    }
                }
                newList.Add(token);
            }
            return newList;
        }

        #endregion

        private void MakeTokensList()
        {
            tokensList = new List<StiToken>();
            position = 0;
            while (true)
            {
                StiToken token = GetNextLexem();
                if (token == null) break;
                tokensList.Add(token);
            }
            tokensList = PostProcessTokensList(tokensList);
        }

        #endregion
    }
}
