using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Footers
{
    public class BalanceFreeHierarchyFooterFactory
    {



        #region Public

        /// <summary>
        /// Читаем все итоги для баланса по подгруппам
        /// </summary>
        /// <param name="balanceFreeHierarchyUns">Идентификаторы балансов, для которых возвращаем итоги по своим подгруппам</param>
        /// <returns></returns>
        public static ConcurrentDictionary<string, Dictionary<string, List<Dict_Balance_FreeHierarchy_Footers>>> GetFreeHierarchyBalanceFootersByBalances(List<string> balanceFreeHierarchyUns)
        {
            if (balanceFreeHierarchyUns == null) return null;

            var result = new ConcurrentDictionary<string, Dictionary<string, List<Dict_Balance_FreeHierarchy_Footers>>>();

            Parallel.ForEach(Partitioner.Create(0, balanceFreeHierarchyUns.Count, Settings.MaxStringRows)
                    , range =>
                    {
                        using (var db = new FSKDataContext(Settings.DbConnectionString)
                        {
                            ObjectTrackingEnabled = false,
                            DeferredLoadingEnabled = false,
                            CommandTimeout = 60,
                        })
                        {
                            foreach (var footersByBalance in db.Dict_Balance_FreeHierarchy_Footers
                                .Where(f => balanceFreeHierarchyUns.Contains(f.BalanceFreeHierarchy_UN))
                                .ToList()
                                .GroupBy(f => f.BalanceFreeHierarchy_UN)) //Группируем по балансам
                            {
                                result[footersByBalance.Key] = footersByBalance.GroupBy(f => f.BalanceFreeHierarchySection_UN) //Группируем по подгруппам
                                  .ToDictionary(f => f.Key, vv => vv.OrderBy(f => f.SortNumber).ToList());
                            }
                        }
                    });

            return result;
        }

        #endregion
    }
}
