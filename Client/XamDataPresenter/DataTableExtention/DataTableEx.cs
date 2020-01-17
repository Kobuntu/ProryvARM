using System;
using System.Data;
using System.Runtime.Serialization;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.Collections.Generic;
using System.Linq;
using Proryv.AskueARM2.Both.VisualCompHelpers.Interfaces;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.Service.Dictionary;
using Proryv.AskueARM2.Client.ServiceReference.Data;
using System.Collections.Concurrent;
using Proryv.AskueARM2.Both.VisualCompHelpers.TClasses;
using Proryv.AskueARM2.Client.Visual.Common;
using System.Collections;

namespace Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter.DataTableExtention
{
    [Serializable]
    public class DataTableEx : DataTable, IDisposable
    {
        public DataTableEx()
            : base()
        {
        }

        public DataTableEx(string tableName)
            : base(tableName)
        {
        }

        public DataTableEx(string tableName, string tableNamespace)
            : base(tableName, tableNamespace)
        {
        }

        /// <summary>
        /// Needs using System.Runtime.Serialization;
        /// </summary>
        public DataTableEx(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected override Type GetRowType()
        {
            return typeof(DataRowEx);
        }

        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new DataRowEx(builder);
        }

        /// <summary>
        /// Какое поле используется для поиска
        /// </summary>
        public string FieldForSearch;

        public DataTableDescription Description;
        public readonly ConcurrentDictionary<string, DetailFieldInfo> ItemsIndexesDict = new ConcurrentDictionary<string, DetailFieldInfo>();

        public void SetFieldInfo(DetailFieldInfo fieldInfo)
        {
            if (fieldInfo == null || ItemsIndexesDict.ContainsKey(fieldInfo.ColumnName)) return;

            fieldInfo.TableEx = this;

            ItemsIndexesDict[fieldInfo.ColumnName] = fieldInfo;
        }

        public void SetFieldInfo(string key, int indx, byte channel, ID_Hierarchy id, EnumDataSourceType? dataSource,
        string measureTypeUn, bool useMeasureModule = true, ID_Hierarchy replacedId = null, string groupByReplacedTiName = null)
        {
            if (ItemsIndexesDict.ContainsKey(key)) return;

            if (string.IsNullOrEmpty(measureTypeUn) && channel > 0)
            {
                //Нужно задать по-умолчанию электроэнергию
            }

            ItemsIndexesDict[key] = new DetailFieldInfo
            {
                IndexHalfhour = indx,
                ChannelType = channel,
                Id = id,
                DataSource = dataSource,
                MeasureUnitUn = measureTypeUn,
                UseMeasureModule = useMeasureModule,
                ColumnName = key,
                TableEx = this,
                ReplacedId = replacedId,
                GroupByReplacedTiName = groupByReplacedTiName,
            };
        }

        public bool TryGetIndexByItemName(string key, out DetailFieldInfo fieldInfo)
        {
            fieldInfo = null;

            return ItemsIndexesDict.TryGetValue(key, out fieldInfo);
        }

        private IMeasure Measure;

        #region Добавляем и наполняем колонки по ТИ

