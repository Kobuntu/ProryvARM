using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.ElectroARM.Controls.Controls.Dialog.Primitives
{
    /// <summary>
    /// Interaction logic for TimeSpanComboBox.xaml
    /// </summary>
    public partial class TimeSpanComboBox
    {
        #region Properties

        public TimeSpan? SelectedTime
        {
            get
            {
                return GetValue(SelectedTimeProperty) as TimeSpan?;
            }
            set
            {
                SetValue(SelectedTimeProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedTimeProperty =
            DependencyProperty.Register("SelectedTime", typeof(TimeSpan?), typeof(TimeSpanComboBox), new PropertyMetadata(null, SelectedTimePropertyChangedCallback));

        private static void SelectedTimePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dc = d as TimeSpanComboBox;
            if (dc != null)
            {
                try
                {
                    if (dc.NotFireChanged) return;

                    dc.NotFireChanged = true;

                    var value = e.NewValue as TimeSpan?;
                    if (!value.HasValue)
                    {
                        dc.Text = string.Empty;
                    }
                    else
                    {
                        dc.Text = string.Format("{0:00}:{1:00}", value.Value.Hours, value.Value.Minutes);
                    }
                    
                }
                finally
                {
                    dc.NotFireChanged = false;
                }
            }
        }

        //public TimeSpan? SelectedTime
        //{
        //    get
        //    {
        //        TimeSpan ts;

        //        var t = SelectedItem as string;

        //        if (t == null || !TimeSpan.TryParse(t, out ts))
        //        {
        //            if (!TimeSpan.TryParse(Text, out ts)) return null;
        //        }

        //        return IsEndOfPeriod.GetValueOrDefault() ? ts.Add(TimeSpan.FromSeconds(59)) : ts;
        //    }
        //    set
        //    {
        //        if (!value.HasValue)
        //        {
        //            this.Text = string.Empty;
        //        }
        //        else
        //        {
        //            this.Text = string.Format("{0:00}:{1:00}", value.Value.Hours, value.Value.Minutes);
        //        }
        //    }
        //}

        public static readonly DependencyProperty IsEndOfPeriodProperty =
            DependencyProperty.Register("IsEndOfPeriod", typeof(bool?), typeof(TimeSpanComboBox), new PropertyMetadata(null, IsEndOfPeriodChangedCallback));

        public bool? IsEndOfPeriod
        {
            get { return GetValue(IsEndOfPeriodProperty) as bool?; }
            set { SetValue(IsEndOfPeriodProperty, value); }
        }

        static void IsEndOfPeriodChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var my = d as TimeSpanComboBox;
            if (my == null) return;

            my.SourceUpdate();
        }


        public static readonly DependencyProperty DiscreteTypeProperty =
            DependencyProperty.Register("DiscreteType", typeof(enumTimeDiscreteType?), typeof(TimeSpanComboBox), new PropertyMetadata(null, DiscreteTypeChangedCallback));

        public enumTimeDiscreteType? DiscreteType
        {
            get { return GetValue(DiscreteTypeProperty) as enumTimeDiscreteType?; }
            set { SetValue(DiscreteTypeProperty, value); }
        }

        static void DiscreteTypeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var my = d as TimeSpanComboBox;
            if (my == null || DesignerProperties.GetIsInDesignMode(my)) return;

            my.SourceUpdate();
        }

        #endregion

        public void SourceUpdate()
        {
            if (!DiscreteType.HasValue) return;

            var dt = DiscreteType.Value;

            var source = new List<string>();
            var selectedIndex = SelectedIndex;

            TimeSpan ts;
            if (IsEndOfPeriod.GetValueOrDefault())
            {
                var seconds = DiscreteType.Value == enumTimeDiscreteType.DBHours ? 3540 : 1799;
                ts = TimeSpan.FromSeconds(seconds);
            }
            else
            {
                ts = TimeSpan.Zero;
            }

            var deltaMinutes = TimeSpan.FromMinutes(((double)dt + 1) * 30);

            var count = 48 / ((int)dt + 1);

            for (int j = 0; j < count; j++)
            {
                source.Add(string.Format("{0:00}:{1:00}", ts.Hours, ts.Minutes));
                ts = ts.Add(deltaMinutes);
            }

            ItemsSource = source;

            if (DiscreteType == enumTimeDiscreteType.DBHours && SelectedIndex < 0)
            {
                if (selectedIndex < 0)
                {
                    SelectedIndex = count - 1;
                }
                else
                {
                    SelectedIndex = (int)selectedIndex / 2;
                }
            }
        }

        public TimeSpanComboBox()
        {
            InitializeComponent();

        }

        public bool NotFireChanged;

        private void ComboBoxSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (NotFireChanged) return;

            var t = SelectedItem as string;
            if (string.IsNullOrEmpty(t)) return;

            TimeSpan ts;
            if (TimeSpan.TryParse(t, out ts))
            {
                NotFireChanged = true;
                SelectedTime = ts;
                Text = string.Format("{0:00}:{1:00}", ts.Hours, ts.Minutes);
                NotFireChanged = false;
            }
        }

        private void ComboBoxTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (NotFireChanged) return;

            if (_changerTimer == null)
            {
                _changerTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(300), DispatcherPriority.Background, OnSearchCallback, this.Dispatcher);
            }

            _changerTimer.Start();
        }

        private DispatcherTimer _changerTimer;
        private CancellationTokenSource cancellFindTokenSource = new CancellationTokenSource();

        private void OnSearchCallback(object sender, EventArgs e)
        {
            if (_changerTimer != null) _changerTimer.Stop();

            var text = Text;
            TimeSpan ts;
            if (!string.IsNullOrEmpty(text) && TimeSpan.TryParse(text, out ts))
            {
                NotFireChanged = true;
                SelectedTime = ts;
                NotFireChanged = false;
            }
        }
    }
}
