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
using System.Activities.Presentation.PropertyEditing;
using System.Activities.Presentation;

namespace Proryv.Workflow.Activity.ARM
{
    /// <summary>
    /// Interaction logic for ReportIDDialog.xaml
    /// </summary>
    public partial class ReportObjectClassIDDialog : WorkflowElementDialog
    {
        public ReportObjectClassIDDialog()
        {
            InitializeComponent();
            Title = "Выбор отчета";
            EnableMinimizeButton = false;
        }

        private void WorkflowElementDialog_Loaded(object sender, RoutedEventArgs e)
        {
            treeView1.Focus();
        }
    }
}
