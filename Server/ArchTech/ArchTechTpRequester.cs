using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.Enums;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Archives;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.Archives.TP;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.ArchTech.Data;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;
using Proryv.AskueARM2.Server.WCF;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using Proryv.Servers.Calculation.DBAccess.Interface.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.ArchTech
{
    public class ArchTechTpRequester : ArchTechRequesterBase
    {
        public List<ID_TypeHierarchy> Tps;
        public HashSet<byte> Channels;

        public ArchTechTpRequester(ArchTechRequestParams requestParams, IGrouping<enumTypeHierarchy, ArchTechRequestParam> objectIds) : base(requestParams)
        {
            Channels = new HashSet<byte>();
            Tps = objectIds
                .Select(id =>
                {
                    Channels.Add(id.ChannelType);
                    return new ID_TypeHierarchy(enumTypeHierarchy.Info_TP, id.ID.ID);
                })
                .ToList();
        }

        public override List<ArchTechArchive> InvokeReadArchive()
        {
            if (!RequestParams.TechProfilePeriod.HasValue)
            {
                //Формулы пока не получается набирать из разных минуток
                RequestParams.TechProfilePeriod = EnumTechProfilePeriod.Трехминутно;
            }

            var tpParams = ArchivesTpFactory.GetTpParams(Tps, enumBusRelation.PPI_OurSide, Errors, RequestParams.DtStart, RequestParams.DtEnd);

            var tpVals = new ArchivesTPValues(null, RequestParams.DtStart, RequestParams.DtEnd, enumBusRelation.PPI_OurSide, enumTimeDiscreteType.DBHalfHours,
                EnumDataSourceType.ByPriority, enumTypeInformation.Energy, RequestParams.UnitDigit, RequestParams.TimeZoneId, false, false, true, false,
                enumOVMode.NormalMode, enumOVMode.NormalMode, false, tpParams: tpParams, channelList: Channels.ToList(),
                isValuesAllTIEnabled: false, formulaTpType: enumClientFormulaTPType.Total, roundData: false,
                isReturnPreviousDispatchDateTime: false, useRoundedTi: false, sectionActVersion: enumSectionActVersion.NoCommentVersion,
                useActUndercount: true, cancellationToken: null, isNotRecalculateResiudes: false, techProfilePeriod: RequestParams.TechProfilePeriod);

            if (tpVals == null) return null;

            var firstDateTimeUTC = RequestParams.DtStart.ClientToUtc(RequestParams.TimeZoneId);

            var result = new List<ArchTechArchive>();

            foreach (var tpVal in tpVals.ArchivesValue30orHour)
            {
                List<TPointValue> values;

                if (tpVal.Val_List == null || tpVal.Val_List.Count == 0 || !tpVal.Val_List.TryGetValue(tpVal.IsMoneyOurSide, out values)) continue;

                var archTechValues = values
                    .Select((v, i) => new { v, i })
                    .ToDictionary(k => k.i, v => new TVALUES_DB
                    {
                        F_VALUE = v.v.F_VALUE,
                        F_FLAG = v.v.F_FLAG,
                    });

                result.Add(new ArchTechArchive(new ID_TypeHierarchy
                {
                    ID = tpVal.TP_Ch_ID.TP_ID,
                    TypeHierarchy = enumTypeHierarchy.Info_TP,
                }, tpVal.TP_Ch_ID.ChannelType, RequestParams.TechProfilePeriod.Value, firstDateTimeUTC, archTechValues));
            }


            return result;
        }
    }
}
