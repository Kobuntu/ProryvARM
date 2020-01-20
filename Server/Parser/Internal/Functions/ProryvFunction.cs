using System;
using System.Text;

namespace Proryv.Servers.Calculation.Parser.Internal.Functions
{
    public class ProryvFunction : IComparable
    {
        #region IComparable
        public int CompareTo(object obj)
        {
            ProryvFunction function = obj as ProryvFunction;
            return this.functionName.CompareTo(function.functionName);
        }
        #endregion

        #region Properties
        private bool useFullPath = true;
        public bool UseFullPath
        {
            get
            {
                return useFullPath;
            }
            set
            {
                useFullPath = value;
            }
        }


        private string category = string.Empty;
        /// <summary>
        /// Gets or sets category of function.
        /// </summary>
        public string Category
        {
            get
            {
                return category;
            }
            set
            {
                category = value;
            }
        }

        private string groupFunctionName = string.Empty;
        /// <summary>
        /// Gets or sets name of group.
        /// </summary>
        public string GroupFunctionName
        {
            get
            {
                return groupFunctionName;
            }
            set
            {
                groupFunctionName = value;
            }
        }


        private string functionName = string.Empty;
        /// <summary>
        /// Gets or sets name of function.
        /// </summary>
        public string FunctionName
        {
            get
            {
                return functionName;
            }
            set
            {
                functionName = value;
            }
        }


        private string description = string.Empty;
        /// <summary>
        /// Gets or sets description of function.
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }


        private Type typeOfFunction = null;
        /// <summary>
        /// Gets or sets type which contain method of function.
        /// </summary>
        public Type TypeOfFunction
        {
            get
            {
                return typeOfFunction;
            }
            set
            {
                typeOfFunction = value;
            }
        }


        private Type returnType = null;
        /// <summary>
        /// Gets or sets return type of function.
        /// </summary>
        public Type ReturnType
        {
            get
            {
                return returnType;
            }
            set
            {
                returnType = value;
            }
        }


        private string returnDescription = string.Empty;
        /// <summary>
        /// Gets or sets description of returns.
        /// </summary>
        public string ReturnDescription
        {
            get
            {
                return returnDescription;
            }
            set
            {
                returnDescription = value;
            }
        }


        private Type[] argumentTypes = null;
        /// <summary>
        /// Gets or sets array which contain types of arguments.
        /// </summary>
        public Type[] ArgumentTypes
        {
            get
            {
                return argumentTypes;
            }
            set
            {
                argumentTypes = value;
            }
        }


        private string[] argumentNames = null;
        /// <summary>
        /// Gets or sets array which contain names of arguments.
        /// </summary>
        public string[] ArgumentNames
        {
            get
            {
                return argumentNames;
            }
            set
            {
                argumentNames = value;
            }
        }


        private string[] argumentDescriptions = null;
        /// <summary>
        /// Gets or sets array which contain descriptions of arguments.
        /// </summary>
        public string[] ArgumentDescriptions
        {
            get
            {
                return argumentDescriptions;
            }
            set
            {
                argumentDescriptions = value;
            }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return FunctionName;
        }