        public Dictionary<string, DetailFieldInfo> AddColumnsTi(ref int k, ArchivesValues_List2 archivesList, bool isSumTiForChart, bool isDetailOv)
        {
            var result = new Dictionary<string, DetailFieldInfo>();

            if (archivesList == null || archivesList.ArchiveValues == null) return result;

            var objectType = typeof(ObjectIdCollection);
            var vType = typeof(IFValue);

            if (isSumTiForChart)
            {
                #region  Нужно просумировать поканально все ТИ

                var tiValues = new List<TArchivesValue>();

                //Перебираем ТИ
                foreach (var aVals in archivesList.ArchiveValues.Values)
                {
                    tiValues.AddRange(aVals);
                }

                var dataSourcePair = EnumClientServiceDictionary.DataSourceTypeList.FirstOrDefault();

                //Перебираем каналы
                foreach (var aVals in tiValues.GroupBy(a => a.TI_Ch_ID.DataSourceType))
                {
                    var valuesDict = new Dictionary<byte, Dictionary<int, IFValue>>();

                    if (dataSourcePair.Key != aVals.Key)
                    {
                        dataSourcePair = EnumClientServiceDictionary.DataSourceTypeList.FirstOrDefault(ds =>
                            ds.Key.HasValue && ds.Key.Value == aVals.Key);
                    }

                    foreach (var aVal in aVals.OrderBy(a => a.TI_Ch_ID.ChannelType))
                    {
                        if (aVal.Val_List == null) continue;

                        Dictionary<int, IFValue> chVals;
                        if (!valuesDict.TryGetValue(aVal.TI_Ch_ID.ChannelType, out chVals))
                        {
                            var columnName = "F_VALUE" + k;

                            //Это новая колонка
                            var col = new DataColumn(columnName, vType)
                            {
                                Caption = "Сумма по " + ChannelFactory.ChanelTypeNameFSK[aVal.TI_Ch_ID.ChannelType]
                                + "\n" + (dataSourcePair.Value ?? "По приоритету")
                            };

                            Columns.Add(col);
                            k++;

                            chVals = new Dictionary<int, IFValue>();
                            valuesDict[aVal.TI_Ch_ID.ChannelType] = chVals;

                            result[columnName] = new DetailFieldInfo
                            {
                                ColumnName = columnName,
                                Id = null,
                                DataSource = aVals.Key,
                                MeasureUnitUn = null,
                                Values = chVals,
                                ChannelType = aVal.TI_Ch_ID.ChannelType,
                            };
                        }

                        //Суммируем поканально
                        for (var i = 0; i < aVal.Val_List.Count; i++)
                        {
                            var v = aVal.Val_List[i];
                            IFValue r; ;
                            if (!chVals.TryGetValue(i, out r) || r == null)
                            {
                                chVals.Add(i, new TVALUES_DB
                                {
                                    F_FLAG = v.F_FLAG,
                                    F_VALUE = v.F_VALUE
                                });
                            }
                            else
                            {
                                r.F_VALUE += v.F_VALUE;
                                r.F_FLAG = r.F_FLAG.CompareAndReturnMostBadStatus(v.F_FLAG);
                            }
                        }
                    }
                }

                #endregion
            }
            else
            {
                #region Просто добавляем колонки по каждой ТИ

                var comparer = new IFreeHierarchyObjectComparerTyped();
                var freeHierarchyComparer = new IFreeHierarchyObjectEqualityComparer();

                foreach (var groupByObject in archivesList.ArchiveValues
                    .Select(pair =>
                    {
                        var ti = EnumClientServiceDictionary.TIHierarchyList[pair.Key];
                        if (ti == null) return null;

                        var ps = EnumClientServiceDictionary.DetailPSList[ti.PS_ID];
                        if (ps == null) return null;

                        return new Tuple<IFreeHierarchyObject, IFreeHierarchyObject, List<TArchivesValue>>(ps, ti, pair.Value);
                    })
                    .Where(tuple => tuple != null)
                    .OrderBy(tuple => tuple.Item1, comparer) //сначала сортируем по ПС
                    .ThenBy(tuple => tuple.Item2, comparer)
                    .GroupBy(tuple => tuple.Item2, freeHierarchyComparer)) //Группируем по объекту
                {
                    var id = new ID_Hierarchy
                    {
                        ID = groupByObject.Key.Id.ToString(),
                        TypeHierarchy = groupByObject.Key.Type,
                    };

                    foreach (var objects in groupByObject)
                    {
                        var isOv = false;
                        Dictionary<int, TDateTimeOV> tiReplaced = null;

                        #region Добавление обычных колонок

                        //Теперь сортируем по категории ед.измерения

                        foreach (var tiValue in objects.Item3.OrderBy(aVal => aVal.TI_Ch_ID.ChannelType)
                            .ThenBy(aVal => aVal.MeasureUnit_UN == null)
                            .ThenBy(aVal => aVal.MeasureUnit_UN))
                        {
                            //Обходной ли это выключатель
                            isOv = tiValue.IsOV;// && tiValue.TiByOvReplaced != null && tiValue.TiByOvReplaced.Count > 0;
                            if (tiReplaced == null) tiReplaced = tiValue.TiByOvReplaced;

                            var columnName = "F_VALUE" + k;

                            if (isOv)
                            {
                                Columns.Add(new DataColumn(columnName, typeof(TOvValue))
                                {
                                    ReadOnly = true,
                                });
                            }
                            else
                            {
                                Columns.Add(new DataColumn(columnName, vType)
                                {
                                    ReadOnly = true,
                                });
                            }

                            k++;

                            result[columnName] = new DetailFieldInfo
                            {
                                ColumnName = columnName,
                                Id = id,
                                DataSource = tiValue.TI_Ch_ID.DataSourceType,
                                MeasureUnitUn = tiValue.MeasureUnit_UN,
                                Values = tiValue.Val_List.Select((s, i) => new { s, i }).ToDictionary(v => v.i, v => v.s as IFValue),
                                ChannelType = tiValue.TI_Ch_ID.ChannelType,
                                UseMeasureModule = string.IsNullOrEmpty(tiValue.MeasureUnit_UN),
                                IsOv = isOv,
                                TiReplaced = tiValue.TiByOvReplaced,
                            };
                        }

                        #endregion

                        #region Если это ОВ, добавляем колонки о замещаемых ТИ

                        if (isOv && isDetailOv)
                        {
                            #region  Это колонка с информацией о замещаемой ТИ

                            var columnName = "ovInfo" + k; //По названию определяемся что это информация об ОВ

                            Columns.Add(new DataColumn(columnName, objectType)
                            {
                                ReadOnly = true,
                            });

                            result[columnName] = new DetailFieldInfo
                            {
                                ColumnName = columnName,
                                Id = new ID_Hierarchy
                                {
                                    ID = id.ID,
                                    TypeHierarchy = enumTypeHierarchy.Info_TI,
                                },
                                DataSource = null,
                                MeasureUnitUn = null,
                                Values = null,
                                ChannelType = 0,
                                TiReplaced = tiReplaced,
                            };

                            #endregion


                            //Объекты которые замещает ОВ
                            foreach (var tiValue in objects.Item3.OrderBy(aVal => aVal.TI_Ch_ID.ChannelType)
                                .ThenBy(aVal => aVal.MeasureUnit_UN == null)
                                .ThenBy(aVal => aVal.MeasureUnit_UN))
                            {
                                if (tiValue.TiByOvReplaced != null)
                                {
                                    foreach (var replacedTi in tiValue.TiByOvReplaced)
                                    {
                                        var groupByReplacedTiName = "ovValue_" + id.ID + "_" + replacedTi.Key + "_";
                                        columnName = groupByReplacedTiName + tiValue.TI_Ch_ID.ChannelType +
                                                     (tiValue.TI_Ch_ID.DataSourceType.HasValue
                                                         ? tiValue.TI_Ch_ID.DataSourceType.Value.ToString()
                                                         : string.Empty);

                                        Columns.Add(
                                            new DataColumn(columnName, vType) //Это колонка со значениями замещаемых ТИ
                                            {
                                                ReadOnly = true,
                                            });

                                        result[columnName] = new DetailFieldInfo
                                        {
                                            ColumnName = columnName,
                                            Id = new ID_Hierarchy
                                            {
                                                ID = tiValue.TI_Ch_ID.TI_ID.ToString(),
                                                TypeHierarchy = enumTypeHierarchy.Info_TI,
                                            },
                                            DataSource = tiValue.TI_Ch_ID.DataSourceType,
                                            MeasureUnitUn = tiValue.MeasureUnit_UN,
                                            Values = tiValue.Val_List.Select((s, i) => new { s, i }).ToDictionary(v => v.i, v => v.s as IFValue),
                                            ChannelType = tiValue.TI_Ch_ID.ChannelType,
                                            TiReplaced = tiValue.TiByOvReplaced,
                                            ReplacedId = new ID_Hierarchy
                                            {
                                                ID = replacedTi.Key.ToString(),
                                                TypeHierarchy = enumTypeHierarchy.Info_TI,
                                            },
                                            GroupByReplacedTiName = groupByReplacedTiName,
                                            UseMeasureModule = string.IsNullOrEmpty(tiValue.MeasureUnit_UN)
                                        };
                                    }
                                }
                            }
                        }

                        #endregion
                    }
                }

                #endregion
            }

            return result;
        }

