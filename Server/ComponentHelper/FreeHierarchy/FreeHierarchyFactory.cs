using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.Balanses;
using Proryv.AskueARM2.Server.DBAccess.Internal.DB;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Balances.Data;
using Proryv.AskueARM2.Server.DBAccess.Public.Common;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;
using Proryv.Servers.Calculation.DBAccess.Common.Ext;
using Proryv.Servers.Calculation.DBAccess.Interface.Documents;
using Proryv.Servers.Calculation.Parser.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Proryv.AskueARM2.Server.VisualCompHelpers.FreeHierarchy
{
    public static class FreeHierarchyFactory
    {
        public static BalanceFreeHierarchyResults BL_GetFreeHierarchyBalanceResult(List<string> balanceFreeHierarchyUNs, DateTime dTStart, DateTime dTEnd, string timeZoneId,
          TExportExcelAdapterType adapterType, bool isGenerateDoc, enumTimeDiscreteType discreteType,
          EnumUnitDigit unitDigit, bool isFormingSeparateList, EnumUnitDigit unitDigitIntegrals, bool isUseThousandKVt, bool printLandscape, byte doublePrecisionProfile, byte doublePrecisionIntegral,
          bool need0, bool isAnalisIntegral, bool setPercisionAsDisplayed, CancellationToken? cancellationToken = null)
        {
            var balance = new BalanceFreeHierarchyResults(balanceFreeHierarchyUNs, dTStart, dTEnd, timeZoneId, adapterType, isGenerateDoc,
                discreteType, unitDigit, unitDigitIntegrals, cancellationToken);

            if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested) return balance;

            if (isGenerateDoc)
            {
                string branchName;
                //Читаем подписантов
                var signaturesByBalance = GetBalanceSignatures(balanceFreeHierarchyUNs, out branchName);

                var po = new ParallelOptions();
                if (cancellationToken.HasValue)
                {
                    po.CancellationToken = cancellationToken.Value;
                }

                po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
                po.CancellationToken.ThrowIfCancellationRequested();

                using (var adapter = new ExcelReportFreeHierarchyAdapter(balance, signaturesByBalance, adapterType, balance.Errors,
                            isFormingSeparateList, isUseThousandKVt, printLandscape, branchName,
                            doublePrecisionProfile, doublePrecisionIntegral, need0, isAnalisIntegral, timeZoneId, setPercisionAsDisplayed))
                {

                    Parallel.ForEach(balanceFreeHierarchyUNs, po, (balanceUn, loopState) =>
                    {
                        BalanceFreeHierarchyCalculatedResult calculatedValue;
                        if (!balance.CalculatedValues.TryGetValue(balanceUn, out calculatedValue)) return;

                        //Формирование документов по каждому балансу
                        try
                        {
                            if (po.CancellationToken.IsCancellationRequested) loopState.Break();

                            calculatedValue.CompressedDoc = CompressUtility.CompressGZip(adapter.BuildBalanceFreeHier(calculatedValue));
                        }
                        catch (Exception ex)
                        {
                            lock (balance.Errors)
                            {
                                balance.Errors.Append("Ошибка генерации документа - " + ex.Message);
                            }
                        }
                    });
                }
            }

            return balance;
        }

        private static Dictionary<string, List<IFreeHierarchyBalanceSignature>> GetBalanceSignatures(List<string> balanceFreeHierarchyUNs, out string branchName)
        {
            var signatures = new Dictionary<string, List<IFreeHierarchyBalanceSignature>>();

            using (var db = new FSKDataContext(Settings.DbConnectionString)
            {
                ObjectTrackingEnabled = false,
                DeferredLoadingEnabled = false,
                CommandTimeout = 60,
            })
            {
                using (var txn = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
                {
                    var ore = db.Dict_SubjOREs.FirstOrDefault();
                    branchName = ore != null ? ore.SubjOREName : string.Empty;

                    foreach (var range in Partitioner.Create(0, balanceFreeHierarchyUNs.Count, Settings.MaxStringRows).GetDynamicPartitions())
                    {
                        var uns = "(" + string.Join(",", balanceFreeHierarchyUNs.GetRange(range.Item1, range.Item2 - range.Item1).Select(un => "'" + un + "'")) + ")";

                        foreach (var signature in db.ExecuteQuery<FreeHierarchyBalanceSignatureInfo>
                            ("select s.BalanceFreeHierarchySignature_UN as BalanceFreeHierarchySignatureUn,PostName, FIO,"
                             + "s.BalanceFreeHierarchySignatureGroup_UN as BalanceFreeHierarchySignatureGroupUn,"
                             + "g.SortNumber as GroupSortNumber, g.Name as GroupName, l.BalanceFreeHierarchy_UN as BalanceFreeHierarchyUn "
                             + "from [dbo].[Info_Balance_FreeHierarchy_List] l "
                             + "cross apply "
                             + "("
                             + "	select distinct BalanceFreeHierarchySignature_UN from "
                             + "	[dbo].[Info_Balance_FreeHierarchy_SignaturesLinks] "
                             + "	where (BalanceFreeHierarchy_UN is not null and BalanceFreeHierarchy_UN = l.BalanceFreeHierarchy_UN) "
                             + "	or (BalanceFreeHierarchyObject_UN is not null and BalanceFreeHierarchyObject_UN = l.BalanceFreeHierarchyObject_UN) "
                             + ") ss "
                             + "join [dbo].[Info_Balance_FreeHierarchy_Signatures] s on s.BalanceFreeHierarchySignature_UN = ss.BalanceFreeHierarchySignature_UN "
                             + "join [dbo].[Info_Balance_FreeHierarchy_SignaturesGroups] g on g.BalanceFreeHierarchySignatureGroup_UN = s.BalanceFreeHierarchySignatureGroup_UN "
                             + "where l.BalanceFreeHierarchy_UN in " + uns
                             + "order by g.SortNumber"))
                        {
                            List<IFreeHierarchyBalanceSignature> sl;
                            if (!signatures.TryGetValue(signature.BalanceFreeHierarchyUn, out sl))
                            {
                                sl = new List<IFreeHierarchyBalanceSignature>();
                                signatures[signature.BalanceFreeHierarchyUn] = sl;
                            }

                            sl.Add(signature);
                        }
                    }
                }
            }

            return signatures;
        }

        /// <summary>
        /// Читаем заголовок баланса
        /// </summary>
        /// <param name="balanceFreeHierarchyUn">Идентификатор баланса</param>
        /// <returns></returns>
        public static Tuple<Guid, MemoryStream, bool> BL_GetBalanceHeader(string balanceFreeHierarchyUn)
        {
            if (string.IsNullOrEmpty(balanceFreeHierarchyUn)) throw new Exception("Неправильный идентификатор баланса");

            var isUseAsTemplate = false;

            using (var db = new FSKDataContext(Settings.DbConnectionString)
            {
                CommandTimeout = 60,
            })
            {
                var bl = db.Info_Balance_FreeHierarchy_Lists.FirstOrDefault(b => Equals(balanceFreeHierarchyUn, b.BalanceFreeHierarchy_UN));
                if (bl == null) throw new Exception("Баланс не найден или удален");

                var balanceFreeHierarchyHeaderUN = bl.BalanceFreeHierarchyHeader_UN;
                if (!balanceFreeHierarchyHeaderUN.HasValue)
                {
                    //Не наден индивидуальный заголовок для данного баланса, читаем обобщенный
                    var hbl = db.Info_Balance_FreeHierarchy_HeadersToBalanceTypes.FirstOrDefault(b => b.BalanceFreeHierarchyType_ID == bl.BalanceFreeHierarchyType_ID);
                    if (hbl == null) return null;//throw new Exception("Не описан шаблона заголовка для данного типа баланса");

                    balanceFreeHierarchyHeaderUN = hbl.BalanceFreeHierarchyHeader_UN;
                    isUseAsTemplate = true;
                }

                var hl = db.Info_Balance_FreeHierarchy_Headers.FirstOrDefault(b => Equals(b.BalanceFreeHierarchyHeader_UN, balanceFreeHierarchyHeaderUN));
                if (hl == null) return null;

                return new Tuple<Guid, MemoryStream, bool>(hl.BalanceFreeHierarchyHeader_UN, new MemoryStream(hl.FsData.ToArray()), isUseAsTemplate);
            }
        }
    }
}
