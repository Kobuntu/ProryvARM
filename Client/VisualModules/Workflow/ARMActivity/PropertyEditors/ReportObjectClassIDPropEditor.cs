using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Activities;
using Microsoft.VisualBasic.Activities;
using System.Windows;
using System.Activities.Presentation.PropertyEditing;
using System.Windows.Controls;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.Data;

namespace Proryv.Workflow.Activity.ARM
{
    class ReportObjectClassIDPropEditor : DialogPropertyValueEditor
    {
        class RepClass
        {
            public string Report_UN { get; set; }
            public string ReportName { get; set; }
            public int ObjectClass { get; set; }
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

        public ReportObjectClassIDPropEditor()
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

            ReportObjectClassIDDialog dialogcontent = new ReportObjectClassIDDialog();
            InArgument<string> propVal = (InArgument<string>)propertyValue.Value;
            string CurrRepId = null;
            if (propVal != null)
                CurrRepId = propVal.Expression.ToString();

            TreeView tree = dialogcontent.treeView1;
            tree.Items.Clear();

            DataTable TempTable = null;

            string sql =
@"set nocount on;set transaction isolation level read uncommitted;
SELECT ReportObjectClass_UN
      ,ObjectClass
      ,StringName
FROM Report_Reports_For_ObjectClass_List
order by ObjectClass, StringName";
            try
            {
                var serverData = ARM_Service.REP_Query_Report(sql, new List<QueryParameter>());
                TempTable = serverData.Key;
            }
            catch (Exception e)
            {
            }

            TreeViewItem Lev1 = null;
            int LastOClas = -1;

            if (TempTable != null)
            {
                foreach (DataRow row in TempTable.Rows)
                {
                    var RClass = new RepClass
                    {
                        Report_UN = row["ReportObjectClass_UN"].ToString(),
                        ReportName = row["StringName"].ToString(),
                        ObjectClass = (int) row["ObjectClass"]
                    };

                    string Level0Name = "Неизвестный";
                    switch (RClass.ObjectClass)
                    {
                        case 1:
                            Level0Name = "Отчеты для ПС";
                            break;
                        case 2:
                            Level0Name = "Отчеты для ТП";
                            break;
                        case 3:
                            Level0Name = "Отчеты для МКД";
                            break;
                    }

                    if (LastOClas != RClass.ObjectClass)
                    {
                        LastOClas = RClass.ObjectClass;
                        Lev1 = new TreeViewItem
                        {
                            Header = Level0Name,
                            IsExpanded = true
                        };
                        tree.Items.Add(Lev1);
                    }

                    var repLev = new TreeViewItem
                    {
                        Header = RClass.ReportName,
                        Tag = RClass
                    };
                    Lev1.Items.Add(repLev);
                    if (CurrRepId == RClass.Report_UN)
                        repLev.IsSelected = true;
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
                            //propVal.Expression = new VisualBasicValue<string>(sel);
                            //propertyValue.Value = new InArgument<string>(new VisualBasicValue<string>(sel));
                            propertyValue.Value = new InArgument<string>(sel);
                        }
                    }
                }
            }
        }

    }
}
