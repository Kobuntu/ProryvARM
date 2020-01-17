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
    public class GetListFormuls : BaseArmActivity<bool>
    {
        public GetListFormuls()
        {
            this.DisplayName = "Получить список формул";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор объекта")]
        [RequiredArgument]
        public InArgument<Int32> ObjectID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Тип объекта")]
        [RequiredArgument]
        [TypeConverter(typeof(enumFormulsObjectTypeTypeConverter))]
        public enumFormulsObjectType ObjectType { get; set; }

        
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список формул")]
        public OutArgument<List<usp2_Info_GetFormulaHierarchy_ListResult>> FormulsList { get; set; }
                        

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            int obj_id = ObjectID.Get(context);


            List<usp2_Info_GetFormulaHierarchy_ListResult> vallist = null ;
            try
            {
                var res = ARM_Service.FL_GetFormulaHierarchy_List(obj_id, (enumTypeHierarchy)ObjectType);
                if (res != null)
                {
                    vallist = res.FormulaHierarchy;
                }
            }

            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            if (vallist == null)
                vallist = new List<usp2_Info_GetFormulaHierarchy_ListResult>();
            FormulsList.Set(context, vallist);

            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}