        /// <summary>
        /// Наполняем строки значениями
        /// </summary>
        /// <param name="k">номер колонки (примерно)</param>
        /// <param name="columnValues">Все значения по названиям колонок</param>
        /// <param name="numbersValues">Общее количество дискретных значений в периода</param>
        /// <param name="isSumTiForChart">Суммировать или нет поканально</param>
        /// <param name="row">Строка в таблице, которую наполняем</param>
        /// <param name="i">Индекс дискретного периода</param>
        public void PopulateRowsTi(ref int k, Dictionary<string, DetailFieldInfo> columnValues, int numbersValues,
            bool isSumTiForChart, DataRowEx row, int i)
        {
            foreach (var fieldInfoPair in columnValues)
            {
                //------------ТИ ФСК-----------------------------
                if (!isSumTiForChart)
                {
                    row.SetValue(fieldInfoPair.Value, i, ref k);
                }
                else if (fieldInfoPair.Value != null && fieldInfoPair.Value.Values != null)
                {
                    row["F_VALUE" + k] = fieldInfoPair.Value.TryGetValue(i);
                    k++;
                }
            }
        }

        #endregion

        #region Добавляем и наполняем колонки по ТП

        /// <summary>
        /// Добавляем колонки по ТП
        /// </summary>
        /// <param name="k"></param>
        /// <param name="archivesTpList"></param>
        /// /// <param name="ListTiViewModel"></param>
        /// <returns></returns>
        public Dictionary<string, DetailFieldInfo> AddColumnsTp(ref int k, ArchivesTPValueAllChannels archivesTpList, 
            List<TRowObjectForGrid> listTiViewModel = null)
        {
            var result = new Dictionary<string, DetailFieldInfo>();

            if (archivesTpList == null || archivesTpList.Result_Values == null) return result;

            var vType = typeof(IFValue);
            var tps = EnumClientServiceDictionary.GetTps();
            if (tps == null) return result;

            foreach (var archivesTpValueAllChannelse in archivesTpList.Result_Values.Select(a =>
            {
                TPoint tp;
                tps.TryGetValue(a.Key, out tp);
                return new Tuple<TPoint, List<TArchivesTPValueAllChannels>>(tp, a.Value);
            }).Where(a => a != null).OrderBy(a => a.Item1.StringName))
            {
                var id = new ID_Hierarchy
                {
                    ID = archivesTpValueAllChannelse.Item1.TP_ID.ToString(),
                    TypeHierarchy = enumTypeHierarchy.Info_TP
                };

                foreach (var tpValue in archivesTpValueAllChannelse.Item2)
                {
                    if (tpValue.Val_List == null) continue;

                    var tp = archivesTpValueAllChannelse.Item1;

                    Dictionary<byte, List<TPointValue>> pointsByChannel;
                    if (!tpValue.Val_List.TryGetValue(tpValue.IsMoneyOurSide, out pointsByChannel) || pointsByChannel == null || pointsByChannel.Count == 0) continue;

                    foreach (var channel in archivesTpList.ChannelType_List.OrderBy(c => c))
                    {
                        List<TPointValue> tpVals;
                        if (!pointsByChannel.TryGetValue(channel, out tpVals) || tpVals == null) continue;

                        var columnName = "F_VALUE" + k;

                        var col = new DataColumn(columnName, vType)
                        {
                            //Caption = "ТП - " + tp.StringName + " " +
                            //          getNameInterface.GetChanelTypeNameByID(channel, false),
                            ReadOnly = true,
                        };

                        Columns.Add(col);
                        k++;

                        result[columnName] = new DetailFieldInfo
                        {
                            ColumnName = columnName,
                            Id = id,
                            DataSource = null,
                            MeasureUnitUn = null,
                            Values = tpVals.Select((s, i) => new { s, i }).ToDictionary(v => v.i, v => v.s as IFValue),
                            ChannelType = channel,
                            UseMeasureModule = true,
                        };
                    }

                    //tpArchives.Add(new );
                }
            }

            return result;
        }

