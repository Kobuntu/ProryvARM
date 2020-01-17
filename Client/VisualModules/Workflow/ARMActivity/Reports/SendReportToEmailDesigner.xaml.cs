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
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;

namespace Proryv.Workflow.Activity.ARM
{
    // Interaction logic for SendReportToEmailDesigner.xaml
    public partial class SendReportToEmailDesigner
    {
        public SendReportToEmailDesigner()
        {
            InitializeComponent();
            List<KeyValuePair<ReportExportFormat,string>> source = new List<KeyValuePair<ReportExportFormat,string>>();
foreach (string techParam in Enum.GetNames(typeof(ReportExportFormat)))
{
    //.Select(s => (ReportExportFormat)Enum.Parse(typeof(ReportExportFormat), s))
 source.Add(new KeyValuePair<ReportExportFormat,string>((ReportExportFormat)Enum.Parse(typeof(ReportExportFormat), techParam),techParam));
}
            comboBoxReportExportFormat.ItemsSource = source;

        }
    }
}
