using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;

namespace Proryv.OPCUA.Common.DB
{
    public class SQLTableTypeAdapter<T> where T : class 
    {
        /// <summary>
        /// Сгенерированная таблица которую пишем в базу данных
        /// </summary>
        DataTable UserDefinedTypeTable;
        //List<PropertyInfo> MappedClassProperties;

        public SQLTableTypeAdapter(IEnumerable<T> ArchivesValues)
        {
            UserDefinedTypeTable = ArchiveValuesToDataTable(ArchivesValues);
        }

        public static DataTable ArchiveValuesToDataTable(IEnumerable<T> ArchivesValues)
        {
            List<PropertyInfo> MappedClassProperties = ReflectionHelper.SQLTableDefinedTypePropertyInfo[typeof(T)];

            var userDefinedTypeTable = new DataTable();
            int columnCount = 0;

            //Добавляем колонки
            foreach (var propInfo in MappedClassProperties)
            {
                Type propType = propInfo.PropertyType;
                if (!propType.IsClass || propType == typeof(string))
                {
                    if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        propType = Nullable.GetUnderlyingType(propType);
                    }

                    userDefinedTypeTable.Columns.Add(new DataColumn(propInfo.Name, propType));

                    columnCount++;
                }
            }

            //Добавляем данные
            foreach (var v in ArchivesValues)
            {
                object[] rows = new object[columnCount];

                for (int i = 0; i < columnCount; i++)
                {
                    PropertyInfo mappedProperty = MappedClassProperties.ElementAtOrDefault(i);
                    if (mappedProperty != null)
                    {
                        rows[i] = mappedProperty.GetValue(v, null);
                    }
                }

                userDefinedTypeTable.Rows.Add(rows);
            }

            return userDefinedTypeTable;
        }

        /// <summary>
        /// Пишем таблицу в SQL 
        /// </summary>
        /// <param name="StoredProcedureName">Название процедуры SQL с помощью которой пишем</param>
        /// <param name="UserDefinedTableName">Название параметра в процедуре, с помощью которой передаем таблицу </param>
        /// <param name="Params">Остальные параметры процедуры</param>
        /// <returns></returns>
        public bool WriteTableToSQL(string StoredProcedureName, string UserDefinedTableName, string ConnectionString, List<Tuple<string, object>> Params = null)
        {
            if (String.IsNullOrEmpty(StoredProcedureName) || String.IsNullOrEmpty(UserDefinedTableName)) return false;
            using (var cnctns = new System.Data.SqlClient.SqlConnection(ConnectionString))
            using (var cmd = cnctns.CreateCommand())
            {
                cmd.CommandText = StoredProcedureName;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;
                if (Params != null && Params.Count > 0)
                {
                    foreach (var param in Params)
                    {
                        if (param != null && param.Item2 != null)
                        {
                            cmd.Parameters.AddWithValue(param.Item1, param.Item2);
                        }
                    }
                }

                cmd.Parameters.AddWithValue(UserDefinedTableName, UserDefinedTypeTable);

                cnctns.Open();
                cmd.ExecuteNonQuery();
            }


            return true;
        }
    }
}
