using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Proryv.Workflow.Activity.Settings;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.Workflow.Activity.ARM
{

    public class GetPsStatisticInfo : BaseArmActivity<bool>
    {
        public GetPsStatisticInfo()
        {
            DisplayName = "Получить статистику для ПС";
            OffsetFromMoscow = 0;
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Идентификатор ПС")]
        public InArgument<int> PsId { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Конечная дата")]
        public InArgument<DateTime> EndDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Смещение относительно Москвы в минутах")]
        [DefaultValue(0)]
        public int OffsetFromMoscow { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Тип ТИ")]
        [DefaultValue(null)]
        public InArgument<enumTIType?> TIType { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Результат")]
        public OutArgument<TStatisticInformation> StatisticInfo { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            int pId = PsId.Get(context);

            try
            {
                //TODO часовой пояс
                var res = ARM_Service.Monit_GetStatisticInformationByPs(pId,
                    StartDateTime.Get(context),
                    EndDateTime.Get(context),
                    null, TIType.Get(context));

                StatisticInfo.Set(context, res);
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
