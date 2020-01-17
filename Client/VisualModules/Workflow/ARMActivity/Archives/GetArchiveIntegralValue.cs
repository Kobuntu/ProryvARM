using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.Workflow.Activity.Settings;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using System.Collections.Concurrent;

namespace Proryv.Workflow.Activity.ARM
{
    public class GetArchiveIntegralValue : BaseArmActivity<bool>
    {
        public GetArchiveIntegralValue()
        {
            this.DisplayName = "Получить интегральное значение";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор ТИ")]
        [RequiredArgument]
        public InArgument<int> TiId { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Канал")]
        [RequiredArgument]
        public InArgument<byte> Channel { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Использовать коэффициенты")]
        [RequiredArgument]
        public bool IsCoeffEnabled { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Конечная дата")]
        public InArgument<DateTime> EndDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Источник")]
        public EnumDataSourceType? DataSourceType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Профиль")]
        [TypeConverter(typeof(EnumProfileTypeConverter))]
        public enumReturnProfile ProfileType { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Тип запрашиваемых значений")]
        [TypeConverter(typeof(EnumEnrgyQualityRequestTypeConverter))]
        public enumEnrgyQualityRequestType RequestType { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Временная зона")]
        [DefaultValue(4)]
        public int TimeZone { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Значение")]
        public OutArgument<double> Value { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Время последнего значения")]
        public OutArgument<DateTime> ValueDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Статус")]
        public OutArgument<int> Status { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Статус строка")]
        public OutArgument<string> StatusStr { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список значений")]
        public OutArgument<List<TINTEGRALVALUES_DB>> ValueList { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            int ti_id = TiId.Get(context);
            
            Value.Set(context,0);
            Status.Set(context,(int)VALUES_FLAG_DB.DataNotFull);
            StatusStr.Set(context,TVALUES_DB.FLAG_to_String(VALUES_FLAG_DB.DataNotFull, ";"));
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
                List<TI_ChanelType> tiList = new List<TI_ChanelType>()
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
                        IsCoeffEnabled, false, false, true, false, false, enumTimeDiscreteType.DBHalfHours, EnumUnitDigit.None, null, false,
                        false);


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
                    var Val = ARM_Service.DS_ReadLastIntegralArchives(tiList);
                    if (Val != null && Val.Count > 0)
                    {
                        var ValDict = Val.ToDictionary(k => k.Key, v => v.Value, new TI_ChanelComparer());
                        foreach (var r in tiList)
                        {
                            TINTEGRALVALUES_DB i_val;

                            if (ValDict.TryGetValue(r, out i_val) && i_val != null)
                            {
                                Value.Set(context, i_val.F_VALUE);
                                Status.Set(context, (int)i_val.F_FLAG);
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
