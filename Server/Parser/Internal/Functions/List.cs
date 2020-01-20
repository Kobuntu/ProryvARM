using System;
using System.Collections;
using System.Collections.Generic;
using Proryv.Servers.Calculation.Parser.Internal.Common;

namespace Proryv.Servers.Calculation.Parser.Internal.Functions
{
    #region List
    /// <summary>
    /// Base class for all List classes.
    /// </summary>
    public interface IStiList : IEnumerable
    {
        #region Properties
        /// <summary>
        /// Gets specified name of list. List name equal to name of list class.
        /// </summary>
        string ListName
        {
            get;
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        Type ListType
        {
            get;
        }

        int Count
        {
            get;
        }
        #endregion

        #region Methods
        void AddElement(object value);

        string ToQueryString();

        string ToQueryString(string quotationMark);

        void Clear();
        #endregion

    }
    #endregion

    #region BoolList
    public class BoolList : List<bool>, IStiList
    {
        #region Properties
        /// <summary>
        /// Gets specified name of List. List name equal to name of List class.
        /// </summary>
        public string ListName
        {
            get
            {
                return "BoolList";
            }
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        public Type ListType
        {
            get
            {
                return typeof(bool);
            }
        }
        #endregion

        #region Methods
        public void AddElement(object value)
        {
            this.Add((bool)ProryvConvert.ChangeType(value, typeof(bool)));
        }

        public string ToQueryString()
        {
            return Func.EngineHelper.ToQueryString(this, null, null);
        }

        public string ToQueryString(string quotationMark)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, null);
        }

