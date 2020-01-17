using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Both.VisualCompHelpers.Interfaces;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.Service.Dictionary;

namespace Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter.DataTableExtention
{
    public class DetailFieldInfo : INotifyPropertyChanged, IHaveMeasureUnit, IDisposable
    {
        /// <summary>
        /// Информация об объекте
        /// </summary>
        public ID_Hierarchy Id { get; set; }
        /// <summary>
        /// ?Индекс получасовки?
        /// </summary>
        public int IndexHalfhour;
        /// <summary>
        /// Измерительный канал
        /// </summary>
        public byte ChannelType { get; set; }
        /// <summary>
        /// Источник
        /// </summary>
        public EnumDataSourceType? DataSource { get; set; }

        /// <summary>
        /// Идентификатор ТИ, которую замещал текущий ОВ
        /// </summary>
        public ID_Hierarchy ReplacedId { get; set; }

        private string _measureUnitUn;
        /// <summary>
        /// Ед. им.
        /// </summary>
        public string MeasureUnitUn
        {
            get { return _measureUnitUn; }
            set
            {
                if (string.Equals(_measureUnitUn, value)) return;

                _measureUnitUn = value;
                _stringName = null;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("MeasureUnitUn"));
            }
        }

        //Использовать или нет преобразователь размерности (кило, мега и т.д.)
        public bool UseMeasureModule;

        public EnumColumnType ColumnType;
        private string _columnName;
        /// <summary>
        /// Информация о колонке
        /// </summary>
        public string ColumnName
        {
            get
            {
                return _columnName;
            }
            set
            {
                if (string.Equals(_columnName, value)) return;

                _columnName = value;

                if (_columnName.StartsWith("ovInfo"))
                {
                    ColumnType = EnumColumnType.OvInfo;
                }
                else if(_columnName.StartsWith("ovValue"))
                {
                    ColumnType = EnumColumnType.OvValue;
                }
                else if (_columnName.StartsWith("FactUnbalanceValue"))
                {
                    ColumnType = EnumColumnType.FactUnbalanceValue;
                }
                else if (_columnName.StartsWith("FactUnbalancePercent"))
                {
                    ColumnType = EnumColumnType.FactUnbalancePercent;
                }
            }
        }

        /// <summary>
        /// Таблица
        /// </summary>
        public DataTableEx TableEx;

        private string _stringName;