        /// <summary>
        /// Наполняем строки значениями
        /// </summary>
        /// <param name="k"></param>
        /// <param name="columnValues"></param>
        /// <param name="numbersValues"></param>
        /// <param name="isSumTiForChart"></param>
        /// <param name="row"></param>
        /// <param name="i"></param>
        public void PopulateRowsTp(ref int k, Dictionary<string, DetailFieldInfo> columnValues, DataRowEx row, int i)
        {
            foreach (var fieldInfoPair in columnValues)
            {
                row.SetValue(fieldInfoPair.Value, i, ref k);
            }
        }

        #endregion

        #region Добавляем и наполняем колонки по балансам

        public Dictionary<string, DetailFieldInfo> AddColumnsBalances(ref int k, BalanceFreeHierarchyResults balanceFreeHierarchy)
        {
            var result = new Dictionary<string, DetailFieldInfo>();

            if (balanceFreeHierarchy == null || balanceFreeHierarchy.CalculatedValues == null) return result;

            var objectType = typeof(ObjectIdCollection);
            var vType = typeof(IFValue);

            var comparer = new IFreeHierarchyObjectComparerTyped();
            var freeHierarchyComparer = new IFreeHierarchyObjectEqualityComparer();

            foreach (var balance in balanceFreeHierarchy.CalculatedValues.Values)
            {
                if (balance.CalculatedByDiscretePeriods == null || balance.CalculatedByDiscretePeriods.Count == 0) continue;

                var id = new ID_Hierarchy
                {
                    ID = balance.BalanceFreeHierarchyUn,
                    TypeHierarchy = enumTypeHierarchy.UniversalBalance
                };

                #region Перебираем подгруппы

                //Фактический небаланс,кВт (по модулю) FactUnbalanceValue

                var columnName = "FactUnbalanceValue" + k;

                Columns.Add(new DataColumn(columnName, vType)
                {
                    ReadOnly = true,
                });

                k++;

                result[columnName] = new DetailFieldInfo
                {
                    ColumnName = columnName,
                    Id = id,
                    UseMeasureModule = true,
                    //MeasureUnitUn = tiValue.MeasureUnit_UN,
                    BalanceValues = balance.CalculatedByDiscretePeriods,
                };

                //Фактический небаланс,%

               columnName = "FactUnbalancePercent" + k;

                Columns.Add(new DataColumn(columnName, vType)
                {
                    ReadOnly = true,
                });

                k++;

                result[columnName] = new DetailFieldInfo
                {
                    ColumnName = columnName,
                    Id = id,
                    UseMeasureModule = false,
                    MeasureUnitUn = "RatioUnit.Percent",
                    BalanceValues = balance.CalculatedByDiscretePeriods,
                };


                #endregion
            }

            return result;
        }

