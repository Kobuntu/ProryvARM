using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Proryv.AskueARM2.Both.VisualCompHelpers.Interfaces;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;
using Proryv.Servers.Calculation.DBAccess.Interface;

namespace Proryv.ElectroARM.Controls.Controls.F_Value
{
    /// <summary>
    /// Логика взаимодействия для IntegralValueBindedToMeasureForm.xaml
    /// </summary>
    public partial class IntegralValueBindedToMeasureForm : IDisposable
    {
        private bool _isHideFlag;
        public bool IsHideFlag
        {
            get { return _isHideFlag; }
            set
            {
                if (_isHideFlag == value) return;

                _isHideFlag = value;
                if (IsHideFlag) FlagImage.Visibility = Visibility.Collapsed;
            }

        }

        private bool _useMeasureModule = true;
        public bool UseMeasureModule
        {
            get { return _useMeasureModule; }
            set
            {
                if (_useMeasureModule == value) return;

                _useMeasureModule = value;
            }
        }

        private IMeasure _measure;

        private IMeasure measure
        {
            get
            {
                if (!_useMeasureModule) return null;

                if (_measure != null) return _measure;

                _measure = this.FindParent<IMeasure>();

                var npc = _measure as INotifyPropertyChanged;
                if (npc != null)
                {
                    npc.PropertyChanged += OnIMeasurePropertyChanged;
                }

                return _measure;
            }
        }

        public IntegralValueBindedToMeasureForm()
        {
            InitializeComponent();
        }

        private void OnIMeasurePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args == null || string.IsNullOrEmpty(args.PropertyName)
                             || !string.Equals(args.PropertyName, "SelectedUnitDigit")) return;

            var convert = DataContext as IConvertible;
            if (convert == null) return;

            ValueLabel.Content = GlobalVisualDictionary.GetDoubleBindedToMeasureForm(convert, measure, false);
        }

        private void FValueBindedToMeasureFormOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext == null) return;

            var convert = DataContext as IConvertible;
            if (convert == null) return;

            var value = DataContext as IArchivesValued;

            if (!_isHideFlag && value != null)
            {
                FlagImage.DataContext = value.F_FLAG;
            }
            else if (FlagImage.Visibility == Visibility.Visible)
            {
                FlagImage.Visibility = Visibility.Collapsed;
            }

            if (value != null)
            {
                lEventDateTime.Content = value.GetEventDateTime();
            }

            ValueLabel.Content = GlobalVisualDictionary.GetDoubleBindedToMeasureForm(convert, measure, false);
        }

        private void FValueBindedToMeasureFormOnUnloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        private void FlagImageOnFlagChanged(object sender, ReceivedDataEventArgs e)
        {
            ToolTip = e.ToolTip;
            ValueLabel.Foreground = GlobalVisualDictionary.FLAGtoBrush(e.Flag);
        }
        public void Dispose()
        {
            if (_measure != null)
            {
                var npc = _measure as INotifyPropertyChanged;
                if (npc != null)
                {
                    npc.PropertyChanged -= OnIMeasurePropertyChanged;
                }

                _measure = null;
            }
        }

    }
}
