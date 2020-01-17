using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using Microsoft.VisualBasic.Devices;

namespace Proryv.Workflow.Activity.ARM.WMI
{
    public abstract class WMIExecuteActivityBase : BaseArmAsyncActivity<bool>
    {
        [Description("Синтаксис SQL для условия выборки при создании запропроса")]
        [DisplayName("Условие выборки (WHERE)")]
        public InArgument<string> WhereCondition { get; set; }

        [Description("Синтаксис SQL для полей в результате выборки")]
        [DisplayName("Поля в результате выборки")]
        public InArgument<string> SelectFields { get; set; }

        protected string Target = "";
        protected string Where = "";
        protected string Service = "";

        private string _selectFields = "";
        protected bool HideExc = false;

        [Description("Подключеие к удаленному компьютеру(Scope). Использовать WMICreateScopeActivity")]
        [DisplayName("Удаленный компьютер")]
        public InArgument<ManagementScope> Scope { get; set; }

        [Description("Результаты выборки. Dictionary ключ: string (имя параметра); значение: object (значение параметра соотв. его типу)")]
        [DisplayName("Результаты выборки")]
        public OutArgument<List<Dictionary<string, object>>> Results { get; set; }

        protected delegate List<Dictionary<string, object>> AsyncExecuteWMIDelegate(
            string target, string whereCondition, ManagementScope wmScope);

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context,
                                                            AsyncCallback callback, object state)
        {
            var currentScope = context.GetValue(Scope);

            var asyncExecute = new AsyncExecuteWMIDelegate(ExecuteWMI);
            context.UserState = asyncExecute;
            
            _selectFields = SelectFields.Get(context);
            HideExc = HideException.Get(context);

            return asyncExecute.BeginInvoke(Target, Where, currentScope, callback, state);
        }

        protected override bool EndExecute(AsyncCodeActivityContext context,
                                           IAsyncResult result)
        {
            var asyncExecute = context.UserState as AsyncExecuteWMIDelegate;
            if (asyncExecute == null) return false;
            var wmiResult = asyncExecute.EndInvoke(result);
            context.SetValue(Results, wmiResult ?? new List<Dictionary<string, object>>());
            return (wmiResult != null);
        }

        protected virtual List<Dictionary<string, object>> ExecuteWMI(string target, string whereCondition, ManagementScope wmScope)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {
                wmScope = CreateScope(wmScope);
                if (_selectFields == null) _selectFields = "";
                if (whereCondition == null) whereCondition = "";
                _selectFields = _selectFields.Trim();
                whereCondition = whereCondition.Trim();
                var wmQueryString = string.Format("SELECT {0} FROM " + target,
                    _selectFields == "" ? "*" : _selectFields);
                if (!string.IsNullOrEmpty(whereCondition))
                    wmQueryString += " WHERE " + whereCondition;

                ObjectQuery wmQuery = new ObjectQuery(wmQueryString);

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmScope, wmQuery))
                {
                    using (var queryCollection = searcher.Get())
                    {
                        foreach (var wmResultLoopVariable in queryCollection)
                        {
                            var wmResult = wmResultLoopVariable;
                            Dictionary<string, object> resultRecord = new Dictionary<string, object>();
                            foreach (var wmResultColumnLoopVariable in wmResult.Properties)
                            {
                                var wmResultColumn = wmResultColumnLoopVariable;
                                resultRecord.Add(wmResultColumn.Name, wmResultColumn.Value);
                            }

                            if (resultRecord.Count > 0)
                                result.Add(resultRecord);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception :" + ex.Message);
                result = null;
                if (HideExc == false)
                    throw;
            }
            return result;
        }

        protected virtual ManagementScope CreateScope(ManagementScope wmScope)
        {
            if (wmScope == null)
            {
                Computer myComputer = new Computer();
                wmScope = new ManagementScope("\\\\" + myComputer.Name + "\\root\\" + Service);
            }
            if (!wmScope.IsConnected)
                wmScope.Connect();
            return wmScope;
        }
    }
}