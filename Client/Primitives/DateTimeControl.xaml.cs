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
using Proryv.AskueARM2.Client.Visual.Common;

namespace Proryv.ElectroARM.Controls.Controls.Dialog.Primitives
{
    /// <summary>
    /// Interaction logic for DateTimeControl.xaml
    /// </summary>
    public partial class DateTimeControl : IDisposable
    {
        public DateTimeControl()
        {
            InitializeComponent();

            _selectedDateChangedDescriptor = DependencyPropertyDescriptor.FromProperty(Xceed.Wpf.Controls.DatePicker.SelectedDateProperty, typeof(Xceed.Wpf.Controls.DatePicker));
            _selectedDateChangedDescriptor.AddValueChanged(dpDate, OnSelectedDateChanged);
        }

        readonly DependencyPropertyDescriptor _selectedDateChangedDescriptor;

        public bool WithTime
        {
            get { return MtbTime.Visibility == Visibility.Visible; }
            set { SetValue(WithTimeProperty, value); }
        }

        public static readonly DependencyProperty WithTimeProperty =
            DependencyProperty.Register("WithTime", typeof(bool), typeof(DateTimeControl), new PropertyMetadata(true, WithTimePropertyChangedCallback));

        private static void WithTimePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dc = d as DateTimeControl;
            if (dc != null)
            {
                try
                {
                    var t = (bool)e.NewValue;
                    dc.MtbTime.Visibility = t ? Visibility.Visible : Visibility.Collapsed;
                }
                catch
                {
                }
            }
        }


        public DateTime? SelectedDate
        {
            get
            {
                var dt = dpDate.SelectedDate;
                if (!dt.HasValue) return dt;

                TimeSpan t;
                return !TimeSpan.TryParse(MtbTime.Text, out t) ? dt.Value.Date : new DateTime(dt.Value.Year, dt.Value.Month, dt.Value.Day, t.Hours, t.Minutes, 0);
            }
            set
            {
                SetValue(SelectedDateProperty, value); 
            }
        }

        public bool Validate(bool isCheckNull)
        {
            if ((isCheckNull && !dpDate.SelectedDate.HasValue)
                ||(dpDate.SelectedDate.HasValue && dpDate.SelectedDate.Value.Year <= 2001))
            {
                return false;
            }

            TimeSpan t;
            return TimeSpan.TryParse(MtbTime.Text, out t);
        }

        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(DateTimeControl), new PropertyMetadata(DateTime.Now, monthPropertyChangedCallback));

        static void monthPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var my = d as DateTimeControl;
            if (my == null)  return;

            var dt = e.NewValue as DateTime?;
            my.dpDate.SelectedDate = dt;
            if (dt.HasValue)
            {
                my.MtbTime.Text = dt.Value.ToString("HH:mm");
            }
        }

        public event EventHandler SelectedDateChanged;

        void raiseEvent()
        {
            if (SelectedDateChanged != null)
            {
                SelectedDateChanged(this, new EventArgs());
            }
        }

        private void OnSelectedDateChanged(object sender, EventArgs e)
        {
            raiseEvent();
        }

        public void Dispose()
        {
            if (_selectedDateChangedDescriptor != null)
            {
                _selectedDateChangedDescriptor.RemoveValueChanged(dpDate, OnSelectedDateChanged);
            }
            SelectedDateChanged = null;
        }

        private void MtbTimeOnTextChanged(object sender, TextChangedEventArgs e)
        {
            raiseEvent();
        }
    }
}
