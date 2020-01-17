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
    public class Get_PS_List_Id : BaseArmActivity<bool>
    {

        public Get_PS_List_Id()
        {
            DisplayName = "Получить список ПС для уровня иерархии";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор уровня 1")]
        [DefaultValue(null)]
        public InArgument<object> HierLev1_ID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор уровня 2")]
        [DefaultValue(null)]
        public InArgument<object> HierLev2_ID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор уровня 3")]
        [DefaultValue(null)]
        public InArgument<object> HierLev3_ID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список идентификаторов ПС")]
        public OutArgument<List<int>> PS_List { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }


        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (HierLev1_ID == null && HierLev2_ID == null && HierLev3_ID == null)
                metadata.AddValidationError("Одно из свойств должно быть определено : 'Идентификатор уровня 1','Идентификатор уровня 2','Идентификатор уровня 3'");
            base.CacheMetadata(metadata);
        }

        protected override bool Execute(CodeActivityContext context)
        {
            Error.Set(context, null);
            var result = new List<int>();

            try
            {
                var hiers = new List<List<int>>();
                var hiers1Str = new[] {(HierLev1_ID.Get(context) ?? "").ToString(), (HierLev2_ID.Get(context) ?? "").ToString(), (HierLev3_ID.Get(context) ?? "").ToString()};

                for (var level = 0; level < 3; level++)
                {
                    var h = new List<int>();
                    var str = hiers1Str[level];
                    if (!string.IsNullOrEmpty(str))
                    {
                        str = hiers1Str[level].Replace(",", ";");
                        foreach (var s in str.Split(';'))
                        {
                            int id;
                            if (int.TryParse(s, out id)) h.Add(id);
                        }
                    }

                    hiers.Add(h);
                }

                var psList = ARM_Service.Tree_GetListPSForHierLevels(hiers[0], hiers[1], hiers[2]);
                if (psList == null || psList.Count == 0)
                {
                    var err = "Не найдено ни одной ПС";
                    Error.Set(context, err);
                    if (!HideException.Get(context))
                        throw new Exception(err);
                }

                foreach (var p in psList)
                    result.Add(p.PS_ID);
            }

            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            PS_List.Set(context, result);
            return string.IsNullOrEmpty(Error.Get(context));
        }


    }
}
