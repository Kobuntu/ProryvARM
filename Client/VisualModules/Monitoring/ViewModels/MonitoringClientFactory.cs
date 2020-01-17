using System.IO;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.UAService;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Proryv.ElectroARM.Controls.Monitoring.Data;
using ProtoBuf;

namespace Proryv.ElectroARM.Monit.DataMonit.Data
{
    public class MonitoringClientFactory
    {
        public readonly DateTime DtStart;
        public readonly DateTime DtEnd;
        readonly double? _maxValue;
        public readonly bool IsConcentratorsEnabled;
        public readonly bool IsMetersEnabled;
        readonly List<int> _selectedEvents;
        readonly List<byte> _selectedСhannels;
        readonly VALUES_FLAG_DB? _filterFlag;
        readonly CancellationToken _cancellationToken;
        public readonly Dictionary<MonitoringObjectKey, TMonitoringAnalyseRow> MonitoringAnalyseDict;
        private readonly Guid _waiterId;
        private readonly ProgressBarButton _progressBar;
        private readonly Action<TStatisticInformation> _showTotalStatistic;
        private TMonitoringAnalyseRowEqualityComparer _rowsComparer;

        public MonitoringClientFactory(DateTime dtStart,
            DateTime dtEnd,
            double? maxValue,
            bool isConcentratorsEnabled,
            bool isMetersEnabled,
            List<int> selectedEvents,
            List<byte> selectedСhannels,
            VALUES_FLAG_DB? filterFlag,
            CancellationToken cancellationToken,
            ProgressBarButton progressBar, Action<TStatisticInformation> showTotalStatistic, Guid waiterId,
            MonitoringDBSupport monitoringDBResult, ModuleType типМодуля)
        {
            DtStart = dtStart;
            DtEnd = dtEnd;
            _maxValue = maxValue;
            IsConcentratorsEnabled = isConcentratorsEnabled;
            IsMetersEnabled = isMetersEnabled;
            _selectedEvents = selectedEvents;
            _selectedСhannels = selectedСhannels;
            _filterFlag = filterFlag;
            _cancellationToken = cancellationToken;
            _rowsComparer = new TMonitoringAnalyseRowEqualityComparer();

            MonitoringAnalyseDict = MakeMonitoringSource(monitoringDBResult, типМодуля);
            _progressBar = progressBar;
            _showTotalStatistic = showTotalStatistic;
            _waiterId = waiterId;
            _progressBar.SetIndeterminat(true);
        }

        /// <summary>
        /// Отсортировываем все ПУ по своим родителям
        /// </summary>
        /// <param name="monitoringDBResult">Плоский результат запроса в БД</param>
        /// <param name="типМодуля">Тип модуля</param>
        /// <returns>Источник данных для формы анализа мониторинга</returns>
        private Dictionary<MonitoringObjectKey, TMonitoringAnalyseRow> MakeMonitoringSource(MonitoringDBSupport monitoringDBResult, ModuleType типМодуля)
        {
            var result = new HashSet<TMonitoringAnalyseRow>(_rowsComparer);
            //Наполняем УСПД, Е422 и 61968, концентраторы
            if (типМодуля == ModuleType.MonitoringAutoSbor)
            {
                //Добавляем УСПД
                if (monitoringDBResult.MonitoringAnalyseUSPD != null)
                {
                    result.UnionWith(monitoringDBResult.MonitoringAnalyseUSPD.ToRow(enumTypeParrentMonitoringHierarchy.USPD, null));
                }

                //Добавляем Е422
                if (monitoringDBResult.MonitoringAnalyseE422 != null)
                {
                    result.UnionWith(monitoringDBResult.MonitoringAnalyseE422.ToRow(enumTypeParrentMonitoringHierarchy.E422, monitoringDBResult.MonitoringAnalyseConcentrator));
                }
            }
            else
            {
                if (monitoringDBResult.MonitoringAnalyse61968 != null)
                {
                    result.UnionWith(monitoringDBResult.MonitoringAnalyse61968.ToRow());
                }
            }

            return result.ToDictionary(k => k.ID, v => v, new MonitoringObjectKeyEqualityComparer());
        }

