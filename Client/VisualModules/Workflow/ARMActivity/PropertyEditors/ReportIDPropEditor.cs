using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.PropertyEditing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.Data;
using System.Activities;
using Microsoft.VisualBasic.Activities;
using System.Windows.Media;

namespace Proryv.Workflow.Activity.ARM
{

  
    class ReportIDPropEditor : DialogPropertyValueEditor
    {
        public class RepClass
        {
            public string Report_UN { get; set; }
            public string ReportName { get; set; }
            //public string HierLev0_Name { get; set; }
            public string HierLev1_Name { get; set; }
            public string HierLev2_Name { get; set; }
            public string HierLev3_Name { get; set; }
            public string PS_Name { get; set; }
            public string User_ID { get; set; }
        }

        public class ConvertEditor : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                InArgument<string> propVal = (InArgument<string>)value;
                if (propVal != null)
                {
                    if (propVal.Expression is VisualBasicValue<string>)
                    {
                        return ((VisualBasicValue<string>)propVal.Expression).ExpressionText;
                    }
                    else
                    return propVal.Expression.ToString();
                }
                else
                    return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                string s = value as string;
                return new InArgument<string>(new VisualBasicValue<string>(s));
            }
        }

        public ReportIDPropEditor()
        {
            this.InlineEditorTemplate = new DataTemplate();

            FrameworkElementFactory stack = new FrameworkElementFactory(typeof(StackPanel));
            stack.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            // button
            FrameworkElementFactory editModeSwitch = new FrameworkElementFactory(typeof(EditModeSwitchButton));
            editModeSwitch.SetValue(EditModeSwitchButton.TargetEditModeProperty, PropertyContainerEditMode.Dialog);
            stack.AppendChild(editModeSwitch);
            // text
            FrameworkElementFactory textBox = new FrameworkElementFactory(typeof(TextBox));

            Binding textBoxBinding = new Binding("Value");
            textBoxBinding.Converter = new ConvertEditor();
            textBox.SetValue(TextBox.TextProperty, textBoxBinding);

            //textBox.SetValue(Label.MaxWidthProperty, 300.0);
            stack.AppendChild(textBox);

            this.InlineEditorTemplate.VisualTree = stack;
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {

            ReportIDDialog dialogcontent = new ReportIDDialog();
            InArgument<string> propVal = (InArgument<string>)propertyValue.Value;
            string CurrRepId = null;
            if (propVal != null)
                CurrRepId = propVal.Expression.ToString();

            var tree = dialogcontent.treeView1;
            tree.Items.Clear();

            DataTable TempTable = null;

            string sql =
@"set nocount on;set transaction isolation level read uncommitted;
SELECT     Info_Report.Report_UN, Info_Report.StringName, Info_Report.HierLev0_ID, Info_Report.HierLev1_ID, Info_Report.HierLev2_ID, Info_Report.HierLev3_ID, 
                      Info_Report.PS_ID, Info_Report.User_ID, Info_Report.CreateDateTime, Info_Report.DispatchDateTime, Info_Report.DateTimeDialog, 
                      Dict_HierLev0.StringName AS Lev0, 
                      Dict_HierLev1.StringName AS Lev1, 
                      Dict_HierLev2.StringName AS Lev2, 
                      Dict_HierLev3.StringName AS Lev3, 
                      Dict_PS.StringName AS PSNAme
FROM         Info_Report LEFT JOIN
                      Dict_PS ON Info_Report.PS_ID = Dict_PS.PS_ID
                      LEFT OUTER JOIN Dict_HierLev2 ON Info_Report.HierLev2_ID = Dict_HierLev2.HierLev2_ID 
                      LEFT OUTER JOIN Dict_HierLev1 ON Info_Report.HierLev1_ID = Dict_HierLev1.HierLev1_ID AND Info_Report.HierLev1_ID = Dict_HierLev1.HierLev1_ID 
                      LEFT OUTER JOIN Dict_HierLev3 ON Info_Report.HierLev3_ID = Dict_HierLev3.HierLev3_ID AND Info_Report.HierLev3_ID = Dict_HierLev3.HierLev3_ID AND 
                      Dict_HierLev2.HierLev2_ID = Dict_HierLev3.HierLev2_ID 
                      LEFT OUTER JOIN Dict_HierLev0 ON Info_Report.HierLev0_ID = Dict_HierLev0.HierLev0_ID
ORDER BY  Info_Report.HierLev0_ID, Info_Report.HierLev1_ID, Info_Report.HierLev2_ID, 
Info_Report.HierLev3_ID, Info_Report.PS_ID";

            try
            {
                var serverData = ARM_Service.REP_Query_Report(sql, new List<QueryParameter>());
                TempTable = serverData.Key;
            }
            catch (Exception e)
            {
            }

            TreeViewItem Lev1 = null;
            TreeViewItem Lev2 = null;
            TreeViewItem Lev3 = null;
            TreeViewItem LevPs = null;
            TreeViewItem lastlev = null;
            if (TempTable != null)
            {
                foreach (DataRow row in TempTable.Rows)
                {
                    var rClass = new RepClass
                    {
                        Report_UN = row["Report_UN"].ToString(),
                        ReportName = row["StringName"].ToString(),
                        HierLev1_Name = row["Lev1"] as string,
                        HierLev2_Name = row["Lev2"] as string,
                        HierLev3_Name = row["Lev3"] as string,
                        PS_Name = row["PSNAme"] as string
                    };

                    if ((Lev1 == null) || (Lev1 != null && Lev1.Header != null &&
                                           Lev1.Header.ToString() != rClass.HierLev1_Name))
                    {
                        Lev1 = new TreeViewItem();
                        if (string.IsNullOrEmpty(rClass.HierLev1_Name))
                            Lev1.Header = rClass.ReportName;
                        else
                            Lev1.Header = rClass.HierLev1_Name;
                        Lev1.IsExpanded = true;
                        tree.Items.Add(Lev1);
                        lastlev = Lev1;
                    }

                    if ((Lev2 == null && !string.IsNullOrEmpty(rClass.HierLev2_Name)) ||
                        (Lev2 != null && Lev2.Header != null && Lev2.Header.ToString() != rClass.HierLev2_Name))
                    {
                        Lev2 = new TreeViewItem();
                        Lev2.Header = rClass.HierLev2_Name;
                        if (Lev1 != null)
                            Lev1.Items.Add(Lev2);
                        Lev2.IsExpanded = true;
                        lastlev = Lev2;
                    }

                    if ((Lev3 == null && !string.IsNullOrEmpty(rClass.HierLev3_Name)) ||
                        (Lev3 != null && Lev3.Header != null && Lev3.Header.ToString() != rClass.HierLev3_Name))
                    {
                        Lev3 = new TreeViewItem
                        {
                            Header = rClass.HierLev3_Name,
                            IsExpanded = true
                        };

                        if (Lev2 != null)
                            Lev2.Items.Add(Lev3);
                        lastlev = Lev3;
                    }

                    if ((LevPs == null && !string.IsNullOrEmpty(rClass.PS_Name)) ||
                        (LevPs != null && LevPs.Header != null && LevPs.Header.ToString() != rClass.PS_Name))
                    {
                        LevPs = new TreeViewItem
                        {
                            Header = rClass.PS_Name,
                            IsExpanded = true
                        };

                        if (Lev3 != null)
                            Lev3.Items.Add(LevPs);
                        
                        lastlev = LevPs;
                    }

                    if (lastlev != null)
                    {
                        var repLev = new TreeViewItem
                        {
                            Header = rClass.ReportName,
                            Tag = rClass
                        };
                        lastlev.Items.Add(repLev);

                        if (CurrRepId == rClass.Report_UN)
                            repLev.IsSelected = true;
                    }
                }
            }

            if (dialogcontent.ShowOkCancel())
            {
                if (tree.SelectedItem != null)
                {
                    var selitem = tree.SelectedItem as TreeViewItem;
                    if (selitem != null && selitem.Tag != null)
                    {
                        var RClass = selitem.Tag as RepClass;
                        if (RClass != null)
                        {
                            var sel = RClass.Report_UN;
                            propertyValue.Value = new InArgument<string>(sel);
                        }
                    }
                }
            }
        }

    }
}
