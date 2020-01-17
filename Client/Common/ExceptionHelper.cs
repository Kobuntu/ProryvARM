using System;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace Proryv.AskueARM2.Client.Visual.Common.Common
{
    public static class ExceptionHelper
    {
        public static void ShowMessage(this Exception ex, FrameworkElement source = null)
        {
            if (ex == null || Manager.UI == null) return;

            var message = new StringBuilder();
            AcumulateInnerExceptions(ex, message);
            if (message.Length == 0) return;
            
            if (source != null)
            {
                Manager.UI.ShowLocalMessage(message.ToString(), source);
            }
            else
            {
                Manager.UI.ShowMessage(message.ToString());
            }
        }

        public static void AcumulateInnerExceptions(this Exception ex, StringBuilder message)
        {
            if (ex == null) return;

            message.Append("Тип    : ").AppendLine(ex.GetType().FullName);
            message.Append("Ошибка : ").AppendLine(ex.Message);
            message.Append("Стек   : ").AppendLine(ex.StackTrace);
            
            //message.AppendLine();

            AcumulateInnerExceptions(ex.InnerException, message);
        }

        private static void ShowAggregateException(this AggregateException aex)
        {
            var ex = aex.Flatten();

            var msg = new StringBuilder();
            if (ex.InnerExceptions != null && ex.InnerExceptions.Count > 0)
            {
                foreach (var iex in ex.InnerExceptions)
                {
                    msg.Append(iex.Message + "\n");
                }
            }

            Manager.UI.ShowMessage(msg.ToString());
        }


        public static string DetalizateException(this Exception ex)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Внутренняя ошибка, свяжитесь пожалуйста с разработчиками:\n {0}", ex.Message);

            var st = new StackTrace(ex, true);
            var frame = st.GetFrame(st.FrameCount - 1);
            if (frame != null)
            {
                var line = frame.GetFileLineNumber();
                if (line > 0)
                {
                    sb.AppendFormat("\nLine number: {0}; File: {1}", frame.GetFileLineNumber(), frame.GetFileName());
                }
            }

            sb.AppendFormat("\nStackTrace:\n{0}", ex.StackTrace);

            if (ex.InnerException != null)
            {
                sb.AppendFormat("\nInnerException:\n{0}", ex.InnerException.Message);
            }

            return sb.ToString();
        }
    }
}
