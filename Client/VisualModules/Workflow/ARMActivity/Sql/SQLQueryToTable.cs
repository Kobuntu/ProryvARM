using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Activities;
using Proryv.Workflow.Activity.Settings;
using System.Data;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.Workflow.Activity.ARM
{
    public class SQLQueryToTable : BaseArmActivity<bool>
    {
        public SQLQueryToTable()
        {
            this.DisplayName = "Выполнить Sql запрос к текущей БД и получить таблицу";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("SQL запрос")]
        public InArgument<string> Sql { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список строк")]
        public OutArgument<DataTable> ResultTable { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            DataTable TempTable = null;

            try
            {
                var serverData = ARM_Service.REP_Query_Report(Sql.Get(context), new List<QueryParameter>());

                if (!string.IsNullOrEmpty(serverData.Value))
                {
                    Error.Set(context, serverData.Value);
                }
                else
                {
                    TempTable = serverData.Key;
                }

            }

            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }


            //List<DataRow> L = new List<DataRow>();
            //if (TempTable != null)
                //foreach (DataRow row in TempTable.Rows)
                    //L.Add(row);

            ResultTable.Set(context, TempTable);
            return string.IsNullOrEmpty(Error.Get(context));

        }
    }
}
