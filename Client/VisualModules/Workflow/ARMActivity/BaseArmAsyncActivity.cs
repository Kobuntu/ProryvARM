using System;
using System.Activities;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Proryv.Workflow.Activity.ARM
{
    public abstract class BaseArmAsyncActivity<TResult> : AsyncCodeActivity<TResult>
    {
        [Description("Скрыть ошибки выполнения")]
        [DisplayName(@"Скрыть ошибки")]
        [DefaultValue(true)]
        public InArgument<bool> HideException{ get; set; }

        [DisplayName(@"Экранное имя")]
        public new string DisplayName { get { return base.DisplayName; } set { base.DisplayName = value; } }

        protected BaseArmAsyncActivity()
        {
            HideException = false;
        }

        // централизованная проверка на пустые свойства (в ошибках показывать имена по русски..)
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            const string s = "Свойство {0} не может быть пустым";

            Type t = this.GetType();
            PropertyInfo[] pia = t.GetProperties();
            foreach (PropertyInfo pi in pia)
            {
                RequiredArgumentAttribute reqAttr = (RequiredArgumentAttribute)pi.GetCustomAttributes(typeof(RequiredArgumentAttribute), false).FirstOrDefault();
                if (reqAttr == null) continue;
                object o = pi.GetValue(this, null);
                if (o != null) continue;
                DisplayNameAttribute dNameAttr = (DisplayNameAttribute)pi.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault();
                if (dNameAttr == null) continue;
                string atrName = "'" + dNameAttr.DisplayName + "'";
                metadata.AddValidationError(string.Format(s, atrName));
            }
            base.CacheMetadata(metadata);
        }



    }
}
