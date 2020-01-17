using Proryv.AskueARM2.Server.DBAccess.Internal;
using Proryv.Servers.Calculation.DBAccess.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Server.DBAccess.Public.Calculation.ArchTech.Data
{
    public abstract class ArchTechRequesterBase
    {
        public StringBuilder Errors;
        public ArchTechRequestParams RequestParams;


        public ArchTechRequesterBase(ArchTechRequestParams requestParams)
        {
            RequestParams = requestParams;
            Errors = new StringBuilder();
            //ArchTechTiArchives = new List<ArchTechArchive>();
        }

        public abstract List<ArchTechArchive> InvokeReadArchive();

        public static ArchTechRequesterBase GetRequester(ArchTechRequestParams requestParams
            , enumTypeHierarchy typeHierarchy, IGrouping<enumTypeHierarchy, ArchTechRequestParam> requestParamByType)
        {
            switch (requestParamByType.Key)
            {
                //ТИ
                case enumTypeHierarchy.Info_TI:
                    return new ArchTechTiRequester(requestParams, requestParamByType);
                //Обычные формулы
                case enumTypeHierarchy.Formula:
                case enumTypeHierarchy.Formula_TP_OurSide:
                    return new ArchTechFormulaRequester(requestParams, requestParamByType);
                //ТП
                case enumTypeHierarchy.Info_TP:
                    return new ArchTechTpRequester(requestParams, requestParamByType);

                default:
                    return null;
            }
        }
    }
}
