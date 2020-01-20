using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.Servers.Calculation.Parser.Internal
{
    public class ValuesDict<TValue> where TValue : struct 
    {
        private readonly Dictionary<string, TValue> _internalDictionary = new Dictionary<string, TValue>();

        public TValue this[string key]
        {
            get { return _internalDictionary[key.ToUpper()]; }
            set { _internalDictionary[key.ToUpper()] = value; }
        }

        public bool Contains(string key)
        {
            return _internalDictionary.ContainsKey(key.ToUpper());
        }
    }
}
