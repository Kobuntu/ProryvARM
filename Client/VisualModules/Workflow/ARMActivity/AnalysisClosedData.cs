using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;

namespace Proryv.Workflow.Activity.ARM
{
    public class AnalysisClosedData : BaseArmActivity<bool>
    {

        public AnalysisClosedData()
        {
            this.DisplayName = "Анализ закрытых данных";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор сечения")]
        [RequiredArgument]
        public InArgument<Int32> ID { get; set; }

        //[Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        //[DisplayName("Идентификатор закрытого периода")]
        //public InArgument<Guid?> ClosedPeriodID { get; set; }

        [RequiredArgument]
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Источник")]
        public EnumDataSourceType? DataSourceType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Расчетные данные")]
        public bool IsReadCalculatedValues { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Смещение времени в БД")]
        public int OffsetFromDataBase { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Результат анализа")]
        public OutArgument<List<TAnalysClosedDataRow>> AnalysResult { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }


        protected override bool Execute(CodeActivityContext context)
        {

            List<ID_TypeHierarchy> idList = new List<ID_TypeHierarchy>();
            ID_TypeHierarchy idTypeHier = new ID_TypeHierarchy();
            idTypeHier.ID = ID.Get(context);
            idTypeHier.ClosedPeriod_ID = null;// ClosedPeriodID.Get(context);
            idTypeHier.TypeHierarchy = enumTypeHierarchy.Section;
            idList.Add(idTypeHier);

            try
            {
                //TODO часовой пояс
                object[] args =
                {
                    IsReadCalculatedValues,
                    StartDateTime.Get(context),
                    idList,
                    DataSourceType,
                    null
                };

                TAnalysClosedData res = ARM_Service.ClosedPeriod_AnalysClosedData(args);
                AnalysResult.Set(context, res.AnalysClosedDataRows);
            }

            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}
