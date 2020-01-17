using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.AskueARM2.Server.DBAccess.Internal.Enums;
using Proryv.AskueARM2.Server.DBAccess.Internal.TClasses;
using Proryv.AskueARM2.Server.DBAccess.Public.Calculation.ArchTech.Data;
using Proryv.AskueARM2.Server.DBAccess.Public.Utils;
using Proryv.AskueARM2.Server.WCF;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.ArchTech
{
    public class ArchTechFormulaRequester : ArchTechRequesterBase
    {
        public List<TFormulaParam> FormulaIds;

        public ArchTechFormulaRequester(ArchTechRequestParams requestParams, IGrouping<enumTypeHierarchy, ArchTechRequestParam> objectIds):base (requestParams)
        {
            RequestParams = requestParams;
            Errors = new StringBuilder();
            FormulaIds = new List<TFormulaParam>();
            var formulasOurSideIds = new HashSet<string>();

            try
            {
                foreach (var rp in objectIds)
                {
                    if (string.IsNullOrEmpty(rp.ID.StringId)) continue;

                    if (rp.ID.TypeHierarchy == enumTypeHierarchy.Formula_TP_OurSide)
                    {
                        //Это формулы ТП, нужно добрать параметры
                        formulasOurSideIds.Add(rp.ID.StringId);
                    }
                    else
                    {
                        FormulaIds.Add(new TFormulaParam
                        {
                            FormulaID = rp.ID.StringId,
                            FormulasTable = enumFormulasTable.Info_Formula_Description,
                        });
                    }
                }
            }
            catch(Exception ex)
            {
                Errors.AppendException(ex);
            }

            var errors = new StringBuilder();
            var formulasTp = TFormulaParam.GetFormulaParamForTPs(new HashSet<TP_ChanelType>(), new List<enumFormulaTPType>(), RequestParams.DtStart, RequestParams.DtEnd, errors,
                         formulaIds: formulasOurSideIds);

            if (errors.Length > 0)
            {
                Errors.Append(errors);
            }

            FormulaIds.AddRange(formulasTp);
        }

        public override List<ArchTechArchive> InvokeReadArchive()
        {
            if (!RequestParams.TechProfilePeriod.HasValue)
            {
                //Формулы пока не получается набирать из разных минуток
                RequestParams.TechProfilePeriod = EnumTechProfilePeriod.Трехминутно;
            }

            var fVals = new FormulasResult(FormulaIds, RequestParams.DtStart, RequestParams.DtEnd, enumTimeDiscreteType.DBHalfHours
                , EnumDataSourceType.ByPriority
                , 0, RequestParams.UseCoeffTransformation, false, enumTypeInformation.Energy, false, RequestParams.UnitDigit, false
                , enumOVMode.NormalMode, RequestParams.TimeZoneId,
                false, true, techProfilePeriod: RequestParams.TechProfilePeriod);

            if (fVals == null) return null;

            if (fVals.Errors!=null && fVals.Errors.Length > 0)
            {
                Errors.Append(fVals.Errors);
            }

            var firstDateTimeUTC = RequestParams.DtStart.ClientToUtc(RequestParams.TimeZoneId);

            var result = new List<ArchTechArchive>();

            foreach (var fVal in fVals.Result_Values)
            {
                if (fVal.Result_Values == null || fVal.Result_Values.Count == 0) continue;

                var ff = fVal.Result_Values.First();
                if (ff.Val_List == null || ff.Val_List.Count == 0) continue;

                var archTechValues = ff.Val_List
                    .Select((v, i) => new { v, i })
                    .ToDictionary(k => k.i, v => v.v);

                result.Add(new ArchTechArchive(new ID_TypeHierarchy
                {
                    StringId = fVal.Formula_UN,
                    TypeHierarchy = fVal.FormulasTable.ToTypeHierarchy(),
                }, 0, RequestParams.TechProfilePeriod.Value, firstDateTimeUTC, archTechValues));
            }


            return result;
        }
    }
}
