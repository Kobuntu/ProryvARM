using System;
using System.Collections;
using System.Collections.Generic;

namespace Proryv.Servers.Calculation.Parser.Internal.Functions
{
    public class ProryvFunctions
    {
        #region Fields
		private static readonly Hashtable functionsToCompile = new Hashtable();
		private static readonly Hashtable functionsToCompileLower = new Hashtable();
		private static readonly Hashtable functions = new Hashtable();
		private static readonly Hashtable functionsLower = new Hashtable();
		#endregion

		#region Methods
        /// <summary>
        /// Removes all functions from report dictionary with specified name.
        /// </summary>
        /// <param name="functionName"></param>
        public static void RemoveFunction(string functionName)
        {
            if (functionsToCompile[functionName] != null)
                functionsToCompile.Remove(functionName);

            if (functionsToCompileLower[functionName.ToLower()] != null)
                functionsToCompileLower.Remove(functionName.ToLower());

            if (functions[functionName] != null)
                functions.Remove(functionName);

            if (functionsLower[functionName.ToLower()] != null)
                functionsLower.Remove(functionName.ToLower());
        }

        public static List<ProryvFunction> GetFunctionsList(string functionName)
        {
            if (functions[functionName] != null)
                return functions[functionName] as List<ProryvFunction>;
            else
                return null;
        }

		public static Hashtable GetFunctionsGrouppedInCategories()
		{
			var hash = new Hashtable();

			var functions = GetFunctions(false);
			foreach (var function in functions)
			{
                var list = hash[function.Category] as List<ProryvFunction>;
				if (list == null)
				{
                    list = new List<ProryvFunction>();
					hash[function.Category] = list;
				}

				list.Add(function);
			}
			
			return hash;
		}

        public static List<ProryvFunction> GetFunctions(string category)
        {
            var functions = GetFunctions(false);
            var func = new List<ProryvFunction>();
            foreach (var function in functions)
            {
                if (function.Category == category)
                {
                    func.Add(function);
                }
            }

            return func;
        }

        public static List<string> GetCategories()
        {
            var hash = new Hashtable();
            
            var functions = GetFunctions(false);
            var categories = new List<string>();
            foreach (var function in functions)
            {
                if (hash[function.Category] == null)
                {
                    categories.Add(function.Category);
                    hash[function.Category] = function.Category;
                }
            }

            return categories;
        }

		/// <summary>
		/// Returns array of asseblies which contains functions.
		/// </summary>
		public static string[] GetAssebliesOfFunctions()
		{
			var functions = GetFunctions(true);
			var assemblies = new Hashtable();

			foreach (var function in functions)
			{
				assemblies[function.TypeOfFunction.Assembly.Location] = function.TypeOfFunction.Assembly.FullName;
			}

			var asms = new string[assemblies.Count];
			assemblies.Keys.CopyTo(asms, 0);
			return asms;
		}


		/// <summary>
		/// Returns array of all functions.
		/// </summary>
		public static ProryvFunction[] GetFunctions(bool isCompile)
		{
            var list = new List<ProryvFunction>();
            var tempFuncs = isCompile ? functionsToCompile : functions;

            foreach (string functionName in tempFuncs.Keys)
			{
				var functionsList = GetFunctions(functionName, isCompile);
				foreach (var function in functionsList)
				{
					list.Add(function);
				}
			}

            return list.ToArray();
		}


        /// <summary>
        /// Returns array of functions with spefified name.
        /// </summary>
        public static ProryvFunction[] GetFunctions(string functionName, bool isCompile)
        {
            if (isCompile)
            {
                var list = functionsToCompile[functionName] as List<ProryvFunction>;
                if (list == null) return null;
                return list.ToArray();
            }
            else
            {
                var list = functions[functionName] as List<ProryvFunction>;
                if (list == null) return null;
                
                return list.ToArray();
            }
        }


