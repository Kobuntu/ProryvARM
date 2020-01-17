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
    public class GetBalanceList : BaseArmActivity<bool>
    {

        public GetBalanceList()
        {
            this.DisplayName = "Перечень балансов для объекта";
        }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Идентификатор объекта")]
        public InArgument<int> ID { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [RequiredArgument]
        [DisplayName("Тип иeрархии объекта")]
        [TypeConverter(typeof(EnumTypeHierarchyTypeConverter))]
        public enumTypeHierarchy HierarchyType { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_In)]
        [DisplayName("Включать вложенные объекты")]
        public bool WithNested { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список балансов ПС")]
        public OutArgument<List<BalanceInfo>> PSBalanceList { get; set; }


        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список балансов уровня 3")]
        public OutArgument<List<HierLev3BalanceInfo>> HierLev3BalanceList { get; set; }

        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Список формул")]
        public OutArgument<List<FormulaInfo>> FormulsList { get; set; }
                
        [Category(ActivitiesSettings.PropertyGridCategoryName_Out)]
        [DisplayName("Ошибка")]
        public OutArgument<string> Error { get; set; }


        protected override bool Execute(CodeActivityContext context)
        {
            List<BalanceInfo> Bps = new List<BalanceInfo>();
            List<HierLev3BalanceInfo> Bhl3 = new List<HierLev3BalanceInfo>();
            List<FormulaInfo> Frmls = new List<FormulaInfo>();

            try
            {
                var res = ARM_Service.BL_Get_Balance_List(ID.Get(context),
                    HierarchyType,
                    WithNested);

                if (res == null)
                {
                    Error.Set(context, "Ошибка получения информации о балансах (null)");
                }
                else
                {
                    if (res.Balance_PS_List_2 != null)
                    {
                        foreach (var item in res.Balance_PS_List_2)
                        {
                            Bps.Add(new BalanceInfo
                            {
                                UserName = item.UserName,
                                BalancePS_UN = item.BalancePS_UN,
                                BalancePSName = item.BalancePSName,
                                User_ID = item.User_ID,
                                HierLev1_ID = item.HierLev1_ID,
                                HierLev2_ID = item.HierLev2_ID,
                                HierLev3_ID = item.HierLev3_ID,
                                PS_ID = item.PS_ID,
                                TI_ID = item.TI_ID,
                                BalancePSType_ID = item.BalancePSType_ID,
                                ForAutoUse = item.ForAutoUse,
                                HighLimit = item.HighLimit,
                                LowerLimit = item.LowerLimit,
                                DispatchDateTime = item.DispatchDateTime

                            });
                        }
                    }

                    if (res.Balance_HierLev3_List != null)
                    {
                        foreach (var item in res.Balance_HierLev3_List)
                        {
                            Bhl3.Add(new HierLev3BalanceInfo
                            {
                                UserName = item.UserName,
                                Balance_HierLev3_UN = item.Balance_HierLev3_UN,
                                BalanceHierLev3Name = item.BalanceHierLev3Name,
                                User_ID = item.User_ID,
                                HierLev1_ID = item.HierLev1_ID,
                                HierLev2_ID = item.HierLev2_ID,
                                HierLev3_ID = item.HierLev3_ID,
                                BalanceHierLev3Type_ID = item.BalanceHierLev3Type_ID,
                                ForAutoUse = item.ForAutoUse,
                                HighLimit = item.HighLimit,
                                LowerLimit = item.LowerLimit

                            });
                        }
                    }

                    if (res.Formula_List != null)
                    {
                        foreach (var item in res.Formula_List)
                        {
                            Frmls.Add(new FormulaInfo()
                            {
                                UserName = item.UserName,
                                Formula_UN = item.Formula_UN,
                                FormulaName = item.FormulaName,
                                User_ID = item.User_ID,
                                HierLev1_ID = item.HierLev1_ID,
                                HierLev2_ID = item.HierLev2_ID,
                                HierLev3_ID = item.HierLev3_ID,
                                PS_ID = item.PS_ID,
                                TI_ID = item.TI_ID,
                                FormulaType_ID = item.FormulaType_ID,
                                HighLimit = item.HighLimit,
                                LowerLimit = item.LowerLimit,
                                FormulaClassification_ID = item.FormulaClassification_ID,
                                Voltage = item.Voltage
                            });
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Error.Set(context, ex.Message);
                if (!HideException.Get(context))
                    throw ex;
            }

            PSBalanceList.Set(context,Bps);
            HierLev3BalanceList.Set(context, Bhl3);
            FormulsList.Set(context, Frmls);

            return string.IsNullOrEmpty(Error.Get(context));
        }

    }

}
