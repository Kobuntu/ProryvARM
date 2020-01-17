using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.ServiceReference.Service.Dictionary;
using Proryv.AskueARM2.Client.ServiceReference.UAService;
using Proryv.AskueARM2.Proto;

namespace Proryv.AskueARM2.Client.ServiceReference.GroupTpFactory
{
    public static class ClientSideMultithreadBuilder
    {
        public static R BuildandWaitResult<R, P>(P groupTpWaiterParams, string methodName
            , Action<string> onError, Action<double> incProgress = null
            , CancellationToken? cancellationToken = null, Action<bool> setIndeterminat = null
            )
            where R : class //Тип для результата
            where P : class //Тип для параметров
        {
            ProtoInitializer.InitProtobuf();

            int voidPacketLimit;

            var setting = EnumClientServiceDictionary.GetGlobalSettingsByName(RegistrySettings.FolderVisual, RegistrySettings.SettingVoidPacketLimit);
            if (setting == null || setting.Setting == null || !int.TryParse(setting.Setting, out voidPacketLimit) ||
                voidPacketLimit < 10) //Максимальное количество не ограничиваем
            {
                voidPacketLimit = 24001;
            }
            
            Guid loaderId;
            try
            {
                MemoryStream compressedParam;
                using (var ms = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(ms, groupTpWaiterParams);
                    ms.Position = 0;
                    compressedParam = CompressUtility.CompressGZip(ms);
                }

                compressedParam.Position = 0;
                loaderId = ARM_Service.Builder_StartWait(compressedParam, methodName);
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                if (onError != null) onError( "Ошибка запуска построителя фак. мощности: " + ex.Message);
                return null;
            }

            var isLastPacket = false;
            var voidCounter = 0;
            var errCounter = 0;
            var requestNumber = 0;
            var ca = new List<byte>();

            var packetSize = 0.0; //Тут надо подумать как возвращать правильный размер, возможно никак

            do
            {
                if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                {
                    ARM_Service.Builder_Cancel(loaderId);
                    break; //Отмена выполнения
                }

                try
                {
                    var packet = ServiceFactory.ArmServiceInvokeSync<BuilderPartResult>("Builder_GetNextPart", requestNumber, loaderId);
                    if (packet != null)
                    {
                        if (!string.IsNullOrEmpty(packet.Errors))
                        {
                            //Это критическая ошибка, продолжать нельзя
                            if (onError != null) onError(packet.Errors);
                            break;
                        }

                        isLastPacket = packet.IsLastPacket;
                        if (isLastPacket)
                        {
                            break;
                        }

                        if (packet.Part != null && packet.Part.Length > 0)
                        {
                            //requestNumber++;
                            if (requestNumber == 0)
                            {
                                if (packet.PacketRemaining == 0) packetSize = 1;
                                else packetSize = 100 / (double)packet.PacketRemaining;

                                if (setIndeterminat != null) setIndeterminat(false);
                            }

                            ca.InsertRange(0, packet.Part.ToArray());
                            requestNumber++;

                            packet.Part.Close();
                            packet.Part.Dispose();

                            voidCounter = 0;

                            Thread.Sleep(10);
                        }
                        else
                        {
                            if (++voidCounter > voidPacketLimit)
                            {
                                isLastPacket = true;
                                if (onError != null) onError("ClientSideMultithreadBuilder: Превышен лимит ожидания пакетов");

                            }
                            else
                            {
                                Thread.Sleep(300);
                            }
                        }

                        errCounter = 0;
                    }
                    else
                    {
                        if (++errCounter > 50)
                        {
                            isLastPacket = true;
                            if (onError != null) onError("ClientSideMultithreadBuilder: Превышен лимит пустых пакетов");
                        }

                        Thread.Sleep(5000);
                    }
                }
                catch (Exception ex)
                {
                    if (++errCounter > 6)
                    {
                        isLastPacket = true;
                        if (onError != null) onError("ClientSideMultithreadBuilder: Сервер не отвечает");
                    }
                    else
                    {
                        if (onError != null) onError(ex.Message);
                        Thread.Sleep(5000);
                    }
                }

                if (incProgress != null) incProgress(packetSize);
            } while (!isLastPacket);

            var compressed = new MemoryStream(ca.ToArray());
            ca = null;

            R result = null;
            if (compressed.Length > 2)
            {
                try
                {

                    using (var ms = CompressUtility.DecompressGZip(compressed))
                    {
                        compressed.Close();
                        compressed.Dispose();

                        ms.Position = 0;
                        result = ProtoBuf.Serializer.Deserialize<R>(ms);
                    }
                }
                catch (Exception ex)
                {
                    if (onError != null) onError("ClientSideMultithreadBuilder: Ошибка обработки результата: " + ex.Message);
                }
            }

            GC.Collect();

            return result;
        }
    }
}
