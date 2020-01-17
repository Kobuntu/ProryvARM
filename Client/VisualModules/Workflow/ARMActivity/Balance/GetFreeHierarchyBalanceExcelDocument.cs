using System;
using System.Activities;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.Workflow.Activity.Settings;
using System.Collections.Generic;

namespace Proryv.Workflow.Activity.ARM
{
    /// <summary>
    /// Баланс свободной иерархии
    /// </summary>
    public class GetFreeHierarchyBalanceExcelDocument : BaseArmActivity<bool>
    {
        public GetFreeHierarchyBalanceExcelDocument()
        {
            this.DisplayName = "Получить универсальный баланс в Excel";
            //TimeZone = 4;
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Идентификатор баланса")]
        [RequiredArgument]
        public InArgument<string> BalanceId { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Начальная дата")]
        public InArgument<DateTime> StartDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Конечная дата")]
        public InArgument<DateTime> EndDateTime { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Период дискретизации")]
        [TypeConverter(typeof(EnumDiscreteTypeConverter))]
        public enumTimeDiscreteType DiscreteType { get; set; }

        
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Документ")]
        public OutArgument<MemoryStream> Document { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            try
            {
                var balanceFreeHierarchyUNs = new List<string> {BalanceId.Get(context)};

                var balanceResult = ARM_Service.BL_GetFreeHierarchyBalanceResult(balanceFreeHierarchyUNs,
                    StartDateTime.Get(context),
                    EndDateTime.Get(context),
                    null,
                    TExportExcelAdapterType.toXLSx, true,
                    DiscreteType, EnumUnitDigit.Kilo, false,
                    EnumUnitDigit.Kilo, true, false, 3, 3, false, false, false);

                if (balanceResult != null)
                {
                    if (balanceResult.CalculatedValues == null || balanceResult.CalculatedValues.Count == 0)
                    {
                        throw new Exception("Документ не сформирован. Неверный идентификатор или баланс удален.");
                    }

                    var calculatedResult = balanceResult.CalculatedValues.First().Value;

                    if (calculatedResult.CompressedDoc == null)
                    {
                        throw new Exception("Документ пуст. Ошибка формирования");
                    }

                    var ms = new MemoryStream();

                    CompressUtility.DecompressGZip(calculatedResult.CompressedDoc).CopyTo(ms);
                    ms.Position = 0;
                    Document.Set(context, ms);
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
