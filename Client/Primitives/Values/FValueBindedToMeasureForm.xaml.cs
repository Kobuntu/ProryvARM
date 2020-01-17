using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Proryv.AskueARM2.Both.VisualCompHelpers.Interfaces;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service;
using Proryv.AskueARM2.Client.Visual;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;

namespace Proryv.ElectroARM.Controls.Controls.F_Value
{
    /// <summary>
    /// Interaction logic for FValueBindedToMeasureForm.xaml
    /// </summary>
    public partial class FValueBindedToMeasureForm : IDisposable
    {
        private bool _isHideFlag;
        public bool IsHideFlag
        {
            get { return _isHideFlag; }
            set
            {
                if (_isHideFlag == value) return;

                _isHideFlag = value;
               // if (IsHideFlag) FlagImage.Visibility = Visibility.Collapsed;
            }

        }

        private object _oldContextValue;
        private object _contextValue;
        public object ContextValue
        {
            get
            {
                return _contextValue;
            }
            set
            {
                if (value == null)
                {
                    _contextValue = null;
                    return;
                }

                _contextValue = value as IFValue;
                if (_contextValue != null) return;

                _contextValue = value as IConvertible;
                if (_contextValue != null) return;

                var summaryResult = value as Infragistics.Windows.DataPresenter.SummaryResult;
                if (summaryResult != null)
                {
                    _contextValue = summaryResult.Value;
                    return;
                }

                var unboundColumn = value as Infragistics.Controls.Grids.UnboundColumnDataContext;
                if (unboundColumn != null)
                {
                    //var dr = unboundColumn.RowData as System.Data.DataRowView;
                    //if (dr == null) return;

                    _contextValue = unboundColumn.Value; //dr[unboundColumn.ColumnKey];
                    return;
                }
            }
        }

        public static readonly DependencyProperty UseMeasureModuleProperty = DependencyProperty.Register(
            "UseMeasureModule", typeof(bool), typeof(FValueBindedToMeasureForm), new PropertyMetadata(true, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var measureForm = d as FValueBindedToMeasureForm;
            if (measureForm != null)
            {
                //measureForm.ValueChanged(e.NewValue);
                measureForm.UseMeasureModule = (bool)e.NewValue;
            }
        }

        private bool? _useMeasureModule;

        public bool UseMeasureModule
        {
            get
            {
                if (!_useMeasureModule.HasValue)
                {
                    _useMeasureModule = (bool)GetValue(UseMeasureModuleProperty);
                }

                return _useMeasureModule.Value;
            }
            set
            {
                if (!_useMeasureModule.HasValue)
                {
                    _useMeasureModule = value;
                    return;
                }

                if (_useMeasureModule == value) return;

                _useMeasureModule = value;
                SetValue(UseMeasureModuleProperty, value);
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(IConvertible), typeof(FValueBindedToMeasureForm), new PropertyMetadata(null, ValueChangedCallback));

        private static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var measureForm = d as FValueBindedToMeasureForm;
            if (measureForm != null)
            {
                measureForm.ContextValue = e.NewValue;
            }
        }

        public IConvertible Value
        {
            get { return GetValue(ValueProperty) as IConvertible; }
            set { SetValue(ValueProperty, value); }
        }

        private System.Windows.Media.Brush _foreground;
        public System.Windows.Media.Brush Foreground
        {
            get { return _foreground; }
            set
            {
                if (_foreground == value) return;

                _foreground = value;
                if (Foreground != null) ValueLabel.Foreground = _foreground;
            }
        }

        private IMeasure _measure;

        private IMeasure MeasureModule
        {
            get
            {
                //if (!UseMeasureModule) return null;

                if (_measure != null) return _measure;

                _measure = TryFindResource("IMeasure") as IMeasure;
                if (_measure == null)
                {
                    _measure = this.FindParent<IMeasure>();
                }

                return _measure;
            }
        }

        public FValueBindedToMeasureForm()
        {
            InitializeComponent();

        }

        private void FlagImageOnFlagChanged(object sender, ReceivedDataEventArgs e)
        {
            ToolTip = e.ToolTip;
            ValueLabel.Foreground = GlobalVisualDictionary.FLAGtoBrush(e.Flag);
        }

        public void UpdateBindingExpression()
        {
            var be = BindingOperations.GetBindingExpressionBase(ValueLabel, ContentControl.ContentProperty);
            if (be != null)
            {
                be.UpdateTarget();
                this.UpdateLayout();
                be.UpdateTarget();
            }
        }

        private void FValueBindedToMeasureFormOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ContextValue = e.NewValue;
            InitForm();
        }