        /// <summary>
        /// Наполняем строки значениями
        /// </summary>
        /// <param name="k">номер колонки (примерно)</param>
        /// <param name="columnValues">Все значения по названиям колонок</param>
        /// <param name="numbersValues">Общее количество дискретных значений в периода</param>
        /// <param name="isSumTiForChart">Суммировать или нет поканально</param>
        /// <param name="row">Строка в таблице, которую наполняем</param>
        /// <param name="i">Индекс дискретного периода</param>
        public void PopulateRowsBalances(ref int k, Dictionary<string, DetailFieldInfo> columnValues, DataRowEx row, int i)
        {
            foreach (var fieldInfoPair in columnValues)
            {
                row.SetValue(fieldInfoPair.Value, i, ref k);
            }
        }

        #endregion

        #region Dispose

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            // подавляем финализацию
            GC.SuppressFinalize(this);
            base.Dispose();
        }

        protected virtual new void Dispose(bool disposing)
        {
            if (_disposed) return;

            try
            {
                Description = null;
                if (ItemsIndexesDict != null)
                {
                    foreach (var key in ItemsIndexesDict.Keys.ToList())
                    {
                        DetailFieldInfo info;
                        if (ItemsIndexesDict.TryRemove(key, out info) && info!=null)
                        {
                            info.Dispose();
                        }
                    }

                    ItemsIndexesDict.Clear();
                }
            }
            finally
            {
                _disposed = true;
            }
        }

        // Деструктор
        ~DataTableEx()
        {
            Dispose(false);
        }

