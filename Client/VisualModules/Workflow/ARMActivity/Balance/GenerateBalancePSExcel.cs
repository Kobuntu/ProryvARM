using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;
using System.ComponentModel;
using System.Activities;
using System.IO;

namespace Proryv.Workflow.Activity.ARM
{
    public class GenerateBalancePSExcel : BaseArmActivity<bool>
    {

        public GenerateBalancePSExcel()
        {
            this.DisplayName = "Сгенерировать Excel-документы для пакета подстанций (Zip архив)";
            TimeZone = 4;
        }

        [RequiredArgument]
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [Description("Список идентификаторов подстанций")]        
        [DisplayName("Список ПС")]
        public InArgument<List<int>> Ps_List { get; set; }

        [RequiredArgument]
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [RequiredArgument]
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Конечная дата")]
        public InArgument<DateTime> EndDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Период дискретизации")]
        [TypeConverter(typeof(EnumDiscreteTypeConverter))]
        public enumTimeDiscreteType DiscreteType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Источник")]
        public EnumDataSourceType? DataSourceType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Профиль")]
        [TypeConverter(typeof(EnumProfileTypeConverter))]
        public enumReturnProfile ProfileType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Энергия или мощность")]
        [TypeConverter(typeof(EnumTypeInformationTypeConverter))]
        public enumTypeInformation isPower { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Временная зона")]
        [DefaultValue(4)]
        public int TimeZone { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Документ")]
        public OutArgument<MemoryStream> Document { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [Description("Список статусов по докуметам")]
        [DisplayName("Список статусов")]
        public OutArgument<List<GenerateDocResult>> ResultReport { get; set; }


        void ReadAndWriteToAnotherStream(Stream source, Stream dest, bool doFlushAndClose)
        {
            byte[] buf = new byte[50000]; int bytestoread = 0; do
            {
                bytestoread = source.Read(buf, 0, buf.Length);
                dest.Write(buf, 0, bytestoread);
            }
            while (bytestoread > 0);
            if (doFlushAndClose)
            {
                dest.Flush();
                dest.Close();
            }
        }

        protected override bool Execute(CodeActivityContext context)
        {
            var buildID = Guid.NewGuid();

            //throw new Exception("Error !!!!!!!!!!"); // test exception

            try
            {
                var psList = Ps_List.Get(context) ;
                if (psList == null || psList.Count == 0)
                {
                    Error.Set(context, "Список ПС не может быть пустым");
                    return false;
                }
                
                //TODO часовой пояс
                var res = ARM_Service.BPS_GenerateBalancePSExcel(buildID,
                    psList, 
                    StartDateTime.Get(context), 
                    EndDateTime.Get(context),
                    DiscreteType,
                    DataSourceType,
                    isPower, false, null);

                if (res != null)
                {
                    var generatedFile = new MemoryStream();
                    var res2 = ARM_Service.AUTODOCUM_LoadGeneratedZip(buildID);
                    if (res2 != null)
                    {
                        ReadAndWriteToAnotherStream(res2, generatedFile, false);
                        generatedFile.Seek(0, SeekOrigin.Begin);

                        Document.Set(context, generatedFile);
                        ResultReport.Set(context, res);
                    }
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
