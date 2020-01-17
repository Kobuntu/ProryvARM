using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Proryv.Workflow.Activity.Settings;
using System.Activities;
using System.Data;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.Workflow.Activity.ARM.NSI
{
    public class GetUSPDList : BaseArmActivity<bool>
    {
        public GetUSPDList()
        {
            DisplayName = "Получить список УСПД";
        }
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список идентификаторов УСПД")]
        public OutArgument<List<int>> USPDList { get; set; }
        
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (USPDList == null)
                metadata.AddValidationError("Не орпределено свойство 'Список идентификаторов УСПД'");
            base.CacheMetadata(metadata);
        }
        protected override bool Execute(CodeActivityContext context)
        {

            DataTable TempTable = null;
            string sql = @"set nocount on;set transaction isolation level read uncommitted;select USPD_ID from Hard_USPD";
            try
            {
                var serverData = ARM_Service.REP_Query_Report(sql, new List<QueryParameter>());

                if (!string.IsNullOrEmpty(serverData.Value))
                {
                    Error.Set(context, serverData.Value);
                }
                else if (serverData.Key!=null)
                {
                    TempTable = serverData.Key;

                    var result = new List<int>();
                    foreach (DataRow r in TempTable.Rows)
                    {
                        if (r.ItemArray!=null && r.ItemArray.Length > 0)
                        {
                            result.Add((int) r.ItemArray[0]);
                        }
                    }
                    USPDList.Set(context, result);
                }

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
