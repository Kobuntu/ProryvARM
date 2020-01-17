using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.Workflow.Activity.Settings;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.Data;

namespace Proryv.Workflow.Activity.ARM.NSI
{
    public class GetUSPDMetersList : BaseArmActivity<bool>
    {
        public GetUSPDMetersList()
        {
            DisplayName = "Получить список ПУ для УСПД";
        }
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор УСПД")]
        [RequiredArgument]
        public InArgument<Int32> USPD_ID { get; set; }
        
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список идентификаторов ПУ")]
        public OutArgument<List<int>> MeterList { get; set; }
        
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (MeterList == null)
                metadata.AddValidationError("Не орпределено свойство 'Список идентификаторов ПУ'");
            base.CacheMetadata(metadata);
        }
        protected override bool Execute(CodeActivityContext context)
        {

            DataTable TempTable = null;
            string sql = @"set nocount on;set transaction isolation level read uncommitted;select Meter_ID from Hard_MetersUSPD_Links where USPD_ID=" + USPD_ID.Get(context).ToString();
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
                        if (r.ItemArray != null && r.ItemArray.Length > 0)
                        {
                            result.Add((int) r.ItemArray[0]);
                        }
                    }
                    MeterList.Set(context, result);
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
