using System;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;

namespace Proryv.Workflow.Activity.ARM.PropertyEditors.DateTimeEditors
{
    /// <summary>
    /// Interaction logic for DateTimeDialogControl.xaml
    /// </summary>
    public partial class DateTimeDialogControl 
    {
        public DateTimeDialogControl(DateTime dt)
        {
            InitializeComponent();
            dpDate.SelectedDate = dt.Date;
            cbHalfHour.ItemsSource = GlobalEnums.HalfHoursList;
            cbHalfHour.SelectedValue = dt.TimeOfDay;
        }
    }
}
