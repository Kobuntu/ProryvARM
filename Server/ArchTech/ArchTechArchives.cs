using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.ArchTech.Data;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.ArchTech
{
    [DataContract]
    public class ArchTechArchives
    {
        [DataMember]
        public ArchTechRequestParams RequestParams;

        [DataMember]
        public StringBuilder Errors;

        [DataMember]
        public List<ArchTechArchive> Values;

        public ArchTechArchives(ArchTechRequestParams requestParams)
        {
            if (requestParams == null || requestParams.ArchTechObjectIds == null || requestParams.ArchTechObjectIds.Count == 0) return;

            Values = new List<ArchTechArchive>();
            Errors = new StringBuilder();

            //Раскидываем по типам объектов
            foreach (var requestParamByType in requestParams.ArchTechObjectIds.GroupBy(g => g.ID.TypeHierarchy))
            {
                var requester = ArchTechRequesterBase.GetRequester(requestParams, requestParamByType.Key, requestParamByType);
                if (requester!=null)
                {
                    Values.AddRange(requester.InvokeReadArchive());
                }
            }
        }
    }
}
