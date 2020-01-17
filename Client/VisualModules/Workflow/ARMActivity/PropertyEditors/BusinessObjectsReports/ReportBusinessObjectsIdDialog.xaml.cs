using System;
using System.Activities.Presentation.Model;
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

namespace Proryv.Workflow.Activity.ARM.PropertyEditors
{
    /// <summary>
    /// Interaction logic for ReportBusinessObjectsIdDialog.xaml
    /// </summary>
    public partial class ReportBusinessObjectsIdDialog
    {
        public ReportBusinessObjectsIdDialog(ModelItem activityItem)
        {
            InitializeComponent();
            Title = "Выбор отчета";
            if (activityItem != null)
            {
                ModelItem = activityItem;
                Owner = activityItem.View;
            }
        }
    }
}
