using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.AskueARM2.Client.ServiceReference.Service.Data
{
    /// <summary>
    /// Метод на загрузку значения если ключ не найден
    /// </summary>
    /// <param name="Key"></param>
    /// <returns></returns>
    public delegate bool KeyLoader<TKey, TValue>(TKey Key, IDictionary<TKey, TValue> dict, out TValue Value)
        where TValue : class;

    /// <summary>
    /// Метод на обновление словаря
    /// </summary>
    /// <typeparam name="TKey">Тип ключа</typeparam>
    /// <typeparam name="TValue">Тип содержимого</typeparam>
    /// <param name="Dict"></param>
    public delegate void ValuesRefresher<TKey, TValue>(IDictionary<TKey, TValue> dict)
        where TValue : class;

    /// <summary>
    /// Метод на предварительную загрузку ТИ, без возврата этих ТИ
    /// </summary>
    /// <param name="Key"></param>
    /// <returns></returns>
    public delegate void ValuesPreparer<TKey, TValue>(HashSet<TKey> KeyList, IDictionary<TKey, TValue> dict, CancellationToken? token = null)
        where TValue : class;

    /// <summary>
    /// Метод на поиск ТИ по параметру
    /// </summary>
    /// <param name="Key"></param>
    /// <returns></returns>
    public delegate List<TValue> ValuesFinder<TKey, TValue>(
        string searchText, string paramName, enumTypeHierarchy parrentHirerarchy, IDictionary<TKey, TValue> dict, List<int> parrentsList, enumTIType? tiTypeFilter = null, int? treeID = null)
        where TValue : class;


    /// <summary>
    /// Метод на подготовку дочерних объектов
    /// </summary>
    /// <returns></returns>
    public delegate HashSet<TKey> ChildrenPreparer<TKey, TValue>(List<TValue> value, IDictionary<TKey, TValue> dict, IEqualityComparer<TKey> comparer, int maxNodeLevel)
        where TValue : class;

    /// <summary>
    /// Метод на предварительную загрузку по списку родителей
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="parents"></param>
    /// <param name="dict"></param>
    public delegate void PreparerByParents<TKey, TValue>(List<ID_TypeHierarchy> parents, List<TKey> keys, IDictionary<TKey, TValue> dict);

    /// <summary>
    /// Перезагрузчик объекта из БД
    /// </summary>
    /// <typeparam name="TKey">Тип ключа</typeparam>
    /// <typeparam name="TValue">Тип объекта</typeparam>
    /// <param name="key">Ключ</param>
    /// <returns>Удачно или нет</returns>
    public delegate void KeyReloader<TKey, TValue>(HashSet<TKey> key, IDictionary<TKey, TValue> dict)
        where TValue : class;

    public class DictionaryLocker
    {
        private static DictionaryLocker locker = new DictionaryLocker();
        private static int count = 0;
        public static System.Threading.ReaderWriterLockSlim dictRWLock;

        public DictionaryLocker()
            : base()
        {
            if (dictRWLock != null)
            {
                throw new Exception();
            }

            dictRWLock = new System.Threading.ReaderWriterLockSlim();

            count++;
            if (count > 1)
            {
                throw new Exception();
            }

        }

    }

    public class HierarchyTreeDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    where TValue : class
    {


        public HierarchyTreeDictionary(KeyLoader<TKey, TValue> keyLoader, ValuesPreparer<TKey, TValue> valuesPreparer = null
            , ValuesFinder<TKey, TValue> valuesFinder = null, IEqualityComparer<TKey> comparer = null
            , ChildrenPreparer<TKey, TValue> childrenPreparer = null
            , KeyReloader<TKey, TValue> keyReloader = null
            , PreparerByParents<TKey, TValue> preparerByParents = null)
        {
            _keyLoader = keyLoader;
            _valuesPreparer = valuesPreparer;
            _valuesFinder = valuesFinder;
            _childrenPreparer = childrenPreparer;
            _keyReloader = keyReloader;
            _preparerByParents = preparerByParents;
            _comparer = comparer;

            _dict = _comparer != null ? new Dictionary<TKey, TValue>(_comparer) : new Dictionary<TKey, TValue>();
        }

        public HierarchyTreeDictionary(KeyLoader<TKey, TValue> keyLoader, ValuesRefresher<TKey, TValue> valuesRefresher, IEqualityComparer<TKey> comparer = null)
        {
            _keyLoader = keyLoader;
            _valuesRefresher = valuesRefresher;
            _comparer = comparer;

            _dict = _comparer != null ? new Dictionary<TKey, TValue>(_comparer) : new Dictionary<TKey, TValue>();
        }

        //private static ConcurrentStack<int> _keysForManyLoading;
        //private static Timer _timerForManyLoading;
        //private static readonly object _manyLoadingSyncLock = new object();

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (TryGetValue(key, out value)) return value;

                return default(TValue);
            }
            set
            {
                if (!_lock.TryEnterWriteLock(_maxLockWait)) return;

                try
                {
                    _dict[key] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

            }

        }

        public void Remove(TKey key)
        {
            if (!_lock.TryEnterWriteLock(_maxLockWait)) return;

            try
            {
                _dict.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public List<TValue> Values
        {
            get
            {
                if (!_lock.TryEnterReadLock(_maxLockWait)) return new List<TValue>();

                try
                {
                    return _dict.Values.ToList();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public List<TValue> GetValues(HashSet<TKey> keys, Action<string> onException = null)
        {
            var result = new List<TValue>();
            var expectedIds = _comparer == null ? new HashSet<TKey>() : new HashSet<TKey>(_comparer);

            if (!_lock.TryEnterUpgradeableReadLock(_maxLockWait)) return result;
            try
            {
                foreach (var key in keys)
                {
                    TValue val;
                    if (_dict.TryGetValue(key, out val))
                    {
                        result.Add(val);
                    }
                    else
                    {
                        expectedIds.Add(key);
                    }
                }

                if (expectedIds.Count <= 0 || _valuesPreparer == null) return result;

                if (_lock.TryEnterWriteLock(_maxLockWait))
                {
                    try
                    {
                        _valuesPreparer(expectedIds, _dict);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }

                foreach (var key in expectedIds)
                {
                    TValue val;
                    if (_dict.TryGetValue(key, out val))
                    {
                        result.Add(val);
                    }
                }
            }
            catch (Exception ex)
            {
                if (onException != null) onException.Invoke(ex.Message);
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }

            return result;
        }

        /// <summary>
        /// Подготавливаем словарь к использованию
        /// </summary>
        /// <param name="keyList">Набор ключей</param>
        /// <param name="onException">Вызываем в случае исключения</param>
        /// <returns>Набор значений</returns>
        public void Prepare(HashSet<TKey> keyList, Action<string> onException = null, CancellationToken? token = null)
        {
            if (keyList == null || keyList.Count == 0 || _valuesPreparer == null) return;

            if (!_lock.TryEnterUpgradeableReadLock(_maxLockWait)) return;
            try
            {
                HashSet<TKey> keysForRequest;

                if (_dict.Count == 0)
                {
                    keysForRequest = keyList;
                }
                else
                {
                    if (_comparer == null)
                    {
                        keysForRequest = new HashSet<TKey>(keyList.Except(_dict.Keys, _comparer), _comparer);
                    }
                    else
                    {
                        keysForRequest = new HashSet<TKey>(keyList.Except(_dict.Keys));
                    }
                }

                if (keysForRequest.Count == 0) return;

                if (_lock.TryEnterWriteLock(_maxLockWait))
                    try
                    {
                        _valuesPreparer(keysForRequest, _dict, token);
                    }
                    catch (AggregateException aex)
                    {
                        if (onException != null) onException.Invoke(aex.ToException().Message);
                    }
                    catch (Exception ex)
                    {
                        if (onException != null) onException.Invoke(ex.Message);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }


        private readonly KeyReloader<TKey, TValue> _keyReloader;
        /// <summary>
        /// Перезагружаем объект из базы данных (обновление полей). Если объект еще не загружен, то он просто загружается.
        /// </summary>
        /// <param name="key"></param>
        public void ReloadValue(HashSet<TKey> keys)
        {
            if (_keyReloader == null) return;

            _lock.EnterWriteLock();
            try
            {
                _keyReloader(keys, _dict);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private readonly PreparerByParents<TKey, TValue> _preparerByParents;


        private readonly ReaderWriterLockSlim _parentsLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Родительские объекты, которые уже подготавливали
        /// </summary>
        private readonly HashSet<ID_TypeHierarchy> _parentsWhereWasPrepared = new HashSet<ID_TypeHierarchy>(new ID_TypeHierarchy_EqualityComparer());

        private readonly ID_TypeHierarchy_EqualityComparer _idTypeHierarchyEquality = new ID_TypeHierarchy_EqualityComparer();

        public void PrepareByParents(List<ID_TypeHierarchy> parents, HashSet<TKey> keys = null, Action<string> onException = null)
        {
            if (_preparerByParents == null || (parents == null && (keys == null || keys.Count == 0))) return;

            var pIds = new List<ID_TypeHierarchy>();
            if (parents != null && parents.Count > 0)
            {
                _parentsLock.EnterReadLock();
                try
                {
                    pIds.AddRange(parents.Except(_parentsWhereWasPrepared, _idTypeHierarchyEquality));
                }
                finally
                {
                    _parentsLock.ExitReadLock();
                }
            }

            var fIds = new List<TKey>();

            if (!_lock.TryEnterUpgradeableReadLock(_maxLockWait)) return;
            try
            {
                if (keys != null && keys.Count > 0)
                {
                    fIds.AddRange(_comparer != null ? keys.Except(_dict.Keys, _comparer) : keys.Except(_dict.Keys));
                }

                //if (pIds.Count == 0 && fIds.Count == 0) return;

                _lock.EnterWriteLock();
                try
                {
                    _preparerByParents(pIds, fIds, _dict);
                }
                catch (AggregateException aex)
                {
                    if (onException != null) onException.Invoke(aex.ToException().Message);
                    pIds.Clear();
                }
                catch (Exception ex)
                {
                    if (onException != null) onException.Invoke(ex.Message);
                    pIds.Clear();
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }

            _parentsLock.EnterWriteLock();
            try
            {
                if (pIds.Count > 0) _parentsWhereWasPrepared.UnionWith(pIds);
            }
            finally
            {
                _parentsLock.ExitWriteLock();
            }
        }

        public List<TValue> Merge(IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        {
            var values = new List<TValue>();

            if (!_lock.TryEnterUpgradeableReadLock(_maxLockWait)) return values;

            try
            {
                var excepted = new List<KeyValuePair<TKey, TValue>>();
                foreach (var pair in enumerable)
                {
                    TValue value;
                    if (!_dict.TryGetValue(pair.Key, out value))
                    {
                        excepted.Add(pair);
                        values.Add(pair.Value);
                    }
                    else
                    {
                        values.Add(value);
                    }
                }

                if (excepted.Count == 0) return values;

                if (!_lock.TryEnterWriteLock(_maxLockWait)) return values;
                try
                {
                    foreach (var pair in excepted)
                    {
                        _dict[pair.Key] = pair.Value;
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }

            return values;
        }

        /// <summary>
        /// Обновляем все записи в словаре из базы данных
        /// </summary>
        public void Refresh()
        {
            refreshDict();
        }

        List<TValue> voidList = new List<TValue>();

        public List<TValue> FindTop100(string searchText, string paramName, enumTypeHierarchy parrentHirerarchy, List<int> parrentsList,
            enumTIType? tiTypeFilter = null, int? treeID = null)
        {
            if (_valuesFinder != null)
            {
                _lock.EnterWriteLock();
                try
                {
                    return _valuesFinder(searchText, paramName, parrentHirerarchy, _dict, parrentsList, tiTypeFilter, treeID);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            return voidList;
        }

        public bool ContainsKey(TKey key, Action<string> onException = null)
        {
            var result = true;
            if (!_lock.TryEnterUpgradeableReadLock(_maxLockWait)) return false;
            try
            {
                if (_dict.ContainsKey(key)) return true;

                _lock.EnterWriteLock();
                try
                {
                    TValue val;
                    return _keyLoader(key, _dict, out val);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                if (onException != null) onException(ex.Message);
                result = false;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }

            return result;
        }

        /// <summary>
        /// Проверка наличия элемента без загрузки из базы данных
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKeyWithoutLoad(TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                return _dict.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool TryGetValue(TKey key, out TValue value, bool withLoad = true, Action<string> onException = null)
        {
            var result = true;
            if (!_lock.TryEnterUpgradeableReadLock(_maxLockWait))
            {
                value = default(TValue);
                return false;
            }

            try
            {
                bool canIGetValue = _dict.TryGetValue(key, out value);
                if (canIGetValue)
                {
                    return true;
                }

                if (_lock.TryEnterWriteLock(_maxLockWait))
                    try
                    {
                        return withLoad && _keyLoader(key, _dict, out value);
                    }
                    catch (Exception ex)
                    {
                        if (onException != null) onException(ex.Message);
                        value = null;
                        result = false;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }

            return result;
        }


        private readonly KeyLoader<TKey, TValue> _keyLoader;

        private readonly ValuesPreparer<TKey, TValue> _valuesPreparer;

        private readonly ValuesFinder<TKey, TValue> _valuesFinder;

        private readonly ValuesRefresher<TKey, TValue> _valuesRefresher;

        private readonly ChildrenPreparer<TKey, TValue> _childrenPreparer;

        private readonly TimeSpan _maxLockWait = new TimeSpan(0,0,20);
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        //Необходима атомарность работы с этим словарем
        private Dictionary<TKey, TValue> _dict;

        /// <summary>
        /// Используется для потоковой блокировки доступа к InternalDictionary переводим в static что бы разные экземпляры словарей не блокировали друг друга
        /// </summary>
        public void Reset()
        {
            _lock.EnterWriteLock();
            try
            {
                _dict = _comparer != null ? new Dictionary<TKey, TValue>(_comparer) : new Dictionary<TKey, TValue>();

                _parentsLock.EnterWriteLock();
                try
                {
                    _parentsWhereWasPrepared.Clear();
                }
                finally
                {
                    _parentsLock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private IEqualityComparer<TKey> _comparer;
        public void Reset(IEnumerable<KeyValuePair<TKey, TValue>> valuePairs, IEqualityComparer<TKey> comparer = null)
        {
            _lock.EnterWriteLock();
            try
            {
                _comparer = comparer;

                _dict = _comparer != null ? new Dictionary<TKey, TValue>(comparer) : new Dictionary<TKey, TValue>();

                foreach (var keyValuePair in valuePairs)
                {
                    _dict[keyValuePair.Key] = keyValuePair.Value;
                }

                _parentsLock.EnterWriteLock();
                try
                {
                    _parentsWhereWasPrepared.Clear();
                }
                finally
                {
                    _parentsLock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _dict.Clear();
                _parentsLock.EnterWriteLock();
                try
                {
                    _parentsWhereWasPrepared.Clear();
                }
                finally
                {
                    _parentsLock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            if (!_lock.TryEnterUpgradeableReadLock(_maxLockWait)) return null;
            try
            {
                if (_dict.Count == 0)
                {
                    refreshDict();
                }

                return _dict.GetEnumerator();
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (!_lock.TryEnterUpgradeableReadLock(_maxLockWait)) return null;
            try
            {
                if (_dict.Count == 0)
                {
                    refreshDict();
                }

                return _dict.GetEnumerator();
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _dict.Keys;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            if (_dict == null) return null;

            _lock.EnterReadLock();
            try
            {
                return _dict.ToDictionary(k => k.Key, v => v.Value);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        void refreshDict()
        {
            if (_valuesRefresher != null)
            {
                _lock.EnterWriteLock();
                try
                {
                    _valuesRefresher(_dict);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }
        
        public HashSet<TKey> ChildrenPrepare(HashSet<TKey> keys, Action<string> onException = null, int maxNodeLevel = 3)
        {
            if (_childrenPreparer == null || _dict == null || keys == null || keys.Count == 0) return new HashSet<TKey>();

            var values = GetValues(keys, onException);
            if (values == null || values.Count == 0) return new HashSet<TKey>();

            _lock.EnterWriteLock();
            try
            {
                return _childrenPreparer(values, _dict, _comparer, maxNodeLevel);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
