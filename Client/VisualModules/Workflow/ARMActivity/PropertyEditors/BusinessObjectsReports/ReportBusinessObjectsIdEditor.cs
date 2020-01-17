using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Converters;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Microsoft.VisualBasic.Activities;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.Visual.Common;

namespace Proryv.Workflow.Activity.ARM.PropertyEditors
{
    class ReportBusinessObjectsIdEditor : DialogPropertyValueEditor
    {
        public class ConvertEditor : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var propVal = value as InArgument<string>;
                if (propVal != null)
                {
                    var expression = propVal.Expression as VisualBasicValue<string>;
                    if (expression != null)
                    {
                        return expression.ExpressionText;
                    }
                    return propVal.Expression.ToString();
                }

                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return new InArgument<string>(value as string);
            }
        }

        public ReportBusinessObjectsIdEditor()
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
            textBox.SetValue(TextBoxBase.IsReadOnlyProperty, true);

            //textBox.SetValue(Label.MaxWidthProperty, 300.0);
            stack.AppendChild(textBox);

            this.InlineEditorTemplate.VisualTree = stack;
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            ModelPropertyEntryToOwnerActivityConverter ownerActivityConverter = new ModelPropertyEntryToOwnerActivityConverter();
            ModelItem activityItem = ownerActivityConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null) as ModelItem;
            var av = activityItem.GetCurrentValue() as SendBusinessObjectsReportToEmail;
            var currReportUn = string.Empty;
            if (av != null && av.Report_id != null)
            {
                var literal = av.Report_id.Expression as Literal<string>;
                if (literal == null) return;
                currReportUn = literal.Value;
            }

            var dialog = new ReportBusinessObjectsIdDialog(activityItem);

            var lv = dialog.lvReports;
            lv.ItemsSource = null;
            List<Info_Report_Stimul> reports = null;
            try
            {
                reports = ServiceFactory.ArmServiceInvokeSync<List<Info_Report_Stimul>>("REP_GetStimulReports");
            }
            catch (Exception ex)
            {
                Manager.UI.ShowMessage(ex.Message);
                return;
            }

            if (reports == null) return;

            lv.ItemsSource = reports;

            if (!string.IsNullOrEmpty(currReportUn))
            {
                var selectedReport = reports.FirstOrDefault(r => r.Report_UN == currReportUn);
                if (selectedReport != null)
                {
                    lv.SelectedItem = selectedReport;
                }
            }


            if (dialog.ShowOkCancel())
            {
                var selectedReport = lv.SelectedItem as Info_Report_Stimul;
                if (selectedReport == null) return;

                if (Equals(selectedReport.Report_UN, currReportUn)) return;

                propertyValue.Value = new InArgument<string>(selectedReport.Report_UN);
                if (!string.IsNullOrEmpty(currReportUn) && av != null && !string.IsNullOrEmpty(av.Args))
                {
                    Manager.UI.ShowMessage("Изменилась бизнес модель. Объекты для построения отчета необходимо выбрать заново!");
                    av.Args = string.Empty;
                }
            }
        }
    }
}
