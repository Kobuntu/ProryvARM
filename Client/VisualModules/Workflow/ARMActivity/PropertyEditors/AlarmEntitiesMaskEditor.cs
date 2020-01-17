using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.PropertyEditing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Proryv.AskueARM2.Client.ServiceReference.ARM_20_Service;
using System.Reflection;

namespace Proryv.Workflow.Activity.ARM.PropertyEditors
{
   
    class AlarmEntitiesMaskEditor : DialogPropertyValueEditor
    {
        public class AlarmEntityItem
        {
            public enumAlarmType ParamId { get; set; }
            public string ParamName { get; set; }
            public bool IsChecked { get; set; }
        }
        public class ConvertBinding : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {

                List<enumAlarmType> v = value as List<enumAlarmType>;

                string s;
                if (v == null)
                    s = "Не определено";
                else
                    s = "Выбрано сущностей : " + v.Count.ToString();

                return s;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return null;
            }
        }

        public AlarmEntitiesMaskEditor()
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

            ConvertBinding convertBind = new ConvertBinding();

            Binding labelBinding = new Binding("Value");
            labelBinding.Converter = new ConvertBinding();
            label.SetValue(Label.ContentProperty, labelBinding);

            label.SetValue(Label.MaxWidthProperty, 300.0);
            stack.AppendChild(label);

            this.InlineEditorTemplate.VisualTree = stack;
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            List<enumAlarmType> propvalue = (List<enumAlarmType>)propertyValue.Value;
            if (propvalue == null)
                propvalue = new List<enumAlarmType>();

            Type T = typeof(enumAlarmType);
            List<AlarmEntityItem> ParamItemList = new List<AlarmEntityItem>();

            enumAlarmTypeTypeConverter enumconv = new enumAlarmTypeTypeConverter();

            string PName;
            foreach (FieldInfo fi in T.GetFields())
            {
                if (!Enum.IsDefined(T, fi.Name)) continue;

                enumAlarmType val = (enumAlarmType)Enum.Parse(T, fi.Name);
                PName = enumconv.GetDescriprion(val);

                bool found = propvalue.IndexOf(val) > -1;

                ParamItemList.Add(new AlarmEntityItem() { ParamId = val, ParamName = PName, IsChecked = found });
            }

            AlarmEntitiesMaskEditorDialog dialogcontent = new AlarmEntitiesMaskEditorDialog();
            dialogcontent.ListAlarmType.ItemsSource = ParamItemList;
            if (dialogcontent.ShowOkCancel())
            {
                List<enumAlarmType> Result = new List<enumAlarmType>();

                foreach (AlarmEntityItem p in ParamItemList)
                {
                    if (p.IsChecked)
                        Result.Add(p.ParamId);
                }

                propertyValue.Value = Result;
            }
        }
    }
}