        public Task RunServerBuild(BackgroundWorker worker)
        {
            var parents = MonitoringAnalyseDict.Keys.ToList();

            try
            {
                //Запрос формирования данных
                ARM_Service_.Monit_GetArchive(_waiterId, parents, _selectedСhannels
                    , DtStart, DtEnd, _maxValue, _selectedEvents, IsConcentratorsEnabled, _filterFlag);
                //Ждем формирования первых пакетов
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage(ex.Message);
                return null;
            }
            

            //if (_showTotalStatistic != null)
            //{
            //    _showTotalStatistic(request.StatisticInformation);
            //}

            _isRequestCompleted = false;
            //double packetSize = request.ProgressPercent;

            //if (request.Result != null && request.Result.Count > 0)
            //{
            //    _stackForBuildResult.PushRange(request.Result.ToArray());
            //    _progressBar.IncValue(packetSize);
            //}

            //Отмена выполнения
            if (worker.CancellationPending || _cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            //Запускаем построитель результатов
            var builderTask = Task.Factory.StartNew(BuildPacketResult, _cancellationToken);

            //Получение следующих пакетов
            bool isLastPacket = false;
            int voidCounter = 0;
            int errCounter = 0;
            bool isFirst = true;
            TStatisticInformation statisticInformation = null;

            int requestNumber = 0;
            do
            {
                try
                {
                    if (_cancellationToken.IsCancellationRequested) break;

                    var packet = ServiceFactory.ArmServiceInvokeSync<Tuple<bool, MemoryStream, string, TStatisticInformation, TMonitoringAnalyseResult>>("Monit_WaitArchives", requestNumber, _waiterId);
                    if (packet != null)
                    {
                        if (!string.IsNullOrEmpty(packet.Item3))
                        {
                            Manager.UI.ShowMessage(packet.Item3);
                        }

                        isLastPacket = packet.Item1;
                        if (isLastPacket)
                        {
                            statisticInformation = packet.Item4;
                            break;
                        }

                        if (packet.Item2 != null)
                        {
                            requestNumber++;
                            //var nextPart = packet.Item2.DecompressAndDeserialize<List<TMonitoringAnalyseResult>>();
                            var decomoressed = CompressUtility.DecompressGZip(packet.Item2);
                            decomoressed.Position = 0;
                            var nextPart = Serializer.Deserialize<List<TMonitoringAnalyseResult>>(decomoressed);
                            if (nextPart != null && nextPart.Count > 0)
                            {
                                _stackForBuildResult.PushRange(nextPart.ToArray());
                                voidCounter = 0;
                                _mres.Set();
                                if (isFirst)
                                {
                                    _progressBar.SetIndeterminat(false);
                                    isFirst = false;
                                }
                            }
                            else
                            {
                                Thread.Sleep(2000);
                            }
                        }
                        else
                        {
                            if (++voidCounter > 501)
                            {
                                isLastPacket = true;
                                Manager.UI.ShowMessage("MonitoringClientFactory: Превышен лимит ожидания пакетов");
                            }
                            else
                            {
                                Thread.Sleep(5000);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }

                    errCounter = 0;
                }
                catch (Exception ex)
                {
                    if (++errCounter > 6)
                    {
                        isLastPacket = true;
                        Manager.UI.ShowMessage("MonitoringClientFactory: Превышен лимит пустых пакетов");
                        _progressBar.SetIndeterminat(false);
                    }
                    else
                    {
                        Manager.UI.ShowMessage(ex.Message);
                        Thread.Sleep(1000);
                    }
                }
                Thread.Sleep(200);
            } while (!isLastPacket);

            if (statisticInformation != null && _showTotalStatistic != null)
            {
                _showTotalStatistic(statisticInformation);
            }

            _isRequestCompleted = true;
            _mres.Set();
            return builderTask;
        }

        private volatile bool _isRequestCompleted;
        private readonly ConcurrentStack<TMonitoringAnalyseResult> _stackForBuildResult = new ConcurrentStack<TMonitoringAnalyseResult>();
        private readonly ManualResetEventSlim _mres = new ManualResetEventSlim();

        /// <summary>
        /// Разгребатель результата
        /// </summary>
        private void BuildPacketResult()
        {
            //Ждем первого получения пакетов
            _mres.Wait(10000);
            const int maxCount = 50;
            do
            {
                if (_mres.IsSet)
                {
                    _mres.Reset();
                }

                if (!_stackForBuildResult.IsEmpty)
                {
                    
                    var packet = new TMonitoringAnalyseResult[maxCount];
                    var recivedCount = _stackForBuildResult.TryPopRange(packet, 0, maxCount);
                    if (recivedCount == 0) continue;
                    for (int i = 0; i < recivedCount; i++)
                    {
                        StartBuildVisual(packet[i]);
                    }

                    _progressBar.IncValue(recivedCount);
                }
                else
                {
                    _mres.Wait(10000);
                }
                
            } while (!_isRequestCompleted || !_stackForBuildResult.IsEmpty);
        }

        private void StartBuildVisual(TMonitoringAnalyseResult analyseResult)
        {
            TMonitoringAnalyseRow row;
            if (analyseResult == null || !MonitoringAnalyseDict.TryGetValue(analyseResult.ParentId, out row) || row == null) return;
            var localSumm = new TStatisticInformation //Инициализация локальной статистики
            {
                DailyInformation = new Dictionary<DateTime, TStatisticInformation>(),
            };


            row.Status = OperationStatus.Processing;
            bool? isError = null; //Для отслеживания ошибки в процессе выполнения

            //Запрос в БД
            row.UpdateMeters(IsMetersEnabled, ref isError,
                localSumm, analyseResult);

            //Если это Е422 то обновляем концентраторы
            if (analyseResult.Children != null && analyseResult.Children.Count > 0 && row.ChildrenConcentrator.Count > 0)
            {
                foreach (var concentrator in row.ChildrenConcentrator)
                {
                    var concentratorResult =
                        analyseResult.Children.FirstOrDefault(a => a.ParentId.MonitoringHierarchy == concentrator.ID.MonitoringHierarchy && a.ParentId.Id == concentrator.ID.Id);

                    if (concentratorResult != null)
                    {
                        concentrator.UpdateMeters(IsMetersEnabled,
                            ref isError, localSumm, concentratorResult);

                        concentrator.Status = isError.GetValueOrDefault()
                            ? OperationStatus.Error
                            : OperationStatus.Done;
                    }
                }
            }
            row.MicrochartSource = localSumm.DailyInformation;
            row.Status = isError.GetValueOrDefault() ? OperationStatus.Error : OperationStatus.Done;
        }
    }
}
