using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.GroupTpFactory;
using Proryv.AskueARM2.Client.ServiceReference.Service.Data;

namespace Proryv.AskueARM2.Client.ServiceReference.Service
{
    /// <summary>
    /// Словарь с ТИ
    /// </summary>
    public partial class EnumClientServiceDictionary
    {
        /// <summary>
        /// Локер для синхронизации словарей TIbyPS и TIHierarchyList
        /// </summary>
        private static readonly ReaderWriterLockSlim _tiByPsToTiDictsSyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public static HierarchyTreeDictionary<int, TInfo_TI> TIHierarchyList =
           new HierarchyTreeDictionary<int, TInfo_TI>(

        #region keyLoader
                (delegate (int key, IDictionary<int, TInfo_TI> dict, out TInfo_TI ti)
                {
                    //KeyLoader
                    ti = null;
                    List<TInfo_TI> validatedTis = null;
                    bool isFound;
                    //Берем все ТИ с ПС
                    try
                    {
                        var tiList = ServiceFactory.ArmServiceInvokeSync<List<TInfo_TI>>("TREE_GetListTI", new List<int> { key });
                        if (tiList == null || tiList.Count <= 0) return false;

                        isFound = UpdateDictTi(dict, tiList, key, out ti, out validatedTis);
                    }
                    catch 
                    {
                        return false;
                    }

                    //Заполняем словарь соотношений ТИ по ПС
                    UpdateTiByPs(validatedTis);

                    return isFound;
                }),

        #endregion
        #region valuesPreparer
                (delegate (HashSet<int> keyList, IDictionary<int, TInfo_TI> dict, CancellationToken? token)
                {
                    if (keyList == null || keyList.Count == 0 || !_tiByPsToTiDictsSyncLock.TryEnterUpgradeableReadLock(_maxLockWait)) return; //Нечего подготавливать
                    try
                    {
                        if (token.HasValue && token.Value.IsCancellationRequested)
                        {
                            return;
                        }

                        var ids = dict.Count > 0 ? keyList.Except(dict.Keys).ToList() : keyList.ToList();
                        if (ids.Count == 0) return;

                        var tiList = ClientSideMultithreadBuilder
                        .BuildandWaitResult<List<TInfo_TI>, HierarchyLoaderParams>(new HierarchyLoaderParams
                        {
                            ParrentList = ids,
                        }, "GetTis", null, cancellationToken: token);

                        if (tiList == null || tiList.Count == 0) return;

                        if (token.HasValue && token.Value.IsCancellationRequested)
                        {
                            return;
                        }

                        if (!_tiByPsToTiDictsSyncLock.TryEnterWriteLock(_maxLockWait)) return;
                        try
                        {
                            var notExistsTis = new List<TInfo_TI>();

                            foreach (TInfo_TI ti in tiList)
                            {
                                if (!dict.ContainsKey(ti.TI_ID))
                                {
                                    notExistsTis.Add(ti);
                                    dict[ti.TI_ID] = ti;
                                }
                            }

                            var comparer = new TInfo_TIEqualityComparer();

                            //Заполняем словарь соотношений ТИ по ПС
                            if (notExistsTis.Count > 0)
                            {
                                foreach (var tiGroupByPS in notExistsTis.GroupBy(k => k.PS_ID))
                                {
                                    List<TInfo_TI> tis;
                                    if (!TIbyPS.TryGetValue(tiGroupByPS.Key, out tis, withLoad: false))
                                    {
                                        TIbyPS[tiGroupByPS.Key] = tis = new List<TInfo_TI>();
                                    }

                                    tis.AddRange(tiGroupByPS.Distinct(comparer));
                                }
                            }
                        }
                        finally
                        {
                            _tiByPsToTiDictsSyncLock.ExitWriteLock();
                        }

                    }
                    finally
                    {
                        _tiByPsToTiDictsSyncLock.ExitUpgradeableReadLock();
                    }
                }),

        #endregion
        #region valuesFinder
                (delegate (string searchText, string paramName, enumTypeHierarchy parrentHirerarchy, IDictionary<int, TInfo_TI> dict, List<int> parrentsList, enumTIType? tiTypeFilter, int? treeID)
                {
                    //ValuesFinder
                    var tiList = new List<TInfo_TI>();
                    try
                    {
                        var tiVals = ARM_Service.TREE_FindTIbyParamName(searchText, paramName, parrentHirerarchy, parrentsList, tiTypeFilter, treeID);
                        if (tiVals != null)
                        {
                            tiList.AddRange(tiVals);
                        }
                    }
                    catch
                    {
                    }

                    return tiList;
                }),

        #endregion
        #region keyReloader
                keyReloader: (delegate (HashSet<int> keys, IDictionary<int, TInfo_TI> dict)
                {
                    try
                    {
                        //Берем все ТИ с ПС
                        List<TInfo_TI> tiList = ARM_Service.TREE_GetListTI(keys.ToList());
                        if (tiList != null && tiList.Count > 0)
                        {
                            List<TInfo_TI> validatedTis;
                            TInfo_TI ti;
                            UpdateDictTi(dict, tiList, -1, out ti, out validatedTis);
                        }
                    }
                    catch
                    {
                    }
                })
        #endregion

               );

