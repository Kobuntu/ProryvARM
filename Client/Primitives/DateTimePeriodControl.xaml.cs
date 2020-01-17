using System;
using System.ComponentModel;
using System.Windows;
using Proryv.AskueARM2.Client.Visual;

namespace Proryv.ElectroARM.Controls.Controls.Dialog.Primitives
{
    /// <summary>
    /// Interaction logic for DateRangeControl.xaml
    /// </summary>
    public partial class DateTimePeriodControl :INotifyPropertyChanged, IDisposable
    {
        public DateTimePeriodControl()
        {
            InitializeComponent();
        }

        public DateTime? StartDate
        {
            get { return startDate.SelectedDate; }
            set
            {
                SetValue(StartDateProperty, value);
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("StartDate"));
            }
        }

        public static readonly DependencyProperty StartDateProperty =
            DependencyProperty.Register("StartDate", typeof(DateTime?), typeof(DateTimePeriodControl), new PropertyMetadata(DateTime.Now, startDateChangedCallback));

        static void startDateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var my = d as DateTimePeriodControl;
            my.startDate.SelectedDate = e.NewValue as DateTime?;
        }

        public DateTime? FinishDate
        {
            get { return endDate.SelectedDate; }
            set
            {
                SetValue(FinishdDateProperty, value);
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("FinishDate"));
            }
        }

        public static readonly DependencyProperty FinishdDateProperty =
            DependencyProperty.Register("FinishDate", typeof(DateTime?), typeof(DateTimePeriodControl), new PropertyMetadata(DateTime.Now, finishDateChangedCallback));

        static void finishDateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var my = d as DateTimePeriodControl;
            my.endDate.SelectedDate = e.NewValue as DateTime?;
        }

        private void StartDate_OnSelectedDateChanged(object sender, EventArgs e)
        {
            //if (!Equals(StartDate, startDate.SelectedDate))
            OnSelectedDateChanged(true);
            StartDate = startDate.SelectedDate;
            if (StartDateChanged != null)
            {
                StartDateChanged(this, new EventArgs());
            }
        }

        private void EndDate_OnSelectedDateChanged(object sender, EventArgs e)
        {
            OnSelectedDateChanged(false);
            FinishDate = endDate.SelectedDate;
            if (FinishDateChanged != null)
            {
                FinishDateChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Событие на смену начальной даты, в конечной дате день ставим из начальной даты
        /// </summary>
        private void OnSelectedDateChanged(bool isStartChanged)
        {
            DatePickerHelper.CorrectDateStartOrDateEnd(startDate.dpDate, endDate.dpDate, isStartChanged);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler StartDateChanged;
        public event EventHandler FinishDateChanged;

        public void Dispose()
        {
            startDate.Dispose();
            endDate.Dispose();
            PropertyChanged = null;
            StartDateChanged = null;
            FinishDateChanged = null;
        }
    }
}
