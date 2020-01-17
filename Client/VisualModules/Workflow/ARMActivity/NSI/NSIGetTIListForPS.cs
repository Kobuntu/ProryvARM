using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;

namespace Proryv.Workflow.Activity.ARM
{

    public class NSIGetTIListForPS : BaseArmActivity<bool>
    {

        public NSIGetTIListForPS()
        {
            this.DisplayName = "Получить список ТИ для подстанции";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [Description("Уникальный номер ПС")]
        [DisplayName("Идентификатор ПС")]
        [RequiredArgument]
        public InArgument<int> ps_id { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список ТИ")]
        public OutArgument<List<TIinfo>> TI_List { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }
        

        protected override bool Execute(CodeActivityContext context)
        {

            if (ps_id.Get(context) == null)
            {
                Error.Set(context, "не определен Идентификатор ПС");
                return false;
            }

            var result = new List<TIinfo>();
            try
            {
                var res = ARM_Service.NSI_Get_TI_List_ForPS(ps_id.Get(context));
                if (res != null)
                {
                    foreach (var nti in res)
                    {
                        if (nti.TI != null && !nti.TI.Deleted)
                        {
                            result.Add(new TIinfo
                            {
                                TI_ID = nti.TI.TI_ID,
                                PS_ID = nti.TI.PS_ID,
                                TIName = nti.TI.TIName,
                                TIType = nti.TI.TIType,
                                TIATSCode = nti.TI.TIATSCode,
                                Commercial = nti.TI.Commercial,
                                Voltage = nti.TI.Voltage,
                                SectionNumber = nti.TI.SectionNumber,
                                AccountType = nti.TI.AccountType,
                                PhaseNumber = nti.TI.PhaseNumber,
                                CustomerKind = nti.TI.CustomerKind
                            });
                        }
                    }
                }

                TI_List.Set(context, result);
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
