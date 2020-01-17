using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.Visual.BalanceEditor;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.UI.Controls.BalanceFreeHierarchyFooter
{
    public class EditFreeHierarchyFooterViewModel : IDisposable, INotifyPropertyChanged
    {
        public EditFreeHierarchyFooterViewModel(List<Dict_Balance_FreeHierarchy_Footers> footers, FrameworkElement layout)
        {
            if (footers == null)
            {
                GridData = new RangeObservableCollection<Dict_Balance_FreeHierarchy_Footers>();
            }
            else
            {
                GridData = new RangeObservableCollection<Dict_Balance_FreeHierarchy_Footers>(footers);
            }

            _layout = layout;
        }


        private FrameworkElement _layout;
       
        public RangeObservableCollection<Dict_Balance_FreeHierarchy_Footers> GridData { get; set; }

        public Dictionary<double, string> Signs
        {
            get
            {
                return SignConverter.Signs;
            }
        }

        private List<Dict_Balance_FreeHierarchy_Section> _sections;
        public List<Dict_Balance_FreeHierarchy_Section> Sections
        {
            get
            {
                return _sections;
            }
            set
            {
                _sections = value;
            }
        }

        private BalanceFreeHierarchyCommand _addCommand;
        public BalanceFreeHierarchyCommand AddCommand
        {
            get
            {
                return _addCommand ??
                 (_addCommand = new BalanceFreeHierarchyCommand(obj =>
                 {
                     //Execute
                     

                 }, obj =>
                 {
                     //CanExecute
                    

                     return true;
                 }));
            }
        }

        #region IDispose

        public void Dispose()
        {
            _layout = null;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

    public class EditFreeHierarchyFooterViewModelContainer
    {
        public List<Dict_Balance_FreeHierarchy_Footers> Footers;
        public List<Dict_Balance_FreeHierarchy_Section> Sections;
        public string BalanceFreeHierarchyUn;
        public string BalanceFreeHierarchySectionUN;
        public string BalanceFreeHierarchySectionName;
    }
}
