using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;
using System.Activities.Presentation.Metadata;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Client.Visual.Common;

namespace Proryv.Workflow.Activity.ARM
{



    public enum GroupTPPowerReportMode : byte // обертка для EnumGroupTPPowerReportMode
    {
        NullValue = 0,
        OpenPeriod = 100,
        ClosedCurrPeriod = 101,
        ClosedNextPeriod = 102,
        ClosedPrevPeriod = 103,
    }

    
    
    public class PredefinedReportDoc : BaseArmActivity<bool>
    {
        public PredefinedReportDoc()
        {
            DisplayName = "Сформировать системный отчет";

            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(PredefinedReportDoc), "UserName", new EditorAttribute(typeof(SendEmailToUserPropEditor), typeof(SendEmailToUserPropEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        [RequiredArgument]
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [RequiredArgument]
        [DisplayName("Конечная дата")]
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        public InArgument<DateTime> EndDateTime { get; set; }

        [RequiredArgument]
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Тип отчета")]
        [TypeConverter(typeof(enumReportTypeTypeConverter))]
        public enumReportType ReportType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Тип расчета")]
        [TypeConverter(typeof(GroupTPPowerReportModeTypeConverter))]
        public GroupTPPowerReportMode GroupTPPowerReportMode { get; set; }

        [RequiredArgument]
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Список идентификаторов")]
        public InArgument<List<int>> ListIDs { get; set; }

        [RequiredArgument]
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Тип идентификаторов")]
        [TypeConverter(typeof(enumTypeHierarchyTypeConverter))]
        public enumTypeHierarchy TypeIDs { get; set; }


        [RequiredArgument]
        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Пользователь")]
        public string UserName { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Смещение времени в БД")]
        public int OffsetFromDataBase { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Документ")]
        public OutArgument<MemoryStream> Document { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Формат файла")]
        public OutArgument<string> FileExtention { get; set; }


        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (string.IsNullOrEmpty(UserName))
                metadata.AddValidationError("Не определен пользователь");
            base.CacheMetadata(metadata);
        }


        protected override bool Execute(CodeActivityContext context)
        {
            Error.Set(context, null);


            EnumGroupTPPowerReportMode? typecalc = null;
            if (GroupTPPowerReportMode == GroupTPPowerReportMode.OpenPeriod)
                typecalc = EnumGroupTPPowerReportMode.OpenPeriod;
            if (GroupTPPowerReportMode == GroupTPPowerReportMode.ClosedCurrPeriod)
                typecalc = EnumGroupTPPowerReportMode.ClosedCurrPeriod;
            if (GroupTPPowerReportMode == GroupTPPowerReportMode.ClosedNextPeriod)
                typecalc = EnumGroupTPPowerReportMode.ClosedNextPeriod;
            if (GroupTPPowerReportMode == GroupTPPowerReportMode.ClosedPrevPeriod)
                typecalc = EnumGroupTPPowerReportMode.ClosedPrevPeriod;



            string userName = UserName;//.Get(context);
            string userID = null;

                if (string.IsNullOrEmpty(userName))
                {
                    Error.Set(context, "Значение свойства 'Пользователь' не может быть пустым");
                    return false;
                }
                try
                {
                    List<UserInfo> UList = ARM_Service.EXPL_Get_All_Users();
                    foreach (UserInfo u in UList)
                    {
                        if (u.UserName.ToLower(System.Globalization.CultureInfo.InvariantCulture) ==
                            userName.ToLower(System.Globalization.CultureInfo.InvariantCulture))
                        {
                            userID = u.User_ID;
                            break;
                        }

                    }

                }
                catch (Exception ex)
                {
                    Error.Set(context, ex.Message);
                    if (!HideException.Get(context))
                        throw ex;
                }

                if (!string.IsNullOrEmpty(Error.Get(context)))
                    return false;

                if (string.IsNullOrEmpty(userID))
                {
                    Error.Set(context, "Не найден пользователь с именем '" + UserName + "'");
                    return false;
                }

            List<int> listIDs = ListIDs.Get(context);
            if (listIDs == null)
            {
                Error.Set(context, "Неопределен список идентификаторов");
                return false;
            }

            List<ID_TypeHierarchy> idList = new List<ID_TypeHierarchy>();

            foreach (int id in listIDs)
            {
                ID_TypeHierarchy idTypeHier = new ID_TypeHierarchy();
                idTypeHier.ID = id;
                idTypeHier.TypeHierarchy = TypeIDs;
                idList.Add(idTypeHier);
            }


            MemoryStream doc = null;
            try
            {
                SectionIntegralComplexResults res;
                if (ReportType == enumReportType.ReportReplacementOfMeters)
                    //TODO часовой пояс
                    res = ARM_Service.Rep_ReplacementOfAccountingFacilities(idList, StartDateTime.Get(context),
                        EndDateTime.Get(context), ReportType, typecalc, userName, null);
                else
                    //TODO часовой пояс
                    res = ARM_Service.REP_OverflowControl(idList, StartDateTime.Get(context),
                        EndDateTime.Get(context), ReportType, typecalc, userName, null, null, false, 3, ",", enumTimeDiscreteType.DBHours, null);


                if (res.Document != null)
                {
                    res.Document.Position = 0;
                    var ms = CompressUtility.DecompressGZip(res.Document);
                    ms.Position = 0;
                    Document.Set(context, ms);
                    //File.WriteAllBytes(@"C:\12\test_SysRep.xls",ms.ToArray());
                }

                switch (res.AdapterType)
                {
                    case TExportExcelAdapterType.toXLS:
                        FileExtention.Set(context, "xls");
                        break;
                    case TExportExcelAdapterType.toXLSx:
                        FileExtention.Set(context, "xlsx");
                        break;
                    case TExportExcelAdapterType.toHTML:
                        FileExtention.Set(context, "html");
                        break;
                    case TExportExcelAdapterType.toPDF:
                        FileExtention.Set(context, "pdf");
                        break;
                }
                
                if (res.Errors != null)
                    Error.Set(context, res.Errors.ToString());

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
