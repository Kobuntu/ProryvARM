using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual.Common;

namespace Proryv.AskueARM2.Client.Visual.Formulas.FormulaEditor.UI
{
    /// <summary>
    /// Interaction logic for OperBeforeEditor.xaml
    /// </summary>
    public partial class OperBeforeEditor : UserControl
    {
        private FormulaRow _formulaRow;

        public OperBeforeEditor()
        {
            InitializeComponent();
            UpdateFunctionDescriptions();
        }

        private object before_GetValue(object source)
        {
            if (_formulaRow == null) return null;
            return _formulaRow.Detail.OperBefore;
        }

        private void before_SetValue(object source, object value)
        {
            if (_formulaRow == null) return;

            _formulaRow.Detail.OperBefore = value.ToString();
        }

        private void OperBeforeEditorOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext == null) return;

            _formulaRow = DataContext as FormulaRow;
        }

        private void butClick(object sender, RoutedEventArgs e)
        {
            if (tooltipPopup.IsOpen)
            {
                //but.IsChecked = false;
                Manager.UI.CloseAllPopups();
                return;
            }
            else
            {
                // открываем Popup
                tooltipPopup.OpenAndRegister(true);

            }
        }

        private void lvFunctionsOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count ==0 || _formulaRow == null) return;
            var item = (KeyValuePair<string, string>)e.AddedItems[0];

            var detail = _formulaRow.Detail;

            detail.OperBefore = (detail.OperBefore ?? "") + item.Key + "(";
            detail.OperAfter = ")" + (detail.OperAfter ?? "");

            Manager.UI.CloseAllPopups();
        }

        private void UpdateFunctionDescriptions()
        {
            Task.Factory.StartNew(() =>
            {
                var source = EnumClientServiceDictionary.EnumFunctionDescriptions;
                lvFunctions.Dispatcher.BeginInvoke((Action) (() =>
                {
                    lvFunctions.ItemsSource = source;
                }));
            });

            
        }
    }
}