        /// <summary>
		/// Adds new function with specified parameters.
		/// </summary>
		/// <param name="category">Category of function.</param>
		/// <param name="functionName">Name of function.</param>
		/// <param name="typeOfFunction">Type which contain method of function.</param>
		/// <param name="returnType">Return type of function.</param>
		public static ProryvFunction AddFunction(string category, string functionName, Type typeOfFunction, Type returnType)
		{
			return AddFunction(category, functionName, string.Empty, typeOfFunction, returnType,
				string.Empty, null, null);
		}

		/// <summary>
		/// Adds new function with specified parameters.
		/// </summary>
		/// <param name="category">Category of function.</param>
		/// <param name="functionName">Name of function.</param>
		/// <param name="description">Description of function.</param>
		/// <param name="typeOfFunction">Type which contain method of function.</param>
		/// <param name="returnType">Return type of function.</param>
		/// <param name="returnDescription">Description of returns.</param>
		public static ProryvFunction AddFunction(string category, string functionName, string description, Type typeOfFunction, Type returnType,
			string returnDescription)
		{
			return AddFunction(category, functionName, description, typeOfFunction, returnType,
				returnDescription, null, null);
		}

		/// <summary>
		/// Adds new function with specified parameters.
		/// </summary>
		/// <param name="category">Category of function.</param>
		/// <param name="functionName">Name of function.</param>
		/// <param name="description">Description of function.</param>
		/// <param name="typeOfFunction">Type which contain method of function.</param>
		/// <param name="returnType">Return type of function.</param>
		/// <param name="returnDescription">Description of returns.</param>
		/// <param name="argumentTypes">Array which contain types of arguments.</param>
		/// <param name="argumentNames">Array which contain names of arguments.</param>
		public static ProryvFunction AddFunction(string category, string functionName, string description, Type typeOfFunction, Type returnType,
			string returnDescription, Type []argumentTypes, string []argumentNames)
		{
			return AddFunction(category, functionName, functionName, description, typeOfFunction, returnType,
				returnDescription, argumentTypes, argumentNames, null);
		}

		/// <summary>
		/// Adds new function with specified parameters.
		/// </summary>
		/// <param name="category">Category of function.</param>
		/// <param name="groupFunctionName">Name of group function. Can be same as function name.</param>
		/// <param name="functionName">Name of function.</param>
		/// <param name="description">Description of function.</param>
		/// <param name="typeOfFunction">Type which contain method of function.</param>
		/// <param name="returnType">Return type of function.</param>
		/// <param name="returnDescription">Description of returns.</param>
		/// <param name="argumentTypes">Array which contain types of arguments.</param>
		/// <param name="argumentNames">Array which contain names of arguments.</param>
		/// <param name="argumentDescriptions">Array which contain descriptions of arguments.</param>
		public static ProryvFunction AddFunction(string category, string groupFunctionName, string functionName, string description, Type typeOfFunction, Type returnType,
			string returnDescription, Type []argumentTypes, string []argumentNames, string []argumentDescriptions)
		{
			if (string.IsNullOrEmpty(groupFunctionName)) groupFunctionName = functionName;

			var function = new ProryvFunction(
				category,
				groupFunctionName,
				functionName, description, 
				typeOfFunction, returnType,
				returnDescription, argumentTypes, argumentNames, argumentDescriptions);

			#region Functions
            var list = functions[groupFunctionName] as List<ProryvFunction>;
			if (list == null)
			{
                list = new List<ProryvFunction>();
				functions[groupFunctionName] = list;
				functionsLower[groupFunctionName.ToLower(System.Globalization.CultureInfo.InvariantCulture)] = list;
			}
			list.Add(function);
			#endregion

			#region Functions to Compile
            list = functionsToCompile[functionName] as List<ProryvFunction>;
			if (list == null)
			{
                list = new List<ProryvFunction>();
				functionsToCompile[functionName] = list;
				functionsToCompileLower[functionName.ToLower(System.Globalization.CultureInfo.InvariantCulture)] = list;
			}
			list.Add(function);
            #endregion

			return function;
		}
		
		#endregion		
		
		static ProryvFunctions()
		{
		}
    }
}
