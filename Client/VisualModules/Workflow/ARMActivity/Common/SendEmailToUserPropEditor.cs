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

namespace Proryv.Workflow.Activity.ARM
{
    class SendEmailToUserPropEditor : PropertyValueEditor
    {
        private ComboBox _owner;

        public SendEmailToUserPropEditor()
        {
           this.InlineEditorTemplate = new DataTemplate();
            
           FrameworkElementFactory stack = new FrameworkElementFactory(typeof(StackPanel));
           FrameworkElementFactory ComboBoxProp = new FrameworkElementFactory(typeof(ComboBox));
            Binding ComboBinding = new Binding("Value");
            ComboBinding.Mode = BindingMode.TwoWay;
            ComboBoxProp.SetValue(ComboBox.TextProperty, ComboBinding);
            ComboBoxProp.SetValue(ComboBox.IsEditableProperty, true);
            stack.AppendChild(ComboBoxProp);

            ComboBoxProp.AddHandler(
               ComboBox.LoadedEvent,
               new RoutedEventHandler(
                   (sender, e) =>
                   {
                       _owner = (ComboBox)sender;
                       _owner.DropDownOpened += new EventHandler(ExpandCombo);

                       _owner.SelectionChanged += EditorSelectionChanged;
                       INotifyPropertyChanged data = _owner.DataContext as INotifyPropertyChanged;
                       if (data != null)
                       {
                           data.PropertyChanged += DatacontextPropertyChanged;
                       }
                       _owner.DataContextChanged += CultureDatacontextChanged;
                   }));

           this.InlineEditorTemplate.VisualTree = stack;

        }

        void ExpandCombo(object sender, EventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            string s = "";
            if (combo.SelectedItem != null)
            s = combo.SelectedItem.ToString();
            combo.Items.Clear();
            List<UserInfo> UInfos = ARM_Service.EXPL_Get_All_Users();
            foreach (UserInfo u in UInfos)
                combo.Items.Add(u.UserName);
            if (!string.IsNullOrEmpty(s))
            {
                combo.SelectedIndex = combo.Items.IndexOf(s);
            }
        }

        private void EditorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_owner == null) return;
            object dataContext = _owner.DataContext;
            if (dataContext == null) return;
            var v = dataContext.GetType().GetProperty("Value",
                                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            if (v == null) return;
            try
            {
                v.SetValue(dataContext, _owner.SelectedItem, new object[] {});
            }
            catch (Exception ex)
            {
                v.SetValue(dataContext, null, new object[] { });
            }
        }

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
