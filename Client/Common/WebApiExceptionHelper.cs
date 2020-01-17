using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.OData.Client;
namespace Proryv.ODataVirtualSource.Common
{
    public static class ExceptionHelper
    {
        public static string ToMessage(this Exception ex)
        {
            var wre = ex as WebRequestException;
            if (wre != null) return wre.Translate();

            var message = new StringBuilder();
            var e = ex;
            while (e != null)
            {
                if (e.InnerException == null)
                {
                    message.Append(e.Message).Append("\n");
                    break;
                }
                
                wre = e.InnerException as WebRequestException;
                if (wre != null) return wre.Translate();

                e = e.InnerException;
            }

            return message.ToString();
        }
    }
}
