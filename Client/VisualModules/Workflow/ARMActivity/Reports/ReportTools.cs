using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.Workflow.Activity.ARM
{
    public static class ReportTools
    {
        public static string CorrectFileName(string str)
        {
            string error = "/\\:*?\"<>|";

            char[] chars = str.ToCharArray();

            for (int index = 0; index < chars.Length; index++)
            {
                int i = error.IndexOf(chars[index]);
                if (i != -1) chars[index] = '_';
            }
            return new string(chars);
        }

    }
}
