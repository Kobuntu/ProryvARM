using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Proryv.AskueARM2.Both.VisualCompHelpers;
using Proryv.AskueARM2.Both.VisualCompHelpers.TClasses;
using Proryv.AskueARM2.Both.VisualCompHelpers.ValuesToXceedGrid;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common;
using Xceed.Wpf.DataGrid;

namespace Proryv.ElectroARM.Controls.Styles
{
    public partial class GridTemplates
    {
        private void SN_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            fill(sender);
        }

        private void Pik_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(sender as DependencyObject))
            {
                fill_Pik(sender);
            }
        }

        private void Pik_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            fill_Pik(sender);
        }

        void fill(object sender)
        {
            var tb = sender as TextBlock;
            var ti = tb.DataContext as TInfo_TI;
            if (ti != null)
                tb.Text = ti.MeterSerialNumber;
        }

        void fill_Pik(object sender)
        {
            var tb = sender as TextBlock;
            var ti = tb.DataContext as TInfo_TI;
            if (ti != null)
                tb.Text = ti.Pik;
        }



        void USPD_SN_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(sender as DependencyObject))
            {
                fill_USPD(sender);
            }
        }

        void fill_USPD(object sender)
        {
            var tb = sender as TextBlock;
            var uspd = tb.DataContext as IHard_USPD;
            if (uspd != null)
                tb.Text = uspd.USPDSerialNumber;
        }

        private void SN_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(sender as DependencyObject))
            {
                fill(sender);
            }
        }
        private void USPD_SN_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            fill_USPD(sender);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = sender as ToggleButton;
            StackPanel s_panel = VisualHelper.FindParent<StackPanel>(btn);

            IFlowBalanceTI FlowBalanceTI_Frame = VisualHelper.FindParent<IFlowBalanceTI>(s_panel);

            ComboBox cmbReturnProfile = (ComboBox)((UserControl)FlowBalanceTI_Frame).FindName("cmbReturnProfile");
            DataGridControl GridControl = (DataGridControl)((UserControl)FlowBalanceTI_Frame).FindName("GridControl");

            TSection _SectionIntegralActs = FlowBalanceTI_Frame.GetSectionAct();

            if (s_panel != null)
            {
                ValuePair<string, TDatePeriod> title_pair = (ValuePair<string, TDatePeriod>)s_panel.DataContext;
                EnumDataSourceType? DataSourceType = cmbReturnProfile.SelectedValue as EnumDataSourceType?;
                bool isReadCalculatedValues = true;
                DateTime dtStart = title_pair.Value.dtStart;

                FlowBalanceTI result = null;

                bool IsAdd = btn.IsChecked.Value;

                if (!IsAdd)
                {
                    result = new FlowBalanceTI() { NumbersValues = Extention.GetNumbersValuesInPeriod(enumTimeDiscreteType.DBHalfHours, dtStart, dtStart.AddMinutes(1410), Manager.Config.SelectedTimeZoneInfo) };
                    ValuesToXceedGrid.UpdateFlowBalanceTIToGrid(GridControl, result, Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary.TGlobalVisualDataRequestObjectsNames.GlobalVisualDataRequestObjectsNames, new ItemSelector(), title_pair, IsAdd);
                }
                else
                {

                    Manager.UI.RunAsync(delegate ()
                    {
                        result = ARM_Service.SIA_GetFlowBalanceTI(_SectionIntegralActs.Section_ID, dtStart, dtStart.AddMinutes(1410),
                            DataSourceType, enumTimeDiscreteType.DBHalfHours, EnumUnitDigit.None, isReadCalculatedValues, Manager.Config.TimeZone);

                    }, delegate ()
                    {
                        if (result != null)
                        {
                            ValuesToXceedGrid.UpdateFlowBalanceTIToGrid(GridControl, result, Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary.TGlobalVisualDataRequestObjectsNames.GlobalVisualDataRequestObjectsNames, new ItemSelector(), title_pair, IsAdd);
                        }
                    });
                }

            }
        }
    }
}
