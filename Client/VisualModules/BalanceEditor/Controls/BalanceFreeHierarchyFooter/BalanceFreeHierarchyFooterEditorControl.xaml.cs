using Infragistics.Windows;
using Infragistics.Windows.Commands;
using Infragistics.Windows.DataPresenter;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.Visual.BalanceEditor;
using Proryv.ElectroARM.Controls.Controls.Tabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.UI.Controls.BalanceFreeHierarchyFooter
{
    /// <summary>
    /// Логика взаимодействия для BalanceFreeHierarchyFooterEditorControl.xaml
    /// </summary>
    public partial class BalanceFreeHierarchyFooterEditorControl : ILocalChildren
    {
        private BalanceFreeHierarchyFooterModel _viewModel;

        public BalanceFreeHierarchyFooterEditorControl(Dict_Balance_FreeHierarchy_Footers footer, 
            List<Dict_Balance_FreeHierarchy_Section> sections, Action afterSave)
        {
            //Resources.Add("Signs", SignConverter.Signs);
            //Resources.Add("Sections", sections);

            InitializeComponent();
            
            DataContext = _viewModel = new BalanceFreeHierarchyFooterModel(footer, this, afterSave);

            _viewModel.Sections = sections;
        }

        

        private void PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            RecordSelector rs = (RecordSelector)Utilities.GetAncestorFromType((DependencyObject)e.OriginalSource, typeof(RecordSelector), false);
            DataRecordCellArea drca = (DataRecordCellArea)Utilities.GetAncestorFromType((DependencyObject)e.OriginalSource, typeof(DataRecordCellArea), false);

            if ((rs != null && rs.IsAddRecord || drca != null && drca.IsAddRecord))
            {
                DataRecord dr;
                if (rs != null)
                {
                    dr = (DataRecord)rs.Record;
                }
                else
                {
                    dr = (DataRecord)drca.Record;
                }

                if (!dr.IsDataChanged)
                {
                    dr.Cells["Coef"].Value = 1;
                }
            }
            else
            {
                //xdgData.ExecuteCommand(DataPresenterCommands.EndEditModeAndCommitRecord);
            }
        }

        #region ILocalChildren
        public Action OnClose { get; set; }

        public EnumLocalModalType LocalModalType
        {
            get
            {
                return EnumLocalModalType.TrueModal;
            }
        }

        public Guid Id { get; set; }

        #endregion
    }
}
