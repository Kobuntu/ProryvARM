using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Proryv.ElectroARM.Alarms.Alarm;

namespace Proryv.ElectroARM.Alarms.Commands
{
    public static class AlarmListODataCommand
    {
        public static readonly RoutedUICommand ConfirmSelected = new RoutedUICommand
        (
            "Подтвердить выделенные строки",
            "ConfirmSelected",
            typeof(AlarmListODataCommand)
        );

        public static readonly RoutedUICommand ConfirmAllFiltered = new RoutedUICommand
        (
            "Подтвердить отфильтрованные",
            "ConfirmAllFiltered",
            typeof(AlarmListODataCommand)
        );
    }
}