        /// <summary>
        /// Для группировки по замещаемой ТИ
        /// </summary>
        public string GroupByReplacedTiName;

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_stringName)) return _stringName;

            var name = new StringBuilder();

            if (ReplacedId != null)
            {
                //Это колонка с идентификаторами замещаемой ТИ
                //return string.Empty;
                var freeHierarchyObject =
                    HierarchyObjectHelper.ToHierarchyObject(ReplacedId.ID, ReplacedId.TypeHierarchy);
                if (freeHierarchyObject != null)
                {
                    name.Append(freeHierarchyObject.Name).Append(" замещаемая ТИ ");
                }
            }

            if (ChannelType > 0)
            {
                string channelName;
                if (ChannelFactory.ChanelTypeNameFSK.TryGetValue(ChannelType, out channelName) &&
                    !string.IsNullOrEmpty(channelName))
                {
                    name.Append(channelName).Append(" ");
                }
                
                if (ChannelType > 10 && Id != null && Id.TypeHierarchy == enumTypeHierarchy.Info_TI)
                {
                    int tiId;
                    int.TryParse(Id.ID, out tiId);
                    name.Append(ChannelDictionaryClass.MakeZoneNamePrefix(tiId, ChannelType, DateTime.Now, DateTime.Now))
                        .Append(" ");
                }
            }

            if (Id != null)
            {
                var freeHierarchyObject =
                    HierarchyObjectHelper.ToHierarchyObject(Id.ID, Id.TypeHierarchy);
                if (freeHierarchyObject != null)
                {
                    name.Append(freeHierarchyObject.Name);
                }
            }

            var k = EnumClientServiceDictionary.DataSourceTypeList.FirstOrDefault(d => d.Key == DataSource);
            if (!string.IsNullOrEmpty(k.Value))
            {
                name.Append("\n").Append(k.Value);
            }
            
            if (!string.IsNullOrEmpty(MeasureUnitUn))
            {
                var measureName = EnumClientServiceDictionary.GetMeasureUnitAbbreviation(MeasureUnitUn);
                name.Append(" ").Append(measureName);
            }

            _stringName = name.ToString();

            return _stringName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Информация по замещаемых ТИ, с какого по какой индекс замещались, сгруппировано по идентификатору замещаемой ТИ
        /// </summary>
        public Dictionary<int, TDateTimeOV> TiReplaced;

        public Dictionary<int, IFValue> Values;

        #region По балансам

        public List<BalanceFreeHierarchyCalculatedDiscretePeriod> BalanceValues;
        #endregion

        public IFValue TryGetValue(int i)
        {
            if (Values == null) return null;

            IFValue val;
            Values.TryGetValue(i, out val);

            return val;
        }

        public IFValue TryGetBalaanceValue(int i, EnumColumnType columnType)
        {
            //Балансы
            if (BalanceValues == null) return null;

            var bv = BalanceValues.ElementAtOrDefault(i);
            if (bv == null) return null;

            //Формируем получасовку, согласно столбцу
            switch (columnType)
            {
                case EnumColumnType.FactUnbalanceValue:
                    return new TVALUES_DB
                    {
                        F_FLAG = bv.F_FLAG,
                        F_VALUE = bv.FactUnbalanceValue,
                    };
                case EnumColumnType.FactUnbalancePercent:
                    return new TVALUES_DB
                    {
                        F_FLAG = bv.F_FLAG,
                        F_VALUE = bv.FactUnbalancePercent,
                    };
                default:
                    return null;
            }
        }

        public TOvValue TryCalculateUnreplacedValue(int i)
        {
            IFValue val;
            if (Values == null || !Values.TryGetValue(i, out val) || val == null) return null;

            var ovVal = new TOvValue
            {
                F_VALUE = val.F_VALUE,
                F_FLAG = val.F_FLAG,
                UnreplacedValue = val.F_VALUE,
            };

            if (!val.F_FLAG.HasFlag(VALUES_FLAG_DB.IsOv) //Если не замещал
                || val.F_VALUE == 0) return ovVal; //Или нет значений

            if (TiReplaced != null)
            {
                foreach (var tiReplaced in TiReplaced.Values)
                {
                    TVALUES_DB replacedVal;
                    if (tiReplaced.ValuesByIndexes == null
                        || !tiReplaced.ValuesByIndexes.TryGetValue(i, out replacedVal)
                        || replacedVal == null) continue;

                    ovVal.UnreplacedValue -= replacedVal.F_VALUE;
                }
            }

            return ovVal;
        }

        #region Dispose

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            // подавляем финализацию
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            try
            {
                BalanceValues = null;
                Values = null;
                TiReplaced = null;
                TableEx = null;
                Id = null;
                ReplacedId = null;
            }
            finally
            {
                _disposed = true;
            }

        }

        // Деструктор
        ~DetailFieldInfo()
        {
            Dispose(false);
        }

        #endregion

        public bool IsOv;
    }

    public class DetailFieldInfoEqualityComparer : IEqualityComparer<DetailFieldInfo>
    {
        public bool Equals(DetailFieldInfo x, DetailFieldInfo y)
        {
            if (x == null || y == null || x.Id == null || y.Id == null) return false;

            return x.Id.TypeHierarchy == y.Id.TypeHierarchy &&
                   string.Equals(x.Id.ID, y.Id.ID, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(DetailFieldInfo obj)
        {
            var typeHierarchy = (uint)obj.Id.TypeHierarchy;
            typeHierarchy = (typeHierarchy << 26) | (typeHierarchy >> (32 - 26));

            int id = 0;
            if (string.IsNullOrEmpty(obj.Id.ID) || int.TryParse(obj.Id.ID, out id))
            {
                return (int)(id ^ typeHierarchy);
            }

            return (int)(obj.Id.ID.GetHashCode() ^ typeHierarchy);
        }
    }

    public enum EnumColumnType
    {
        None = 0,
        OvInfo = 1,
        OvValue = 2,

        FactUnbalanceValue = 3,
        FactUnbalancePercent = 4,
    }
}