        #endregion 
    }

    [Serializable]
    public class DataRowEx : DataRow, IFindableItem
    {
        public DataRowEx()
            : base(null)
        {
        }

        public DataRowEx(DataRowBuilder builder)
            : base(builder)
        {
        }

        #region Расширяем стандартный DataRow

        public void SetValue(DetailFieldInfo fieldInfo, int i, ref int k)
        {
            base[fieldInfo.ColumnName] = BuildValue(fieldInfo, i, ref k);
            
            if (fieldInfo.IndexHalfhour > 0 &&  fieldInfo.ReplacedId == null) return;

            var tex = Table as DataTableEx;
            if (tex == null) return;

            tex.SetFieldInfo(fieldInfo);
        }

        private object BuildValue(DetailFieldInfo fieldInfo, int i, ref int k)
        {
            switch (fieldInfo.ColumnType)
            {
                case EnumColumnType.OvInfo:
                    // Это информация об ОВ
                    {

                        var replacedIds = new List<ID_Hierarchy>();
                        //Перебираем все ТИ, которые могут быть замещены в этом дискретном периоде

                        if (fieldInfo.TiReplaced != null)
                        {
                            foreach (var tiReplaced in fieldInfo.TiReplaced)
                            {
                                if (tiReplaced.Value.ValuesByIndexes == null) continue;

                                if (!tiReplaced.Value.ValuesByIndexes.ContainsKey(i)) continue;

                                replacedIds.Add(new ID_Hierarchy
                                {
                                    ID = tiReplaced.Key.ToString(),
                                    TypeHierarchy = enumTypeHierarchy.Info_TI,
                                });
                            }
                        }

                        return new ObjectIdCollection(replacedIds);
                    }

                case EnumColumnType.OvValue:
                    //Это значение замещаемой ТИ
                    {
                        TDateTimeOV vals;
                        TVALUES_DB val = null;
                        int replacedTiId;

                        if (fieldInfo.ReplacedId != null && int.TryParse(fieldInfo.ReplacedId.ID, out replacedTiId)
                            && fieldInfo.TiReplaced.TryGetValue(replacedTiId, out vals)
                            && vals != null && vals.ValuesByIndexes != null)
                        {
                            vals.ValuesByIndexes.TryGetValue(i, out val);
                        }

                        return val;
                    }
                case EnumColumnType.FactUnbalancePercent:
                case EnumColumnType.FactUnbalanceValue:
                    {
                        return fieldInfo.TryGetBalaanceValue(i, fieldInfo.ColumnType);
                    }
                default:
                    {
                        //Обычное значение
                        k++;
                        if (fieldInfo.Values != null)
                        {
                            if (fieldInfo.IsOv)
                            {
                                //Считаем неразнесенное значение
                                return fieldInfo.TryCalculateUnreplacedValue(i);
                            }
                        }

                        return fieldInfo.TryGetValue(i);
                    }
            }
            return null;
        }

        public void SetValue(string key, int indxValue, byte channel, ID_Hierarchy id, EnumDataSourceType? dataSource, string measureTypeUn, 
            object value, bool useMeasureModule = true, ID_Hierarchy replacedId = null, string groupByReplacedTiName = null)
        {
            base[key] = value;
            if (indxValue > 0 && replacedId == null) return;

            var tex = Table as DataTableEx;
            if (tex == null) return;
                        
            tex.SetFieldInfo(key, indxValue, channel, id, dataSource, measureTypeUn, useMeasureModule, replacedId, groupByReplacedTiName);
        }

        public bool TryGetIndexByItemName(string key, out DetailFieldInfo fieldInfo)
        {
            fieldInfo = null;

            var tex = Table as DataTableEx;
            if (tex == null) return false;

            return tex.ItemsIndexesDict.TryGetValue(key, out fieldInfo);
        }


        #region IFindableItem

        public object GetItemForSearch()
        {
            var t = this.Table as DataTableEx;
            if (t == null) return null;

            if (string.IsNullOrEmpty(t.FieldForSearch)) return null;

            return this[t.FieldForSearch];
        }

        public IEnumerable GetChildren()
        {
            return null;
        }

        #endregion

        //public object this[string key]
        //{
        //    get
        //    {
        //        return base[key];
        //    }

        //    set { base[key] = value; }
        //}

        #endregion
    }

    [Serializable]
    public class DataColumnEx : DataColumn
    {
        public DataColumnEx(string columnName, Type dataType): base(columnName, dataType)
        {

        }

        public string GroupName;
        public bool IsFValue;
    }
}
