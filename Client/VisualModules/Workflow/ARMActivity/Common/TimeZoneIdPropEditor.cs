using System;
using System.Activities.Presentation.PropertyEditing;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Proryv.AskueARM2.Client.Visual.Common.GlobalDictionary;

namespace Proryv.Workflow.Activity.ARM.Common
{
    public class TimeZoneIdPropEditor : PropertyValueEditor
    {
        private ComboBox _owner;

        public TimeZoneIdPropEditor()
        {
            this.InlineEditorTemplate = new DataTemplate();

            var stack = new FrameworkElementFactory(typeof(StackPanel));
            var comboBoxProp = new FrameworkElementFactory(typeof(ComboBox));
            var comboBinding = new Binding("Value") {Mode = BindingMode.TwoWay};
            comboBoxProp.SetValue(ComboBox.TextProperty, comboBinding);
            comboBoxProp.SetValue(ComboBox.IsEditableProperty, true);
            stack.AppendChild(comboBoxProp);

            comboBoxProp.AddHandler(
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(
                    (sender, e) =>
                    {
                        _owner = (ComboBox)sender;
                        _owner.DropDownOpened += ExpandCombo;

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
            var combo = (ComboBox)sender;
            var s = "";
            if (combo.SelectedItem != null)
                s = combo.SelectedItem.ToString();
            combo.Items.Clear();

            foreach (var tzi in GlobalEnumsDictionary.RussianTimeZones.Values)
            {
                combo.Items.Add(tzi.DisplayName);
            }

            if (!string.IsNullOrEmpty(s))
            {
                combo.SelectedIndex = combo.Items.IndexOf(s);
            }
        }

        private void EditorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_owner == null) return;
            var dataContext = _owner.DataContext;
            if (dataContext == null) return;
            var v = dataContext.GetType().GetProperty("Value",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            if (v == null) return;
            try
            {
                v.SetValue(dataContext, _owner.SelectedItem, new object[] { });
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
            var old = e.OldValue as INotifyPropertyChanged;
            if (old != null)
            {
                old.PropertyChanged -= DatacontextPropertyChanged;
            }
            var newDataContext = e.NewValue as INotifyPropertyChanged;
            if (newDataContext != null)
            {
                newDataContext.PropertyChanged += DatacontextPropertyChanged;
            }
        }
    }
}
