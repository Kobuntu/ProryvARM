using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ProtoBuf;

namespace Proryv.ElectroARM.ODataGrid.Common
{
    public static class ProtoHelper
    {
        public static string ProtoSerializeToStr(this object obj)
        {
            using (MemoryStream msTestString = new MemoryStream())
            {
                Serializer.Serialize(msTestString, obj);

                return Convert.ToBase64String(msTestString.GetBuffer(), 0, (int)msTestString.Length);
            }
        }

        public static object ProtoDeserializeFromStr(this string serialized, Type type)
        {
            byte[] byteAfter64 = Convert.FromBase64String(serialized);
            using (var afterStream = new MemoryStream(byteAfter64))
            {
                return Serializer.Deserialize(type, afterStream);
            }
        }
    }
}
