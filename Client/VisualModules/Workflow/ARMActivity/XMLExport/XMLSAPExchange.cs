using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Proryv.Workflow.Activity.Settings;
using System.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.IO;
using Proryv.AskueARM2.Client.ServiceReference.DeclaratorService;

namespace Proryv.Workflow.Activity.ARM
{
    public class XMLSAPExchange : BaseArmActivity<bool>
    {
        public XMLSAPExchange()
        {
            this.DisplayName = "SAP импорт данных (замены счетчиков, абонентов)";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Импортируемый файл")]
        [RequiredArgument]
        public InArgument<MemoryStream> Document_In { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Результат обработки файла")]
        [RequiredArgument]
        public OutArgument<MemoryStream> Document_Out { get; set; }

        [Description("Строка ошибки")]
        [DisplayName("Ошибка")]
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        public OutArgument<string> Error { get; set; }

        
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("User_ID")]
        [RequiredArgument]
        public InArgument<string> User_ID { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("FileName")]
        [RequiredArgument]
        public InArgument<string> FileName { get; set; }


        protected override bool Execute(CodeActivityContext context)
        {

            MemoryStream document = Document_In.Get(context);
            string user_ID = User_ID.Get(context);
            string fileName = FileName.Get(context);
            
            try
            {

                StreamExchange res = DeclaratorService.XMLImportMeterReplaces(new StreamExchange() { User_ID = user_ID, FileName = fileName, XMLStream = document });

                if (!string.IsNullOrEmpty(res.Errors))
                    Error.Set(context, res.Errors);


                Document_Out.Set(context, res.XMLStream);
                
                

            }
            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
            }
            finally
            {
                if (document != null)
                {
                    document.Close();
                    document.Dispose();
                }
            }

            return string.IsNullOrEmpty(Error.Get(context));
        }

    }
}
