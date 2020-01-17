using System.Collections;
using System.Collections.Generic;

namespace Proryv.AskueARM2.Server.DBAccess.Internal.Formulas
{
    public class FunctionArgumentCollection<T> : List<object>,
        IEnumerable<T>, ITypedIndexer<T> where T : class
    {
        public FunctionArgumentCollection(int capacity) : base(capacity)
        {
        }


        public FunctionArgumentCollection() : base()
        {
            base.Capacity = 4;
        }


        public T this[int index]
        {
            get
            {
                if (base[index] != null)
                    return  base[index] as T;
                return null;

            }
            set
            {
                while (index+1 > base.Count)
                {
                    base.Add(null);
                }
                base[index] = value;
            }
        }

        public object GetTypedValue(int index)
        {
            var val = base[index];
            if (val != null && val.GetType() == typeof(T))
                return (T) val;
            return null;
        }

        public void SetTypedValue(int index, object value)
        {
            base[index] = value;
        }

        public int GetLenght()
        {
            return base.Count;
        }


        private class CustomEnumerator : IEnumerator<T>
        {
            private readonly List<object> _list;
            private int currentindex = 0;

            public CustomEnumerator(List<object> list)
            {
                _list = list;
            }

            public void Dispose()
            {
                //throw new NotImplementedException();
            }

            public bool MoveNext()
            {
                return _list.Count > currentindex + 1;
            }

            public void Reset()
            {
                currentindex = 0;
            }

            public T Current
            {
                get
                {
                    if (_list[currentindex] != null)
                    {
                        return _list[currentindex] as T;
                    }
                    return null;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    currentindex++;
                    return Current;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new CustomEnumerator(this);
        }
    }
}