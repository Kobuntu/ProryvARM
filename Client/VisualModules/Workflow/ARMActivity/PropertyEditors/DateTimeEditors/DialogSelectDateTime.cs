using System;
using System.Activities;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;

namespace Proryv.Workflow.Activity.ARM.PropertyEditors.DateTimeEditors
{
    internal class DialogSelectDateTime : DialogPropertyValueEditor
    {
        #region ConvertEditor

        public class ConvertEditor : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return new InArgument<object[]>(value as object[]);
            }
        }

        #endregion

        public DialogSelectDateTime()
        {
            this.InlineEditorTemplate = new DataTemplate();

            var stack = new FrameworkElementFactory(typeof(DockPanel));
            stack.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            stack.SetValue(DockPanel.LastChildFillProperty, true);

            // button
            var editModeSwitch = new FrameworkElementFactory(typeof(EditModeSwitchButton));
            editModeSwitch.SetValue(DockPanel.DockProperty, Dock.Right);
            editModeSwitch.SetValue(EditModeSwitchButton.TargetEditModeProperty, PropertyContainerEditMode.Dialog);
            stack.AppendChild(editModeSwitch);

            // text
            var textBox = new FrameworkElementFactory(typeof(TextBox));
            var textBoxBinding = new Binding("Value") { Converter = new ConvertEditor(), Mode = BindingMode.OneWay, };
            textBox.SetValue(TextBox.TextProperty, textBoxBinding);
            textBox.SetValue(TextBoxBase.IsReadOnlyProperty, true);

            //textBox.SetValue(Label.MaxWidthProperty, 300.0);
            stack.AppendChild(textBox);

            this.InlineEditorTemplate.VisualTree = stack;
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            var dialog = new DateTimeDialogControl(DateTime.Now.Date) {Title = "Задайте дату/время"};

            if (dialog.ShowOkCancel())
            {
                if (dialog.dpDate.SelectedDate.HasValue && dialog.cbHalfHour.SelectedItem is KeyValuePair<TimeSpan, string>)
                {
                    propertyValue.Value = new InArgument<DateTime>(
                        dialog.dpDate.SelectedDate.Value.Date.Add(((KeyValuePair<TimeSpan, string>) dialog.cbHalfHour.SelectedItem).Key));
                }
            }
        }
    }
}