        public string GetLongFunctionString(ProryvReportLanguageType language)
        {
            if (language == ProryvReportLanguageType.CSharp)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(ConvertTypeToString(this.ReturnType, language));
                sb.Append("  ");
                sb.Append(this.FunctionName);
                sb.Append(" (");

                int index = 0;
                if (ArgumentTypes != null)
                {
                    foreach (Type argumentType in this.ArgumentTypes)
                    {
                        string argumentName = this.ArgumentNames[index];
                        if (!argumentType.IsArray)
                        {
                            sb.Append(ConvertTypeToString(argumentType, language));
                            sb.Append(" ");
                        }
                        sb.Append(argumentName);
                        index++;
                        if (index != this.ArgumentTypes.Length)
                            sb.Append(", ");
                    }
                }

                sb.Append(")");
                return sb.ToString();
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(this.FunctionName);
                sb.Append("(");

                int index = 0;
                if (ArgumentTypes != null)
                {
                    foreach (Type argumentType in this.ArgumentTypes)
                    {
                        string argumentName = this.ArgumentNames[index];
                        sb.Append(argumentName);
                        sb.Append(" As ");
                        sb.Append(ConvertTypeToString(argumentType, language));
                        index++;
                        if (index != this.ArgumentTypes.Length)
                            sb.Append(", ");
                    }
                }

                sb.Append(")");
                if (this.ReturnType != typeof(void))
                {
                    sb.Append(" As " + ConvertTypeToString(this.ReturnType, language));
                }

                return sb.ToString();
            }
        }

        public string GetFunctionString(ProryvReportLanguageType language)
        {
            return GetFunctionString(language, true);
        }

        public string GetFunctionString(ProryvReportLanguageType language, bool addFunctionName)
        {
            StringBuilder sb = new StringBuilder();

            if (addFunctionName)
                sb.Append(this.FunctionName);
            sb.Append(" (");

            int index = 0;
            if (ArgumentTypes != null)
            {
                foreach (Type argumentType in this.ArgumentTypes)
                {
                    string argumentName = this.ArgumentNames[index];
                    if (argumentType.IsArray)
                        sb.Append(argumentName);
                    else
                        sb.Append(ConvertTypeToString(argumentType, language));
                    index++;
                    if (index != this.ArgumentTypes.Length)
                        sb.Append(", ");
                }
            }

            sb.Append(")");
            if (this.ReturnType != typeof(void))
            {
                sb.Append(" : " + ConvertTypeToString(this.ReturnType, language));
            }

            return sb.ToString();
        }


        public string ConvertTypeToString(Type type, ProryvReportLanguageType language)
        {
            string typeStr = ConvertTypeToStringEx(type, language);
            if (string.IsNullOrEmpty(typeStr)) return type.ToString();
            return typeStr;
        }
        #endregion

        /// <summary>
        /// Creates a new object of the type StiFunction.
        /// </summary>
        /// <param name="category">Category of function.</param>
        /// <param name="functionName">Name of function.</param>
        /// <param name="typeOfFunction">Type which contain method of function.</param>
        /// <param name="returnType">Return type of function.</param>
        internal ProryvFunction(string category, string functionName, Type typeOfFunction, Type returnType) :
            this(category, functionName, string.Empty, typeOfFunction, returnType,
            string.Empty, null, null)
        {
        }

        /// <summary>
        /// Creates a new object of the type StiFunction.
        /// </summary>
        /// <param name="category">Category of function.</param>
        /// <param name="functionName">Name of function.</param>
        /// <param name="description">Description of function.</param>
        /// <param name="typeOfFunction">Type which contain method of function.</param>
        /// <param name="returnType">Return type of function.</param>
        /// <param name="returnDescription">Description of returns.</param>
        internal ProryvFunction(string category, string functionName, string description, Type typeOfFunction, Type returnType,
            string returnDescription) :
            this(category, functionName, description, typeOfFunction, returnType,
            returnDescription, null, null)
        {
        }


        /// <summary>
        /// Creates a new object of the type StiFunction.
        /// </summary>
        /// <param name="category">Category of function.</param>
        /// <param name="functionName">Name of function.</param>
        /// <param name="description">Description of function.</param>
        /// <param name="typeOfFunction">Type which contain method of function.</param>
        /// <param name="returnType">Return type of function.</param>
        /// <param name="returnDescription">Description of returns.</param>
        /// <param name="argumentTypes">Array which contain types of arguments.</param>
        /// <param name="argumentNames">Array which contain names of arguments.</param>
        internal ProryvFunction(string category, string functionName, string description, Type typeOfFunction, Type returnType,
            string returnDescription, Type[] argumentTypes, string[] argumentNames) :
            this(category, functionName, functionName, description, typeOfFunction, returnType,
                returnDescription, argumentTypes, argumentNames, null)
        {
        }


        /// <summary>
        /// Creates a new object of the type StiFunction.
        /// </summary>
        /// <param name="category">Category of function.</param>
        /// <param name="groupFunctionName">Name of function group.</param>
        /// <param name="functionName">Name of function.</param>
        /// <param name="description">Description of function.</param>
        /// <param name="typeOfFunction">Type which contain method of function.</param>
        /// <param name="returnType">Return type of function.</param>
        /// <param name="returnDescription">Description of returns.</param>
        /// <param name="argumentTypes">Array which contain types of arguments.</param>
        /// <param name="argumentNames">Array which contain names of arguments.</param>
        /// <param name="argumentDescriptions">Array which contain descriptions of arguments.</param>
        internal ProryvFunction(string category, string groupFunctionName, string functionName, string description, Type typeOfFunction, Type returnType,
            string returnDescription, Type[] argumentTypes, string[] argumentNames, string[] argumentDescriptions)
        {
            this.category = category;
            this.description = description;
            this.returnDescription = returnDescription;
            this.groupFunctionName = groupFunctionName;
            this.functionName = functionName;
            this.typeOfFunction = typeOfFunction;
            this.returnType = returnType;
            this.argumentTypes = argumentTypes;
            this.argumentNames = argumentNames;
            this.argumentDescriptions = argumentDescriptions;

            if (argumentNames != null && argumentTypes != null &&
                argumentNames.Length != argumentTypes.Length)
                throw new ArgumentException("Length of array 'argumentNames' must be equal to length of array 'argumentTypes'!");
        }

        public static string ConvertTypeToStringEx(Type type, ProryvReportLanguageType language)
        {
            if (language == ProryvReportLanguageType.CSharp)
            {
                if (type == typeof(int)) return "int";
                if (type == typeof(uint)) return "uint";
                if (type == typeof(long)) return "long";
                if (type == typeof(ulong)) return "ulong";
                if (type == typeof(string)) return "string";
                if (type == typeof(bool)) return "bool";
                if (type == typeof(object)) return "object";
                if (type == typeof(void)) return "void";
                if (type == typeof(byte)) return "byte";
                if (type == typeof(sbyte)) return "sbyte";
                if (type == typeof(short)) return "short";
                if (type == typeof(ushort)) return "ushort";
                if (type == typeof(char)) return "char";
                if (type == typeof(double)) return "double";
                if (type == typeof(float)) return "float";
                if (type == typeof(decimal)) return "decimal";
                if (type == typeof(DateTime)) return "DateTime";
                if (type == typeof(TimeSpan)) return "TimeSpan";

                if (type == typeof(byte?)) return "byte?";
                if (type == typeof(sbyte?)) return "sbyte?";
                if (type == typeof(bool?)) return "bool?";
                if (type == typeof(byte?)) return "char?";
                if (type == typeof(short?)) return "short?";
                if (type == typeof(ushort?)) return "ushort?";
                if (type == typeof(int?)) return "int?";
                if (type == typeof(uint?)) return "uint?";
                if (type == typeof(long?)) return "long?";
                if (type == typeof(ulong?)) return "ulong?";
                if (type == typeof(double?)) return "double?";
                if (type == typeof(float?)) return "float?";
                if (type == typeof(decimal?)) return "decimal?";
                if (type == typeof(DateTime?)) return "DateTime?";
                if (type == typeof(TimeSpan?)) return "TimeSpan?";
                if (type == typeof(Guid?)) return "Guid?";
                if (type == null) return null;

                if (Nullable.GetUnderlyingType(type) != null)
                    return string.Format("{0}?", Nullable.GetUnderlyingType(type));

                if (type.IsGenericType) return "Object";
            }
            else
            {
                if (type == typeof(int)) return "Integer";
                if (type == typeof(long)) return "Long";
                if (type == typeof(string)) return "String";
                if (type == typeof(bool)) return "Boolean";
                if (type == typeof(object)) return "Object";
                if (type == typeof(void)) return "Void";
                if (type == typeof(byte)) return "Byte";
                if (type == typeof(short)) return "Short";
                if (type == typeof(char)) return "Char";
                if (type == typeof(double)) return "Double";
                if (type == typeof(float)) return "Single";
                if (type == typeof(decimal)) return "Decimal";
                if (type == typeof(DateTime)) return "DateTime";

                if (type == typeof(byte?)) return "Nullable(Of Byte)";
                if (type == typeof(bool?)) return "Nullable(Of Boolean)";
                if (type == typeof(byte?)) return "Nullable(Of Char)";
                if (type == typeof(short?)) return "Nullable(Of Short)";
                if (type == typeof(int?)) return "Nullable(Of Integer)";
                if (type == typeof(long?)) return "Nullable(Of Long)";
                if (type == typeof(double?)) return "Nullable(Of Double)";
                if (type == typeof(float?)) return "Nullable(Of Single)";
                if (type == typeof(decimal?)) return "Nullable(Of Decimal)";
                if (type == typeof(DateTime?)) return "Nullable(Of DateTime)";
                if (type == typeof(TimeSpan?)) return "Nullable(Of TimeSpan)";
                if (type == typeof(Guid?)) return "Nullable(Of Guid)";

                if (Nullable.GetUnderlyingType(type) != null)
                    return string.Format("Nullable(Of {0})", Nullable.GetUnderlyingType(type));

                if (type.IsGenericType) return "Object";
            }

            return null;
        }
    }
}
