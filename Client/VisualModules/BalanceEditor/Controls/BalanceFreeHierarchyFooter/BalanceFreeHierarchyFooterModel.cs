using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Common;
using Proryv.AskueARM2.Client.Visual.BalanceEditor;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Proryv.AskueARM2.Client.Visual.Formulas.BalanceEditor.UI.Controls.BalanceFreeHierarchyFooter
{
    public class BalanceFreeHierarchyFooterModel : IDisposable, INotifyPropertyChanged
    {
        public BalanceFreeHierarchyFooterModel(Dict_Balance_FreeHierarchy_Footers footer, FrameworkElement layout, Action afterSave)
        {
            _footer = footer;
            _afterSave = afterSave;
            EditFooter = new Dict_Balance_FreeHierarchy_Footers
            {
                BalanceFreeHierarchyFooterName = footer.BalanceFreeHierarchyFooterName,
                BalanceFreeHierarchyFooter_UN = footer.BalanceFreeHierarchyFooter_UN,
                BalanceFreeHierarchySection_UN = footer.BalanceFreeHierarchySection_UN,
                BalanceFreeHierarchySubsection_UN = footer.BalanceFreeHierarchySubsection_UN,
                BalanceFreeHierarchy_UN = footer.BalanceFreeHierarchy_UN,
                Coef = footer.Coef,
                SortNumber = footer.SortNumber,
                UseInTotalResult = footer.UseInTotalResult,
            };

            _layout = layout;

            GridData = new RangeObservableCollection<Dict_Balance_FreeHierarchy_Footer_Description>();

            UpdateDescriptions();
        }

        private FrameworkElement _layout;
        private Action _afterSave;
        private Dict_Balance_FreeHierarchy_Footers _footer;
        private Dict_Balance_FreeHierarchy_Footers _editFooter;
        public Dict_Balance_FreeHierarchy_Footers EditFooter {
            get
            {
                return _editFooter;
            }
            set
            {
                _editFooter = value;
                RaisePropertyChanged("Footer");
            }
        }

        public RangeObservableCollection<Dict_Balance_FreeHierarchy_Footer_Description> GridData { get; set; }

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

        private void UpdateDescriptions()
        {
            if (string.IsNullOrEmpty(EditFooter.BalanceFreeHierarchyFooter_UN)) return; //Это новый итог, читать описани ене нужно

            _layout.RunBackgroundAsync<List<Dict_Balance_FreeHierarchy_Footer_Description>>("BL_GetFreeHierarchyBalanceDescriptions",
                OnUpdateDescriptions, EnumServiceType.ArmService, EditFooter.BalanceFreeHierarchyFooter_UN);
        }

        private void OnUpdateDescriptions(List<Dict_Balance_FreeHierarchy_Footer_Description> description)
        {
            if (description == null || description.Count == 0) return;

            GridData.AddRange(description);
        }


        private BalanceFreeHierarchyCommand _saveCommand;
        public BalanceFreeHierarchyCommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                 (_saveCommand = new BalanceFreeHierarchyCommand(obj =>
                 {
                     //Execute
                     //Пронумеровываем описание заново, согласно новому порядку в списке
                     var description = new List<Dict_Balance_FreeHierarchy_Footer_Description>();
                     byte sortNumber = 0;
                     foreach (var fd in GridData)
                     {
                         fd.SortNumber = sortNumber++;
                         description.Add(fd);
                     }

                     _layout.RunBackgroundAsync<Tuple<string, bool>>("BL_SaveFreeHierarchyBalanceFooter",
                         OnSave, EnumServiceType.ArmService, _editFooter, description, Manager.User.User_ID);

                 }, obj=>
                 {
                     //CanExecute
                     if (this._editFooter == null)
                     {
                         ReasonOfDisabled = "Отсутствует заголовок";
                         return false;
                     }

                     if (this.GridData == null || this.GridData.Count == 0)
                     {
                         ReasonOfDisabled = "Отредактируйте описание итога";
                         return false;
                     }

                     return true;
                 }));
            }
        }

        private string _reasonOfDisabled;
        public string ReasonOfDisabled
        {
            get
            {
                return _reasonOfDisabled;
            }
            set
            {
                if (string.Equals(_reasonOfDisabled, value)) return;

                _reasonOfDisabled = value;
                RaisePropertyChanged("ReasonOfDisabled");
            }
        }

        private bool CanUpdateBalance(object parameter)
        {
            return true;
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

        private void OnSave(Tuple<string, bool> tuple)
        {
            if (tuple == null || string.IsNullOrEmpty(tuple.Item1)) return;

            _footer.BalanceFreeHierarchyFooter_UN = tuple.Item1;
            _footer.BalanceFreeHierarchyFooterName = _editFooter.BalanceFreeHierarchyFooterName;
            _footer.BalanceFreeHierarchyFooter_UN = _editFooter.BalanceFreeHierarchyFooter_UN;
            _footer.BalanceFreeHierarchySection_UN = _editFooter.BalanceFreeHierarchySection_UN;
            _footer.BalanceFreeHierarchySubsection_UN = _editFooter.BalanceFreeHierarchySubsection_UN;
            _footer.BalanceFreeHierarchy_UN = _editFooter.BalanceFreeHierarchy_UN;
            _footer.Coef = _editFooter.Coef;
            _footer.SortNumber = _editFooter.SortNumber;
            _footer.UseInTotalResult = _editFooter.UseInTotalResult;

            Manager.UI.ShowLocalMessage("Cохранено", _layout);

            Manager.UI.CloseLocalModal(_afterSave, _layout);
        }
    }
}