        public override bool Equals(object obj)
        {
            var list2 = obj as BoolList;
            if (list2 == null || this.Count != list2.Count) return false;

            for (var index = 0; index < this.Count; index++)
            {
                if (Comparer.Default.Compare(this[index], list2[index]) != 0) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion

    #region CharList
    public class CharList : List<char>, IStiList
    {
        #region Properties
        /// <summary>
        /// Gets specified name of List. List name equal to name of List class.
        /// </summary>
        public string ListName
        {
            get
            {
                return "CharList";
            }
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        public Type ListType
        {
            get
            {
                return typeof(char);
            }
        }
        #endregion

        #region Methods
        public void AddElement(object value)
        {
            this.Add((char)ProryvConvert.ChangeType(value, typeof(char)));
        }

        public string ToQueryString()
        {
            return Func.EngineHelper.ToQueryString(this, null, null);
        }

        public string ToQueryString(string quotationMark)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, null);
        }

        public override bool Equals(object obj)
        {
            var list2 = obj as CharList;
            if (list2 == null || this.Count != list2.Count) return false;

            for (var index = 0; index < this.Count; index++)
            {
                if (Comparer.Default.Compare(this[index], list2[index]) != 0) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion

    #region DateTimeList
    public class DateTimeList : List<DateTime>, IStiList
    {
        #region Properties
        /// <summary>
        /// Gets specified name of List. List name equal to name of List class.
        /// </summary>
        public string ListName
        {
            get
            {
                return "DateTimeList";
            }
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        public Type ListType
        {
            get
            {
                return typeof(DateTime);
            }
        }
        #endregion

        #region Methods
        public bool Contains(DateTime? value)
        {
            return this.Contains(value.GetValueOrDefault());
        }

        public void AddElement(object value)
        {
            this.Add((DateTime)ProryvConvert.ChangeType(value, typeof(DateTime)));
        }

        public string ToQueryString()
        {
            return Func.EngineHelper.ToQueryString(this, null, null);
        }
        public string ToQueryString(string quotationMark)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, null);
        }

        public string ToQueryString(string quotationMark, string dateTimeFormat)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, dateTimeFormat);
        }

        public override bool Equals(object obj)
        {
            var list2 = obj as DateTimeList;
            if (list2 == null || this.Count != list2.Count) return false;

            for (var index = 0; index < this.Count; index++)
            {
                if (Comparer.Default.Compare(this[index], list2[index]) != 0) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion

    #region TimeSpanList
    public class TimeSpanList : List<TimeSpan>, IStiList
    {
        #region Properties
        /// <summary>
        /// Gets specified name of List. List name equal to name of List class.
        /// </summary>
        public string ListName
        {
            get
            {
                return "TimeSpanList";
            }
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        public Type ListType
        {
            get
            {
                return typeof(TimeSpan);
            }
        }
        #endregion

        #region Methods
        public bool Contains(TimeSpan? value)
        {
            return Contains(value.GetValueOrDefault());
        }

        public void AddElement(object value)
        {
            this.Add((TimeSpan)ProryvConvert.ChangeType(value, typeof(TimeSpan)));
        }

        public string ToQueryString()
        {
            return Func.EngineHelper.ToQueryString(this, null, null);
        }
        public string ToQueryString(string quotationMark)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, null);
        }

        public string ToQueryString(string quotationMark, string dateTimeFormat)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, dateTimeFormat);
        }

        public override bool Equals(object obj)
        {
            var list2 = obj as TimeSpanList;
            if (list2 == null || this.Count != list2.Count) return false;

            for (var index = 0; index < this.Count; index++)
            {
                if (Comparer.Default.Compare(this[index], list2[index]) != 0) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion

    #region DecimalList
    public class DecimalList : List<decimal>, IStiList
    {
        #region Properties
        /// <summary>
        /// Gets specified name of List. List name equal to name of List class.
        /// </summary>
        public string ListName
        {
            get
            {
                return "DecimalList";
            }
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        public Type ListType
        {
            get
            {
                return typeof(decimal);
            }
        }
        #endregion

        #region Methods
        public void AddElement(object value)
        {
            this.Add((decimal)ProryvConvert.ChangeType(value, typeof(decimal)));
        }

        public string ToQueryString()
        {
            return Func.EngineHelper.ToQueryString(this, null, null);
        }

        public string ToQueryString(string quotationMark)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, null);
        }

        public override bool Equals(object obj)
        {
            var list2 = obj as DecimalList;
            if (list2 == null || this.Count != list2.Count) return false;

            for (var index = 0; index < this.Count; index++)
            {
                if (Comparer.Default.Compare(this[index], list2[index]) != 0) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion

    #region FloatList
    public class FloatList : List<float>, IStiList
    {
        #region Properties
        /// <summary>
        /// Gets specified name of List. List name equal to name of List class.
        /// </summary>
        public string ListName
        {
            get
            {
                return "FloatList";
            }
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        public Type ListType
        {
            get
            {
                return typeof(float);
            }
        }
        #endregion

        #region Methods
        public void AddElement(object value)
        {
            this.Add((float)ProryvConvert.ChangeType(value, typeof(float)));
        }

        public string ToQueryString()
        {
            return Func.EngineHelper.ToQueryString(this, null, null);
        }

        public string ToQueryString(string quotationMark)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, null);
        }

        public override bool Equals(object obj)
        {
            var list2 = obj as FloatList;
            if (list2 == null || this.Count != list2.Count) return false;

            for (var index = 0; index < this.Count; index++)
            {
                if (Comparer.Default.Compare(this[index], list2[index]) != 0) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion

    #region DoubleList
    public class DoubleList : List<double>, IStiList
    {
        #region Properties
        /// <summary>
        /// Gets specified name of List. List name equal to name of List class.
        /// </summary>
        public string ListName
        {
            get
            {
                return "DoubleList";
            }
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        public Type ListType
        {
            get
            {
                return typeof(double);
            }
        }
        #endregion

        #region Methods
        public void AddElement(object value)
        {
            this.Add((double)ProryvConvert.ChangeType(value, typeof(double)));
        }

        public string ToQueryString()
        {
            return Func.EngineHelper.ToQueryString(this, null, null);
        }

        public string ToQueryString(string quotationMark)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, null);
        }

        public override bool Equals(object obj)
        {
            var list2 = obj as DoubleList;
            if (list2 == null || this.Count != list2.Count) return false;

            for (var index = 0; index < this.Count; index++)
            {
                if (Comparer.Default.Compare(this[index], list2[index]) != 0) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion

    #region ByteList
    public class ByteList : List<byte>, IStiList
    {
        #region Properties
        /// <summary>
        /// Gets specified name of List. List name equal to name of List class.
        /// </summary>
        public string ListName
        {
            get
            {
                return "ByteList";
            }
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        public Type ListType
        {
            get
            {
                return typeof(byte);
            }
        }
        #endregion

        #region Methods
        public void AddElement(object value)
        {
            this.Add((byte)ProryvConvert.ChangeType(value, typeof(byte)));
        }

        public string ToQueryString()
        {
            return Func.EngineHelper.ToQueryString(this, null, null);
        }

        public string ToQueryString(string quotationMark)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, null);
        }

        public override bool Equals(object obj)
        {
            var list2 = obj as ByteList;
            if (list2 == null || this.Count != list2.Count) return false;

            for (var index = 0; index < this.Count; index++)
            {
                if (Comparer.Default.Compare(this[index], list2[index]) != 0) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion

    #region ShortList
    public class ShortList : List<short>, IStiList
    {
        #region Properties
        /// <summary>
        /// Gets specified name of List. List name equal to name of List class.
        /// </summary>
        public string ListName
        {
            get
            {
                return "ShortList";
            }
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        public Type ListType
        {
            get
            {
                return typeof(short);
            }
        }
        #endregion

        #region Methods
        public void AddElement(object value)
        {
            this.Add((short)ProryvConvert.ChangeType(value, typeof(short)));
        }

        public string ToQueryString()
        {
            return Func.EngineHelper.ToQueryString(this, null, null);
        }

        public string ToQueryString(string quotationMark)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, null);
        }

        public override bool Equals(object obj)
        {
            var list2 = obj as ShortList;
            if (list2 == null || this.Count != list2.Count) return false;

            for (var index = 0; index < this.Count; index++)
            {
                if (Comparer.Default.Compare(this[index], list2[index]) != 0) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion

    #region IntList
    public class IntList : List<int>, IStiList
    {
        #region Properties
        /// <summary>
        /// Gets specified name of List. List name equal to name of List class.
        /// </summary>
        public string ListName
        {
            get
            {
                return "IntList";
            }
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        public Type ListType
        {
            get
            {
                return typeof(int);
            }
        }
        #endregion

        #region Methods
        public void AddElement(object value)
        {
            this.Add((int)ProryvConvert.ChangeType(value, typeof(int)));
        }

        public string ToQueryString()
        {
            return Func.EngineHelper.ToQueryString(this, null, null);
        }

        public string ToQueryString(string quotationMark)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, null);
        }

        public override bool Equals(object obj)
        {
            var list2 = obj as IntList;
            if (list2 == null || this.Count != list2.Count) return false;

            for (var index = 0; index < this.Count; index++)
            {
                if (Comparer.Default.Compare(this[index], list2[index]) != 0) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion

    #region LongList
    public class LongList : List<long>, IStiList
    {
        #region Properties
        /// <summary>
        /// Gets specified name of List. List name equal to name of List class.
        /// </summary>
        public string ListName
        {
            get
            {
                return "LongList";
            }
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        public Type ListType
        {
            get
            {
                return typeof(long);
            }
        }
        #endregion

        #region Methods
        public void AddElement(object value)
        {
            this.Add((long)ProryvConvert.ChangeType(value, typeof(long)));
        }

        public string ToQueryString()
        {
            return Func.EngineHelper.ToQueryString(this, null, null);
        }

        public string ToQueryString(string quotationMark)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, null);
        }

        public override bool Equals(object obj)
        {
            var list2 = obj as LongList;
            if (list2 == null || this.Count != list2.Count) return false;

            for (var index = 0; index < this.Count; index++)
            {
                if (Comparer.Default.Compare(this[index], list2[index]) != 0) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion

    #region GuidList
    public class GuidList : List<Guid>, IStiList
    {
        #region Properties
        /// <summary>
        /// Gets specified name of List. List name equal to name of List class.
        /// </summary>
        public string ListName
        {
            get
            {
                return "GuidList";
            }
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        public Type ListType
        {
            get
            {
                return typeof(Guid);
            }
        }
        #endregion

        #region Methods
        public void AddElement(object value)
        {
            this.Add((Guid)ProryvConvert.ChangeType(value, typeof(Guid)));
        }

        public string ToQueryString()
        {
            return Func.EngineHelper.ToQueryString(this, null, null);
        }

        public string ToQueryString(string quotationMark)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, null);
        }

        public override bool Equals(object obj)
        {
            var list2 = obj as GuidList;
            if (list2 == null || this.Count != list2.Count) return false;

            for (var index = 0; index < this.Count; index++)
            {
                if (Comparer.Default.Compare(this[index], list2[index]) != 0) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion

    #region StringList
    public class StringList : List<string>, IStiList
    {
        #region Properties
        /// <summary>
        /// Gets specified name of List. List name equal to name of List class.
        /// </summary>
        public string ListName
        {
            get
            {
                return "StringList";
            }
        }

        /// <summary>
        /// Gets the type of List items. 
        /// </summary>
        public Type ListType
        {
            get
            {
                return typeof(string);
            }
        }
        #endregion

        #region Methods
        public void AddElement(object value)
        {
            this.Add((string)ProryvConvert.ChangeType(value, typeof(string)));
        }

        public string ToQueryString()
        {
            return Func.EngineHelper.ToQueryString(this, null, null);
        }

        public string ToQueryString(string quotationMark)
        {
            return Func.EngineHelper.ToQueryString(this, quotationMark, null);
        }

        public override bool Equals(object obj)
        {
            var list2 = obj as StringList;
            if (list2 == null || this.Count != list2.Count) return false;

            for (var index = 0; index < this.Count; index++)
            {
                if (Comparer.Default.Compare(this[index], list2[index]) != 0) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion
}
