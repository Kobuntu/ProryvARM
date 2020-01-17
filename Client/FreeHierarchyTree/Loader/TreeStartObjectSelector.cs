using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.ServiceReference.Data.FreeHierarchy;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.Common;
using Proryv.AskueARM2.Client.Visual.Common.Configuration;
using Proryv.AskueARM2.Client.Visual.Common.FreeHierarchy;
using Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.TreeSelector;
using Proryv.ElectroARM.Controls.Controls.Popup.Finder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Proryv.ElectroARM.Controls.Controls.FreeHierarchyTree.Loader
{
    public class TreeStartObjectSelector : IDisposable
    {
        private const short limit = 3000;

        private FreeHierarchyTree _tree;

        /// <summary>
        /// Версия работы с выделяемыми объектами
        /// </summary>
        private readonly short _versionNumber;

        /// <summary>
        /// Локер для синхронизации словарей TIbyPS и TIHierarchyList
        /// </summary>
        private static readonly ReaderWriterLockSlim _treeItemsSyncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Объекты для выбора по старому методу 0 и 1
        /// </summary>
        public List<FreeItemSelected> ObjectsFromLocal;

        /// <summary>
        /// Объекты для выбора по новому методу 2
        /// </summary>
        public Dictionary<string, List<List<ID_TypeHierarchy>>> ObjectsFromSQL;

        public TreeStartObjectSelector(object serializedSet, short versionNumber, FreeHierarchyTree tree)
        {
            _tree = tree;
            _versionNumber = versionNumber;

            Task.Factory.StartNew(() =>
            {
                if (!_treeItemsSyncLock.TryEnterWriteLock(TimeSpan.FromSeconds(2))) return;

                try
                {
                    switch (versionNumber)
                    {
                        case 0:
                        case 1:
                            InitV1(serializedSet);
                            break;
                        case 2:
                            InitV2(serializedSet);
                            break;
                    }
                }
                finally
                {
                    _treeItemsSyncLock.ExitWriteLock();
                }
            });
        }

        #region Выборка десериализованного списка

        public void SelectUnselect(ICollection<FreeHierarchyTreeItem> items)
        {
            if (items == null || !items.Any()) return;

            if (!_treeItemsSyncLock.TryEnterReadLock(TimeSpan.FromMinutes(2))) return;
            try
            {
                switch (_versionNumber)
                {
                    case 0:
                    case 1:
                        //SelectUnselectV1(items);
                        break;
                    case 2:
                        Task.Factory.StartNew(() => SelectUnselectV2(items));
                        
                        break;
                }
            }
            finally
            {
                _treeItemsSyncLock.ExitReadLock();
            }
        }

        private void SelectUnselectV2(ICollection<FreeHierarchyTreeItem> items)
        {
            if (ObjectsFromSQL == null || ObjectsFromSQL.Count == 0) return;

            var descriptor = _tree.GetDescriptor();
            if (descriptor == null) return;

            descriptor.SelectFromSets(items, ObjectsFromSQL, true);
        }

        #endregion

        #region Десереализация сохраненного объекта

        private void InitV1(object serializedSet)
        {
            ObjectsFromLocal = new List<FreeItemSelected>();

            var saved = serializedSet as IEnumerable<string>;
            if (saved != null)
            {
                try
                {
                    foreach (var item in saved)
                    {
                        if (_versionNumber == 1)
                        {
                            ObjectsFromLocal.Add(ProtoHelper.ProtoDeserializeFromString<FreeItemSelected>(item));
                        }
                        else
                        {
                            ObjectsFromLocal.Add(CommonEx.DeserializeFromString<FreeItemSelected>(item));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ShowMessage();
                }
            }
        }

        private void InitV2(object serializedSet)
        {
#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            FreeHierarchySelectedInfo selectedInfo = null;

            try
            {
                selectedInfo = ProtoHelper.ProtoDeserializeFromString<FreeHierarchySelectedInfo>(serializedSet as string);
            }
            catch (Exception ex)
            {
                ex.ShowMessage();
            }

            if (selectedInfo == null || selectedInfo.Items == null || !selectedInfo.Items.Any()) return;

#if DEBUG
            sw.Stop();
            Console.WriteLine("InitV2, ProtoDeserializeFromString {0} млс", sw.ElapsedMilliseconds);
            sw.Restart();
#endif

            //Пока просто выделяем выбранные
            ObjectsFromSQL = XamTreeFinder.BuildPathFromSQL(selectedInfo
                .Items
                .Take(limit)
                .Where(s => s.Id != null && s.IsSelect.GetValueOrDefault())
                .Select(s => s.Id).ToList(), _tree.Tree_ID);

#if DEBUG
            sw.Stop();
            Console.WriteLine("InitV2, BuildPathFromSQL {0} млс", sw.ElapsedMilliseconds);
#endif


        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
            // подавляем финализацию
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_treeItemsSyncLock.TryEnterWriteLock(TimeSpan.FromMinutes(2))) return;

            try
            {
                ObjectsFromLocal = null;
                ObjectsFromSQL = null;
            }
            finally
            {
                _treeItemsSyncLock.ExitWriteLock();
            }

            _tree = null;
        }

        // Деструктор
        ~TreeStartObjectSelector()
        {
            Dispose(false);
        }
    }
}
