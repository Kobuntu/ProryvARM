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
using Proryv.AskueARM2.Client.Visual.Common;
using System.Activities.Presentation.Converters;
using System.Activities.Presentation.Model;
using System.Activities.Presentation;
using System.Threading;
using System.Windows.Threading;

namespace Proryv.Workflow.Activity.ARM
{

  
    class ManageDeviceParametrsPropEditor : DialogPropertyValueEditor
    {

        public class ConvertEditor : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                string s = value as string;

                CommandInfo cInfo ;
                try
                {
                    cInfo = s.DeserializeFromString<CommandInfo>();
                }
                catch (Exception ex)
                {
                    cInfo = null;
                }

                if (cInfo == null)
                    s = "Не определено";
                else
                    s = "Есть (" + cInfo.MeterType+")";
                return s;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return null;
            }
        }

        public ManageDeviceParametrsPropEditor()
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
        }

        public static void DoEvents()
        {
            Thread.Sleep(5);
            DispatcherFrame f = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
            (SendOrPostCallback)delegate(object arg)
            {
                DispatcherFrame fr = arg as DispatcherFrame;
                fr.Continue = false;
            }, f);
            Dispatcher.PushFrame(f);
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            CommandInfo oldSetting = null; // -- если возникнет потребность в редактировании, то тут будут лежать предыдущие настройки
            string s = propertyValue.Value as string; 
            if (!string.IsNullOrEmpty(s))
            {
                try
                {
                    oldSetting = s.DeserializeFromString<CommandInfo>();
                }
                catch (Exception)
                {
                    oldSetting = null;
                }
            }

            CommandInfo paramscommand = null;
            bool isCloseDialog = false;

            Manager.Modules.CreateCommandDialog(oldSetting, delegate(CommandInfo command)
            {
                isCloseDialog = true;
                paramscommand = command;
            },true);

            while (!isCloseDialog)
            {
                DoEvents();
            }
            if (paramscommand == null)
                propertyValue.Value = null;
            else
                propertyValue.Value = paramscommand.SerializeToString<CommandInfo>();

        }

    }
}
