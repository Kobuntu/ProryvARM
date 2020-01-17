using System;
using System.Activities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;

namespace Proryv.Workflow.Activity.ARM
{
    public class GetArchiveIntegralValueEx:GetArchiveIntegralValue
    {
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Только авточтение")]
        [RequiredArgument]
        public bool OnlyAutoRead { get; set; }

        public GetArchiveIntegralValueEx()
        {
            this.DisplayName = "Получить интегральное значение(Расширенное)";
        }
        protected override bool Execute(CodeActivityContext context)
        {
            int ti_id = TiId.Get(context);

            Value.Set(context, 0);
            Status.Set(context, (int)VALUES_FLAG_DB.DataNotFull);
            StatusStr.Set(context, TVALUES_DB.FLAG_to_String(VALUES_FLAG_DB.DataNotFull, ";"));
            ValueDateTime.Set(context, DateTime.MinValue);

            List<TINTEGRALVALUES_DB> _valueList = new List<TINTEGRALVALUES_DB>();
            ValueList.Set(context, _valueList);

            if (RequestType == enumEnrgyQualityRequestType.Archive)
                if (StartDateTime.Get(context) == null || EndDateTime.Get(context) == null)
                {
                    Error.Set(context, "Для архивных значений начальная и конечная дата должна быть определена");
                    return false;
                }

            try
            {
                var tiList = new List<TI_ChanelType>()
                                                 {
                                                     new TI_ChanelType()
                                                         {
                                                             TI_ID = ti_id,
                                                             ChannelType = Channel.Get(context),
                                                             DataSourceType = DataSourceType,
                                                         }
                                                 };

                if (RequestType == enumEnrgyQualityRequestType.Archive)
                {
                    //TODO часовой пояс
                    var values = ARM_Service.DS_GetIntegralsValues_List(tiList,
                        StartDateTime.Get(context),
                        EndDateTime.Get(context),
                        IsCoeffEnabled, false, false, OnlyAutoRead, false, false, enumTimeDiscreteType.DBHalfHours, EnumUnitDigit.None, null, false, false);


                    if (values != null && values.IntegralsValue30orHour != null && values.IntegralsValue30orHour.Count > 0)
                    {
                        var integralTiValue = values.IntegralsValue30orHour[0];
                        double diff = 0;
                        var flag = VALUES_FLAG_DB.None;
                        if (integralTiValue.Val_List != null)
                        {
                            foreach (var iVal in integralTiValue.Val_List)
                            {
                                diff += iVal.F_VALUE_DIFF;
                                flag = flag.CompareAndReturnMostBadStatus(iVal.F_FLAG);
                                _valueList.Add(iVal);
                            }
                        }

                        Value.Set(context, diff);
                        var stat = flag;
                        Status.Set(context, (int) stat);
                        StatusStr.Set(context, TVALUES_DB.FLAG_to_String(stat, ";"));

                        if (integralTiValue.Val_List != null && integralTiValue.Val_List.Count > 0)
                        {
                            ValueDateTime.Set(context, integralTiValue.Val_List.Last().EventDateTime);
                        }
                    }
                }
                else
                {
                    var val = ARM_Service.DS_ReadLastIntegralArchives(tiList);
                    if (val != null && val.Count > 0)
                    {
                        var valDict = val.ToDictionary(k => k.Key, v => v.Value, new TI_ChanelComparer());
                        foreach (var r in tiList)
                        {
                            TINTEGRALVALUES_DB i_val;

                            if (valDict.TryGetValue(r, out i_val) && i_val != null)
                            {
                                Value.Set(context, i_val.F_VALUE);
                                Status.Set(context, (int) i_val.F_FLAG);
                                StatusStr.Set(context, TVALUES_DB.FLAG_to_String(i_val.F_FLAG, ";"));
                                ValueDateTime.Set(context, i_val.EventDateTime);
                                _valueList.Add(i_val);
                                break;
                            }
                        }
                    }

                }

            }

            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            ValueList.Set(context, _valueList);

            return string.IsNullOrEmpty(Error.Get(context));
        }
    }
}
