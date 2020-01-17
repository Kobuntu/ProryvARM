using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.Workflow.Activity.Settings;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.Workflow.Activity.ARM
{
    public class GetAllTIJournals : BaseArmActivity<bool>
    {
        public GetAllTIJournals()
        {
            this.DisplayName = "Получить журналы для ТИ";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Список идентификаторов ТИ")]
        public InArgument<List<int>> TIList { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Конечная дата")]
        public InArgument<DateTime> EndDateTime { get; set; }
                
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Журнал событий")]
        public OutArgument<List<EventsJournalTI>> EventsJournal { get; set; }
        
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {

            if (TIList.Get(context) == null)
            {
                Error.Set(context, "Не определен список идентификаторов ТИ");
                return false;
            }

            var inList = TIList.Get(context);
            if (inList.Count == 0)
            {
                Error.Set(context, "Список идентификаторов ТИ не должен быть пустым");
                return false;
            }

            var result = new List<EventsJournalTI>();
            try
            {
                var res = ARM_Service.JTI_GetAllTIJournals(inList, StartDateTime.Get(context),EndDateTime.Get(context));
                if (res != null)
                {
                    foreach (var jti in res)
                    {
                        result.Add(new EventsJournalTI
                        {
                            TI_ID = jti.TI_ID,
                            EventDateTime = jti.EventDateTime,
                            EventCode = jti.EventCode,
                            DispatchDateTime = jti.DispatchDateTime,
                            ExtendedEventCode = jti.ExtendedEventCode,
                            Event61968Domain_ID = jti.Event61968Domain_ID,
                            Event61968DomainPart_ID = jti.Event61968DomainPart_ID,
                            Event61968Type_ID = jti.Event61968Type_ID,
                            Event61968Index_ID = jti.Event61968Index_ID,
                            Event61968Param = jti.Event61968Param
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            EventsJournal.Set(context, result);
            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}
