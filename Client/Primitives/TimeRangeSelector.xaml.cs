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
using Proryv.ElectroARM.Controls.Controls.Dialog.Primitives;
using Xceed.Wpf.Controls;
using Proryv.AskueARM2.Client.Visual.Common;
using System.Collections;

namespace Proryv.AskueARM2.Client.Visual
{
    /// <summary>
    /// Interaction logic for TimeRangeSelector.xaml
    /// </summary>
    public partial class TimeRangeSelector : UserControl
    {
        public TimeRangeSelector()
        {
            InitializeComponent();
            int jan_ind = sp.Children.IndexOf(jan), cur_year = DateTime.Now.Year, cur_month = DateTime.Now.Month;
            for (int i = 1; i <= 12; i++, jan_ind++)
            {
                var but = sp.Children[jan_ind] as Button;
                if (i <= cur_month) but.Content = (but.Content as string) + ", " + cur_year;
                else but.Content = (but.Content as string) + ", " + (cur_year-1);
            }
        }

        public string DateStart { get; set; }

        public string DateEnd { get; set; }

        public string TimeStart { get; set; }

        public string TimeEnd { get; set; }

        Xceed.Wpf.Controls.DatePicker dateStart = null, dateEnd = null;
        MonthYear myDateStart = null, myDateEnd = null; 
        ComboBox timeStart = null, timeEnd = null;

        private void currentMonth_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now;
            setDate((new DateTime(now.Year, now.Month, 1)).DateTimeToWCFDateTime(), (new DateTime(now.Year, now.Month, now.Day)).DateTimeToWCFDateTime());
        }

        private void lastDay_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now.Date.AddDays(-1);
            setDate(now.DateTimeToWCFDateTime(), now.DateTimeToWCFDateTime());
        }

        private void lastMonth_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now;
            var fd = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
            var ld = new DateTime(fd.Year, fd.Month, DateTime.DaysInMonth(fd.Year, fd.Month));
            setDate(fd.DateTimeToWCFDateTime(), ld.DateTimeToWCFDateTime());
        }

        void setTime()
        {
            if (timeStart != null && timeStart.ItemsSource != null && timeStart.ItemsSource is IList)
            {
                if ((timeStart.ItemsSource as IList).Count > 0)
                    timeStart.SelectedIndex = 0;

                if (timeEnd != null)
                {
                    if ((timeStart.ItemsSource as IList).Count > 0)
                        timeEnd.SelectedIndex = (timeStart.ItemsSource as IList).Count - 1;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var uc = (this.Parent as FrameworkElement).FindParent<UserControl>();
            if (uc == null) return;

            var dtStartObject = uc.FindName(DateStart);
            if (dtStartObject is Xceed.Wpf.Controls.DatePicker)
            {
                dateStart = dtStartObject as Xceed.Wpf.Controls.DatePicker;
                dateEnd = uc.FindName(DateEnd) as Xceed.Wpf.Controls.DatePicker;
            }
            else if (dtStartObject is DateControl)
            {
                dateStart = (dtStartObject as DateControl).dpDate;
                dateEnd = (uc.FindName(DateEnd) as DateControl).dpDate;
            }
            else if (dtStartObject is DateTimeControl)
            {
                dateStart = (dtStartObject as DateTimeControl).dpDate;
                dateEnd = (uc.FindName(DateEnd) as DateTimeControl).dpDate;
            }
            else if (dtStartObject is MonthYear)
            {
                myDateStart = dtStartObject as MonthYear;
                myDateEnd = uc.FindName(DateEnd) as MonthYear;
            }

            timeStart = uc.FindName(TimeStart) as ComboBox;
            timeEnd = uc.FindName(TimeEnd) as ComboBox;
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            menu.IsOpen = true;
        }

        private void month_Click(object sender, RoutedEventArgs e)
        {
            var month = Convert.ToInt32((sender as Button).Tag);
            var year = DateTime.Today.Year;
            if (month > DateTime.Today.Month) year--;
            setDate(new DateTime(year, month, 1).DateTimeToWCFDateTime(), new DateTime(year, month, DateTime.DaysInMonth(year, month)).DateTimeToWCFDateTime());
        }

        void setDate(DateTime start, DateTime end)
        {
            if (dateStart != null)
            {
                if (
                    (dateStart.SelectedDate == null || (dateStart.SelectedDate != null && dateStart.SelectedDate.Value != start))
                    &&
                    !(dateEnd.SelectedDate == null || (dateEnd.SelectedDate != null && dateEnd.SelectedDate.Value != start))
                    ) dateStart.SelectedDate = start;
                else 
                {
                    dateStart.Tag = true;
                    dateStart.SelectedDate = start;
                    dateStart.Tag = null;
                }

                if (dateEnd != null)
                {
                    dateEnd.SelectedDate = end;
                }
            }
            else if (myDateStart != null)
            {
                myDateStart.SelectedDate = start;
                myDateEnd.SelectedDate = end;
            }
            setTime();

            if (menu != null)
            {
                menu.IsOpen = false;
            }
        }
    }
}
