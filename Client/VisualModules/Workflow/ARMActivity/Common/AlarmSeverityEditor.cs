using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.PropertyEditing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Reflection;
using System.Globalization;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.Activities;
using Microsoft.VisualBasic.Activities;

namespace Proryv.Workflow.Activity.ARM
{
    class AlarmSeverityEditor : PropertyValueEditor
    {
        private ComboBox _owner;

        public class AlarmSeverityEditorConvertor : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var intVal = value as InArgument<System.Int32>;
                if (intVal != null)
                {
                    var val = intVal.Expression as System.Activities.Expressions.Literal<int>;
                    if (val != null)
                        return val.Value;
                    else
                    {

                        var valvb = intVal.Expression as VisualBasicValue<int>;
                        if (valvb != null)
                        {
                            int result = 1;
                            int.TryParse(valvb.ExpressionText, out result);
                            return result;
                        }


                    }
                }

                return 1;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value != null)
                {
                    var stringVal = value.ToString();
                    return new InArgument<int>(new VisualBasicValue<int>(stringVal));
                }
                return new InArgument<int>(new VisualBasicValue<int>("1"));


            }
        }

        public AlarmSeverityEditor()
        {
            this.InlineEditorTemplate = new DataTemplate();

            FrameworkElementFactory stack = new FrameworkElementFactory(typeof(StackPanel));
            FrameworkElementFactory ComboBoxProp = new FrameworkElementFactory(typeof(ComboBox));
            ComboBoxProp.SetValue(ComboBox.ItemsSourceProperty, SeverityLevels);

            ComboBoxProp.SetValue(ComboBox.SelectedValuePathProperty, "Key");
            ComboBoxProp.SetValue(ComboBox.DisplayMemberPathProperty, "Value");
            Binding ComboBinding = new Binding("Value") { Converter = new AlarmSeverityEditorConvertor() };
            ComboBinding.Mode = BindingMode.TwoWay;
            ComboBoxProp.SetValue(ComboBox.SelectedValueProperty, ComboBinding);
            ComboBoxProp.SetValue(ComboBox.IsEditableProperty, true);




            stack.AppendChild(ComboBoxProp);

            this.InlineEditorTemplate.VisualTree = stack;

        }


        List<KeyValuePair<int, string>> SeverityLevels = new List<KeyValuePair<int, string>>()
        {
            //new KeyValuePair<int, string>(0,"Нет"),
              new KeyValuePair<int, string>(1,"Нормальный"),
                new KeyValuePair<int,string>(2,"Предупреждение"),
                  new KeyValuePair<int, string>(3,"Критический"),
        };

        private void DatacontextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // deserialize from name.
            if (e.PropertyName == "Value")
            {
                object value = sender
                    .GetType()
                    .GetProperty("Value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                    .GetValue(sender, new object[] { });

                if (value != null)
                {
                    if (value is string)
                    {
                        //CultureInfo setCulture = new CultureInfo(value.ToString());
                        //_owner.SelectedItem = setCulture;
                    }
                }
            }
        }

        /// <summary>
        /// Called when the context is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private void CultureDatacontextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            INotifyPropertyChanged old = e.OldValue as INotifyPropertyChanged;
            if (old != null)
            {
                old.PropertyChanged -= DatacontextPropertyChanged;
            }
            INotifyPropertyChanged newDataContext = e.NewValue as INotifyPropertyChanged;
            if (newDataContext != null)
            {
                newDataContext.PropertyChanged += DatacontextPropertyChanged;
            }
        }

    }
}
