using System;
using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using Proryv.AskueARM2.Client.ServiceReference.Service.Extensions;
using Proryv.AskueARM2.Client.ServiceReference.StimulReportService;
using Proryv.AskueARM2.Client.Visual.Common;
using Proryv.AskueARM2.Client.Visual.Common.Modules;
using Proryv.Workflow.Activity.ARM.PropertyEditors.BusinessObjectsReports;

namespace Proryv.Workflow.Activity.ARM.PropertyEditors.HierarchyObjectSelector
{
    internal class HierarchyObjectIdEditor : DialogPropertyValueEditor
    {
        #region ConvertEditor

        public class ConvertEditor : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var propVal = value as string;
                if (propVal != null)
                {
                    if (propVal == string.Empty) return "<Объекты не соответствуют типу отчета!>";

                    MultiPsSelectedArgs args = null;
                    try
                    {
                        args = propVal.DeserializeFromString<MultiPsSelectedArgs>();
                    }
                    catch (Exception ex)
                    {

                    }

                    if (args == null) return "<Ошибка десериализации>";

                    var pss = args.PSList;
                    if (pss != null && pss.Count > 0)
                    {
                        var n = new StringBuilder();
                        foreach (var id in pss.Take(3))
                        {
                            if (id.ID == -1)
                            {
                                n.Append("<Все >");
                                break;
                            }

                            var hierObject = id.ToHierarchyObject();
                            if (hierObject == null) continue;
                            n.Append(hierObject.Name).Append(", ");
                            if (n.Length > 400) break;
                        }

                        if (n.Length > 2) n.Remove(n.Length - 2, 1);

                        if (pss.Count > 3) n.Append("...");
                        return n.ToString();

                    }

                    //Manager.UI.ShowMessage("Выберите объекты!");
                    return "< Объекты не выбраны >";
                }
                return "";
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return new InArgument<object[]>(value as object[]);
            }
        }

        #endregion

        public HierarchyObjectIdEditor()
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

            Binding textBoxBinding = new Binding("Value") { Converter = new ConvertEditor(), Mode = BindingMode.OneWay, };
            textBox.SetValue(TextBox.TextProperty, textBoxBinding);
            textBox.SetValue(TextBoxBase.IsReadOnlyProperty, true);

            //textBox.SetValue(Label.MaxWidthProperty, 300.0);
            stack.AppendChild(textBox);

            this.InlineEditorTemplate.VisualTree = stack;
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            propertyValue.Value = new MultiPsSelectedArgs().SerializeToString<MultiPsSelectedArgs>();
            string title;
            var selector = Manager.Modules.CreateModule(ModuleType.MultiPSSelector, out title, enumDateRangeMode.WithoutDateMode, enumTreeMode.AnyObject, 
                false);

            selector.Width = SystemParameters.PrimaryScreenWidth - 30;
            selector.Height = SystemParameters.PrimaryScreenHeight - 30;

            ShowMultiPsSelector(propertyValue, selector, title);
        }

        private void ShowMultiPsSelector(PropertyValue propertyValue, FrameworkElement selector, string title)
        {
            var dialog = new ReportBusinessObjectsArgsDialog();
            dialog.ccMultiPSSelector.Content = selector;
            dialog.Title = title;

            if (dialog.ShowOkCancel())
            {
                var iArgs = selector as IModuleArgs;
                if (iArgs != null)
                {
                    propertyValue.Value = iArgs.GetArgs().SerializeToString<MultiPsSelectedArgs>();
                    /* todo Доработать
                    Literal<string> literal;
                    if (av.AttachName == null || av.AttachName.Expression == null || (literal = av.AttachName.Expression as Literal<string>) == null || string.IsNullOrEmpty(literal.Value))
                    {
                        av.AttachName = new InArgument<string>(businessObjectName + ".xls");
                    }
                     * */
                }
            }
        }
    }
}
