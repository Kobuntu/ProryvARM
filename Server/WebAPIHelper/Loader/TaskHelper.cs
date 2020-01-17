using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proryv.AskueARM2.Server.DBAccess.ErrorLog;

namespace Proryv.AskueARM2.Server.WCF.Loader
{
    public static class TaskHelper
    {
        //Пытаемся глотать неперехваченные исключения (пишем при этом в лог) (это чтобы не зависал сервис)
        public static void LoggedExceptions(this Task task)
        {
            task.ContinueWith(t =>
            {
                var ignore = t.Exception;
                if (ignore.InnerExceptions != null)
                {
                    foreach (var ex in ignore.InnerExceptions)
                    {
                        JournalApplicationErrorLogDBSupport.WriteToJournalApplicationErrorLog(ex.Message, ex.StackTrace, null, null);
                        System.Threading.Thread.Sleep(500);
                    }
                }

            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
