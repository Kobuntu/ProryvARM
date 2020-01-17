using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Proryv.AskueARM2.Server.WCF.Loader
{
    [DataContract]
    public class BuilderPartResult
    {
        /// <summary>
        /// Это последний пакет, больше не будет
        /// </summary>
        [DataMember]
        public bool IsLastPacket;
        /// <summary>
        /// Номер пакета
        /// </summary>
        [DataMember]
        public int RequestNumber;
        /// <summary>
        /// Пакет
        /// </summary>
        [DataMember]
        public MemoryStream Part;
        /// <summary>
        /// Ошибки
        /// </summary>
        [DataMember]
        public string Errors;
        /// <summary>
        /// Общее количество оставшихся пакетов
        /// </summary>
        [DataMember]
        public int PacketRemaining;

        public BuilderPartResult(bool isLastPacket, int requestNumber, MemoryStream part, string errors,
            int packetRemaining)
        {
            IsLastPacket = isLastPacket;
            RequestNumber = requestNumber;
            Part = part;
            Errors = errors;
            PacketRemaining = packetRemaining;
        }
    }
}
