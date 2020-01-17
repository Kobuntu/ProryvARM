using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;
using System.ComponentModel;
using System.Activities;
using System.IO;
using Proryv.AskueARM2.Both.VisualCompHelpers;

namespace Proryv.Workflow.Activity.ARM
{
    public class GetArchiveValues : BaseArmActivity<bool>
    {
        public GetArchiveValues()
        {
            this.DisplayName = "Получить архивные значения";
            UnitDigit = EnumUnitDigit.None;
            Channel = enumChanelType.AI;
            this.ObjectType = enumArchiveObjectType.Info_TI;
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор объекта")]
        [RequiredArgument]
        public InArgument<object> ObjectID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Тип данных")]
        [RequiredArgument]
        [TypeConverter(typeof(EnumArchiveObjectTypeTypeConverter))]
        public enumArchiveObjectType ObjectType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Канал")]
        [RequiredArgument]
        [TypeConverter(typeof(enumenumChanelTypeConverter))]
        public enumChanelType Channel { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Использовать коэффициенты")]
        [RequiredArgument]
        public bool IsCoeffEnabled { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Конечная дата")]
        public InArgument<DateTime> EndDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Период дискретизации")]
        [TypeConverter(typeof(EnumDiscreteTypeConverter))]
        public enumTimeDiscreteType DiscreteType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Источник")]
        public EnumDataSourceType? DataSourceType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Профиль")]
        [TypeConverter(typeof(EnumProfileTypeConverter))]
        public enumReturnProfile ProfileType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Разрядность")]
        [TypeConverter(typeof(EnumUnitDigitTypeConverter))]
        public EnumUnitDigit UnitDigit { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Смещение относительно Москвы в минутах")]
        [DefaultValue(0)]
        public int OffsetFromMoscow { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список значений")]
        public OutArgument<List<TVALUES_DB>> ValueList { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Сумма значений")]
        public OutArgument<double> SummValues { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Статус суммы")]
        public OutArgument<int> Status { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Статус строка суммы")]
        public OutArgument<string> StatusStr { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }


        protected override bool Execute(CodeActivityContext context)
        {
            object obj_id = ObjectID.Get(context);
            if (obj_id == null)
            {
                Error.Set(context, "Не опреден идентификатор объекта");
                return false;
            }

            SummValues.Set(context, 0);
            Status.Set(context, (int)VALUES_FLAG_DB.DataNotFull);
            StatusStr.Set(context, TVALUES_DB.FLAG_to_String(VALUES_FLAG_DB.DataNotFull, ";"));

            var found = false;
            List<TVALUES_DB> vallist = null ;
            try
            {
                var tidList = new List<ID_Hierarchy_Channel>();
                var IH_Chanel = new ID_Hierarchy_Channel();
                IH_Chanel.ID = obj_id.ToString();
                IH_Chanel.TypeHierarchy = (enumTypeHierarchy)ObjectType;
                IH_Chanel.Channel = (byte)Channel;
                tidList.Add(IH_Chanel);

                //TODO часовой пояс
                var res = ARM_Service.DS_GetArchivesHierObjectLastHalfHours(
                    tidList,
                    IsCoeffEnabled,
                    StartDateTime.Get(context),
                    EndDateTime.Get(context),
                    DataSourceType,
                    DiscreteType, UnitDigit, false, null);


                if (res != null && res.Result_Values!=null && res.Result_Values.Count > 0)
                {
                    var resultValues = new Dictionary<ID_Hierarchy_Channel, List<TVALUES_DB>>(res.Result_Values, new ID_Hierarchy_Channel_EqualityComparer());

                    if (resultValues.TryGetValue(IH_Chanel, out vallist) && vallist != null)
                    {
                        found = true;
                        var R = vallist.Accomulate();
                        SummValues.Set(context, R.F_VALUE);
                        Status.Set(context, (int)R.F_FLAG);
                        StatusStr.Set(context, TVALUES_DB.FLAG_to_String(R.F_FLAG, ";"));
                    }
                }
            }

            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            if (vallist == null)
                vallist = new List<TVALUES_DB>();
            ValueList.Set(context, vallist);

            return string.IsNullOrEmpty(Error.Get(context));
        }
    }
}