        public static bool UpdateDictTi(IDictionary<int, TInfo_TI> dict, IEnumerable<TInfo_TI> recivedTis, int key, out TInfo_TI foundedTi, out List<TInfo_TI> validatedTis)
        {
            bool isFound = false;
            foundedTi = null;
            validatedTis = new List<TInfo_TI>();

            foreach (TInfo_TI t in recivedTis)
            {
                TInfo_TI existTi;
                if (!dict.TryGetValue(t.TI_ID, out existTi) || existTi == null)
                {
                    //Такой ТИ Еще нет
                    dict[t.TI_ID] = t;
                }
                else
                {
                    //Есть, обновляем поля
                    if (InfoTiPropertyDict == null)
                    {
                        InfoTiPropertyDict = new ConcurrentDictionary<string, PropertyInfo>(typeof(TInfo_TI)
                            .GetProperties()
                            .ToDictionary(p => p.Name));

                    }

                    foreach (var propertyPair in InfoTiPropertyDict)
                    {
                        var prop = propertyPair.Value;
                        if (!prop.CanWrite) continue;

                        try
                        {
                            prop.SetValue(existTi, prop.GetValue(t, null), null);
                        }
                        catch (Exception) { }
                    }
                }

                validatedTis.Add(existTi ?? t);

                if (!isFound && t.TI_ID == key)
                {
                    foundedTi = existTi ?? t;
                    isFound = true;
                }
            }

            return isFound;
        }

        /// <summary>
        /// Словарь соотношений ТИ по ПС
        /// </summary>
        public static HierarchyTreeDictionary<int, List<TInfo_TI>> TIbyPS = new HierarchyTreeDictionary<int, List<TInfo_TI>>(
            (delegate (int key, IDictionary<int, List<TInfo_TI>> dict, out List<TInfo_TI> tiList)
            {
                #region keyLoader

                if (!_tiByPsToTiDictsSyncLock.TryEnterUpgradeableReadLock(_maxLockWait))
                {
                    tiList = null;
                    return false;
                }

                try
                {
                    tiList = ARM_Service.TREE_GetTIHierarchyList(new List<int> { key }, false, false);

                    if (tiList == null || !_tiByPsToTiDictsSyncLock.TryEnterWriteLock(_maxLockWait)) return true;

                    try
                    {
                        if (tiList.Count == 0)
                        {
                            //На этой ПС нет ТИ, больше не нужно по ней ничего полгружать
                            dict[key] = tiList;
                        }
                        else
                        {
                            //Предполагаем что этих объектов нет и в общем словаре ТИ, добавляем туда
                            var validatedTis = TIHierarchyList.Merge(tiList.Select(tt => new KeyValuePair<int, TInfo_TI>(tt.TI_ID, tt)));
                            var tiGroupByPS = validatedTis
                                .Where(t => t.PS_ID == key)
                                .ToList();

                            List<TInfo_TI> tisByPs;
                            if (!dict.TryGetValue(key, out tisByPs) || tisByPs == null)
                            {
                                dict[key] = tiGroupByPS;
                            }
                            else
                            {
                                tisByPs.Clear();
                                tisByPs.AddRange(tiGroupByPS);
                            }
                        }
                    }
                    finally
                    {
                        _tiByPsToTiDictsSyncLock.ExitWriteLock();
                    }

                    return true;
                }
                catch 
                {
                    tiList = null;
                    return false;
                }
                finally
                {
                    _tiByPsToTiDictsSyncLock.ExitUpgradeableReadLock();
                }

                #endregion

            }),

        #region valuesPreparer

            (delegate (HashSet<int> keyList, IDictionary<int, List<TInfo_TI>> dict, CancellationToken? token)
            {
                if (keyList == null || keyList.Count == 0 || !_tiByPsToTiDictsSyncLock.TryEnterUpgradeableReadLock(_maxLockWait)) return; //Нечего подготавливать
                try
                {
                    if (token.HasValue && token.Value.IsCancellationRequested)
                    {
                        return;
                    }

                    var ids = dict.Count > 0 ? keyList.Except(dict.Keys).ToList() : keyList.ToList();
                    if (ids.Count == 0) return;

                    //var tiList = ARM_Service.TREE_GetTIHierarchyList(ids, false, false);

                    var tiList = ClientSideMultithreadBuilder
                        .BuildandWaitResult<List<TInfo_TI>, HierarchyLoaderParams>(new HierarchyLoaderParams
                        {
                            ParrentList = ids,
                            IsCA = false,
                            IsTP = false,
                        }, "GetTisByParent", null, cancellationToken: token);

                    if (tiList == null)
                    {
                        return;
                    }

                    if (token.HasValue && token.Value.IsCancellationRequested)
                    {
                        return;
                    }

                    if (!_tiByPsToTiDictsSyncLock.TryEnterWriteLock(_maxLockWait)) return;
                    try
                    {
                        //if (tiList == null)
                        //{
                        //    //По этим ПС не вернулись ТИ, помечаем пустыми
                        //    foreach (var psId in ids)
                        //    {
                        //        dict[psId] = new List<TInfo_TI>();
                        //    }
                        //}
                        //else
                        //{
                            //Предполагаем что этих объектов нет и в общем словаре ТИ, добавляем туда
                            var tisByPs = TIHierarchyList
                            .Merge(tiList.Select(tt => new KeyValuePair<int, TInfo_TI>(tt.TI_ID, tt)))
                            .GroupBy(g => g.PS_ID)
                            .ToDictionary(k => k.Key, v => v.ToList());

                            foreach (var psId in ids)
                            {
                                List<TInfo_TI> tis;
                                if (tisByPs.TryGetValue(psId, out tis))
                                {
                                    dict[psId] = tis;
                                }
                                else
                                {
                                    dict[psId] = new List<TInfo_TI>();
                                }
                            }
                        //}
                    }
                    finally
                    {
                        _tiByPsToTiDictsSyncLock.ExitWriteLock();
                    }
                }
                finally
                {
                    _tiByPsToTiDictsSyncLock.ExitUpgradeableReadLock();
                }
            })

        #endregion

            , null);
    }
}
