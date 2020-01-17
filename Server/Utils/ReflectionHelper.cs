using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Reflection;
using System.Collections;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Utils
{
    public class ReflectionHelper
    {
        public static SQLTableDefinedTypePropertyInfoList<Type, List<PropertyInfo>> SQLTableDefinedTypePropertyInfo = new SQLTableDefinedTypePropertyInfoList<Type, List<PropertyInfo>>();

        public static SQLTableDefinedTypePropertyInfoDict<Type, Dictionary<string, PropertyInfo>> SQLTableDefinedTypePropertyDict = new SQLTableDefinedTypePropertyInfoDict<Type, Dictionary<string, PropertyInfo>>();

        public static object ReflectOnPath(object o, string path, Dictionary<string, PropertyInfo> rProperties)
        {
            object value = o;
            Dictionary<string, PropertyInfo> properties = rProperties;
            PropertyInfo info = null;
            bool isFirst = true;

            string[] pathComponents = path.Replace("__", ".").Split('.');
            foreach (var component in pathComponents)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    if (value != null)
                    {
                        properties = SQLTableDefinedTypePropertyDict[value.GetType()];
                    }
                    else
                    {
                        break;
                    }
                }

                //Пытаемся определить элемент ли это массива (есть что то типа [0])
                string s;

                //Проверяем из коллекции ли это элемент
                int i = component.IndexOf('[');
                if (i > 0)
                {
                    int num; //Номер индекса
                    int j = component.IndexOf(']');
                    if (j > 0 && j > i && int.TryParse(component.Substring(i + 1, j - i - 1), out num))
                    {
                        s = component.Substring(0, i);
                        if (properties != null && properties.TryGetValue(s, out info) && info != null)
                        {
                            var listable = info.GetValue(value, null) as IList;
                            if (listable != null)
                            {
                                try
                                {
                                    value = listable[num];
                                }
                                catch { }
                            }
                        }
                    }
                }
                else
                {
                    if (properties != null && properties.TryGetValue(component, out info) && info != null)
                    {
                        value = info.GetValue(value, null);
                    }
                }
            }
            return value;
        }
    }

    public class IgnoredSqlTableTypeAdapterMemberAttribute : Attribute
    {
    };


    public class SQLTableDefinedTypePropertyInfoList<TKey, TValue>
        where TKey : Type
        where TValue : List<PropertyInfo>
    {
        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (dict.TryGetValue(key, out result) && result != null)
                {
                    return result;
                }
                result = key.GetProperties()
                            .Where(p => !p.PropertyType.IsClass || p.PropertyType == typeof(string)).Where(i=> !i.GetCustomAttributes(typeof(IgnoredSqlTableTypeAdapterMemberAttribute), true).Any())
                            .ToList() as TValue;
                dict[key] = result;
                return result;
            }
            set
            {
                try
                {
                    dict[key] = value;
                }
                catch { }
            }
        }

        ConcurrentDictionary<TKey, TValue> dict = new ConcurrentDictionary<TKey, TValue>();
    }

    public class SQLTableDefinedTypePropertyInfoDict<TKey, TValue>
        where TKey : Type
        where TValue : Dictionary<string, PropertyInfo>
    {
        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (dict.TryGetValue(key, out result) && result != null)
                {
                    return result;
                }
                result = key.GetProperties()
                            .Where(p => !p.PropertyType.IsClass || p.PropertyType == typeof(string))
                            .ToDictionary(k => k.Name, v => v) as TValue;
                dict[key] = result;
                return result;
            }
            set
            {
                try
                {
                    dict[key] = value;
                }
                catch { }
            }
        }

        ConcurrentDictionary<TKey, TValue> dict = new ConcurrentDictionary<TKey, TValue>();
    }
}
