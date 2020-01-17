using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.PropertyEditing;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;

namespace Proryv.Workflow.Activity.ARM
{

    class MinuteCountPropertyEditor : DialogPropertyValueEditor
    {

        //public class ConvertEditor : IValueConverter
        //{
        //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        //    {
        //        TimeSpan v = value as TimeSpan;
        //        string s;
        //        if (login == null)
        //            s = "Не определено";
        //        else
        //            s = "Есть (" + login.UserName + ")";
        //        return s;
        //    }

        //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        //    {
        //        return null;
        //    }
        //}

        public MinuteCountPropertyEditor()
        {

            this.InlineEditorTemplate = new DataTemplate();

            FrameworkElementFactory stack = new FrameworkElementFactory(typeof(StackPanel));
            stack.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            // button
            FrameworkElementFactory editModeSwitch = new FrameworkElementFactory(typeof(EditModeSwitchButton));
            editModeSwitch.SetValue(EditModeSwitchButton.TargetEditModeProperty, PropertyContainerEditMode.Dialog);
            stack.AppendChild(editModeSwitch);
            // text
            FrameworkElementFactory label = new FrameworkElementFactory(typeof(Label));

            Binding labelBinding = new Binding("Value");
            //labelBinding.Converter = new ConvertEditor();
            label.SetValue(Label.ContentProperty, labelBinding);

            stack.AppendChild(label);
            this.InlineEditorTemplate.VisualTree = stack;
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            TimeSpan value;
            try
            {
                value = (TimeSpan)propertyValue.Value;
            }
            catch (Exception)
            {
                value = TimeSpan.MinValue;
            }
            MinuteCountPropertyDialog dialog = new MinuteCountPropertyDialog();
            if (value != null)
            {
                dialog.days.Text = value.Days.ToString();
                dialog.hours.Text = value.Hours.ToString();
                dialog.mins.Text = value.Minutes.ToString();
            }

            if (dialog.ShowOkCancel())
            {

                try
                {
                    value = new TimeSpan(Convert.ToInt32(dialog.days.Text), Convert.ToInt32(dialog.hours.Text), Convert.ToInt32(dialog.mins.Text), 0);
                }
                catch (Exception)
                {
                    MessageBox.Show("Неправильно указано время актуальности воздействия!");
                    return;
                }
                propertyValue.Value = value;
            }
        }
    }

}
