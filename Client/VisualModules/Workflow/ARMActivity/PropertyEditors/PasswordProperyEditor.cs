using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.PropertyEditing;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Security;
using System.Windows.Media;

namespace Proryv.Workflow.Activity.ARM
{
    class PasswordProperyEditor : DialogPropertyValueEditor
    {

        public class ConvertEditor : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                LoginParams login = value as LoginParams;
                string s;
                if (login == null)
                    s = "Не определено";
                else
                    s = "Есть (" + login.UserName + ")";
                return s;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return null;
            }
        }

        public PasswordProperyEditor()
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
            labelBinding.Converter = new ConvertEditor();
            label.SetValue(Label.ContentProperty, labelBinding);

            stack.AppendChild(label);
            this.InlineEditorTemplate.VisualTree = stack;
/*
            this.InlineEditorTemplate = new DataTemplate();

            FrameworkElementFactory stack = new FrameworkElementFactory(typeof(StackPanel));
            stack.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            FrameworkElementFactory textBox = new FrameworkElementFactory(typeof(PasswordBox));
            textBox.Name = "Passw";

            //PasswordHelper.SetAttach(textBox, true);
            //textBox.SetBinding(PasswordHelper.PasswordProperty, property.CreateBinding());
            //Binding textBoxBinding = new Binding("Value");
            //Binding BoundBoxBinding = new Binding("Password");
            //BoundBoxBinding.Source = textBox;
            //BoundBoxBinding.
            // textBoxBinding.Converter = new ConvertEditor();
            //textBox.SetValue(PasswordBox., textBoxBinding);

            //textBox.SetValue(Label.MaxWidthProperty, 300.0);
            stack.AppendChild(textBox);

            //var pb = GetChild<PasswordBox>(stack);

            this.InlineEditorTemplate.VisualTree = stack;

            //VisualTreeHelper.GetChild(
            //PasswordBox pb = this.InlineEditorTemplate.FindName("Passw", null) as PasswordBox;
 */
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            LoginParams login = propertyValue.Value as LoginParams;
            PasswordProperyDialog LoginDialog = new PasswordProperyDialog();
            if (login != null)
            {
                LoginDialog.textBoxUserName.Text = login.UserName;
                LoginDialog.passwordBoxPassw.Password = login.Password;
            }

            if (LoginDialog.ShowOkCancel())
            {
                login = new LoginParams();
                login.UserName =  LoginDialog.textBoxUserName.Text ;
                login.Password = LoginDialog.passwordBoxPassw.Password;
                propertyValue.Value = login;
            }
        }
    }
}
