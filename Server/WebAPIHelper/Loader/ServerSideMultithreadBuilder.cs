using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Public.Common;
using Proryv.AskueARM2.Server.DBAccess.Public.GroupTP;
using Proryv.AskueARM2.Server.WCF.AutoMapper;

namespace Proryv.AskueARM2.Server.WCF.GroupTp
{
    public class ServerSideMultithreadBuilder: IDisposable 
    {
        const int packetSize = 500000; //TODO проверить на веб версии!!
        private readonly CancellationToken _cancellationToken;

        public readonly Guid LoaderId;
        public ConcurrentStack<MemoryStream> CompressedProtoResult;

        private Action _finishAction;
        private Action<string> _onCriticalError;

        public ServerSideMultithreadBuilder(Guid loaderId, Action finishAction
            , Action<string> onCriticalError, CancellationToken cancellationToken
            , Func<object> buildAction)
        {
            _finishAction = finishAction;
            _onCriticalError = onCriticalError;
            LoaderId = loaderId;
            _cancellationToken = cancellationToken;
            CompressedProtoResult = new ConcurrentStack<MemoryStream>();

            AutomapBootstrap.InitializeMap();

            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var compressed = buildAction() as MemoryStream;
                        if (compressed == null || !compressed.CanRead) return;

                        var lenght = compressed.Length;

                        //Делим готовый отчет на равные партии, ждем запросов партий от клиента
                        foreach (var range in Partitioner.Create(0, lenght, packetSize).GetDynamicPartitions())
                        {
                            var bufferResult = new byte[range.Item2 - range.Item1];
                            compressed.Read(bufferResult, 0, (int)(range.Item2 - range.Item1));

                            CompressedProtoResult.Push(new MemoryStream(bufferResult));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_onCriticalError != null) _onCriticalError(ex.Message);
                    }
                    finally
                    {
                        try
                        {
                            if (_finishAction != null)
                            {
                                _finishAction();
                            }
                        }
                        catch
                        {
                        }

                        _finishAction = null;
                    }
                },
                _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Dispose()
        {
            _finishAction = null;
            _onCriticalError = null;
        }
    }
}