        private void InitForm()
        {
            if (_contextValue == null)
            {
                ValueLabel.Text = string.Empty;
                _oldContextValue = null;
                FlagImage.DataContext = null;

                return;
            }

            //if (!IsLoaded) return;

            var fValue = _contextValue as IFValue;
            //if (_oldContextValue!=null && Equals(_contextValue, _oldContextValue))
            //{
            //    if (FlagImage.DataContext == null && fValue != null)
            //    {
            //        FlagImage.DataContext = fValue.F_FLAG;
            //    }
            //    return;
            //}

            if (!_isHideFlag && fValue != null)
            {
                FlagImage.DataContext = fValue.F_FLAG;

                if (FlagImage.Visibility != Visibility.Visible)
                {
                    FlagImage.Visibility = Visibility.Visible;
                }
            }

            var convertible = _contextValue as IConvertible;
            if (convertible == null) return;

            if (ValueLabel.Visibility != Visibility.Visible)
            {
                ValueLabel.Visibility = Visibility.Visible;
            }

            ValueLabel.Text = GlobalVisualDictionary.GetTextBindedToMeasureForm(convertible, MeasureModule, UseMeasureModule);

            _oldContextValue = _contextValue;
            if (MeasureModule!=null && MeasureModule.MeasureUnitSelectedInfo != null)
            {
                _activeMeasureUnitUn = MeasureModule.MeasureUnitSelectedInfo.ActiveMeasureUnitUn;
                _reactiveMeasureUnitUn = MeasureModule.MeasureUnitSelectedInfo.ReactiveMeasureUnitUn;
            }
        }
                
        private bool _isBinded;
        private string _activeMeasureUnitUn;
        private string _reactiveMeasureUnitUn;

        private void FValueBindedToMeasureFormOnLoaded(object sender, RoutedEventArgs e)
        {
            if (MeasureModule == null || MeasureModule.MeasureUnitSelectedInfo == null
               || !string.Equals(_activeMeasureUnitUn, MeasureModule.MeasureUnitSelectedInfo.ActiveMeasureUnitUn)
               || !string.Equals(_reactiveMeasureUnitUn, MeasureModule.MeasureUnitSelectedInfo.ReactiveMeasureUnitUn))
            {
                InitForm();
            }

            if (_isBinded) return;

            var npc = MeasureModule as INotifyPropertyChanged;
            if (npc != null)
            {
                npc.PropertyChanged += OnIMeasurePropertyChanged;
            }
            _isBinded = true;
        }

        private void FValueBindedToMeasureFormOnUnloaded(object sender, RoutedEventArgs e)
        {
            if (!_isBinded) return;

            var npc = MeasureModule as INotifyPropertyChanged;
            if (npc != null)
            {
                npc.PropertyChanged -= OnIMeasurePropertyChanged;
            }

            _measure = null;
            _isBinded = false;
        }

        private void OnIMeasurePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args == null || string.IsNullOrEmpty(args.PropertyName)
                             || !string.Equals(args.PropertyName, "MeasureUnitSelectedInfo")) return;

            var convert = (_contextValue as IConvertible) ?? Value;
            if (convert == null) return;

            ValueLabel.Text = GlobalVisualDictionary.GetTextBindedToMeasureForm(convert, MeasureModule, UseMeasureModule);
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

            _contextValue = null;
            _oldContextValue = null;
            _isBinded = false;
        }
    }
}
